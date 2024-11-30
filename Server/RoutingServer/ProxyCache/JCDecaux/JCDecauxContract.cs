using System.Collections.Generic;
using ProxyCache.GenericProxyCache;
using ProxyCache.Models;

namespace ProxyCache.JCDecaux
{
    internal class JCDecauxContract : ICacheable
    {
        public List<Contract> Contracts { get; set; }
        public JCDecauxContract() {}

        void ICacheable.Fill(object obj)
        {
            var jcDecauxRest = new JCDecauxREST();
            Contracts = jcDecauxRest.GetAllContracts().Result;
        }
    }

}
