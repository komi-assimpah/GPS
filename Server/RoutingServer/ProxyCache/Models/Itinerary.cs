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
    public class Itinerary
    {
        // Résumé général de l'itinéraire
        [DataMember]
        [JsonProperty("summary")]
        public ItinerarySummary Summary { get; set; }

        // Liste des segments (chaque segment contient des étapes ou des détails spécifiques)
        [DataMember]
        [JsonProperty("segments")]
        public List<Segment> Segments { get; set; }

        // Géométrie (chemin) de l'itinéraire, souvent encodée sous forme de polyline
        [DataMember]
        [JsonProperty("geometry")]
        public string Geometry { get; set; }
    }

    [DataContract]
    public class ItinerarySummary
    {
        // Distance totale de l'itinéraire en mètres
        [DataMember]
        [JsonProperty("distance")]
        public double Distance { get; set; }

        // Durée totale de l'itinéraire en secondes
        [JsonProperty("duration")]
        [DataMember]
        public double Duration { get; set; }
    }


    [DataContract]
    public class Segment
    {
        [DataMember]
        [JsonProperty("distance")]
        public double Distance { get; set; }

        [DataMember]
        [JsonProperty("duration")]
        public double Duration { get; set; }

        // Liste des étapes dans ce segment
        [DataMember]
        [JsonProperty("steps")]
        public List<Step> Steps { get; set; }
    }

    [DataContract]
    public class Step
    {
        // Distance de l'étape en mètres
        [DataMember]
        [JsonProperty("distance")]
        public double Distance { get; set; }

        // Durée de l'étape en secondes
        [DataMember]
        [JsonProperty("duration")]
        public double Duration { get; set; }

        // Direction ou instruction textuelle pour cette étape
        [DataMember]
        [JsonProperty("instruction")]
        public string Instruction { get; set; }

        [DataMember]
        [JsonProperty("start_location")]
        public List<double> StartLocation { get; set; } // [longitude, latitude]

        [DataMember]
        [JsonProperty("end_location")]
        public List<double> EndLocation { get; set; } // [longitude, latitude]
    }
}



