using System.Collections.Generic;
using System.Threading.Tasks;
using ProxyCache.Models;

namespace ProxyCache.JCDecaux
{
    internal class JCDecauxStation
    {
        public List<Station> Stations { get; set; }
        public string ContractName { get; set; }

        public JCDecauxStation()
        {
            // Paramètre par défaut en cas de création sans paramètre explicite
            Stations = new List<Station>();
            ContractName = string.Empty;
        }

        public void InitializeStations(string contractName)
        {
            if (string.IsNullOrWhiteSpace(contractName))
                throw new System.ArgumentException("Contract name cannot be null or empty.");

            ContractName = contractName;
            FillStations().Wait();
        }

        private async Task FillStations()
        {
            var jcDecauxRest = new JCDecauxREST();
            Stations = await jcDecauxRest.GetAllStationsOfAContractAsync(ContractName);
        }
    }
}
