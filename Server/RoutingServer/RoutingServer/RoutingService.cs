using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ProxyCache.Models;
using RoutingServer.ProxyServiceReference;
using Contract = RoutingServer.ProxyServiceReference.Contract;

namespace RoutingServer
{
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom de classe "Service1" à la fois dans le code et le fichier de configuration.
    public class RoutingService : IRoutingService
    {
        private ProxyServiceReference.ProxyServiceClient client = new ProxyServiceReference.ProxyServiceClient();

        async Task<List<Contract>> IRoutingService.GetContractsFromProxy()
        {
            Contract[] contractsArray = await client.GetAllContractsAsync();
            return contractsArray.ToList(); // Convertit le tableau en liste et retourne la liste des contrats
        }
    }
}
