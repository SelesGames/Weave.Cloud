using System;
using System.Collections.Generic;
using System.ServiceModel.Web;
using Weave.RssAggregator.Core.Services.HighFrequency;
using System.ServiceModel;
using System.Text;

namespace Weave.RssAggregator.Core
{
    public static class WcfEndpointCreator
    {
        public static void CreateEndpoint(string ipAddress)
        {
            var binding = new WebHttpBinding();
            binding.TransferMode = TransferMode.StreamedResponse;

            WebServiceHost clientPolicyHost = new WebServiceHost(new RssAggregator(), new Uri(ipAddress));
            clientPolicyHost.AddServiceEndpoint(typeof(IRssAggregator), binding, string.Empty);
            clientPolicyHost.AddServiceEndpoint(typeof(IPing), new WebHttpBinding(), "/Ping");

           // ServiceHost host = new ServiceHost(new RssAggregator(), new Uri(ipAddress));
            //host.AddServiceEndpoint(typeof(IRssAggregator), binding, string.Empty);
            clientPolicyHost.Open();
            //host.Open();
        }
    }
}
