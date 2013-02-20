using Microsoft.ServiceBus;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Weave.RssAggregator.HighFrequency
{
    [ServiceContract(Namespace = "urn:hf")]
    public interface IHighFrequencyFeedRetriever
    {
        [OperationContract]
        Task<HighFrequencyFeed> GetFeed(string feedUrl);
    }

    public interface IHighFrequencyFeedRetrieverChannel : IHighFrequencyFeedRetriever, IClientChannel { }

    public class HighFrequencyFeedRetrieverClient : IHighFrequencyFeedRetriever, IDisposable
    {
        ChannelFactory<IHighFrequencyFeedRetrieverChannel> currentFactory;
        IHighFrequencyFeedRetrieverChannel currentChannel;
        bool isLastStateFaulted = false;

        public async Task<HighFrequencyFeed> GetFeed(string feedUrl)
        {
            EnsureActiveChannel();

            try
            {
                var result = await currentChannel.GetFeed(feedUrl);
                return result;
            }
            catch (Exception e)
            {
                isLastStateFaulted = true;
                throw;
            }
        }

        void EnsureActiveChannel()
        {
            if (currentChannel == null || currentFactory == null || isLastStateFaulted)
            {
                InitializeChannel();
            }
        }

        void InitializeChannel()
        {
            Dispose();

            isLastStateFaulted = false;

            var relayBinding = new NetTcpRelayBinding
            {
                ConnectionMode = TcpRelayConnectionMode.Hybrid
            };
            relayBinding.Security.Mode = EndToEndSecurityMode.None;
            //var relayBinding = new NetTcpRelayBinding();

            currentFactory = new ChannelFactory<IHighFrequencyFeedRetrieverChannel>(
                relayBinding,
                new EndpointAddress(
                    ServiceBusEnvironment.CreateServiceUri("sb", "weave-interop", "hf")));

            //var ipString = "net.tcp://127.0.0.1:9352";

            //currentFactory = new ChannelFactory<IHighFrequencyFeedRetrieverChannel>(
            //    new NetTcpBinding(),
            //    new EndpointAddress(ipString));

            currentFactory.Endpoint.Behaviors.Add(new TransportClientEndpointBehavior
            {
                TokenProvider = TokenProvider.CreateSharedSecretTokenProvider("owner", "R92FFdAujgEDEPnjLhxMfP06fH+qhmMwwuXetdyAEZM=")
            });

            currentChannel = currentFactory.CreateChannel();

            currentChannel.Faulted += OnFaulted;
            currentFactory.Faulted += OnFaulted;
        }

        void OnFaulted(object sender, EventArgs e)
        {
            isLastStateFaulted = true;
        }


        public void Dispose()
        {
            if (currentChannel != null)
            {
                currentChannel.Faulted += OnFaulted;
                try
                {
                    currentChannel.Close();
                    currentChannel.Dispose();
                }
                catch { }
            }

            if (currentFactory != null)
            {
                currentFactory.Faulted += OnFaulted;
                try
                {
                    currentFactory.Close();
                }
                catch { }
            }
        }
    }
}
