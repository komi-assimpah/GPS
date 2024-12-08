using ProxyCache.Models;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;


namespace RoutingServer
{
    [ServiceContract]
    public interface IRoutingService
    {
        [OperationContract]
        //[WebInvoke(Method = "GET", UriTemplate = "/suggestJourney?startLat={startLat}&startLng={startLng}&endLat={endLat}&endLng={endLng}&clientId={clientId}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        //Dictionary<string, Itinerary> suggestJourney(string startLat, string startLng, string endLat, string endLng, string clientId);

        [WebInvoke(Method = "GET", 
                   UriTemplate = "/suggestJourney?startLat={startLat}&startLng={startLng}&endLat={endLat}&endLng={endLng}&clientId={clientId}", 
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped)]

        Dictionary<string, Itinerary> suggestJourney(string startLat, string startLng, string endLat, string endLng, string clientId);




    }
}