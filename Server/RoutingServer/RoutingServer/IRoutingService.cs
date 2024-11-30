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
        [WebInvoke(Method = "GET", UriTemplate = "/itinerary?", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        Dictionary<string, Itinerary> suggestJourney(string startAddress, string endAddress);
    }
}