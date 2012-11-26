using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using Weave.RssAggregator.HighFrequency;

namespace Weave.RssAggregator.WorkerRole.Legacy
{
    public static class WcfEndpointCreator
    {
        public static void CreateEndpoint(string ipAddress, HighFrequencyFeedCache cache)
        {
            var binding = new WebHttpBinding();
            binding.TransferMode = TransferMode.StreamedResponse;

            WebServiceHost clientPolicyHost = new WebServiceHost(new RssAggregator(cache), new Uri(ipAddress));
            clientPolicyHost.AddServiceEndpoint(typeof(IRssAggregator), binding, string.Empty);
            clientPolicyHost.AddServiceEndpoint(typeof(IPing), new WebHttpBinding(), "/Ping");

            clientPolicyHost.Open();
        }
    }
}
