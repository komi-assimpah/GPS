using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProxyCache.Models
{
    [DataContract]
    public class Position
    {
        [DataMember]
        public double Lat { get; set; }

        [DataMember]
        public double Lng { get; set; }
    }
}
