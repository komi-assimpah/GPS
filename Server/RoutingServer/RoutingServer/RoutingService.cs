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

        public async Task<List<Contract>> GetContractsFromProxy()
        {
            var client = new ProxyServiceClient(new BasicHttpBinding(), new EndpointAddress("http://localhost:8733/Design_Time_Addresses/ProxyCache/ProxyService/"));

            var contractsArray = await client.GetAllContractsAsync();

            return contractsArray.ToList();
            // Convertit le tableau en liste et retourne la liste des contrats
        }
    }
}
