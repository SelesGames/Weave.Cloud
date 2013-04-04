using System.Runtime.Serialization;

namespace Weave.UserFeedAggregator.DTOs.ServerIncoming
{
    [DataContract]
    public class Feed
    {
        [DataMember(Order=1)]  public string FeedName { get; set; }
        [DataMember(Order=2)]  public string FeedUri { get; set; }
        [DataMember(Order=3)]  public string Category { get; set; }
        [DataMember(Order=4)]  public ArticleViewingType ArticleViewingType { get; set; }
    }
}
