using Common.Azure.ServiceBus;
using Common.Azure.ServiceBus.Reactive;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.RssAggregator.LibraryClient;
using System.Reactive.Linq;

namespace Weave.RssAggregator.LowFrequency
{
    public class FeedCache : IDisposable
    {
        Dictionary<string, CachedFeed> feeds = new Dictionary<string, CachedFeed>();
        CompositeDisposable disposables = new CompositeDisposable();

        string feedLibraryUrl;
        DbClient dbClient;
        SubscriptionConnector subscriptionConnector;




        #region Constructor

        public FeedCache(string feedLibraryUrl, DbClient dbClient, SubscriptionConnector subscriptionConnector)
        {
            if (string.IsNullOrEmpty(feedLibraryUrl)) throw new ArgumentException("feedLibraryUrl cannot be null: FeedCache ctor");
            if (dbClient == null) throw new ArgumentNullException("dbClient cannot be null: FeedCache ctor");
            if (subscriptionConnector == null) throw new ArgumentNullException("subscriptionConnector cannot be null: FeedCache ctor");

            this.feedLibraryUrl = feedLibraryUrl;
            this.dbClient = dbClient;
            this.subscriptionConnector = subscriptionConnector;
        }

        #endregion


        FeedUpdateNotice Parse(BrokeredMessage message)
        {
            FeedUpdateNotice notice = null;

            try
            {
                var properties = message.Properties;
                var id = message.MessageId;

                if (!EnumerableEx.IsNullOrEmpty(properties) && 
                    properties.ContainsKey("FeedId") && 
                    properties.ContainsKey("RefreshTime"))
                {
                    var feedId = (Guid)properties["FeedId"];
                    var refreshTime = (DateTime)properties["RefreshTime"];

                    notice = new FeedUpdateNotice(message)
                    {
                        FeedId = feedId,
                        RefreshTime = refreshTime,
                    };
                }
            }
            catch { }

            return notice;
        }

        public async Task InitializeAsync()
        {
            var feedClient = new FeedLibraryClient(feedLibraryUrl);

            await feedClient.LoadFeedsAsync();
            var libraryFeeds = feedClient.Feeds;

            var highFrequencyFeeds = libraryFeeds
                .Distinct()
                .Select(o => new CachedFeed(o.FeedName, o.FeedUri))
                .ToList();

            feeds = highFrequencyFeeds.ToDictionary(o => o.FeedUri);

            var mediators = highFrequencyFeeds.Select(o => new HFeedDbMediator(dbClient, o));
            var client = await subscriptionConnector.CreateClient();
            var observable = client.AsObservable();//.Select(Parse).OfType<FeedUpdateNotice>();

//            var options = new OnMessageOptions
//            {
//                AutoComplete = false,               // Indicates if the message pump should call Complete() on messages after the callback has completed processing.
//                MaxConcurrentCalls = 1,             // Indicates the maximum number of concurrent calls to the callback the pump should initiate. 
//            };

//            options.ExceptionReceived += LogErrors; // Enables notification of any errors encountered by the message pump.

//            // Start receiving messages
//            client.OnMessageAsync(async receivedMessage => // Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
//            {
//#if DEBUG
//                System.Diagnostics.Trace.WriteLine("Processing", receivedMessage.SequenceNumber.ToString());
//#endif

//                // Process the message
//                foreach (var mediator in mediators)
//                {
//                    await mediator.OnBrokeredMessageUpdateReceived(receivedMessage);
//                }
//            }, options);


            foreach (var mediator in mediators)
            {
                await mediator.LoadLatestNews();
                mediator.Subscribe(observable);
            }
        }

        //void LogErrors(object sender, ExceptionReceivedEventArgs e)
        //{
        //    if (e != null && e.Exception != null)
        //        System.Diagnostics.Trace.WriteLine(e.Exception.Message);
        //}

        public FeedResult ToFeedResult(FeedRequest request)
        {
            var feedUrl = request.Url;
            if (feeds.ContainsKey(feedUrl))
            {
                var feed = feeds[feedUrl];
                return feed.ToFeedResult(request);
            }
            else
                return new FeedResult { Id = request.Id, Status = FeedResultStatus.Failed };
        }

        public bool ContainsValid(string feedUrl)
        {
            if (feeds.ContainsKey(feedUrl))
            {
                var feed = feeds[feedUrl];
                if (feed.LastFeedState != CachedFeed.FeedState.Uninitialized)
                    return true;
            }

            return false;
        }

        public CachedFeed Get(string feedUrl)
        {
            return feeds[feedUrl];
        }

        public void Dispose()
        {
            disposables.Dispose();
            feeds = null;
        }
    }
}
