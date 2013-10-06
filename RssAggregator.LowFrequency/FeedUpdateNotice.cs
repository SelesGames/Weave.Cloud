using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading.Tasks;

namespace Weave.RssAggregator.LowFrequency
{
    public class FeedUpdateNotice
    {
        BrokeredMessage message;

        public FeedUpdateNotice(BrokeredMessage message)
        {
            this.message = message;
        }

        public string MessageId { get; set; }
        public Guid FeedId { get; set; }
        public string FeedUri { get; set; }
        public DateTime RefreshTime { get; set; }

        public Task MarkNoticeAsRead()
        {
            return message.CompleteAsync();
        }
    }
}
