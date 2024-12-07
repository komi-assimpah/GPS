using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using RoutingServer.ContractTypes;
using ProxyCache.Models;


namespace RoutingServer
{
    public class RoutingService : IRoutingService
    {
        private ProxyServiceReference.ProxyServiceClient client = new ProxyServiceReference.ProxyServiceClient();

        public Dictionary<string, Itinerary> suggestJourney(string startLng, string startLat, string endLng, string endLat)
        {
            Console.WriteLine("\nPlanning walking journey...");

            Position startPosition = new Position
            {
                Lat = double.Parse(startLat.ToString()),
                Lng = double.Parse(startLng.ToString())
            };

            Position endPosition = new Position
            {
                Lat = double.Parse(endLat.ToString()),
                Lng = double.Parse(endLng.ToString())
            };

            Itinerary footItinerary = GetItineraryFromProxy(startPosition, endPosition).Result;
            bool notByFoot = footItinerary.Distance == 0 || footItinerary.Duration == 0 || footItinerary.Instructions.Count == 0;
            if (notByFoot)
                Console.WriteLine("\nUnable to compute walking itinerary");
            footItinerary.Duration = double.MaxValue;

            Console.WriteLine("\nPlanning cycling journey...");

            int index = 0;

            bool notCycling = false;

            double bikeJourneyDuration = double.MaxValue;
            double bikeJourneyDistance = double.MaxValue;

            Dictionary<string, Itinerary> bikeJourneyItineraries = new Dictionary<string, Itinerary>();

            var contracts = resolveContracts(startPosition, endPosition);

            if (contracts.Start.Contract == contracts.End.Contract)
            {
                if (!contracts.Start.Station.Equals(contracts.End.Station))
                {
                    if (!startPosition.Equals(contracts.Start.Station.Position))
                    {
                        Itinerary footItinerary1 = GetItineraryFromProxy(startPosition, contracts.Start.Station.Position).Result;
                        if (footItinerary1.Distance != 0)
                            bikeJourneyItineraries.Add(((index++) + 1) + "_walking", footItinerary1);
                        else
                            notCycling = true;
                    }
                    
                    Itinerary bikeItinerary = GetItineraryFromProxy(contracts.Start.Station.Position, contracts.End.Station.Position, "cycling-regular").Result;
                    if (bikeItinerary.Distance != 0)
                        bikeJourneyItineraries.Add(((index++) + 1) + "_cycling", bikeItinerary);
                    else
                        notCycling = true;

                    if (!endPosition.Equals(contracts.End.Station.Position))
                    {
                        Itinerary footItinerary2 = GetItineraryFromProxy(contracts.End.Station.Position, endPosition).Result;
                        if (footItinerary2.Distance != 0)
                            bikeJourneyItineraries.Add(((index++) + 1) + "_walking", footItinerary2);
                        else
                            notCycling = true;
                    }
                }
                else {
                    bikeJourneyItineraries = new Dictionary<string, Itinerary>
                    {
                        { "1_walking", footItinerary }
                    };
                }
            }
            else
            {
                bool departOnBike = false;
                bool arriveOnBike = false;

                Position finalPosition = startPosition;

                List<Contract> suitableContracts = GetContractsFromProxy().Result;
                List<Contract> contractsToUse = new List<Contract>();

                Contract contract;

                Itinerary entryItinerary = null;
                Itinerary bikeItinerary = null;
                Itinerary exitItinerary = null;

                List<Contract> newSuitableContracts;

                while (suitableContracts.Count > 0 && notCycling == false)
                {
                    (contract, entryItinerary, bikeItinerary, exitItinerary, newSuitableContracts) = GetSuitableContracts(startPosition, endPosition, suitableContracts);

                    if (contract == null && entryItinerary == null && bikeItinerary == null && exitItinerary == null && newSuitableContracts.Count == 0)
                    {
                        Console.WriteLine("\nNo suitable contracts found");
                        notCycling = true;
                        break;
                    }
                    else if (index == 0 && finalPosition.Equals(bikeItinerary.Instructions.First().Position))
                    {
                        departOnBike = true;
                    }

                    notCycling = !departOnBike && (entryItinerary == null);
                    notCycling = !notCycling && (bikeItinerary == null);

                    if (!notCycling)
                    {
                        bikeJourneyItineraries.Add((index + 1) + "_walking", entryItinerary);
                        index++;

                        bikeJourneyItineraries.Add((index + 1) + "_cycling", bikeItinerary);
                        finalPosition = bikeItinerary.Instructions.Last().Position;
                        index++;
                    }

                    if (finalPosition.Equals(endPosition))
                    {
                        arriveOnBike = true;
                    }
                    notCycling = !arriveOnBike && !notCycling && (exitItinerary == null);

                    suitableContracts = newSuitableContracts;
                    contractsToUse.Add(contract);
                }
                if (arriveOnBike && !notCycling)
                {
                        bikeJourneyItineraries.Add((index + 1) + "_walking", exitItinerary);

                        bikeJourneyDuration = bikeJourneyItineraries.Values.Sum(itinerary => itinerary.Duration);
                        bikeJourneyDistance = bikeJourneyItineraries.Values.Sum(itinerary => itinerary.Distance);
                }
            }

            if (notCycling)
            {
                Console.WriteLine("\nNo suitable cycling itinerary found");
                notCycling = true;
            }

            if (notCycling && notByFoot)
            {
                Console.WriteLine("\nUnable to find a journey");
                return new Dictionary<string, Itinerary>();
            }

            if (footItinerary.Duration < bikeJourneyDuration)
            {
                Console.WriteLine("\nWalking is the best option");
                return new Dictionary<string, Itinerary>
                {
                    { "1_walking", footItinerary }
                };
            }
            else
            {
                Console.WriteLine("\nCycling is the best option");
                return bikeJourneyItineraries;
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

        private async Task<Itinerary> GetItineraryFromProxy(Position startPosition, Position endPosition, string profile = "foot-walking")
        {
            Itinerary itinerary = await client.GetItineraryAsync(startPosition, endPosition, profile);
            return itinerary;
        }

        private Dictionary<string, (Station Station, Contract Contract)> computeClosestStations(Position startPosition, Position endPosition, List<Contract> contracts)
        {
            var closest = new Dictionary<string, (Station Station, Contract Contract)>
            {
                { "Start", (null, null) }, { "End", (null, null) }
            };

            double minStartDistance = double.MaxValue;
            double minEndDistance = double.MaxValue;

            foreach (var contract in contracts)
            {
                foreach (Station station in GetStationsFromProxy(contract.Name).Result)
                {
                    double distance = GetItineraryFromProxy(station.Position, startPosition).Result.Distance;

                    if (distance < minStartDistance && station.Status == "OPEN" && station.AvailableBikes > 0)
                        (closest["Start"], minStartDistance) = ((station, contract), distance);
                    if (distance < minEndDistance && station.Status == "OPEN" && station.AvailableBikeStands > 0)
                        (closest["End"], minEndDistance) = ((station, contract), distance);

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

                    double distance = GetItineraryFromProxy(station.Position, closestStation.Position).Result.Distance;

                    if (distance < minDistance && distance != 0)
                        (minDistance, closestStation) = (distance, station);
                }
            }

            return closestStation;
        }

        public ((Station Station, Contract Contract) Start, (Station Station, Contract Contract) End) resolveContracts(Position startPosition, Position endPosition)
        {
            var closest = computeClosestStations(startPosition, endPosition, GetContractsFromProxy().Result);
            return (closest["Start"], closest["End"]);
        }

        public (Contract BestContract, Itinerary EntryItinerary, Itinerary bikeItinerary, Itinerary ExitItinerary, List<Contract> PossibleContracts) GetSuitableContracts(Position startPosition, Position endPosition, List<Contract> suitableContracts)
        {
            var possibleContracts = new List<Contract>();
            Dictionary<Contract, Itinerary> entryItineraries = new Dictionary<Contract, Itinerary>();

            Itinerary entryItinerary = null;
            Itinerary cyclingItinerary = null;
            Itinerary exitItinerary = null;

            Contract bestContract = null;

            double minDuration = double.MaxValue;

            var footItinerary = GetItineraryFromProxy(startPosition, endPosition, "foot-walking").Result;

            if (footItinerary.Distance == 0 || footItinerary.Duration == 0 || footItinerary.Instructions.Count == 0)
            {
                Console.WriteLine("Walking itinerary could not be computed.");
                return (null, null, null, null, new List<Contract>());
            }

            foreach (var contract in suitableContracts)
            {
                Console.WriteLine($"Evaluating contract: {contract.Name}");

                ((Station Station, Itinerary Itinerary) Entry, (Station Station, Itinerary Itinerary) Exit) = FindSideStations(startPosition, endPosition, contract);

                if (Entry == (null, null) || Exit == (null, null))
                {
                    Console.WriteLine($"No valid entry/exit stations for contract: {contract.Name}");
                    continue;
                }

                if (!startPosition.Equals(Entry.Station.Position)) entryItineraries.Add(contract, Entry.Itinerary);

                var bikeItinerary = GetItineraryFromProxy(Entry.Station.Position, Exit.Station.Position, "cycling-regular").Result;

                if (bikeItinerary.Distance == 0 || bikeItinerary.Duration == 0 || bikeItinerary.Instructions.Count == 0)
                {
                    Console.WriteLine($"Biking itinerary could not be computed for contract: {contract.Name}");
                    return (null, null, null, null, new List<Contract>());
                }

                double totalDuration = Entry.Itinerary.Duration + bikeItinerary.Duration + Exit.Itinerary.Duration;

                Console.WriteLine($"Contract {contract.Name}: Total duration (foot + bike) = {totalDuration} seconds");

                if (totalDuration < footItinerary.Duration)
                {
                    Console.WriteLine($"Contract {contract.Name} is advantageous. Adding to possible contracts.");
                    possibleContracts.Add(contract);

                    if (minDuration > totalDuration)
                    {
                        minDuration = totalDuration;
                        entryItinerary = (!startPosition.Equals(Entry.Station.Position)) ? Entry.Itinerary : null;
                        cyclingItinerary = bikeItinerary;
                        exitItinerary = (!endPosition.Equals(Exit.Station.Position)) ? Exit.Itinerary : null;
                        bestContract = contract;
                    }
                }
                else
                {
                    Console.WriteLine($"Contract {contract.Name} is not advantageous.");
                }
            }

            possibleContracts = possibleContracts.Where(contract =>
            {
                bool got = entryItineraries.TryGetValue(contract, out Itinerary itinerary);
                return (got) ? itinerary.Distance < entryItinerary.Distance : false;
            }).ToList();

            return (bestContract, entryItinerary, cyclingItinerary, exitItinerary, possibleContracts);
        }

        private ((Station Station, Itinerary Itinerary) Entry, (Station Station, Itinerary Itinerary) Exit) FindSideStations(Position startPosition, Position endPosition, Contract contract)
        {

            List<string> cities = contract.Cities;

            var closest = computeClosestStations(startPosition, endPosition, new List<Contract> { contract });

            Position center = new Position
            {
                Lat = (startPosition.Lat + endPosition.Lat) / 2,
                Lng = (startPosition.Lng + endPosition.Lng) / 2
            };

            if (CalculateDistance(center, startPosition) < CalculateDistance(center, closest["Start"].Station.Position))
                return ((null, null), (null, null));

            List<Station> stations = GetStationsFromProxy(contract.Name).Result;
            List<Station> standStations = stations.Where(station => station.Status == "OPEN" && station.AvailableBikeStands > 0).ToList();
            List<Station> bikeStations = stations.Where(station => station.Status == "OPEN" && station.AvailableBikes > 0).ToList();

            if (standStations.Count == 0 || bikeStations.Count == 0)
                return ((null, null), (null, null));

            var entryStation = stations
                .Select(station => new
                {
                    Station = station,
                    Itinerary = GetItineraryFromProxy(startPosition, station.Position).Result
                })
                .OrderBy(stationInfo => stationInfo.Itinerary.Distance)
                .FirstOrDefault();

            var exitStation = stations
                .Select(station => new
                {
                    Station = station,
                    Itinerary = GetItineraryFromProxy(station.Position, endPosition).Result
                })
                .OrderBy(stationInfo => stationInfo.Itinerary.Distance)
                .FirstOrDefault();

            if (entryStation == null || exitStation == null || entryStation.Station == exitStation.Station)
                return ((null, null), (null, null));

            return ((entryStation.Station, entryStation.Itinerary), (exitStation.Station, exitStation.Itinerary));
        }

        private double CalculateDistance(Position pos1, Position pos2)
        {
            double earthRadiusKm = 6371;

            double dLat = DegreesToRadians(pos2.Lat - pos1.Lat);
            double dLng = DegreesToRadians(pos2.Lng - pos1.Lng);

            double lat1 = DegreesToRadians(pos1.Lat);
            double lat2 = DegreesToRadians(pos2.Lat);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Sin(dLng / 2) * Math.Sin(dLng / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }


    }

}