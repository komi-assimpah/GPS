using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using ProxyCache.Models;

namespace ProxyCache
{
    [ServiceContract]
    public interface IProxyService
    {
        //===============================JCDecaux======================================
        [OperationContract]
        Task<List<Contract>> GetAllContractsAsync();

        [OperationContract]
        Task<List<Station>> GetContractStationsAsync(string contractName, int pageNumber);


        //================================OpenStreetMap================================
        [OperationContract]
        Task<Position> ResolveAddressAsync(string address);

        [OperationContract]
        Task<Itinerary> GetItineraryAsync(Position startPosition, Position endPosition, string profile);
    }
}
