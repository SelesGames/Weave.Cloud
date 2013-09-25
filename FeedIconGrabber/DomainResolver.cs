using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Parsing;

namespace FeedIconGrabber
{
    /// <summary>
    /// Figures out the domain for any given RSS url
    /// </summary>
    public class DomainResolver
    {
        public async Task<string> GetDomainUrl(string rssUrl)
        {
            var feed = new Feed
            {
                IsAggressiveDomainDiscoveryEnabled = true,
                FeedUri = rssUrl
            };
            await feed.Update();
            return feed.DomainUrl;
        }
    }
}
