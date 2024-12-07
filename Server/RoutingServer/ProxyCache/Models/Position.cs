using System.Runtime.Serialization;

namespace ProxyCache.Models
{
    [DataContract]
    public class Position
    {
        [DataMember(Name = "lat")]
        public double Lat { get; set; }

        [DataMember(Name = "lng")]
        public double Lng { get; set; }
    }
}
