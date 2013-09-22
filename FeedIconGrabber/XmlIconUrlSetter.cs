using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Weave.RssAggregator.LibraryClient;


namespace FeedIconGrabber
{
    public class XmlIconUrlSetter
    {
        readonly string AzurePath = "http://weave.blob.core.windows.net/icons/";
        readonly string PATH = @"C:\WORK\CODE\SELES GAMES\icons";
        readonly string FEEDS_FILEPATH = @"C:\WORK\CODE\SELES GAMES\Weave.Cloud\masterfeeds.xml";

        public async Task RewriteXml()
        {
            var feedLibraryClient = new FeedLibraryClient(FEEDS_FILEPATH);
            await feedLibraryClient.LoadFeedsAsync();

            var xml = feedLibraryClient.Xml;
            var feeds = xml.Descendants("Feed");
            var existingFiles = Directory.EnumerateFiles(PATH).Select(o => o.Replace(PATH + "\\", null)).ToList();

            foreach (var feed in feeds)
            {
                var feedName = feed.Attribute("Name").Value;

                var matchingFile = existingFiles.FirstOrDefault(o => o.StartsWith(feedName, StringComparison.OrdinalIgnoreCase));
                if (matchingFile == null)
                    continue;

                feed.Add(new XAttribute("IconUrl", AzurePath + matchingFile));
            }

            xml.Save(FEEDS_FILEPATH, SaveOptions.DisableFormatting);
        }
    }
}
