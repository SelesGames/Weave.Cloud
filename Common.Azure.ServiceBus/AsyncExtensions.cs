using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Azure.ServiceBus
{
    public static class AsyncExtensions
    {
        #region TopicClient extensions

        public static Task SendAsync(this TopicClient client, BrokeredMessage message)
        {
            return Task.Factory.FromAsync(client.BeginSend, client.EndSend, message, null);
        }

        public static Task SendBatchAsync(this TopicClient client, IEnumerable<BrokeredMessage> messages)
        {
            return Task.Factory.FromAsync(client.BeginSendBatch, client.EndSendBatch, messages, null);
        }

        #endregion




        #region QueueClient extensions

        public static Task SendAsync(this QueueClient client, BrokeredMessage message)
        {
            return Task.Factory.FromAsync(client.BeginSend, client.EndSend, message, null);
        }

        public static Task SendBatchAsync(this QueueClient client, IEnumerable<BrokeredMessage> messages)
        {
            return Task.Factory.FromAsync(client.BeginSendBatch, client.EndSendBatch, messages, null);
        }

        #endregion




        #region NamespaceManager extensions

        public static Task<bool> TopicExistsAsync(this NamespaceManager namespaceManager, string path)
        {
            return Task.Factory.FromAsync<string, bool>(namespaceManager.BeginTopicExists, namespaceManager.EndTopicExists, path, null);
        }

        public static Task<TopicDescription> CreateTopicAsync(this NamespaceManager namespaceManager, string path)
        {
            return Task.Factory.FromAsync<string, TopicDescription>(namespaceManager.BeginCreateTopic, namespaceManager.EndCreateTopic, path, null);
        }

        public static Task<bool> QueueExistsAsync(this NamespaceManager namespaceManager, string path)
        {
            return Task.Factory.FromAsync<string, bool>(namespaceManager.BeginQueueExists, namespaceManager.EndQueueExists, path, null);
        }

        public static Task<QueueDescription> CreateQueueAsync(this NamespaceManager namespaceManager, string path)
        {
            return Task.Factory.FromAsync<string, QueueDescription>(namespaceManager.BeginCreateQueue, namespaceManager.EndCreateQueue, path, null);
        }

        public static Task<bool> SubscriptionExistsAsync(this NamespaceManager namespaceManager, string topicPath, string subscriptionName)
        {
            return Task.Factory.FromAsync<string, string, bool>(namespaceManager.BeginSubscriptionExists, namespaceManager.EndSubscriptionExists, topicPath, subscriptionName, null);
        }

        public static Task<SubscriptionDescription> CreateSubscriptionAsync(this NamespaceManager namespaceManager, SubscriptionDescription description)
        {
            return Task.Factory.FromAsync<SubscriptionDescription, SubscriptionDescription>(namespaceManager.BeginCreateSubscription, namespaceManager.EndCreateSubscription, description, null);
        }

        public static Task<SubscriptionDescription> CreateSubscriptionAsync(this NamespaceManager namespaceManager, string topicPath, string subscriptionName)
        {
            return Task.Factory.FromAsync<string, string, SubscriptionDescription>(namespaceManager.BeginCreateSubscription, namespaceManager.EndCreateSubscription, topicPath, subscriptionName, null);
        }

        public static Task<SubscriptionDescription> CreateSubscriptionAsync(this NamespaceManager namespaceManager, SubscriptionDescription description, Filter filter)
        {
            return Task.Factory.FromAsync<SubscriptionDescription, Filter, SubscriptionDescription>(namespaceManager.BeginCreateSubscription, namespaceManager.EndCreateSubscription, description, filter, null);
        }

        public static Task<SubscriptionDescription> CreateSubscriptionAsync(this NamespaceManager namespaceManager, SubscriptionDescription description, RuleDescription ruleDescription)
        {
            return Task.Factory.FromAsync<SubscriptionDescription, RuleDescription, SubscriptionDescription>(namespaceManager.BeginCreateSubscription, namespaceManager.EndCreateSubscription, description, ruleDescription, null);
        }

        public static Task<SubscriptionDescription> CreateSubscriptionAsync(this NamespaceManager namespaceManager, string topicPath, string subscriptionName, Filter filter)
        {
            return Task.Factory.FromAsync<string, string, Filter, SubscriptionDescription>(namespaceManager.BeginCreateSubscription, namespaceManager.EndCreateSubscription, topicPath, subscriptionName, filter, null);
        }

        public static Task<SubscriptionDescription> CreateSubscriptionAsync(this NamespaceManager namespaceManager, string topicPath, string subscriptionName, RuleDescription ruleDescription)
        {
            return Task.Factory.FromAsync<string, string, RuleDescription, SubscriptionDescription>(namespaceManager.BeginCreateSubscription, namespaceManager.EndCreateSubscription, topicPath, subscriptionName, ruleDescription, null);
        }

        #endregion




        #region SubscriptionClient extensions

        public static Task<BrokeredMessage> ReceiveAsync(this SubscriptionClient client)
        {
            return Task.Factory.FromAsync<BrokeredMessage>(client.BeginReceive, client.EndReceive, null);
        }

        public static Task<BrokeredMessage> ReceiveAsync(this SubscriptionClient client, long sequenceNumber)
        {
            return Task.Factory.FromAsync<long, BrokeredMessage>(client.BeginReceive, client.EndReceive, sequenceNumber, null);
        }

        public static Task<BrokeredMessage> ReceiveAsync(this SubscriptionClient client, TimeSpan serverWaitTime)
        {
            return Task.Factory.FromAsync<TimeSpan, BrokeredMessage>(client.BeginReceive, client.EndReceive, serverWaitTime, null);
        }

        #endregion




        #region BrokeredMessage extensions

        public static Task CompleteAsync(this BrokeredMessage message)
        {
            return Task.Factory.FromAsync(message.BeginComplete, message.EndComplete, null);
        }

        #endregion
    }
}
