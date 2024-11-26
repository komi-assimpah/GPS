using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using RoutingServer.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Globalization;


namespace RoutingServer
{
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom de classe "Service1" à la fois dans le code et le fichier de configuration.
    public class RoutingServer : IRoutingServer
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<List<Contract>> GetAllContractsAsync()
        {
            string responseBody = await CallJCDecaux("contracts?");
            List<Contract> contracts = JsonConvert.DeserializeObject<List<Contract>>(responseBody);
            return contracts;
        }

        public async Task<List<Station>> GetAllStationsOfAContractAsync(string contractName)
        {
            string responseBody = await CallJCDecaux($"stations?contract={contractName}&");
            List<Station> stations = JsonConvert.DeserializeObject<List<Station>>(responseBody);
            return stations;
        }


        public async Task<Station> GetASpecificStationAsync(int stationNumber, string contractName)
        {
            List<Station> stations = await GetAllStationsOfAContractAsync(contractName);
            foreach (Station station in stations)
            {
                if (station.Number == stationNumber)
                {
                    return station;
                }
            }
            return null;
        }

        //could be optimized
        public async Task<Station> GetClosestStationAsync(int stationNumber, string contractName)
        {

            double minDistance = double.MaxValue;
            Station choosenStation = await GetASpecificStationAsync(stationNumber, contractName);
            Station closestStation = null;

            List<Contract> contracts = await GetAllContractsAsync();
            foreach (Contract contract in contracts)
            {
                List<Station> stations = await GetAllStationsOfAContractAsync(contract.Name);

                foreach (Station station in stations)
                {
                    double distance = Math.Sqrt(
                        Math.Pow(station.Position.Lat - choosenStation.Position.Lat, 2)
                        + Math.Pow(station.Position.Lng - choosenStation.Position.Lng, 2));
                    if (distance < minDistance && distance != 0)
                    {
                        minDistance = distance;
                        closestStation = station;
                    }
                }
            }

            return closestStation;
        }



        public async Task<Dictionary<string, (Station Station, Contract Contract)>> ComputeClosestAvailableAsync(List<Position> locations, List<Contract> contracts)
        {
            double minOriginDistance = double.MaxValue;
            double minDestinationDistance = double.MaxValue;

            var closest = new Dictionary<string, (Station Station, Contract Contract)>
            {
                { "Origin", (null, null) },
                { "Destination", (null, null) }
            };

            foreach (var contract in contracts)
            {
                List<Station> stations = await GetAllStationsOfAContractAsync(contract.Name);

                foreach (Station station in stations)
                {
                    Position position = station.Position;
                    if (station.AvailableBikes > 0)
                    {
                        double distance = Math.Sqrt(
                                           Math.Pow(position.Lat - locations[0].Lat, 2)
                                           + Math.Pow(position.Lng - locations[0].Lng, 2)
                        );
                        if (distance < minOriginDistance)
                        {
                            minOriginDistance = distance;
                            closest["Origin"] = (station, contract);
                        }
                    }

                    if (station.AvailableBikeStands > 0)
                    {
                        double distance = Math.Sqrt(
                            Math.Pow(position.Lat - locations[1].Lat, 2)
                            + Math.Pow(position.Lng - locations[1].Lng, 2)
                        );
                        if (distance < minDestinationDistance)
                        {
                            minDestinationDistance = distance;
                            closest["Destination"] = (station, contract);
                        }
                    }
                }
            }

            return closest;
        }

        private async Task<string> CallJCDecaux(string content)
        {
            string url = "https://api.jcdecaux.com/vls/v1/";
            string key = "apiKey=d6bd03040ecae0e3244f4ba002c10bb9a34d85df";
            try
            {
                var response = await client.GetAsync(url + content + key);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();

                return responseContent;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine($"Error calling JC Decaux : {ex.Message}");
                return string.Empty;
            }
        }

    }
}
