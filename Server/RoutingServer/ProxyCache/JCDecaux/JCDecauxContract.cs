using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProxyCache.Models;

namespace ProxyCache.JCDecaux
{
    internal class JCDecauxContract
    {
        public List<Contract> Contracts { get; set; }
        public JCDecauxContract()
        {
            FillContracts().Wait();
        }

        private async Task FillContracts()
        {
            var jcDecauxRest = new JCDecauxREST();
            Contracts = await jcDecauxRest.GetAllContractsAsync();
        }

    }

}
