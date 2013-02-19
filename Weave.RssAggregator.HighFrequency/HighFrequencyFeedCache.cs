using Microsoft.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Weave.RssAggregator.HighFrequency
{
    public class HighFrequencyFeedCache : IDisposable
    {
        Dictionary<string, HighFrequencyFeed> feeds = new Dictionary<string, HighFrequencyFeed>();
        CompositeDisposable disposables = new CompositeDisposable();

        int highFrequencyRefreshSplit;
        TimeSpan highFrequencyRefreshPeriod;




        #region Constructors

        public HighFrequencyFeedCache(string feedLibraryUrl, SequentialProcessor processor)
        {
            if (string.IsNullOrEmpty(feedLibraryUrl)) return;

            var feedClient = new FeedLibraryClient();
            var libraryFeeds = feedClient.GetFeeds(feedLibraryUrl);

            var highFrequencyFeeds = libraryFeeds
                .Distinct()
                .Select(o => new HighFrequencyFeed(o.Name, o.Url))
                .ToList();

            foreach (var feed in highFrequencyFeeds)
            {
                if (processor != null)
                    processor.Register(feed);
            }

            feeds = highFrequencyFeeds.ToDictionary(o => o.FeedUri);
        }

        public HighFrequencyFeedCache(
                                    string feedLibraryUrl,
                                    SequentialProcessor processor, 
                                    int highFrequencyRefreshSplit, 
                                    TimeSpan highFrequencyRefreshPeriod)

            : this(feedLibraryUrl, processor)
        {
            if (string.IsNullOrEmpty(feedLibraryUrl)) return;

            // set some default values
            this.highFrequencyRefreshPeriod = highFrequencyRefreshPeriod;
            this.highFrequencyRefreshSplit = highFrequencyRefreshSplit;
        }

        #endregion




        public void StartFeedRefreshTimer()
        {
            disposables.Clear();

            var feedsList = feeds.Select(o => o.Value).ToList();

            foreach (var feed in feedsList)
                feed.Refresh();

            var indexedHFFeeds = feedsList.Select((hfFeed, i) => new { i, feed = hfFeed }).ToList();

            var fullPeriodInMs = highFrequencyRefreshPeriod.TotalMilliseconds;
            var splitInterval = fullPeriodInMs / (double)highFrequencyRefreshSplit;

            var disp = Observable
                .Interval(TimeSpan.FromMilliseconds(splitInterval))
                .Subscribe(
                    i =>
                    {
                        var bucket = i % highFrequencyRefreshSplit;
                        foreach (var indexedFeedUrl in indexedHFFeeds)
                        {
                            if (indexedFeedUrl.i % highFrequencyRefreshSplit == bucket)
                                indexedFeedUrl.feed.Refresh();
                        }
                    },
                    exception => { ; });

            disposables.Add(disp);
        }

        //public HighFrequencyFeed GetFeedByUrl(string url)
        //{
        //    HighFrequencyFeed feed = null;

        //    if (feeds.ContainsKey(url))
        //    {
        //        feed = feeds[url];
        //    }

        //    return feed;
        //}

        //public FeedResult ToFeedResult(FeedRequest request)
        //{
        //    var feedUrl = request.Url;
        //    if (feeds.ContainsKey(feedUrl))
        //    {
        //        var feed = feeds[feedUrl];
        //        return feed.ToFeedResult(request);
        //    }
        //    else
        //        return new FeedResult { Id = request.Id, Status = FeedResultStatus.Failed };
        //}

        //public async Task DoShit()
        //{
        //    await Task.Delay(20000);

        //    List<HighFrequencyFeed> channelFeeds = new List<HighFrequencyFeed>();
        //    List<string> failedUris = new List<string>();

        //    var sw = System.Diagnostics.Stopwatch.StartNew();
        //    using (var client = new HighFrequencyFeedRetrieverClient())
        //    {
        //        foreach (var feed in feeds.Select(o => o.Value.FeedUri))
        //        {
        //            try
        //            {
        //                var channelFeed = await client.GetFeed(feed);
        //                channelFeeds.Add(channelFeed);
        //            }
        //            catch (Exception ex)
        //            {
        //                DebugEx.WriteLine(ex);
        //                failedUris.Add(feed);
        //            }
        //        }
        //    }
        //    sw.Stop();
        //    DebugEx.WriteLine(channelFeeds.ToString() + sw.ElapsedMilliseconds);
        //    DebugEx.WriteLine(failedUris);
        //}

        //public bool Contains(string feedUrl)
        //{
        //    return feeds.ContainsKey(feedUrl);
        //}

        public void Dispose()
        {
            disposables.Dispose();
            feeds = null;
        }
    }



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
