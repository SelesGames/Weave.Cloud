﻿using Common.Caching;
using System.Threading.Tasks;
using Weave.Mobilizer.DTOs;
using Weave.Readability;

namespace Weave.Mobilizer.Cache
{
    public class ReadabilityCache : IBasicCache<string, Task<MobilizerResult>>
    {
        NLevelCache<string, Task<MobilizerResult>> cache;
        ReadabilityClient readabilityClient;

        public ReadabilityCache(ReadabilityClient readabilityClient, params IExtendedCache<string, Task<MobilizerResult>>[] caches)
        {
            this.readabilityClient = readabilityClient;
            this.cache = new NLevelCache<string, Task<MobilizerResult>>(caches);
        }

        public Task<MobilizerResult> Get(string key)
        {
            return cache.GetOrAdd(key, GetFromReadability);
        }

        async Task<MobilizerResult> GetFromReadability(string url)
        {
            var result = await readabilityClient.GetAsync(url);

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