using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ProxyCache.Models;
using RoutingServer.ProxyServiceReference;

namespace RoutingServer
{
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom de classe "Service1" à la fois dans le code et le fichier de configuration.
    public class RoutingService : IRoutingService
    {
        //private readonly ProxyServiceClient client;
        ProxyServiceClient client = new ProxyServiceClient(new BasicHttpBinding(), new EndpointAddress("http://localhost:8733/Design_Time_Addresses/ProxyCache/ProxyService/"));


        public async Task<List<Contract>> GetContractsFromProxy()
        {
            //var client = new ProxyServiceClient(new BasicHttpBinding(), new EndpointAddress("http://localhost:8733/Design_Time_Addresses/ProxyCache/ProxyService/"));

            var contractsArray = await client.GetAllContractsAsync();

            return contractsArray.ToList();
            // Convertit le tableau en liste et retourne la liste des contrats
        }

        public async Task<List<Station>> GetStationsFromProxy(string contractName)
        {
            var stationsArray = await client.GetAllStationsOfAContractAsync(contractName);
            return stationsArray.ToList();

        }


        /*

        public Station GetASpecificStationAsync(int stationNumber, string contractName)
        {
            //string url = $"{Utils.jcdAPIBaseURL}/stations/{stationNumber}?contract={contractName}&apiKey={Utils.jcdecauxAPIKey}";
            //HttpResponseMessage response = await client.GetAsync(url);
            //response.EnsureSuccessStatusCode();
            //string responseBody = await response.Content.ReadAsStringAsync();
            //Station station = JsonConvert.DeserializeObject<Station>(responseBody);
            //return station;


            List<Station> stations = GetAllStationsOfAContractAsync(contractName);
            foreach (Station station in stations)
            {
                if (station.Number == stationNumber)
                {
                    return station;
                }
            }
            return null;
        }

        //could be optimized and maybe moved to the ProxyService
        public Station GetClosestStationAsync(int stationNumber, string contractName)
        {

            double minDistance = double.MaxValue;
            Station choosenStation = GetASpecificStationAsync(stationNumber, contractName);
            Station closestStation = null;

            List<Contract> contracts = GetAllContractsAsync();
            foreach (Contract contract in contracts)
            {
                List<Station> stations = GetAllStationsOfAContractAsync(contract.Name);

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


        public Dictionary<string, (Station Station, Contract Contract)> computeClosestStations(string startAddress, string endAddress)
        {
            var closest = new Dictionary<string, (Station Station, Contract Contract)>
            {
                { "StartClosest", (null, null) }, { "StartAvailable", (null, null) },
                { "EndClosest", (null, null) }, { "EndAvailable", (null, null) }
            };

            var positions = convertPositions(startAddress, endAddress);
            if (positions == (null, null))
            {
                Console.WriteLine("\nUnable to compute closest stations");
                return closest;
            }

            double minStartDistance = double.MaxValue;
            double minEndDistance = double.MaxValue;
            double minAvailableStartDistance = double.MaxValue;
            double minAvailableEndDistance = double.MaxValue;

            foreach (var contract in retrieveContracts())
            {
                foreach (Station station in retrieveStations(contract.Name))
                {
                    (closest["StartClosest"], closest["StartAvailable"], (minStartDistance, minAvailableStartDistance)) = updateDistances(
                        closest["StartClosest"], closest["StartAvailable"], (station, contract), positions.startPosition, (minStartDistance, minAvailableStartDistance)
                    );
                    (closest["EndClosest"], closest["EndAvailable"], (minEndDistance, minAvailableEndDistance)) = updateDistances(
                        closest["EndClosest"], closest["EndAvailable"], (station, contract), positions.endPosition, (minEndDistance, minAvailableEndDistance)
                    );
                }
            }

            return closest;
        }*/
    }
}
