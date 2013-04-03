using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Weave.UserFeedAggregator.DTOs.ServerIncoming
{
    [DataContract]
    public class Feed
    {
        [DataMember(Order= 1)]  public Guid Id { get; set; }
        [DataMember(Order= 2)]  public string FeedName { get; set; }
        [DataMember(Order= 3)]  public string FeedUri { get; set; }
        [DataMember(Order= 4)]  public string Category { get; set; }
        [DataMember(Order= 5)]  public string Etag { get; set; }
        [DataMember(Order= 6)]  public string LastModified { get; set; }
        [DataMember(Order= 7)]  public string MostRecentNewsItemPubDate { get; set; }
        [DataMember(Order= 8)]  public DateTime LastRefreshedOn { get; set; }
        [DataMember(Order= 9)]  public ArticleViewingType ArticleViewingType { get; set; }
    }
}
