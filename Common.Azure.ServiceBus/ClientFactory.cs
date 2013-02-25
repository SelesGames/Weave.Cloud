using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading.Tasks;

namespace Common.Azure.ServiceBus
{
    public class ClientFactory
    {
        ServiceBusCredentials credentials;
        NamespaceManager namespaceManager;

        public ClientFactory(ServiceBusCredentials credentials)
        {
            this.credentials = credentials;
            Initialize();
        }

        void Initialize()
        {
            // Using Http to be friendly with outbound firewalls
            ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;

            // Create the namespace manager which gives you access to 
            // management operations
            namespaceManager = credentials.CreateNamespaceManager("sb", string.Empty);
        }

        public async Task<TopicClient> CreateTopicClient(string topicPath)
        {
            // Create the topic if it does not exist already
            if (!await namespaceManager.TopicExistsAsync(topicPath))
            {
                await namespaceManager.CreateTopicAsync(topicPath);
            }

            // Get a client to the queue
            return CreateMessagingFactory().CreateTopicClient(topicPath);
        }

        public Task<SubscriptionClient> CreateSubscriptionClient(string topicPath, string subscriptionName, SubscriptionDescription description, ReceiveMode receiveMode = ReceiveMode.PeekLock)
        {
            return CreateSubscriptionClient(topicPath, subscriptionName, nm => nm.CreateSubscriptionAsync(description), receiveMode);
        }

        public Task<SubscriptionClient> CreateSubscriptionClient(string topicPath, string subscriptionName, ReceiveMode receiveMode = ReceiveMode.PeekLock)
        {
            return CreateSubscriptionClient(topicPath, subscriptionName, nm => nm.CreateSubscriptionAsync(topicPath, subscriptionName), receiveMode);
        }

        public Task<SubscriptionClient> CreateSubscriptionClient(string topicPath, string subscriptionName, SubscriptionDescription description, Filter filter, ReceiveMode receiveMode = ReceiveMode.PeekLock)
        {
            return CreateSubscriptionClient(topicPath, subscriptionName, nm => nm.CreateSubscriptionAsync(description, filter), receiveMode);
        }

        public Task<SubscriptionClient> CreateSubscriptionClient(string topicPath, string subscriptionName, SubscriptionDescription description, RuleDescription ruleDescription, ReceiveMode receiveMode = ReceiveMode.PeekLock)
        {
            return CreateSubscriptionClient(topicPath, subscriptionName, nm => nm.CreateSubscriptionAsync(description, ruleDescription), receiveMode);
        }
        
        public Task<SubscriptionClient> CreateSubscriptionClient(string topicPath, string subscriptionName, Filter filter, ReceiveMode receiveMode = ReceiveMode.PeekLock)
        {
            return CreateSubscriptionClient(topicPath, subscriptionName, nm => nm.CreateSubscriptionAsync(topicPath, subscriptionName, filter), receiveMode);
        }

        public Task<SubscriptionClient> CreateSubscriptionClient(string topicPath, string subscriptionName, RuleDescription ruleDescription, ReceiveMode receiveMode = ReceiveMode.PeekLock)
        {
            return CreateSubscriptionClient(topicPath, subscriptionName, nm => nm.CreateSubscriptionAsync(topicPath, subscriptionName, ruleDescription), receiveMode);
        }




        #region private helper methods

        async Task<SubscriptionClient> CreateSubscriptionClient(
            string topicPath, 
            string subscriptionName, 
            Func<NamespaceManager, Task<SubscriptionDescription>> subscriptionCreator,
            ReceiveMode receiveMode = ReceiveMode.PeekLock)
        {
            // Create the topic if it does not exist already
            if (!await namespaceManager.TopicExistsAsync(topicPath))
            {
                await namespaceManager.CreateTopicAsync(topicPath);
            }

            // Create the subscription if it does not exist already
            if (!await namespaceManager.SubscriptionExistsAsync(topicPath, subscriptionName))
            {
                await subscriptionCreator(namespaceManager);
            }

            // Get a client to the queue
            return CreateMessagingFactory().CreateSubscriptionClient(topicPath, subscriptionName, receiveMode);
        }

        MessagingFactory CreateMessagingFactory()
        {
            return MessagingFactory.Create(
                namespaceManager.Address,
                namespaceManager.Settings.TokenProvider);
        }

        #endregion
    }
}
