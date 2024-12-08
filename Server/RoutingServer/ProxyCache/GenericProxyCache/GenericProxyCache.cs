using System;
using System.Runtime.Caching;

namespace ProxyCache.GenericProxyCache
{
    internal class GenericProxyCache<T> where T : ICacheable, new()
    {
        private static readonly ObjectCache _cache = MemoryCache.Default;
        private static readonly CacheItemPolicy _defaultCachePolicy = new CacheItemPolicy
        {
            AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration
        };

        // Get with default expiration
        public T Get(string cacheItemName, object obj)
        {
            return Get(cacheItemName, _defaultCachePolicy, obj);
        }

        // Get with expiration in seconds
        public T Get(string cacheItemName, double dt_seconds, object obj)
        {
            var expiration = DateTimeOffset.Now.AddSeconds(dt_seconds);
            var cachePolicy = new CacheItemPolicy { AbsoluteExpiration = expiration };
            return Get(cacheItemName, cachePolicy, obj);
        }

        public T Get(string cacheItemName, DateTimeOffset dt, object obj)
        {
            var cachePolicy = new CacheItemPolicy { AbsoluteExpiration = dt };
            return Get(cacheItemName, cachePolicy, obj);
        }

        private T Get(string cacheItemName, CacheItemPolicy cachePolicy, object obj)
        {
            var cacheItem = _cache.Get(cacheItemName);
            if (cacheItem == null)
            {
                var newItem = new T();
                newItem.Fill(obj);
                _cache.Add(cacheItemName, newItem, cachePolicy);
                return newItem;
            }
            return (T)cacheItem;
        }
    }
}
