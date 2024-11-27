using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProxyCache.Models
{
    [DataContract]
    public class City
    {
        [DataMember]
        public string Name { get; set; }
    }
}
