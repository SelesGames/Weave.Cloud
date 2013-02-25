using Common.Azure.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Weave.RssAggregator.HighFrequency
{
    public class ServiceBusUpdater : ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>
    {
        TopicConnector topicConnector;

        public ServiceBusUpdater(TopicConnector topicConnector)
        {
            this.topicConnector = topicConnector;
        }

        public bool IsHandledFully { get; private set; }

        public async Task ProcessAsync(HighFrequencyFeedUpdateDto update)
        {
            var client = await topicConnector.GetClient();

            using (var ms = new MemoryStream())
            {
                var message = new BrokeredMessage();
                message.Properties["FeedId"] = update.FeedId;
                message.Properties["RefreshTime"] = update.RefreshTime;
                message.Properties["FeedUri"] = update.FeedUri;
                message.TimeToLive = TimeSpan.FromMinutes(15);
                await client.SendAsync(message);
                DebugEx.WriteLine("** SERVICE BUS ** processed: {0}", update.FeedUri);
            }
        }
    }
}
