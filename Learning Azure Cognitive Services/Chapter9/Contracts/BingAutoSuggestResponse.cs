using System.Runtime.Serialization;

namespace Chapter9.Contracts
{
    [DataContract]
    public class BingAutoSuggestResponse
    {
        [DataMember]
        public string _type { get; set; }
        [DataMember]
        public Querycontext queryContext { get; set; }
        [DataMember]
        public Suggestiongroup[] suggestionGroups { get; set; }
    }

    [DataContract]
    public class Querycontext
    {
        [DataMember]
        public string originalQuery { get; set; }
    }

    [DataContract]
    public class Suggestiongroup
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public Searchsuggestion[] searchSuggestions { get; set; }
    }

    [DataContract]
    public class Searchsuggestion
    {
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public string displayText { get; set; }
        [DataMember]
        public string query { get; set; }
        [DataMember]
        public string searchKind { get; set; }
    }

}
