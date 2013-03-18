using ProtoBuf;
using System;
using System.Collections.Generic;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.Article.Service.WorkerRole.DTOs
{
    [ProtoContract]
    public class FavoriteNewsItem
    {
        [ProtoMember (1)] public Guid Id { get; set; }
        [ProtoMember (2)] public Guid FeedId { get; set; }
        [ProtoMember (3)] public string Title { get; set; }
        [ProtoMember (4)] public string PublishDateTime { get; set; }
        [ProtoMember (5)] public string Link { get; set; }
        [ProtoMember (6)] public string ImageUrl { get; set; }
        [ProtoMember (7)] public string Description { get; set; }
        [ProtoMember (8)] public string YoutubeId { get; set; }
        [ProtoMember (9)] public string VideoUri { get; set; }
        [ProtoMember(10)] public string PodcastUri { get; set; }
        [ProtoMember(11)] public string ZuneAppId { get; set; }
        [ProtoMember(12)] public string Notes { get; set; }
        [ProtoMember(13)] public string SourceName { get; set; }
        [ProtoMember(14)] public List<string> Tags { get; set; }
        [ProtoMember(15)] public Image Image { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
