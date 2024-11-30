using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProxyCache.Models;

namespace ProxyCache.JCDecaux
{
    internal class JCDecauxREST
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<List<Contract>> GetAllContracts()
        {
            string url = $"{Utils.JCDecauxBaseURL}/contracts?apiKey={Utils.JCDecauxAPIKey}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                List<Contract> contracts = JsonConvert.DeserializeObject<List<Contract>>(responseBody);
                return contracts;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\nError calling JC Decaux\n{ex.Message}");
                return new List<Contract>();
            }
        }

        public async Task<List<Station>> GetContractStations(string contractName)
        {
            string url = $"{Utils.JCDecauxBaseURL}/stations?contract={contractName}&apiKey={Utils.JCDecauxAPIKey}";

            try {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                List<Station> stations = JsonConvert.DeserializeObject<List<Station>>(responseBody);
                return stations;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\nError calling JC Decaux\n{ex.Message}");
                return new List<Station>();
            }
        }
    }

}
