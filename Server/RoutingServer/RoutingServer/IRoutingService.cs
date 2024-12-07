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
        [WebGet(UriTemplate = "/suggestJourney?startLng={startLng}&startLat={startLat}&endLng={endLng}&endLat={endLat}")]
        Dictionary<string, Itinerary> suggestJourney(string startLng, string startLat, string endLng, string endLat);
    }
}