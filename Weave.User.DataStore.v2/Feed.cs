using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Weave.User.DataStore.v2
{
    [DataContract]
    public class Feed
    {
        [DataMember(Order= 1)]  public Guid Id { get; set; }
        [DataMember(Order= 2)]  public string FeedUri { get; set; }
        [DataMember(Order= 3)]  public string FeedName { get; set; }
        [DataMember(Order= 4)]  public string IconUri { get; set; }

        [DataMember(Order= 5)]  public string Category { get; set; }
        [DataMember(Order= 6)]  public string Etag { get; set; }
        [DataMember(Order= 7)]  public string LastModified { get; set; }
        [DataMember(Order= 8)]  public string MostRecentNewsItemPubDate { get; set; }
        [DataMember(Order= 9)]  public DateTime LastRefreshedOn { get; set; }
        [DataMember(Order=10)]  public DateTime MostRecentEntrance { get; set; }
        [DataMember(Order=11)]  public DateTime PreviousEntrance { get; set; }
        [DataMember(Order=12)]  public ArticleViewingType ArticleViewingType { get; set; }
        [DataMember(Order=13)]  public List<NewsItem> News { get; set; }
    }
}
