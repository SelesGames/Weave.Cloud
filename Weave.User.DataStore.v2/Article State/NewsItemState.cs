using System.Runtime.Serialization;

namespace Weave.User.DataStore.v2
{
    [DataContract]
    public struct NewsItemState
    {
        [DataMember(Order= 1)]  public bool IsFavorite { get; set; }
        [DataMember(Order= 2)]  public bool HasBeenViewed { get; set; }
    }
}