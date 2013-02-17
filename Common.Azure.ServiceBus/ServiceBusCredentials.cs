using Microsoft.ServiceBus;

namespace Common.Azure.ServiceBus
{
    public class ServiceBusCredentials
    {
        public string Namespace { get; set; }
        public string IssuerName { get; set; }
        public string IssuerKey { get; set; }
    }

    public static class ServiceBusCredentialsExtensions
    {
        public static NamespaceManager CreateNamespaceManager(this ServiceBusCredentials c, string schema = "sb", string servicePath = "")
        {
            // Create the namespace manager which gives you access to
            // management operations
            var uri = ServiceBusEnvironment.CreateServiceUri(schema, c.Namespace, servicePath);
            var tP = TokenProvider.CreateSharedSecretTokenProvider(c.IssuerName, c.IssuerKey);
            return new NamespaceManager(uri, tP);
        }
    }
}
