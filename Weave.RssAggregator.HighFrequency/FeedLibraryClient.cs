using SelesGames.Rest;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Weave.RssAggregator.HighFrequency
{
    public class FeedLibraryClient
    {
        public IEnumerable<Feed> GetFeeds(string feedLibraryUrl)
        {
            var client = new LinqToXmlRestClient<List<Feed>> { UseGzip = true };
            var task = client.GetAndParseAsync(
                feedLibraryUrl, 
                doc => doc.Descendants("Feed").Select(Parse).ToList(), 
                System.Threading.CancellationToken.None);

            task.Wait();

            var xmlFeeds = task.Result;
            //var doc = XDocument.Load(feedLibraryUrl);

            //var xmlFeeds = doc.Descendants("Feed")
            //    .Select(Parse)
            //    .ToList()
            //    .Distinct()
            //    .ToList();

            return xmlFeeds;
        }

        Feed Parse(XElement element)
        {
            var name = element.Attribute("Name").ValueOrDefault();
            var url = element.ValueOrDefault();

            bool suppressDescription = false;
            var t = element.Attribute("sd").ValueOrDefault();
            bool.TryParse(t, out suppressDescription);

            return new Feed { Name = name, Url = url };
        }
    }

    public class Feed
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
