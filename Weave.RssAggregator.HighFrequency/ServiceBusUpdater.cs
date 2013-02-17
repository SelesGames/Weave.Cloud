using Common.Azure.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Weave.RssAggregator.Parsing;

namespace Weave.RssAggregator.HighFrequency
{
    public class ServiceBusUpdater : ISequentialAsyncProcessor<Tuple<HighFrequencyFeed, List<Entry>>>
    {
        TopicConnector topicConnector;

        public ServiceBusUpdater(TopicConnector topicConnector)
        {
            this.topicConnector = topicConnector;
        }

        public bool IsHandledFully { get; private set; }

        public async Task ProcessAsync(Tuple<HighFrequencyFeed, List<Entry>> feedTuple)
        {
            var feed = feedTuple.Item1;
            var client = await topicConnector.GetClient();

            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, feed.News);
                ms.Position = 0;
                var message = new BrokeredMessage(ms, false);
                //message.ContentType = "application/protobuf";
                //message.Label = string.Format("{0}: {1}", feed.FeedId, feed.FeedUri);
                message.Properties["FeedId"] = feed.FeedId;
                message.TimeToLive = TimeSpan.FromHours(24);
                await client.SendAsync(message);
                DebugEx.WriteLine("service bus processed: {0}", feed.FeedUri);
            }
        }
    }
}
