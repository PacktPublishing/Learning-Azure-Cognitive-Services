using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Chapter6.Contracts
{
    [DataContract]
    public class SpellCheckResponse
    {
        [DataMember]
        public string _type { get; set; }

        [DataMember]
        public List<FlaggedTokens> flaggedTokens { get; set; }
    }

    [DataContract]
    public class FlaggedTokens
    {
        [DataMember]
        public int offset { get; set; }

        [DataMember]
        public string token { get; set; }

        [DataMember]
        public string type { get; set; }

        [DataMember]
        public List<Suggestions> suggestions { get; set; }
    }

    [DataContract]
    public class Suggestions
    {
        [DataMember]
        public string suggestion { get; set; }

        [DataMember]
        public double score { get; set; }
    }
}
