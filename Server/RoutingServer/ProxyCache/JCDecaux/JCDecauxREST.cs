using System;
using System.Collections.Generic;
using ProxyCache.Models;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static System.Collections.Specialized.BitVector32;

namespace ProxyCache.JCDecaux
{
    internal class JCDecauxREST
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<List<Contract>> GetAllContractsAsync()
        {
            string url = $"{Utils.jcdAPIBaseURL}/contracts?apiKey={Utils.jcdecauxAPIKey}";
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                List<Contract> contracts = JsonConvert.DeserializeObject<List<Contract>>(responseBody);
                return contracts;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
            return new List<Contract>(); // Return an empty list in case of an exception
        }

        public async Task<List<Station>> GetAllStationsOfAContractAsync(string contractName)
        {
            string url = $"{Utils.jcdAPIBaseURL}/stations?contract={contractName}";
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            List<Station> stations = JsonConvert.DeserializeObject<List<Station>>(responseBody);
            return stations;
        }


        public async Task<Station> GetASpecificStationAsync(int stationNumber, string contractName)
        {
            //string url = $"{Utils.jcdAPIBaseURL}/stations/{stationNumber}?contract={contractName}&apiKey={Utils.jcdecauxAPIKey}";
            //HttpResponseMessage response = await client.GetAsync(url);
            //response.EnsureSuccessStatusCode();
            //string responseBody = await response.Content.ReadAsStringAsync();
            //Station station = JsonConvert.DeserializeObject<Station>(responseBody);
            //return station;


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

        //could be optimized and maybe moved to the ProxyService
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




    }

}
