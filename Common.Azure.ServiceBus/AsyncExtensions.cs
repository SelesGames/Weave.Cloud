using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
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

        #endregion
    }
}
