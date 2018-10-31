using System;
using System.Runtime.Serialization;

namespace End_to_End.Contracts
{
    [DataContract]
    public class BingNewsResponse
    {
        [DataMember]
        public string _type { get; set; }
        [DataMember]
        public string readLink { get; set; }
        [DataMember]
        public int totalEstimatedMatches { get; set; }
        [DataMember]
        public Value[] value { get; set; }
    }

    [DataContract]
    public class Value
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public Image image { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public About[] about { get; set; }
        [DataMember]
        public Provider[] provider { get; set; }
        [DataMember]
        public DateTime datePublished { get; set; }
        [DataMember]
        public Mention[] mentions { get; set; }
        [DataMember]
        public bool headline { get; set; }
        [DataMember]
        public Clusteredarticle[] clusteredArticles { get; set; }
    }

    [DataContract]
    public class Image
    {
        [DataMember]
        public Thumbnail thumbnail { get; set; }
    }

    [DataContract]
    public class Thumbnail
    {
        [DataMember]
        public string contentUrl { get; set; }
        [DataMember]
        public int width { get; set; }
        [DataMember]
        public int height { get; set; }
    }

    [DataContract]
    public class About
    {
        [DataMember]
        public string readLink { get; set; }
        [DataMember]
        public string name { get; set; }
    }

    [DataContract]
    public class Provider
    {
        [DataMember]
        public string _type { get; set; }
        [DataMember]
        public string name { get; set; }
    }

    [DataContract]
    public class Mention
    {
        [DataMember]
        public string name { get; set; }
    }

    [DataContract]
    public class Clusteredarticle
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public About[] about { get; set; }
        [DataMember]
        public Provider[] provider { get; set; }
        [DataMember]
        public DateTime datePublished { get; set; }
    }
    
    #region Web search response

    [DataContract]
    public class WebSearchResponse
    {
        [DataMember]
        public string _type { get; set; }
        [DataMember]
        public Webpages webPages { get; set; }
        [DataMember]
        public Images images { get; set; }
        [DataMember]
        public Relatedsearches relatedSearches { get; set; }
        [DataMember]
        public Videos videos { get; set; }
        [DataMember]
        public Rankingresponse rankingResponse { get; set; }
    }

    [DataContract]
    public class Webpages
    {
        [DataMember]
        public string webSearchUrl { get; set; }
        [DataMember]
        public int totalEstimatedMatches { get; set; }
        [DataMember]
        public WebValue[] value { get; set; }
    }

    [DataContract]
    public class WebValue
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public string displayUrl { get; set; }
        [DataMember]
        public string snippet { get; set; }
        [DataMember]
        public Deeplink[] deepLinks { get; set; }
        [DataMember]
        public DateTime dateLastCrawled { get; set; }
        [DataMember]
        public About[] about { get; set; }
    }

    [DataContract]
    public class Deeplink
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string url { get; set; }
    }

    [DataContract]
    public class Images
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string readLink { get; set; }
        [DataMember]
        public string webSearchUrl { get; set; }
        [DataMember]
        public bool isFamilyFriendly { get; set; }
        [DataMember]
        public Value1[] value { get; set; }
        [DataMember]
        public bool displayShoppingSourcesBadges { get; set; }
        [DataMember]
        public bool displayRecipeSourcesBadges { get; set; }
    }

    [DataContract]
    public class Value1
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string webSearchUrl { get; set; }
        [DataMember]
        public string thumbnailUrl { get; set; }
        [DataMember]
        public DateTime datePublished { get; set; }
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
    }

    [DataContract]
    public class Relatedsearches
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public Value2[] value { get; set; }
    }

    [DataContract]
    public class Value2
    {
        [DataMember]
        public string text { get; set; }
        [DataMember]
        public string displayText { get; set; }
        [DataMember]
        public string webSearchUrl { get; set; }
    }

    [DataContract]
    public class Videos
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string readLink { get; set; }
        [DataMember]
        public string webSearchUrl { get; set; }
        [DataMember]
        public bool isFamilyFriendly { get; set; }
        [DataMember]
        public Value3[] value { get; set; }
        [DataMember]
        public string scenario { get; set; }
    }

    [DataContract]
    public class Value3
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
        public Thumbnail1 thumbnail { get; set; }
        [DataMember]
        public bool allowMobileEmbed { get; set; }
        [DataMember]
        public bool isSuperfresh { get; set; }
    }

    [DataContract]
    public class Thumbnail1
    {
        [DataMember]
        public int width { get; set; }
        [DataMember]
        public int height { get; set; }
    }

    [DataContract]
    public class Publisher
    {
        [DataMember]
        public string name { get; set; }
    }

    [DataContract]
    public class Rankingresponse
    {
        [DataMember]
        public Mainline mainline { get; set; }
        [DataMember]
        public Sidebar sidebar { get; set; }
    }

    [DataContract]
    public class Mainline
    {
        [DataMember]
        public Item[] items { get; set; }
    }

    [DataContract]
    public class Item
    {
        [DataMember]
        public string answerType { get; set; }
        [DataMember]
        public int resultIndex { get; set; }
        [DataMember]
        public Value4 value { get; set; }
    }

    [DataContract]
    public class Value4
    {
        [DataMember]
        public string id { get; set; }
    }

    [DataContract]
    public class Sidebar
    {
        [DataMember]
        public Item1[] items { get; set; }
    }

    [DataContract]
    public class Item1
    {
        [DataMember]
        public string answerType { get; set; }
        [DataMember]
        public Value5 value { get; set; }
    }

    [DataContract]
    public class Value5
    {
        [DataMember]
        public string id { get; set; }
    }

    #endregion Web search response
}