using System;
using System.Runtime.Serialization;

namespace Chapter9.Contracts
{

    #region Image search response 

    [DataContract]
    public class ImageSearchResponse
    {
        [DataMember]
        public string _type { get; set; }
        [DataMember]
        public Instrumentation instrumentation { get; set; }
        [DataMember]
        public string readLink { get; set; }
        [DataMember]
        public string webSearchUrl { get; set; }
        [DataMember]
        public int totalEstimatedMatches { get; set; }
        [DataMember]
        public Value[] value { get; set; }
        [DataMember]
        public int nextOffsetAddCount { get; set; }
        [DataMember]
        public bool displayShoppingSourcesBadges { get; set; }
        [DataMember]
        public bool displayRecipeSourcesBadges { get; set; }
    }

    [DataContract]
    public class Value
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string webSearchUrl { get; set; }
        [DataMember]
        public string thumbnailUrl { get; set; }
        [DataMember]
        public object datePublished { get; set; }
        [DataMember]
        public string contentUrl { get; set; }
        [DataMember]
        public string hostPageUrl { get; set; }
        [DataMember]
        public string contentSize { get; set; }
        [DataMember]
        public string encodingFormat { get; set; }
        [DataMember]
        public string hostPageDisplayUrl { get; set; }
        [DataMember]
        public int width { get; set; }
        [DataMember]
        public int height { get; set; }
        [DataMember]
        public Thumbnail thumbnail { get; set; }
        [DataMember]
        public string imageInsightsToken { get; set; }
        [DataMember]
        public string imageId { get; set; }
        [DataMember]
        public string accentColor { get; set; }
    }

    #endregion Image search response

    #region Common response classes

    [DataContract]
    public class Instrumentation
    {
        [DataMember]
        public string pageLoadPingUrl { get; set; }
    }

    [DataContract]
    public class Thumbnail
    {
        [DataMember]
        public int width { get; set; }
        [DataMember]
        public int height { get; set; }
    }

    #endregion Common response classes

    #region VideoSearchResponse

    [DataContract]
    public class VideoSearchResponse
    {
        [DataMember]
        public string _type { get; set; }
        [DataMember]
        public Instrumentation instrumentation { get; set; }
        [DataMember]
        public string readLink { get; set; }
        [DataMember]
        public string webSearchUrl { get; set; }
        [DataMember]
        public int totalEstimatedMatches { get; set; }
        [DataMember]
        public VideoValue[] value { get; set; }
        [DataMember]
        public int nextOffsetAddCount { get; set; }
    }

    [DataContract]
    public class VideoValue
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public string webSearchUrl { get; set; }
        [DataMember]
        public string thumbnailUrl { get; set; }
        [DataMember]
        public DateTime datePublished { get; set; }
        [DataMember]
        public Publisher[] publisher { get; set; }
        [DataMember]
        public Creator creator { get; set; }
        [DataMember]
        public string contentUrl { get; set; }
        [DataMember]
        public string hostPageUrl { get; set; }
        [DataMember]
        public string encodingFormat { get; set; }
        [DataMember]
        public string hostPageDisplayUrl { get; set; }
        [DataMember]
        public int width { get; set; }
        [DataMember]
        public int height { get; set; }
        [DataMember]
        public string duration { get; set; }
        [DataMember]
        public string motionThumbnailUrl { get; set; }
        [DataMember]
        public string embedHtml { get; set; }
        [DataMember]
        public bool allowHttpsEmbed { get; set; }
        [DataMember]
        public int viewCount { get; set; }
        [DataMember]
        public Thumbnail thumbnail { get; set; }
        [DataMember]
        public string videoId { get; set; }
        [DataMember]
        public bool allowMobileEmbed { get; set; }
        [DataMember]
        public bool isSuperfresh { get; set; }
    }

    [DataContract]
    public class Creator
    {
        [DataMember]
        public string name { get; set; }
    }

    [DataContract]
    public class Publisher
    {
        [DataMember]
        public string name { get; set; }
    }

    #endregion VideoSearchResponse
}