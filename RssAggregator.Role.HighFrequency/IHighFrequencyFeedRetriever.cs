using System.ServiceModel;
using Weave.RssAggregator.HighFrequency;

namespace RssAggregator.Role.HighFrequency
{
    [ServiceContract(Namespace = "urn:hf")]
    public interface IHighFrequencyFeedRetriever
    {
        [OperationContract]
        HighFrequencyFeed GetFeed(string feedUrl);
    }

    public interface IHighFrequencyFeedRetrieverChannel : IHighFrequencyFeedRetriever, IClientChannel { }

    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class HighFrequencyFeedRetriever : IHighFrequencyFeedRetriever
    {
        HighFrequencyFeedCache cache;

        public HighFrequencyFeedRetriever(HighFrequencyFeedCache cache)
        {
            this.cache = cache;
        }

        public HighFrequencyFeed GetFeed(string feedUrl)
        {
            return cache.GetFeedByUrl(feedUrl);
        }
    }
}
