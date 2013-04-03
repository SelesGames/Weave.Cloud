using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Weave.Article.Service.DTOs
{
    [DataContract]
    public class FavoriteNewsItem
    {
        [DataMember(Order= 1)] public Guid Id { get; set; }
        [DataMember(Order= 2)] public Guid FeedId { get; set; }
        [DataMember(Order= 3)] public string Title { get; set; }
        [DataMember(Order= 4)] public string PublishDateTime { get; set; }
        [DataMember(Order= 5)] public string Link { get; set; }
        [DataMember(Order= 6)] public string ImageUrl { get; set; }
        [DataMember(Order= 7)] public string Description { get; set; }
        [DataMember(Order= 8)] public string YoutubeId { get; set; }
        [DataMember(Order= 9)] public string VideoUri { get; set; }
        [DataMember(Order=10)] public string PodcastUri { get; set; }
        [DataMember(Order=11)] public string ZuneAppId { get; set; }
        [DataMember(Order=12)] public string Notes { get; set; }
        [DataMember(Order=13)] public string SourceName { get; set; }
        [DataMember(Order=14)] public List<string> Tags { get; set; }
        [DataMember(Order=15)] public Image Image { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
