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



        //la limit maximal de contenu est 129
        // a defaut de ne pas pouvoir recupérer toutes les stations,
        // on pourrais ne necupéré que ceux qui nous intéresse par exp ceux où available_bikes > 0
        public async Task<List<Station>> GetAllStationsOfAContractAsync(string contractName, int offset = 0, int limit = 129)
        {
            string url = $"{Utils.jcdAPIBaseURL}/stations?contract={contractName}&apiKey={Utils.jcdecauxAPIKey}";


            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            List<Station> allStations = JsonConvert.DeserializeObject<List<Station>>(responseBody);

            // Appliquer la limitation et l'offset côté client
            return allStations.Skip(offset).Take(limit).ToList();
        }

        //fonctionne mais parceque la limite n'est pas fixé, le server n'arrives pas à gérer les resultats et envoie donc une érreur
        /*public async Task<List<Station>> GetAllStationsOfAContractAsync(string contractName)
        {
            string url = $"{Utils.jcdAPIBaseURL}/stations?contract={contractName}&apiKey={Utils.jcdecauxAPIKey}";


            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            List<Station> stations = JsonConvert.DeserializeObject<List<Station>>(responseBody);
            return stations;
        }*/

    }

}
