using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Weave.User.DataStore.v2
{
    [DataContract]
    public class UserInfo
    {
        [DataMember(Order= 1)]  public Guid Id { get; set; }
        [DataMember(Order= 2)]  public int FeedCount { get; set; }
        [DataMember(Order= 3)]  public List<Feed> Feeds { get; set; }

        [DataMember(Order= 4)]  public DateTime PreviousLoginTime { get; set; }
        [DataMember(Order= 5)]  public DateTime CurrentLoginTime { get; set; }

        [DataMember(Order= 6)]  public string ArticleDeletionTimeForMarkedRead { get; set; }
        [DataMember(Order= 7)]  public string ArticleDeletionTimeForUnread { get; set; }


        public override string ToString()
        {
            return string.Format("{0} - {1}", Id.ToString("N"), CurrentLoginTime);
        }
    }
}