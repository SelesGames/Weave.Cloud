using Common.Caching;
using System.Threading.Tasks;
using Weave.Mobilizer.DTOs;

namespace Weave.Mobilizer.Cache
{
    public class MobilizerResultCache : IBasicCache<string, Task<MobilizerResult>>
    {
        NLevelCache<string, Task<MobilizerResult>> cache;
        IMobilizerStrategy mobilizerStrategy;

        public MobilizerResultCache(IMobilizerStrategy mobilizerStrategy, params IExtendedCache<string, Task<MobilizerResult>>[] caches)
        {
            this.mobilizerStrategy = mobilizerStrategy;
            this.cache = new NLevelCache<string, Task<MobilizerResult>>(caches);
        }

        public Task<MobilizerResult> Get(string key)
        {
            return cache.GetOrAdd(key, GetFromMobilizer);
        }

        async Task<MobilizerResult> GetFromMobilizer(string url)
        {
            var mobilizer = mobilizerStrategy.Select(url);
            var result = await mobilizer.Mobilize(url);

            // parse out the lead_image_url from the article content - clients should figure out their own way to display the lead_image_url
            //var sw = System.Diagnostics.Stopwatch.StartNew();
            var parser = new Weave.Mobilizer.HtmlParser.Parser();
            parser.ProcessContent(result);
            //sw.Stop();
            //System.Diagnostics.Debug.WriteLine(string.Format("elapsed time: {0} ms", sw.ElapsedMilliseconds));
            return result;
        }
    }
}