using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Weave.User.DataStore.v2
{
    [DataContract]
    public class MasterNewsItemCollection
    {
        [DataMember(Order= 1)]  public Guid UserId { get; set; }
        [DataMember(Order= 2)]  public List<NewsItem> News { get; set; }
    }
}
