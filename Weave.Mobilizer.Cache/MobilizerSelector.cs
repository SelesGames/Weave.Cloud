using Weave.Mobilizer.Cache.Readability;

namespace Weave.Mobilizer.Cache
{
    public class MobilizerSelector : IMobilizerStrategy
    {
        ReadabilityClient readabilityClient;

        public MobilizerSelector(ReadabilityClient readabilityClient)
        {
            this.readabilityClient = readabilityClient;
        }

        public IMobilizer Select(string url)
        {
            return readabilityClient;
        }
    }
}
