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
        [DataMember(Name = "distance")]
        public double Distance { get; set; }

        [DataMember(Name = "duration")]
        public double Duration { get; set; }

        [DataMember(Name = "type")]
        public int Type { get; set; }

        [DataMember(Name = "instruction")]
        public string Instruction { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "way_points")]
        public List<int> WayPoints { get; set; }
    }
}
