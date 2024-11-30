using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProxyCache.JCDecaux;
using ProxyCache.Models;

namespace ProxyCache
{
    public class ProxyService : IProxyService
    {
        public static readonly ThreadLocal<string> contractNameToFetchFrom = new ThreadLocal<string>();
        private readonly GenericProxyCache<JCDecauxContract> contractsCache = new GenericProxyCache<JCDecauxContract>();
        private readonly GenericProxyCache<JCDecauxStation> stationsCache = new GenericProxyCache<JCDecauxStation>();

        //=========================== JCDecaux ===========================

        public List<Contract> GetAllContracts()
        {
            return GetAllContractsAsync().GetAwaiter().GetResult();
        }

        private async Task<List<Contract>> GetAllContractsAsync()
        {
            return await Task.Run(() => contractsCache.Get("contracts").Contracts);
        }

        public List<Station> GetAllStationsOfAContract(string contractName)
        {
            if (string.IsNullOrWhiteSpace(contractName))
            {
                throw new ArgumentException("The contract name must be provided.", nameof(contractName));
            }

            Console.WriteLine($"[DEBUG] Received contractName: {contractName}");
            contractNameToFetchFrom.Value = contractName;
            Console.WriteLine($"[DEBUG] contractNameToFetchFrom after initialization: {contractNameToFetchFrom.Value}");

            return GetAllStationsOfAContractAsync(contractName).GetAwaiter().GetResult();
        }

        private async Task<List<Station>> GetAllStationsOfAContractAsync(string contractName)
        {
            Console.WriteLine($"[DEBUG] Fetching stations for contract: {contractName}");
            return await Task.Run(() =>
            {
                var stationData = stationsCache.Get("stations_of_" + contractName, DateTimeOffset.Now.AddMinutes(5));
                stationData.InitializeStations(contractName); // Appelle explicitement l'initialisation
                return stationData.Stations;
            });
        }





        //not tested yet

         /*public async Task<Station> GetASpecificStation(int stationNumber, string contractName)
         {
             List<Station> stations = await GetAllStationsOfAContract(contractName);
             foreach (Station station in stations)
             {
                 if (station.Number == stationNumber)
                 {
                     return station;
                 }
             }
             return null;
         }



         //===========================OpenStreetMap===========================
         public async Task<City> GetNearestCity(Position position)
         {
             var city = await OpenStreetMapREST.GetNearestCity(position);
             return city;
         }

         public async Task<Position> ResolveAddress(string address)
         {
             var position = await OpenStreetMapREST.ResolveAddress(address);
             return position;
         }

         public async Task<Itinerary> GetItinerary(Position departPosition, Position arrivalPosition)
         {
             var itinerary = await OpenStreetMapREST.GetItinerary(departPosition, arrivalPosition);
             return itinerary;
         }*/

    }


}
