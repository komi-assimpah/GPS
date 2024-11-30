using System.Collections.Generic;
using ProxyCache.GenericProxyCache;
using ProxyCache.Models;

namespace ProxyCache.JCDecaux
{
    internal class JCDecauxStation : ICacheable
    {
        public List<Station> Stations { get; set; }
        public JCDecauxStation() {}
        void ICacheable.Fill(object obj)
        {
            var jcDecauxRest = new JCDecauxREST();
            string contractName = (string)obj;
            Stations = jcDecauxRest.GetContractStations(contractName).Result;
        }
    }

}
