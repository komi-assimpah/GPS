using ProxyCache.GenericProxyCache;
using ProxyCache.Models;

namespace ProxyCache.OpenStreetMap
{
    internal class ORSMItinerary : ICacheable
    {
        public Itinerary Itinerary { get; set; }

        public ORSMItinerary() {}

        void ICacheable.Fill(object obj)
        {
            (Position startPosition, Position endPosition, string profile) = ((Position, Position, string))obj;
            Itinerary = OpenStreetMapREST.GetItinerary(startPosition, endPosition, profile).Result;
        }
    }
}
