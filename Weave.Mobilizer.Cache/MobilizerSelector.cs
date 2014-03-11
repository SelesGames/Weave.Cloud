using System;
using System.Collections.Generic;
using System.Linq;
using Weave.Mobilizer.Cache.Readability;
using Weave.Mobilizer.Cache.ReadSharp;

namespace Weave.Mobilizer.Cache
{
    public class MobilizerSelector : IMobilizerStrategy
    {
        readonly ReadabilityClient readabilityClient;
        readonly ReadSharpClient readSharpClient;

        readonly List<string> readSharpHostList = new List<string>
{
    "www.wpcentral.com",
};

        public MobilizerSelector(ReadabilityClient readabilityClient)
        {
            this.readabilityClient = readabilityClient;
            this.readSharpClient = new ReadSharpClient();
        }

        public IMobilizer Select(string url)
        {
            var unescapedUrl = Uri.UnescapeDataString(url);
            var asUri = new Uri(unescapedUrl);
            var host = asUri.DnsSafeHost;

            if (readSharpHostList.Any(o => host.StartsWith(o, StringComparison.OrdinalIgnoreCase)))
            {
                return readSharpClient;
            }
            else
            {
                return readabilityClient;
            }
        }
    }
}
