using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ProxyCache.Models;
using System.Threading.Tasks;
using System.ServiceModel.Web;

namespace ProxyCache
{
    [ServiceContract]
    public interface IProxyService
    {
        //===============================JCDecaux================================
        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "/contracts", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        Task<List<Contract>> GetAllContractsAsync();

        // Lecture des stations d'un contrat donné avec HTTP GET
        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "/stations/byContract?contractName={contractName}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        Task<List<Station>> GetAllStationsOfAContractAsync(string contractName);

        // Lecture d'une station spécifique d'un contrat donné avec HTTP GET
        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "/stations/byNumber?stationNumber={stationNumber}&contractName={contractName}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        Task<Station> GetASpecificStationAsync(int stationNumber, string contractName);


        //================================OpenStreetMap================================
        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "/resolveAddress?address={address}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        Task<Position> ResolveAddressAsync(string address);
        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "/resolvePosition?position={position}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        Task<City> GetNearestCityAsync(Position position);

        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "/itinerary?departPosition={departPosition}&arrivalPosition={arrivalPosition}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        Task<Itinerary> GetItineraryAsync(Position departPosition, Position arrivalPosition);


    }
}
