using ProtoBuf;

namespace Weave.RssAggregator.Core.DTOs.Outgoing
{
    [ProtoContract]
    public class NewsItem
    {
        [ProtoMember(1)] public string Title { get; set; }
        [ProtoMember(2)] public string PublishDateTime { get; set; }
        [ProtoMember(3)] public string Link { get; set; }
        [ProtoMember(4)] public string ImageUrl { get; set; }
        [ProtoMember(5)] public string Description { get; set; }
        [ProtoMember(6)] public string YoutubeId { get; set; }
        [ProtoMember(7)] public string VideoUri { get; set; }
        [ProtoMember(8)] public string PodcastUri { get; set; }
        [ProtoMember(9)] public string ZuneAppId { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
