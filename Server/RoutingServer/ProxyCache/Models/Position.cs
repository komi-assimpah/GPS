using System.Runtime.Serialization;

namespace ProxyCache.Models
{
    [DataContract]
    public class Position
    {
        [DataMember(Name = "lng")]
        public double Lat { get; set; }

        [DataMember(Name = "lat")]
        public double Lng { get; set; }
    }
}
