using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ProxyCache.JCDecaux;
using ProxyCache.Models;
using ProxyCache.OpenStreetMap;
using System.Threading.Tasks;

namespace ProxyCache
{
    public class ProxyService : IProxyService
    {
        private readonly GenericProxyCache<JCDecauxContract> contractsCache = new GenericProxyCache<JCDecauxContract>();
        private readonly GenericProxyCache<JCDecauxStation> stationsCache = new GenericProxyCache<JCDecauxStation>();

        //===========================JCDecaux===========================
        public async Task<List<Contract>> GetAllContractsAsync()
        {
            return await Task.Run(() => contractsCache.Get("contracts").Contracts);
        }

        public async Task<List<Station>> GetAllStationsOfAContractAsync(string contractName)
        {
            var result = await Task.Run(() => stationsCache.Get("stations_of_" + contractName, DateTimeOffset.Now.AddMinutes(5)).Stations);
            return result;

            //return await Task.Run(() => contractsCache.Get(contractName).Stations);

            //string responseBody = await CallJCDecaux($"stations?contract={contractName}&");
            //List<Station> stations = JsonConvert.DeserializeObject<List<Station>>(responseBody);
            //return stations;
        }

        public async Task<Station> GetASpecificStationAsync(int stationNumber, string contractName)
        {
            List<Station> stations = await GetAllStationsOfAContractAsync(contractName);
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
        public async Task<City> GetNearestCityAsync(Position position)
        {
            var city = await OpenStreetMapREST.GetNearestCity(position);
            return city;
        }

        public async Task<Position> ResolveAddressAsync(string address)
        {
            var position = await OpenStreetMapREST.ResolveAddress(address);
            return position;
        }

        public async Task<Itinerary> GetItineraryAsync(Position departPosition, Position arrivalPosition)
        {
            var itinerary = await OpenStreetMapREST.GetItinerary(departPosition, arrivalPosition);
            return itinerary;
        }

    }


}
