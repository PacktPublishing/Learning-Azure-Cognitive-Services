using System;
using System.Runtime.Serialization;

namespace Chapter7.Contracts
{
    [DataContract]
    public class RecommandationModel
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public DateTime creationTime { get; set; }
        [DataMember]
        public string modelStatus { get; set; }
    }
}