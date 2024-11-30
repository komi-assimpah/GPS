using System;
using System.Runtime.Caching;

namespace ProxyCache.JCDecaux
{
    internal class GenericProxyCache<T> where T : new()
    {
        private static readonly ObjectCache _cache = MemoryCache.Default;
        private static readonly CacheItemPolicy _defaultCachePolicy = new CacheItemPolicy { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration };

        // Get with default expiration
        public T Get(string cacheItemName)
        {
            return Get(cacheItemName, _defaultCachePolicy);
        }

        // Get with expiration in seconds
        public T Get(string cacheItemName, double dt_seconds)
        {
            var expiration = DateTimeOffset.Now.AddSeconds(dt_seconds);
            var cachePolicy = new CacheItemPolicy { AbsoluteExpiration = expiration };
            return Get(cacheItemName, cachePolicy);
        }

        public T Get(string cacheItemName, DateTimeOffset dt)
        {
            var cachePolicy = new CacheItemPolicy { AbsoluteExpiration = dt };
            return Get(cacheItemName, cachePolicy);
        }

        private T Get(string cacheItemName, CacheItemPolicy cachePolicy)
        {
            var cacheItem = _cache.Get(cacheItemName);
            if (cacheItem == null)
            {
                var newItem = CreateInstance();
                _cache.Add(cacheItemName, newItem, cachePolicy);
                return newItem;
            }
            return (T)cacheItem;
        }

        private T CreateInstance()
        {
            try
            {
                return new T();
            }
            catch (MissingMethodException)
            {
                throw new InvalidOperationException($"The type {typeof(T).Name} must have a parameterless constructor.");
            }
        }
    }
}
