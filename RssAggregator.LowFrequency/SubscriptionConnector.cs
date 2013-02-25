using Common.Azure.ServiceBus;
using Common.Azure.ServiceBus.Reactive;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Weave.RssAggregator.LowFrequency
{
    public class SubscriptionConnector
    {
        ClientFactory factory;
        string topicPath;

        public SubscriptionConnector(ClientFactory factory, string topicPath)
        {
            this.factory = factory;
            this.topicPath = topicPath;
        }

        public Task<SubscriptionClient> CreateClient()
        {
            var subscriptionName = "test";
            return factory.CreateSubscriptionClient(topicPath, subscriptionName);
        }

        public async Task<IObservable<BrokeredMessage>> CreateObservable()
        {
            var client = await CreateClient();

            var sub = new Subject<BrokeredMessage>();

            client.AsObservable().Subscribe(sub.OnNext, sub.OnError, sub.OnCompleted);

            return sub.AsObservable();
        }
    }
}
