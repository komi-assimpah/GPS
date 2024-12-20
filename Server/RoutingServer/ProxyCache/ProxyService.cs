﻿using System;
using System.Collections.Generic;
using ProxyCache.GenericProxyCache;
using ProxyCache.JCDecaux;
using ProxyCache.OpenStreetMap;
using System.Threading.Tasks;
using ProxyCache.Models;
using System.Linq;

namespace ProxyCache
{
    public class ProxyService : IProxyService
    {
        private readonly GenericProxyCache<JCDecauxContract> contractsCache = new GenericProxyCache<JCDecauxContract>();
        private readonly GenericProxyCache<JCDecauxStation> stationsCache = new GenericProxyCache<JCDecauxStation>();

        private readonly GenericProxyCache<OSMPosition> positionCache = new GenericProxyCache<OSMPosition>();
        private readonly GenericProxyCache<ORSItinerary> itineraryCache = new GenericProxyCache<ORSItinerary>();


        //===========================JCDecaux===========================

        public async Task<List<Contract>> GetAllContractsAsync()
        {
            return await Task.Run(() => contractsCache.Get("contracts", "").Contracts);
        }

        public async Task<List<Station>> GetContractStationsAsync(string contractName, int pageNumber)
        {
            var stations = await Task.Run(() =>
                stationsCache.Get("stations_of_" + contractName, DateTimeOffset.Now.AddMinutes(5), contractName).Stations
            );

            var batch = stations.Skip(Utils.batchSize * (pageNumber - 1)).Take(Utils.batchSize).ToList();

            return batch;
        }


        //===========================OpenStreetMap===========================

        public async Task<Position> ResolveAddressAsync(string address)
        {
            return await Task.Run(() => positionCache.Get(address, address).Position);
        }

        public async Task<Itinerary> GetItineraryAsync(Position startPosition, Position endPosition, string profile)
        {
            string cacheKey = $"{startPosition.Lat}_{startPosition.Lng}_{endPosition.Lat}_{endPosition.Lng}_{profile}";

            return await Task.Run(() => itineraryCache.Get(
                cacheKey,
                DateTimeOffset.Now.AddMinutes(3),
                (startPosition, endPosition, profile)).Itinerary
            );
        }

    }


}
