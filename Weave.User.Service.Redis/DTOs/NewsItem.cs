using System;
using System.Runtime.Serialization;
using Weave.User.BusinessObjects;

namespace Weave.User.Service.Redis.DTOs
{
    /// <summary>
    /// The canonical form of a news item aso stored in the redis cache.  
    /// Because news items are not inherently tethered to a particular feed, they have no 
    /// FeedId field.
    /// </summary>
    [DataContract]
    public class NewsItem
    {
        [DataMember(Order= 1)]  public Guid Id { get; set; }
        [DataMember(Order= 2)]  public DateTime UtcPublishDateTime { get; set; }
        [DataMember(Order= 3)]  public string UtcPublishDateTimeString { get; set; }
        [DataMember(Order= 4)]  public string Title { get; set; }
        [DataMember(Order= 5)]  public string Link { get; set; }
        [DataMember(Order= 6)]  public string ImageUrl { get; set; }
        [DataMember(Order= 7)]  public string YoutubeId { get; set; }
        [DataMember(Order= 8)]  public string VideoUri { get; set; } 
        [DataMember(Order= 9)]  public string PodcastUri { get; set; }
        [DataMember(Order=10)]  public string ZuneAppId { get; set; }
        [DataMember(Order=11)]  public Image Image { get; set; }




        #region ToString override

        public override string ToString()
        {
            return string.Format("{0}: {1}", Id.ToString("N"), Title);
        }

        #endregion
    }
}
