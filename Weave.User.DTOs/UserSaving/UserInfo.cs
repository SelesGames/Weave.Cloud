using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Weave.User.DataStore
{
    [DataContract]
    public class UserInfo
    {
        [DataMember(Order= 1)]  public Guid Id { get; set; }
        [DataMember(Order= 2)]  public int FeedCount { get; set; }
        [DataMember(Order= 3)]  public List<Feed> Feeds { get; set; }

        [DataMember(Order= 4)]  public DateTime PreviousLoginTime { get; set; }
        [DataMember(Order= 5)]  public DateTime CurrentLoginTime { get; set; }
    }
}