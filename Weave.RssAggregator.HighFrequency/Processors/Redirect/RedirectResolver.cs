using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Weave.RssAggregator.HighFrequency
{
    public class RedirectResolver : ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>
    {
        public bool IsHandledFully { get { return false; } }

        public async Task ProcessAsync(HighFrequencyFeedUpdateDto o)
        {
            //return Task.WhenAll(o.Entries.Select(ProcessEntry));
            if (o == null || EnumerableEx.IsNullOrEmpty(o.Entries))
                return;

            try
            {
                await Task.WhenAll(o.Entries.Select(ProcessEntry));
            }
            catch { }
        }

        async Task ProcessEntry(EntryWithPostProcessInfo e)
        {
            try
            {
                var link = e.Link;
                var finalLinkLocation = await SelesGames.HttpClient.UrlHelper.GetFinalRedirectLocation(link);
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
