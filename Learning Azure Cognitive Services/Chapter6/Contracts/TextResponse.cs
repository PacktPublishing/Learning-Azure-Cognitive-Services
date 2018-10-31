using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Chapter6.Contracts
{
    [DataContract]
    public class TextErrors
    {
        [DataMember]
        public string id { get; set; }

        [DataMember]
        public string message { get; set; }
    }

    #region Language response

    [DataContract]
    public class TextResponse
    {
        [DataMember]
        public List<TextLanguageDocuments> documents { get; set; }

        [DataMember]
        public List<TextErrors> errors { get; set; }
    }

    [DataContract]
    public class TextLanguageDocuments
    {
        [DataMember]
        public string id { get; set; }

        [DataMember]
        public List<TextDetectedLanguages> detectedLanguages { get; set; }
    }

    [DataContract]
    public class TextDetectedLanguages
    {
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string iso6391Name { get; set; }

        [DataMember]
        public double score { get; set; }
    }

    #endregion

    #region Sentiment response

    [DataContract]
    public class TextSentimentResponse
    {
        [DataMember]
        public List<TextSentimentDocuments> documents { get; set; }

        [DataMember]
        public List<TextErrors> errors { get; set; }
    }

    [DataContract]
    public class TextSentimentDocuments
    {
        [DataMember]
        public double score { get; set; }

        [DataMember]
        public string id { get; set; }
    }

    #endregion

    #region Key phrases response

    [DataContract]
    public class TextKeyPhrasesResponse
    {
        [DataMember]
        public List<TextKeyPhrasesDocuments> documents { get; set; }

        [DataMember]
        public List<TextErrors> errors { get; set; }
    }

    [DataContract]
    public class TextKeyPhrasesDocuments
    {
        [DataMember]
        public List<string> keyPhrases { get; set; }

        [DataMember]
        public string id { get; set; }
    }

    #endregion
}