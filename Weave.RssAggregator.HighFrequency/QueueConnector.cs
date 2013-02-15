using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;

namespace Weave.RssAggregator.HighFrequency
{
    public static class QueueConnector
    {
        // Thread-safe. Recommended that you cache rather than recreating it
        // on every request.
        public static QueueClient OrdersQueueClient;


        // Obtain these values from the Management Portal
        public const string Namespace = "weave-interop";
        public const string IssuerName = "owner";
        public const string IssuerKey = "R92FFdAujgEDEPnjLhxMfP06fH+qhmMwwuXetdyAEZM=";


        // The name of your queue
        public const string QueueName = "FeedArticleListUpdateQueue";


        public static NamespaceManager CreateNamespaceManager()
        {
            // Create the namespace manager which gives you access to
            // management operations
            var uri = ServiceBusEnvironment.CreateServiceUri("sb", Namespace, String.Empty);
            var tP = TokenProvider.CreateSharedSecretTokenProvider(IssuerName, IssuerKey);
            return new NamespaceManager(uri, tP);
        }


        public static void Initialize()
        {
            // Using Http to be friendly with outbound firewalls
            ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;


            // Create the namespace manager which gives you access to 
            // management operations
            var namespaceManager = CreateNamespaceManager();


            // Create the queue if it does not exist already
            if (!namespaceManager.QueueExists(QueueName))
            {
                namespaceManager.CreateQueue(QueueName);
            }


            // Get a client to the queue
            var messagingFactory = MessagingFactory.Create(
                namespaceManager.Address,
                namespaceManager.Settings.TokenProvider);

            OrdersQueueClient = messagingFactory.CreateQueueClient(QueueName);
        }
    }
}
