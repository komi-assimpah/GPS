using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using ProxyCache.Models;


namespace RoutingServer
{
    public class RoutingService : IRoutingService
    {
        private ProxyServiceReference.ProxyServiceClient client = new ProxyServiceReference.ProxyServiceClient();
        private readonly ActiveMqProducer producer = new ActiveMqProducer();

        Dictionary<string, Itinerary> result;

        public Dictionary<string, Itinerary> suggestJourney(string startLat, string startLng, string endLat, string endLng, string clientId)
        {
            Utils.AddCorsHeaders();

            Console.WriteLine("\nPlanning walking journey...");

            Position startPosition = new Position
            {
                Lat = double.Parse(startLat.Trim(',')),
                Lng = double.Parse(startLng.Trim(','))
            };

            Position endPosition = new Position
            {
                Lat = double.Parse(endLat.Trim(',')),
                Lng = double.Parse(endLng.Trim(','))
            };

            var footItinerary = GetItineraryFromProxy(startPosition, endPosition).Result;
            Boolean notByFoot = footItinerary.Distance == 0 || footItinerary.Duration == 0 || footItinerary.Instructions.Count == 0;
            if (notByFoot)
                Console.WriteLine("\nUnable to compute walking itinerary");

            Console.WriteLine("\nPlanning cycling journey...");

            Boolean notCycling;
            var closest = computeClosestStations(startPosition, endPosition);
            if ((closest["StartAvailable"].Station == null || closest["EndAvailable"].Station == null))
                notCycling = false;

            var footItinerary1 = GetItineraryFromProxy(startPosition, closest["StartAvailable"].Station.Position).Result;
            var footItinerary2 = GetItineraryFromProxy(closest["EndAvailable"].Station.Position, endPosition).Result;
            var bikeItinerary = GetItineraryFromProxy(startPosition, endPosition, "cycling").Result;

            Boolean notByFoot1 = footItinerary1.Distance == 0 || footItinerary1.Duration == 0 || footItinerary1.Instructions.Count == 0;
            Boolean notByFoot2 = footItinerary2.Distance == 0 || footItinerary2.Duration == 0 || footItinerary2.Instructions.Count == 0;
            notCycling = bikeItinerary.Distance == 0 || bikeItinerary.Duration == 0 || bikeItinerary.Instructions.Count == 0;

            if (notCycling || notByFoot1 || notByFoot2)
            {
                Console.WriteLine("\nUnable to compute biking itinerary");
                notCycling = true;
            }

            if (notByFoot && notCycling)
            {
                Console.WriteLine("\nUnable to find a journey");
                return new Dictionary<string, Itinerary>();
            }

            double bikeAndFootDistance = bikeItinerary.Distance + footItinerary1.Distance + footItinerary2.Distance;
            double bikeAndFootDuration = bikeItinerary.Duration + footItinerary1.Duration + footItinerary2.Duration;

            if (footItinerary.Duration < bikeAndFootDuration)
            {
                Console.WriteLine("\nWalking is the best option");
                result = new Dictionary<string, Itinerary>
                {
                    { "walking", footItinerary }
                };

                // Publier l'itinéraire dans ActiveMQ
                Console.WriteLine("Publication de l'itinéraire dans ActiveMQ...");
                producer.SendMessage($"ItinerarySuggested", clientId, result);

                return result;

            }
            else
            {
                Console.WriteLine("\nCycling is the best option");
                result = new Dictionary<string, Itinerary>
                {
                    { "walking1", footItinerary1 },
                    { "cycling", bikeItinerary },
                    { "walking2", footItinerary2 }
                };

                // Publier l'itinéraire dans ActiveMQ
                Console.WriteLine("Publication de l'itinéraire dans ActiveMQ...");
                producer.SendMessage($"ItinerarySuggested", clientId, result);

                return result;
            }
        }

        private async Task<List<Contract>> GetContractsFromProxy()
        {
            Contract[] contractsArray = await client.GetAllContractsAsync();
            return contractsArray.ToList();
        }

        public async Task<List<Station>> GetStationsFromProxy(string contractName)
        {
            int pageNumber = 0;
            List<Station> stations = new List<Station>();
            while (true)
            {
                Station[] stationsArray = await client.GetContractStationsAsync(contractName, pageNumber);
                foreach (Station station in stationsArray)
                    stations.Add(station);
                if (stationsArray.Length < 100)
                    break;
                pageNumber++;
            }
            return stations;
        }

