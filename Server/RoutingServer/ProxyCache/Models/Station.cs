using System.Runtime.Serialization;

namespace ProxyCache.Models
{
    [DataContract]
    public class Station
    {
        [DataMember(Name = "number")]
        public int Number { get; set; }

        [DataMember(Name = "contract_name")]
        public string ContractName { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "address")]
        public string Address { get; set; }

        [DataMember(Name = "position")]
        public Position Position { get; set; }

        [DataMember(Name = "banking")]
        public bool Banking { get; set; }

        [DataMember(Name = "bonus")]
        public bool Bonus { get; set; }

        [DataMember(Name = "bike_stands")]
        public int BikeStands { get; set; }

        [DataMember(Name = "available_bike_stands")]
        public int AvailableBikeStands { get; set; }

        [DataMember(Name = "available_bikes")]
        public int AvailableBikes { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "last_update")]
        public long LastUpdate { get; set; }
    }
}
