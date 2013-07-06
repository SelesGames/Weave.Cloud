using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Weave.User.DataStore
{
    [DataContract]
    public class FeedNews
    {
        [DataMember(Order= 1)]  public Guid FeedId { get; set; }
        [DataMember(Order= 2)]  public List<NewsItem> News { get; set; }
    }
}
