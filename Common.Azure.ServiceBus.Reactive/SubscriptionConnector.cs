using Microsoft.ServiceBus.Messaging;
using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Common.Azure.ServiceBus.Reactive
{
    public class SubscriptionConnector
    {
        ClientFactory factory;
        string topicPath, subscriptionName;
        ReceiveMode receiveMode;

        public SubscriptionConnector(ClientFactory factory, string topicPath, string subscriptionName, ReceiveMode receiveMode = ReceiveMode.PeekLock)
        {
            this.factory = factory;
            this.topicPath = topicPath;
            this.subscriptionName = subscriptionName;
            this.receiveMode = receiveMode;
        }

        public Task<SubscriptionClient> CreateClient()
        {
            return factory.CreateSubscriptionClient(topicPath, subscriptionName, receiveMode);
        }
    }
}
