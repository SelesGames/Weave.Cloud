using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Weave.RssAggregator.LibraryClient;

namespace FeedIconGrabber
{
    public class FeedUrlIconMappingsCreator
    {
        readonly string FEEDS_FILE = @"C:\WORK\CODE\SELES GAMES\Weave.Cloud\masterfeeds.xml";
        readonly string PATH = @"C:\WORK\CODE\SELES GAMES";

        public async Task BeginDownload()
        {
            var feedLibraryClient = new FeedLibraryClient(FEEDS_FILE);
            await feedLibraryClient.LoadFeedsAsync();

            var feeds = feedLibraryClient.Feeds
                .Select(o =>
                    new FeedUrlIconMapping
                    {
                        Url = o.FeedUri,
                        IconUrl = o.IconUrl,
                    }
                );

            var feedUrlIconMappings = new FeedUrlIconMappings();
            feedUrlIconMappings.AddRange(feeds);

            string fileName = string.Format("{0}\\{1}", PATH, "iconMap.xml");

            var serializer = new XmlSerializer(typeof(FeedUrlIconMappings), "");

            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                serializer.Serialize(fs, feedUrlIconMappings);
                fs.Flush();
                fs.Close();
            }
        }
    }
}