        private async Task<Position> resolveAddressFromProxy(string address)
        {
            Position position = await client.ResolveAddressAsync(address);
            return position;
        }

        private async Task<Itinerary> GetItineraryFromProxy(Position startPosition, Position endPosition, string profile = "walking")
        {
            Itinerary itinerary = await client.GetItineraryAsync(startPosition, endPosition, profile);
            return itinerary;
        }

        private Dictionary<string, (Station Station, Contract Contract)> computeClosestStations(Position startPosition, Position endPosition)
        {
            var closest = new Dictionary<string, (Station Station, Contract Contract)>
            {
                { "StartClosest", (null, null) }, { "StartAvailable", (null, null) },
                { "EndClosest", (null, null) }, { "EndAvailable", (null, null) }
            };

            double minStartDistance = double.MaxValue;
            double minEndDistance = double.MaxValue;
            double minAvailableStartDistance = double.MaxValue;
            double minAvailableEndDistance = double.MaxValue;

            foreach (var contract in GetContractsFromProxy().Result)
            {
                foreach (Station station in GetStationsFromProxy(contract.Name).Result)
                {
                    (closest["StartClosest"], closest["StartAvailable"], (minStartDistance, minAvailableStartDistance)) = updateDistances(
                        closest["StartClosest"], closest["StartAvailable"], (station, contract), startPosition, (minStartDistance, minAvailableStartDistance), "start"
                    );
                    (closest["EndClosest"], closest["EndAvailable"], (minEndDistance, minAvailableEndDistance)) = updateDistances(
                        closest["EndClosest"], closest["EndAvailable"], (station, contract), endPosition, (minEndDistance, minAvailableEndDistance), "end"
                    );
                }
            }

            return closest;
        }

        private Station getStation(int stationNumber, string contractName)
        {
            foreach (Station station in GetStationsFromProxy(contractName).Result)
            {
                if (station.Number == stationNumber)
                    return station;
            }

            return null;
        }

        private Station getClosestStation(int stationNumber, string contractName)
        {
            Station choosenStation = getStation(stationNumber, contractName);
            if (choosenStation == null)
                return null;

            Station closestStation = null;
            double minDistance = double.MaxValue;

            foreach (Contract contract in GetContractsFromProxy().Result)
            {
                foreach (Station station in GetStationsFromProxy(contract.Name).Result)
                {
                    double distance = Math.Sqrt(
                        Math.Pow(station.Position.Lat - choosenStation.Position.Lat, 2)
                        + Math.Pow(station.Position.Lng - choosenStation.Position.Lng, 2));

                    if (distance < minDistance && distance != 0)
                        (minDistance, closestStation) = (distance, station);
                }
            }

            return closestStation;
        }

        private (Position startPosition, Position endPosition) convertPositions(string startAddress, string endAddress)
        {
            Position startPosition = resolveAddressFromProxy(startAddress).Result;
            Position endPosition = resolveAddressFromProxy(endAddress).Result;

            if (startPosition == null || endPosition == null)
            {
                Console.WriteLine("\nUnable to resolve addresses\n");
                return (null, null);
            }

            return (startPosition, endPosition);
        }

        private (
            (Station Station, Contract contract) closest, (Station Station, Contract contract) closestAvailable,
            (double minDistance, double minAvailableDistance)
        ) updateDistances(
            (Station, Contract) closest, (Station, Contract) closestAvailable, (Station Station, Contract Contract) current,
            Position sidePosition, (double minDistance, double minAvailableDistance) distances, string side
        )
        {
            double minDistance = distances.minDistance;
            double minAvailableDistance = distances.minAvailableDistance;

            double distance = Math.Sqrt(
                Math.Pow(current.Station.Position.Lat - sidePosition.Lat, 2)
                + Math.Pow(current.Station.Position.Lng - sidePosition.Lng, 2)
            );

            if (distance < distances.minDistance)
                (closest, minDistance) = (current, distance);
            if (distance < distances.minAvailableDistance && current.Station.Status == "OPEN"
                && ((side == "start" && current.Station.AvailableBikes > 0)
                || (side == "end" && current.Station.AvailableBikeStands > 0)))
                (closestAvailable, minAvailableDistance) = (current, distance);

            return (closest, closestAvailable, (minDistance, minAvailableDistance));
        }






    }



}


