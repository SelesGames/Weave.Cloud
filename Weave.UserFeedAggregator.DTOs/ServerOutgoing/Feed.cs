using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Weave.UserFeedAggregator.DTOs.ServerOutgoing
{
    [DataContract]
    public class Feed
    {
        [DataMember(Order=1)]   public Guid Id { get; set; }
        [DataMember(Order=2)]   public string Name { get; set; }
        [DataMember(Order=3)]   public string Uri { get; set; }
        [DataMember(Order=4)]   public string Category { get; set; }
        [DataMember(Order=5)]   public ArticleViewingType ArticleViewingType { get; set; }
        [DataMember(Order=6)]   public int NewArticleCount { get; set; }
        [DataMember(Order=7)]   public int UnreadCount { get; set; }
        [DataMember(Order=8)]   public int TotalArticleCount { get; set; }

        [DataMember(Order=9, IsRequired=false)]
        public List<NewsItem> News { get; set; }
    }
}
