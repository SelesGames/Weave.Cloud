using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Weave.UserFeedAggregator.DTOs.ServerOutgoing
{
    /// <summary>
    /// DTO representing what is shown in the article list screen
    /// </summary>
    [DataContract]
    public class UserNews
    {
        [DataMember(Order= 1)]  public Guid Id { get; set; }
        [DataMember(Order= 2)]  public int FeedCount { get; set; }
        [DataMember(Order= 3)]  public int NewsCount { get; set; }
        [DataMember(Order= 4)]  public int Take { get; set; }
        [DataMember(Order= 5)]  public int Skip { get; set; }
        [DataMember(Order= 6)]  public bool HasMore { get; set; }

        [DataMember(Order= 7)]  public List<Feed> Feeds { get; set; }
        [DataMember(Order= 8)]  public List<NewsItem> News { get; set; }

        [DataMember(Order=98)]  public TimeSpan DataStoreReadTime { get; set; }
        [DataMember(Order=99)]  public TimeSpan DataStoreWriteTime { get; set; }
    }
}
