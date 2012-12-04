
namespace Weave.RssAggregator.Parsing
{
    public class Entry
    {
        public string Title { get; set; }
        public string PublishDateTime { get; set; }
        public string Link { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public string YoutubeId { get; set; }
        public string VideoUri { get; set; }
        public string PodcastUri { get; set; }
        public string ZuneAppId { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
