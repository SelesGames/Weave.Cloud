using ProtoBuf;
using System;
using System.Runtime.Serialization;

namespace Weave.User.DTOs
{
    [ProtoContract]
    public class NewsItem
    {
        [DataMember][ProtoMember (1)]   public Guid Id { get; set; }
        [DataMember][ProtoMember (1)]   public Guid FeedId { get; set; }
        [DataMember][ProtoMember (2)]   public string Title { get; set; }
        [DataMember][ProtoMember (3)]   public string Link { get; set; }
        [DataMember][ProtoMember (4)]   public string ImageUrl { get; set; }
        [DataMember][ProtoMember (5)]   public string YoutubeId { get; set; }
        [DataMember][ProtoMember (6)]   public string VideoUri { get; set; } 
        [DataMember][ProtoMember (7)]   public string PodcastUri { get; set; }
        [DataMember][ProtoMember (8)]   public string ZuneAppId { get; set; }
        [DataMember][ProtoMember (9)]   public DateTime PublishDateTime { get; set; }
        [DataMember][ProtoMember(10)]   public DateTime OriginalDownloadDateTime { get; set; }
        [DataMember][ProtoMember(11)]   public bool IsFavorite { get; set; }
        [DataMember][ProtoMember(12)]   public bool HasBeenViewed { get; set; }
    }
}
