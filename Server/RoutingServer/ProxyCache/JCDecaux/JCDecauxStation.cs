using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProxyCache.Models;
using static System.Collections.Specialized.BitVector32;

namespace ProxyCache.JCDecaux
{
    internal class JCDecauxStation
    {
        public List<Station> Stations { get; set; }
        public string ContractName { get; set; }
        public JCDecauxStation(string contractName)
        {
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
