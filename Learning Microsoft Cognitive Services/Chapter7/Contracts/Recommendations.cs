using System.Runtime.Serialization;

namespace Chapter7.Contracts
{
    [DataContract]
    public class RecommendedItem
    {
        [DataMember]
        public string recommendedItemId { get; set; }
        [DataMember]
        public float score { get; set; }
    }
}