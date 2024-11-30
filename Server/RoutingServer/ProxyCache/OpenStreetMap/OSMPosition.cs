using ProxyCache.GenericProxyCache;
using ProxyCache.Models;

namespace ProxyCache.OpenStreetMap
{
    internal class OSMPosition : ICacheable
    {
        public Position Position { get; set; }

        public OSMPosition() {}

        void ICacheable.Fill(object obj)
        {
            string address = (string)obj;
            Position = OpenStreetMapREST.ResolveAddress(address).Result;
        }
    }
}
