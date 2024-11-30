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
        List<Contract> GetAllContracts();

        [OperationContract]
        List<Station> GetAllStationsOfAContract(string contractName);


        /*       
            // Lecture d'une station spécifique d'un contrat donné avec HTTP GET
            [OperationContract]
            Task<Station> GetASpecificStation(int stationNumber, string contractName);


            //================================OpenStreetMap================================
            [OperationContract]
            Task<Position> ResolveAddress(string address);
            [OperationContract]
            Task<City> GetNearestCity(Position position);

            [OperationContract]
            Task<Itinerary> GetItinerary(Position departPosition, Position arrivalPosition);
                                                                                       
         */


    }
}
