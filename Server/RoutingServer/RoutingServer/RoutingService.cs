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
        private readonly ProxyServiceReference.ProxyServiceClient client;
        private readonly ActiveMqProducer producer;

        public RoutingService()
        {
            client = new ProxyServiceReference.ProxyServiceClient();
            producer = new ActiveMqProducer();
        }

        // Principal point d'entrée pour suggérer un itinéraire entre deux points
        public Dictionary<string, Itinerary> suggestJourney(string startLat, string startLng, string endLat, string endLng, string clientId)
        {
            // Ajouter les en-têtes CORS pour permettre l'accès cross-origin
            Utils.AddCorsHeaders();

            // Convertir les coordonnées en objets Position
            var startPosition = ParsePosition(startLat, startLng);
            var endPosition = ParsePosition(endLat, endLng);

            Console.WriteLine("\nPlanning walking journey...");
            // Calcul de l'itinéraire à pied
            var footItinerary = GetSafeItinerary(startPosition, endPosition, "foot-walking");

            if (IsItineraryInvalid(footItinerary))
            {
                Console.WriteLine("\nUnable to compute walking itinerary");
            }

            Console.WriteLine("\nPlanning cycling journey...");
            // Planification de l'itinéraire en vélo
            var cyclingItineraries = PlanCyclingJourney(startPosition, endPosition, footItinerary);

            // Déterminer la meilleure option entre marcher ou faire du vélo
            return DetermineBestOption(clientId, footItinerary, cyclingItineraries);
        }

        // Planifie l'itinéraire en vélo, incluant les segments de marche pour accéder aux stations
        private Dictionary<string, Itinerary> PlanCyclingJourney(Position start, Position end, Itinerary footItinerary)
        {
            var cyclingItineraries = new Dictionary<string, Itinerary>();
            var contracts = GetContracts();

            // Trouver les stations les plus proches pour le début et la fin
            var closestStations = FindClosestStations(start, end, contracts);

            // Ajouter les segments de marche et de vélo s'il existe des stations valides
            if (closestStations.Start.Station != null && closestStations.End.Station != null)
            {
                AddWalkingSegment(cyclingItineraries, start, closestStations.Start.Station.Position, "walking-to-bike");
                AddCyclingSegment(cyclingItineraries, closestStations.Start.Station.Position, closestStations.End.Station.Position, "cycling");
                AddWalkingSegment(cyclingItineraries, closestStations.End.Station.Position, end, "walking-from-bike");
            }

            return cyclingItineraries;
        }

        // Détermine l'option optimale (marche ou vélo) et publie les résultats dans ActiveMQ
        private Dictionary<string, Itinerary> DetermineBestOption(string clientId, Itinerary footItinerary, Dictionary<string, Itinerary> cyclingItineraries)
        {
            var footDuration = footItinerary?.Duration ?? double.MaxValue;
            var cyclingDuration = cyclingItineraries.Values.Sum(itinerary => itinerary?.Duration ?? double.MaxValue);

            if (footDuration > cyclingDuration)
            {
                Console.WriteLine("\nWalking is the best option");
                var result = new Dictionary<string, Itinerary> { { "walking", footItinerary } };
                PublishToActiveMq(clientId, result);
                return result;
            }
            else if (cyclingItineraries.Any())
            {
                Console.WriteLine("\nCycling is the best option");
                PublishToActiveMq(clientId, cyclingItineraries);
                return cyclingItineraries;
            }
            else
            {
                Console.WriteLine("\nNo suitable journey found");
                return new Dictionary<string, Itinerary>();
            }
        }

        // Convertit des coordonnées de chaîne de caractères en Position
        private Position ParsePosition(string lat, string lng)
        {
            return new Position
            {
                Lat = double.Parse(lat.Trim(',')),
                Lng = double.Parse(lng.Trim(','))
            };
        }

        // Vérifie si un itinéraire est invalide (distance/durée nulle ou pas d'instructions)
        private bool IsItineraryInvalid(Itinerary itinerary)
        {
            return itinerary.Distance == 0 || itinerary.Duration == 0 || itinerary.Instructions.Count == 0;
        }

        // Appelle le service pour obtenir un itinéraire, avec gestion des erreurs
        private Itinerary GetSafeItinerary(Position start, Position end, string profile = "foot-walking")
        {
            try
            {
                return client.GetItineraryAsync(start, end, profile).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching itinerary: {ex.Message}");
                return new Itinerary { Distance = 0, Duration = 0, Instructions = new List<Instruction>() };
            }
        }

        // Ajoute un segment de marche à la collection d'itinéraires
        private void AddWalkingSegment(Dictionary<string, Itinerary> itineraries, Position start, Position end, string segmentName)
        {
            var walkingItinerary = GetSafeItinerary(start, end, "foot-walking");
            if (!IsItineraryInvalid(walkingItinerary))
            {
                itineraries.Add(segmentName, walkingItinerary);
            }
        }

        // Ajoute un segment de vélo à la collection d'itinéraires
        private void AddCyclingSegment(Dictionary<string, Itinerary> itineraries, Position start, Position end, string segmentName)
        {
            var cyclingItinerary = GetSafeItinerary(start, end, "cycling-road");
            if (!IsItineraryInvalid(cyclingItinerary))
            {
                itineraries.Add(segmentName, cyclingItinerary);
            }
        }

        // Récupère la liste des contrats depuis le service proxy
        private List<Contract> GetContracts()
        {
            try
            {
                return client.GetAllContractsAsync().Result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching contracts: {ex.Message}");
                return new List<Contract>();
            }
        }

        // Trouve les stations les plus proches pour le départ et l'arrivée
        /*private (StationInfo Start, StationInfo End) FindClosestStations(Position start, Position end, List<Contract> contracts)
        {
            var closestStart = FindClosestStation(start, contracts, true);
            var closestEnd = FindClosestStation(end, contracts, false);

            return (closestStart, closestEnd);
        }*/

        // Trouve les stations les plus proches pour le départ et l'arrivée, tout en vérifiant qu'elles appartiennent au même contrat
        private (StationInfo Start, StationInfo End) FindClosestStations(Position start, Position end, List<Contract> contracts)
        {
            double minDistanceStart = double.MaxValue;
            double minDistanceEnd = double.MaxValue;

            StationInfo closestStart = default;
            StationInfo closestEnd = default;

            foreach (var contract in contracts)
            {
                var stations = GetStations(contract.Name);

                // Trouver la station de départ la plus proche pour ce contrat
                var startStation = stations
                    .Where(station => IsStationValid(station, true)) // Station avec vélos disponibles
                    .OrderBy(station => CalculateDistance(start, station.Position))
                    .FirstOrDefault();

                // Trouver la station d'arrivée la plus proche pour ce contrat
                var endStation = stations
                    .Where(station => IsStationValid(station, false)) // Station avec stands disponibles
                    .OrderBy(station => CalculateDistance(end, station.Position))
                    .FirstOrDefault();

                // Si les deux stations existent pour ce contrat, calculer les distances
                if (startStation != null && endStation != null)
                {
                    double distanceStart = CalculateDistance(start, startStation.Position);
                    double distanceEnd = CalculateDistance(end, endStation.Position);

                    // Mettre à jour si les distances sont plus petites
                    if (distanceStart < minDistanceStart && distanceEnd < minDistanceEnd)
                    {
                        minDistanceStart = distanceStart;
                        minDistanceEnd = distanceEnd;

                        closestStart = new StationInfo(startStation, contract);
                        closestEnd = new StationInfo(endStation, contract);
                    }
                }
            }

            return (closestStart, closestEnd);
        }



        // Recherche la station la plus proche pour une position donnée
        private StationInfo FindClosestStation(Position position, List<Contract> contracts, bool findBikeStations)
        {
            double minDistance = double.MaxValue;
            Station closestStation = null;
            Contract closestContract = null;

            foreach (var contract in contracts)
            {
                foreach (var station in GetStations(contract.Name))
                {
                    if (IsStationValid(station, findBikeStations))
                    {
                        var distance = CalculateDistance(position, station.Position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestStation = station;
                            closestContract = contract;
                        }
                    }
                }
            }

            return new StationInfo(closestStation, closestContract);
        }

        // Vérifie si une station est valide (ouverte et disponible pour vélos ou stands)
        //bool findBikeStations: true pour les stations de vélos, false pour les stations de stands
        private bool IsStationValid(Station station, bool findBikeStations)
        {
            return station.Status == "OPEN" &&
                   ((findBikeStations && station.AvailableBikes > 0) ||
                    (!findBikeStations && station.AvailableBikeStands > 0));
        }

        // Calcule la distance entre deux positions géographiques
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

        // Convertit des degrés en radians
        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        // Récupère les stations pour un contrat donné
        private List<Station> GetStations(string contractName)
        {
            try
            {
                return client.GetContractStationsAsync(contractName, 0).Result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching stations for contract {contractName}: {ex.Message}");
                return new List<Station>();
            }
        }

        // Publie les résultats dans ActiveMQ
        private void PublishToActiveMq(string clientId, Dictionary<string, Itinerary> itineraries)
        {
            try
            {
                producer.SendMessage($"ItinerarySuggested", clientId, itineraries);
                Console.WriteLine("Itinerary published to ActiveMQ.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing itinerary to ActiveMQ: {ex.Message}");
            }
        }
    }

    // Structure pour contenir une station et son contrat associé
    public struct StationInfo
    {
        public Station Station { get; }
        public Contract Contract { get; }

        public StationInfo(Station station, Contract contract)
        {
            Station = station;
            Contract = contract;
        }
    }
}
