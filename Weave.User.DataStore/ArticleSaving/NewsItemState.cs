using System;
using System.Runtime.Serialization;

namespace Weave.User.DataStore
{
    [DataContract]
    public class NewsItemState
    {
        [DataMember(Order=1)]   public Guid Id { get; set; }
        [DataMember(Order=2)]   public bool IsFavorite { get; set; }
        [DataMember(Order=3)]   public bool HasBeenViewed { get; set; }
    }
}
