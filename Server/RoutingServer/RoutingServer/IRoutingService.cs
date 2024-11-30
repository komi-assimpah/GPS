using ProxyCache.Models;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;


namespace RoutingServer
{
    [ServiceContract]
    public interface IRoutingService
    {
        [OperationContract]
        [WebGet(UriTemplate = "/suggestJourney?startAddress={startAddress}&endAddress={endAddress}")]
        Dictionary<string, Itinerary> suggestJourney(string startAddress, string endAddress);
    }
}