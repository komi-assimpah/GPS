using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using ProxyCache.Models;


namespace RoutingServer
{
    [ServiceContract]
    public interface IRoutingService
    {
        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "/contracts", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        Task<List<Contract>> GetContractsFromProxy();

        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "/stations?contract={contractName}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        Task<List<Station>> GetStationsFromProxy(string contractName);


    }

}
