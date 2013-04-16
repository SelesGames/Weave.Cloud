using System.Runtime.Serialization;

namespace Weave.UserFeedAggregator.DTOs.ServerIncoming
{
    [DataContract]
    public class NewFeed
    {
        [DataMember(Order=1)]  public string Name { get; set; }
        [DataMember(Order=2)]  public string Uri { get; set; }
        [DataMember(Order=3)]  public string Category { get; set; }
        [DataMember(Order=4)]  public ArticleViewingType ArticleViewingType { get; set; }
    }
}
