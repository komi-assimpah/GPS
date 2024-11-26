using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using RoutingServer.Models;
using System.Threading.Tasks;
using System.ServiceModel.Web;

namespace RoutingServer
{
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom d'interface "IService1" à la fois dans le code et le fichier de configuration.
    [ServiceContract]
    public interface IRoutingServer
    {
        // TODO: ajoutez vos opérations de service ici

        // Lecture des contrats avec HTTP GET
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



        //TOREWRITE
        // Obtenir la station la plus proche d'une localisation avec HTTP POST
        //[OperationContract]
        //[WebInvoke(Method = "POST", UriTemplate = "/stations/closest", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        //Task<Station> GetClosestStationAsync(int stationNumber, string contractName);

        // Calcul de l'itinéraire avec HTTP POST
        //[OperationContract]
        //[WebInvoke(Method = "POST", UriTemplate = "/route", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        //Task<Dictionary<string, (Station Station, Contract Contract)>> ComputeClosestAvailableAsync(List<Position> locations, List<Contract> contracts);
    
    
    }
    
}