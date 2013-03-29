using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Weave.User.DataStore
{
    [DataContract]
    public class UserFeeds
    {
        [DataMember(Order= 1)]  public Guid UserId { get; set; }
        [DataMember(Order= 2)]  public List<FeedNews> FeedsNews { get; set; }
    }
}
