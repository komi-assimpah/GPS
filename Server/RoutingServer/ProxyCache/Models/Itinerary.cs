using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProxyCache.Models
{

    [DataContract]
    public class Step
    {
        [DataMember]
        [JsonProperty("geometry")]
        public string Geometry { get; set; }

        [DataMember]
        [JsonProperty("maneuver")]
        public Maneuver Maneuver { get; set; }

        [DataMember]
        [JsonProperty("mode")]
        public string Mode { get; set; }

        [DataMember]
        [JsonProperty("driving_side")]
        public string DrivingSide { get; set; }

        [DataMember]
        [JsonProperty("name")]
        public string Name { get; set; }

        [DataMember]
        [JsonProperty("intersections")]
        public List<Intersection> Intersections { get; set; }

        [DataMember]
        [JsonProperty("weight")]
        public double Weight { get; set; }

        [DataMember]
        [JsonProperty("distance")]
        public double Distance { get; set; }

        [DataMember]
        [JsonProperty("duration")]
        public double Duration { get; set; }
    }


    [DataContract]
    public class Maneuver
    {
        [DataMember]
        [JsonProperty("Exit")]
        public int Exit { get; set; }

        [DataMember]
        [JsonProperty("Bearing_after")]
        public int Bearing_after { get; set; }

        [DataMember]
        [JsonProperty("Bearing_before")]
        public int Bearing_before { get; set; }

        [DataMember]
        [JsonProperty("Location")]
        public List<double> Location { get; set; }

        [DataMember]
        [JsonProperty("Modifier")]
        public string Modifier { get; set; }

        [DataMember]
        [JsonProperty("Type")]
        public string Type { get; set; }
    }

    [DataContract]
    public class Intersection
    {

        [DataMember]
        [JsonProperty("out")]
        public int Out { get; set; }

        [DataMember]
        [JsonProperty("in")]
        public int In { get; set; }

        [DataMember]
        [JsonProperty("entry")]
        public List<bool> Entry { get; set; }

        [DataMember]
        [JsonProperty("bearings")]
        public List<int> Bearings { get; set; }

        [DataMember]
        [JsonProperty("location")]
        public List<double> Location { get; set; }

    }

}



