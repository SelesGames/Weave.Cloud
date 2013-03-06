using Microsoft.ServiceBus.Messaging;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Common.Azure.ServiceBus.Reactive
{
    public class SubscriptionConnector
    {
        ClientFactory factory;
        string topicPath, subscriptionName;

        public SubscriptionConnector(ClientFactory factory, string topicPath, string subscriptionName)
        {
            this.factory = factory;
            this.topicPath = topicPath;
            this.subscriptionName = subscriptionName;
        }

        public async Task<IObservable<BrokeredMessage>> CreateObservable()
        {
            var client = await CreateClient();

            var sub = new Subject<BrokeredMessage>();

            client.AsObservable().Subscribe(sub.OnNext, sub.OnError, sub.OnCompleted);

            return sub.AsObservable();
        }

        Task<SubscriptionClient> CreateClient()
        {
            return factory.CreateSubscriptionClient(topicPath, subscriptionName);
        }
    }
}
