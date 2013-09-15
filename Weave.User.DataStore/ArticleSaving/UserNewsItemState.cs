using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Weave.User.DataStore
{
    [DataContract]
    public class UserNewsItemState
    {
        [DataMember(Order= 1)]  public Guid Id { get; set; }
        [DataMember(Order= 2)]  public List<NewsItemState> NewsItemStates { get; set; }
    }
}
