using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProxyCache.Models
{
    [DataContract]
    public class Contract
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember(Name = "commercial_name")]
        public string CommercialName { get; set; }

        [DataMember]
        public List<string> Cities { get; set; }

        [DataMember(Name = "country_code")]
        public string CountryCode { get; set; }
    }
}
