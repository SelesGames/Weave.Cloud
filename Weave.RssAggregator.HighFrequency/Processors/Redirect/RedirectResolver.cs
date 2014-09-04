using SelesGames.HttpClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;

namespace Weave.RssAggregator.HighFrequency
{
    public class RedirectResolver : IAsyncProcessor<HighFrequencyFeedUpdate>
    {
        static TimeSpan REDIRECT_TIMEOUT = TimeSpan.FromSeconds(10);

        public async Task ProcessAsync(HighFrequencyFeedUpdate o)
        {
            if (o == null || EnumerableEx.IsNullOrEmpty(o.Entries))
                return;

            try
            {
                await Task.WhenAll(o.Entries.Select(ProcessEntry));
            }
            catch { }
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
                }
                DebugEx.WriteLine("Fixed redirect for {0}", e.Link);
            }
            catch(Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
        }
    }
}
