using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System.Threading.Tasks;

namespace Common.Azure.ServiceBus
{
    public class SubscriptionConnector
    {        
        // Thread-safe. Recommended that you cache rather than recreating it
        // on every request.
        SubscriptionClient subscriptionClient;
        ServiceBusCredentials credentials;
        string topicName, subscriptionName;
        ReceiveMode receiveMode;
        bool isClientInitialized = false;


        public SubscriptionConnector(ServiceBusCredentials credentials, string topicName, string subscriptionName, ReceiveMode receiveMode = ReceiveMode.PeekLock)
        {
            this.credentials = credentials;
            this.topicName = topicName;
            this.subscriptionName = subscriptionName;
            this.receiveMode = receiveMode;
        }

        public async Task<SubscriptionClient> GetClient()
        {
            if (!isClientInitialized)
                await Initialize();

            return subscriptionClient;
        }

        async Task Initialize()
        {
            // Using Http to be friendly with outbound firewalls
            ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;


            // Create the namespace manager which gives you access to 
            // management operations
            var namespaceManager = credentials.CreateNamespaceManager("sb", string.Empty);


            // Create the queue if it does not exist already
            if (!await namespaceManager.TopicExistsAsync(topicName))
            {
                await namespaceManager.CreateTopicAsync(topicName);
            }

            if (!await namespaceManager.SubscriptionExistsAsync(topicName, subscriptionName))
            {
                namespaceManager.CreateSubscription
            }


            // Get a client to the queue
            var messagingFactory = MessagingFactory.Create(
                namespaceManager.Address,
                namespaceManager.Settings.TokenProvider);

            subscriptionClient = messagingFactory.CreateSubscriptionClient(topicName, subscriptionName, receiveMode);

            isClientInitialized = true;
        }
    }
}
