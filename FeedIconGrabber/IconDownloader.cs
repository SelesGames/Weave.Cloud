using SelesGames.HttpClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Parsing;
using Weave.RssAggregator.LibraryClient;

namespace FeedIconGrabber
{
    public class IconDownloader
    {
        readonly string PATH = @"C:\WORK\CODE\SELES GAMES\icons";

        public async Task BeginDownload()
        {
            var feedLibraryClient = new FeedLibraryClient(@"C:\WORK\CODE\SELES GAMES\Weave.Cloud\masterfeeds.xml");
            await feedLibraryClient.LoadFeedsAsync();

            var feeds = feedLibraryClient.Feeds
                //.Where(o => o.Category.Equals("technology", StringComparison.OrdinalIgnoreCase))
                .Select(o =>
                    new FeedWithName
                    {
                        Feed = new Feed
                        {
                            FeedUri = o.FeedUri,
                            IsAggressiveDomainDiscoveryEnabled = true
                        },
                        Name = o.FeedName
                    }
                )
                .ToList();


            var existingFiles = Directory.EnumerateFiles(PATH).Select(o => o.Replace(PATH + "\\", null)).ToList();

            foreach (var feed in feeds)
            {
                if (existingFiles.Any(o => o.StartsWith(feed.Name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                await TryDownloadingFeed(feed);
            }
        }

        async Task TryDownloadingFeed(FeedWithName feed)
        {
            try
            {
                await feed.Feed.Update();
                var grabber = new DomainIconAcquirer(feed.Feed.DomainUrl);
                var iconUrl = await grabber.GetIconUrl();

                var client = new SmartHttpClient();

                var extension = Path.GetExtension(iconUrl);

                var response = await client.GetAsync(iconUrl);
                if (string.IsNullOrEmpty(extension))
                {
                    var mimeType = response.HttpResponseMessage.Content.Headers.ContentType.MediaType;
                    extension = GetImageExtensionFromMimeType(mimeType);
                }

                string fileName = string.Format("{0}\\{1}{2}", PATH, feed.Name, extension);

                using (var fs = new FileStream(fileName, FileMode.Create))
                {
                    await response.HttpResponseMessage.Content.CopyToAsync(fs);
                    fs.Flush();
                    fs.Close();
                }
            }
            catch { }
        }

        static string GetImageExtensionFromMimeType(string mimeType)
        {
            string extension = null;

            if (mimeType.Equals("image/png", StringComparison.OrdinalIgnoreCase))
                extension = ".png";
            else if (mimeType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase))
                extension = ".jpg";
            else if (mimeType.Equals("image/x-icon", StringComparison.OrdinalIgnoreCase))
                extension = ".ico";
            else if (mimeType.Equals("image/vnd.microsoft.icon", StringComparison.OrdinalIgnoreCase))
                extension = ".ico";

            return extension;
        }

        class FeedWithName
        {
            public Feed Feed { get; set; }
            public string Name { get; set; }
        }
    }
}
