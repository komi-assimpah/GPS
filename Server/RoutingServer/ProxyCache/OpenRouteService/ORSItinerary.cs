using ProxyCache.GenericProxyCache;
using ProxyCache.Models;

namespace ProxyCache.OpenStreetMap
{
    internal class ORSItinerary : ICacheable
    {
        public Itinerary Itinerary { get; set; }

        public ORSItinerary() {}

        void ICacheable.Fill(object obj)
        {
            (Position startPosition, Position endPosition, string profile) = ((Position, Position, string))obj;
            Itinerary = OpenRouteServiceREST.GetItinerary(startPosition, endPosition, profile).Result;
        }
    }
}
