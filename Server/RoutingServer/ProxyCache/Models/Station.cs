using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProxyCache.Models
{
    [DataContract]
    public class Station
    {
        [DataMember]
        public int Number { get; set; }

        [DataMember(Name = "contract_name")]
        public string ContractName { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public Position Position { get; set; }

        [DataMember]
        public bool Banking { get; set; }

        [DataMember]
        public bool Bonus { get; set; }

        [DataMember(Name = "bike_stands")]
        public int BikeStands { get; set; }

        [DataMember(Name = "available_bike_stands")]
        public int AvailableBikeStands { get; set; }

        [DataMember(Name = "available_bikes")]
        public int AvailableBikes { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember(Name = "last_update")]
        public long LastUpdate { get; set; }
    }
}
