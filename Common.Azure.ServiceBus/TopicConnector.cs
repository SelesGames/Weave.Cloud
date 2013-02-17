using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System.Threading.Tasks;

namespace Common.Azure.ServiceBus
{
    public class TopicConnector
    {
        // Thread-safe. Recommended that you cache rather than recreating it
        // on every request.
        TopicClient topicClient;
        ServiceBusCredentials credentials;
        string topicName;
        bool isClientInitialized = false;

        
        public TopicConnector(ServiceBusCredentials credentials, string topicName)
        {
            this.credentials = credentials;
            this.topicName = topicName;
        }

        public async Task<TopicClient> GetClient()
        {
            if (!isClientInitialized)
                await Initialize();

            return topicClient;
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


            // Get a client to the queue
            var messagingFactory = MessagingFactory.Create(
                namespaceManager.Address,
                namespaceManager.Settings.TokenProvider);

            topicClient = messagingFactory.CreateTopicClient(topicName);

            isClientInitialized = true;
        }
    }
}
