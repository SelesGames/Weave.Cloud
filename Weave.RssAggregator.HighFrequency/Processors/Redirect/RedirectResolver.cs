using SelesGames.HttpClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;

namespace Weave.FeedUpdater.HighFrequency
{
    public class RedirectResolver : IAsyncProcessor<HighFrequencyFeedUpdate>
    {
        static TimeSpan REDIRECT_TIMEOUT = TimeSpan.FromSeconds(10);

        public async Task ProcessAsync(HighFrequencyFeedUpdate o)
        {
            if (o == null || EnumerableEx.IsNullOrEmpty(o.Entries))
                return;

            await Task.WhenAll(o.Entries.Select(ProcessEntry));
        }

        static async Task ProcessEntry(ExpandedEntry e)
        {
            try
            {
                var link = e.Link;
                var finalLinkLocation = await UrlHelper.GetFinalRedirectLocation(link, REDIRECT_TIMEOUT);
                if (link != finalLinkLocation)
                {
                    e.Link = finalLinkLocation;
                    DebugEx.WriteLine("FIXED REDIRECT\r\n: original: {0}\r\n new {1}\r\n\r\n", link, e.Link);
                }
            }
            catch(Exception ex)
            {
                DebugEx.WriteLine("Failed to find redirect for {0}\r\n{1}\r\n", e.Link, ex.ToString());
            }
        }
    }
}
