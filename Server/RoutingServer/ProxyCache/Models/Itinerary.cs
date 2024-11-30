using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ProxyCache.Models
{

    [DataContract]
    public class Itinerary
    {
        [DataMember(Name = "distance")]
        public double Distance { get; set; }

        [DataMember(Name = "duration")]
        public double Duration { get; set; }

        [DataMember(Name = "instructions")]
        public List<Instruction> Instructions { get; set; }
    }

    [DataContract]
    public class Instruction
    {
        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "position")]
        public Position Position { get; set; }
    }

    [DataContract]
    public class Step
    {
        [DataMember(Name = "geometry")]
        public string Geometry { get; set; }

        [DataMember(Name = "maneuver")]
        public Maneuver Maneuver { get; set; }

        [DataMember(Name = "mode")]
        public string Mode { get; set; }

        [DataMember(Name = "driving_side")]
        public string Driving_side { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "intersections")]
        public List<Intersection> Intersections { get; set; }

        [DataMember(Name = "weight")]
        public double Weight { get; set; }

        [DataMember(Name = "distance")]
        public double Distance { get; set; }

        [DataMember(Name = "duration")]
        public double Duration { get; set; }
    }

    [DataContract]
    public class Maneuver
    {
        [DataMember(Name = "exit")]
        public int Exit { get; set; }

        [DataMember(Name = "bearing_after")]
        public int Bearing_after { get; set; }

        [DataMember(Name = "bearing_before")]
        public int Bearing_before { get; set; }

        [DataMember(Name = "location")]
        public List<double> Location { get; set; }

        [DataMember(Name = "modifier")]
        public string Modifier { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }
    }

    [DataContract]
    public class Intersection
    {
        [DataMember(Name = "out")]
        public int Out { get; set; }

        [DataMember(Name = "in")]
        public int In { get; set; }

        [DataMember(Name = "entry")]
        public List<bool> Entry { get; set; }

        [DataMember(Name = "bearings")]
        public List<int> Bearings { get; set; }

        [DataMember(Name = "location")]
        public List<double> Location { get; set; }
    }
}
