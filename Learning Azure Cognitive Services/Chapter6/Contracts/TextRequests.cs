using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Chapter6.Contracts
{
    [DataContract]
    public class TextDocumentRequest
    {
        [DataMember]
        public string language { get; set; }

        [DataMember]
        public string id { get; set; }

        [DataMember]
        public string text { get; set; }
    }

    [DataContract]
    public class TextRequests
    {
        [DataMember]
        public List<TextDocumentRequest> documents { get; set; }
    }
}