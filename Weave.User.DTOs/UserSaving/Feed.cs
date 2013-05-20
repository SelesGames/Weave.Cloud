using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

//namespace Weave.User.DataStore
//{
//    [DataContract]
//    public class Feed
//    {
//        [DataMember(Order= 1)]  public Guid Id { get; set; }
//        [DataMember(Order= 2)]  public string FeedName { get; set; }
//        [DataMember(Order= 3)]  public string FeedUri { get; set; }
//        [DataMember(Order= 4)]  public string Category { get; set; }
//        [DataMember(Order= 5)]  public string Etag { get; set; }
//        [DataMember(Order= 6)]  public string LastModified { get; set; }
//        [DataMember(Order= 7)]  public string MostRecentNewsItemPubDate { get; set; }
//        [DataMember(Order= 8)]  public DateTime LastRefreshedOn { get; set; }
//        [DataMember(Order= 9)]  public ArticleViewingType ArticleViewingType { get; set; }


//        //public Guid NewsHash { get; set; }
//        //public List<UpdateParameters> UpdateHistory { get; set; }

//        // maybe??
//        [DataMember(Order=10)]  public List<NewsItem> News { get; set; }

//        [DataMember(Order=11)]  public DateTime MostRecentEntrance { get; set; }
//        [DataMember(Order=12)]  public DateTime PreviousEntrance { get; set; }
//    }
//}

namespace Weave.User.DataStore
{
    [DataContract]
    public class Feed
    {
        [DataMember(Order= 1)]  public Guid Id { get; set; }
        [DataMember(Order= 2)]  public string FeedUri { get; set; }
        [DataMember(Order= 3)]  public string FeedName { get; set; }
        [DataMember(Order= 4)]  public string Category { get; set; }
        [DataMember(Order= 5)]  public string Etag { get; set; }
        [DataMember(Order= 6)]  public string LastModified { get; set; }
        [DataMember(Order= 7)]  public string MostRecentNewsItemPubDate { get; set; }
        [DataMember(Order= 8)]  public DateTime LastRefreshedOn { get; set; }
        [DataMember(Order= 9)]  public DateTime MostRecentEntrance { get; set; }
        [DataMember(Order=10)]  public DateTime PreviousEntrance { get; set; }
        [DataMember(Order=11)]  public ArticleViewingType ArticleViewingType { get; set; }
        [DataMember(Order=12)]  public List<NewsItem> News { get; set; }
    }
}
