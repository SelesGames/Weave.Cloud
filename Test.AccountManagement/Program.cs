using Common.Azure.SmartBlobClient;
using Ninject;
using ProtoBuf;
using SelesGames.HttpClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
//using Weave.AccountManagement.DTOs;
using Weave.User.DataStore;
using Weave.AccountManagement.WebRole.Startup;
using Weave.RssAggregator.Core.DTOs.Incoming;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.UserFeedAggregator.Role;
using Weave.UserFeedAggregator.Repositories;
using Weave.UserFeedAggregator.Role.Controllers;

namespace Test.AccountManagement
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                TestRole2().Wait();
                //TestRole().Wait();
                //TestUserAccounts2().Wait();
                //TestSmartHttpClient().Wait();
                //TestUserCreationRetrievalEtc().Wait();
                Console.WriteLine("all tests passed!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            while (true)
                Console.ReadLine();
        }

        static async Task TestRole2()
        {
            var blobClient = new SmartBlobClient("weaveuser", "GBzJEaV/B5JQTmLFj/N7VJoYGZBQcEhasXha3RKbd4BRUVN5aaJ01KMo0MNNtNHnVhzJmqlDgqEyk4CPEvX56A==", "user", false);
            var userRepo = new UserRepository(blobClient);
            var controller = new UserFeedController(userRepo);

            var user = await controller.RefreshAndReturnNews(Guid.Parse("19060d80-f525-4b62-b907-75e02819b28b"));
            DebugEx.WriteLine(user);
        }

        static async Task TestRole()
        {
            var blobClient = new SmartBlobClient("weaveuser", "GBzJEaV/B5JQTmLFj/N7VJoYGZBQcEhasXha3RKbd4BRUVN5aaJ01KMo0MNNtNHnVhzJmqlDgqEyk4CPEvX56A==", "user", false);
            var userRepo = new UserRepository(blobClient);
            var controller = new UserFeedController(userRepo);

            var feeds = new MockFeeds();
            var user = new UserInfo
            {
                Id = Guid.NewGuid(),
                Feeds = feeds,
            };

            user = await controller.AddUserAndReturnNews(user);
            DebugEx.WriteLine(user);
        }

        static async Task TestUserAccounts2()
        {
            var blobClient = new SmartBlobClient("weaveuser", "GBzJEaV/B5JQTmLFj/N7VJoYGZBQcEhasXha3RKbd4BRUVN5aaJ01KMo0MNNtNHnVhzJmqlDgqEyk4CPEvX56A==", "user", false);
            var userRepo = new UserRepository(blobClient);


            var feeds = new MockFeeds();

            var id = Guid.NewGuid();

            var user = new UserInfo
            {
                Id = id,
                Feeds = feeds.Take(2).ToList(),
            };

            await userRepo.Save(user);

            user = null;

            user = await userRepo.Get(id);
            Console.WriteLine(user.Feeds.Count);

            user.Feeds.AddRange(feeds.Skip(3).Take(1));

            await userRepo.Save(user);

            user = null;

            user = await userRepo.Get(id);
            Console.WriteLine(user.Feeds.Count);
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

        //static async Task TestUserCreationRetrievalEtc()
        //{
        //    var feeds = new MockFeeds();

        //    var kernel = new NinjectKernel();
        //    var manager = kernel.Get<Weave.AccountManagement.DTOs.UserManager>();

        //    var id = Guid.Parse("6bfb9049-5c48-47d0-95c6-da97e7922bf6");

        //    //var id = Guid.NewGuid();

        //    //var user = new UserInfo
        //    //{
        //    //    Id = id,
        //    //    Feeds = feeds.Take(2).ToList(),
        //    //};

        //    //await manager.Save(user);

        //    //user = null;

        //    var user = await manager.Get(id);
        //    Console.WriteLine(user.Feeds.Count);

        //    user.Feeds.AddRange(feeds.Skip(3).Take(1));

        //    await manager.Save(user);

        //    user = null;

        //    user = await manager.Get(id);
        //    Console.WriteLine(user.Feeds.Count);
        //}
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
