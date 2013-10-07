using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Weave.RssAggregator.LibraryClient;
using Common.JsonDotNet;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;

namespace FeedIconGrabber
{
    public class FeedUrlIconMappingsCreator
    {
        readonly string FEEDS_FILE = @"C:\WORK\CODE\SELES GAMES\Weave.Cloud\masterfeeds.xml";
        readonly string PATH = @"C:\WORK\CODE\SELES GAMES\Weave.Cloud";

        public async Task BeginDownload()
        {
            var feedLibraryClient = new FeedLibraryClient(FEEDS_FILE);
            await feedLibraryClient.LoadFeedsAsync();

            var feeds = new List<FeedUrlIconMapping>();

            foreach (var feed in feedLibraryClient.Feeds)
            {
                var feedIconMap = new FeedUrlIconMapping
                {
                    Url = feed.FeedUri,
                    IconUrl = feed.IconUrl
                };

                feeds.Add(feedIconMap);

                if (!string.IsNullOrWhiteSpace(feed.CorrectedUri))
                {
                    feedIconMap = new FeedUrlIconMapping
                    {
                        Url = feed.CorrectedUri,
                        IconUrl = feed.IconUrl
                    };
                    feeds.Add(feedIconMap);
                }
            }

           feeds = feeds.OrderBy(o => o.Url).ToList();

            var feedUrlIconMappings = new FeedUrlIconMappings();
            feedUrlIconMappings.AddRange(feeds);

            string fileName = string.Format("{0}\\{1}", PATH, "iconMap.json");

            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                await fs.WriteObject(
                    feedUrlIconMappings, 
                    new JsonSerializerSettings { Formatting = Formatting.Indented },
                    Encoding.UTF8);

                fs.Flush();
                fs.Close();
            }
        }
    }
}
