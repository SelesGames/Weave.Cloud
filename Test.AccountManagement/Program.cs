using Ninject;
using ProtoBuf;
using SelesGames.HttpClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Weave.AccountManagement.DTOs;
using Weave.AccountManagement.WebRole.Startup;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Test.AccountManagement
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //TestSmartHttpClient().Wait();
                TestUserCreationRetrievalEtc().Wait();
                Console.WriteLine("all tests passed!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            while (true)
                Console.ReadLine();
        }

        static async Task TestSmartHttpClient()
        {
            var client = new SmartHttpClient(ContentEncoderSettings.Json, CompressionSettings.None);

            var result = await client.PostAsync<List<FeedRequest>, List<FeedResult>>("http://weave2.cloudapp.net/api/Weave", new List<FeedRequest>
            {
                new FeedRequest { Id="1", Url = "http://www.theverge.com/rss/index.xml" }
            });
            DebugEx.WriteLine(result);
        }

        static async Task TestUserCreationRetrievalEtc()
        {
            var feeds = new MockFeeds();

            var kernel = new NinjectKernel();
            var manager = kernel.Get<UserManager>();

            var id = Guid.Parse("6bfb9049-5c48-47d0-95c6-da97e7922bf6");

            //var id = Guid.NewGuid();

            //var user = new UserInfo
            //{
            //    Id = id,
            //    Feeds = feeds.Take(2).ToList(),
            //};

            //await manager.Save(user);

            //user = null;

            var user = await manager.Get(id);
            Console.WriteLine(user.Feeds.Count);

            user.Feeds.AddRange(feeds.Skip(3).Take(1));

            await manager.Save(user);

            user = null;

            user = await manager.Get(id);
            Console.WriteLine(user.Feeds.Count);
        }
    }

    class MockFeeds : List<Feed>
    {
        public MockFeeds()
        {
            Add(new Feed 
            { 
                Id = Guid.NewGuid(), 
                FeedName = "Engadget", 
                Category = "Technology", 
                ArticleViewingType = ArticleViewingType.Mobilizer,
                FeedUri = "http://www.engadget.com/rss.xml",
            });
            Add(new Feed
            {
                Id = Guid.NewGuid(),
                FeedName = "GigaOM",
                Category = null,
                ArticleViewingType = ArticleViewingType.Mobilizer,
                FeedUri = "http://feeds.feedburner.com/ommalik",
            });

            Add(new Feed
            {
                Id = Guid.NewGuid(),
                FeedName = "Mashable",
                Category = "Technology",
                ArticleViewingType = ArticleViewingType.Mobilizer,
                FeedUri = "http://feeds.mashable.com/Mashable",
            });

            Add(new Feed
            {
                Id = Guid.NewGuid(),
                FeedName = "The Verge",
                Category = "Technology",
                ArticleViewingType = ArticleViewingType.Mobilizer,
                FeedUri = "http://www.theverge.com/rss/index.xml",
            });
        }
    }
}
