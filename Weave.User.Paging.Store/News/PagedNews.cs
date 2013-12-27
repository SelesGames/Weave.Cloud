using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Weave.User.Paging.Store.News
{
    [DataContract]
    public class PagedNews
    {
        [DataMember(Order= 1)]  public int Index { get; set; }
        [DataMember(Order= 2)]  public int NewsCount { get; set; }
        [DataMember(Order= 3)]  public List<NewsItem> News { get; set; }
    }
}
