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
                CreateNewUser().Wait();
                //TestRole2().Wait();
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

        static UserController CreateController()
        {
            var blobClient = new SmartBlobClient("weaveuser", "GBzJEaV/B5JQTmLFj/N7VJoYGZBQcEhasXha3RKbd4BRUVN5aaJ01KMo0MNNtNHnVhzJmqlDgqEyk4CPEvX56A==", "user", false);
            var userRepo = new UserRepository(blobClient);
            var controller = new UserController(userRepo);
            return controller;
        }

        static async Task TestRole2()
        {
            var controller = CreateController();

            var user = await controller.RefreshAndReturnNews(Guid.Parse("0f53d7de-0772-41e6-b7fa-ad673b75ba23"));
            var article = user.Feeds.SelectMany(o => o.News).First();
            await controller.MarkArticleRead(user.Id, article.FeedId, article.Id);
            DebugEx.WriteLine(user);
        }

        static async Task CreateNewUser()
        {
            var controller = CreateController();

            var user = new Weave.UserFeedAggregator.DTOs.ServerIncoming.UserInfo
            {
                Id = Guid.NewGuid(),
                Feeds = new MockFeeds(),
            };

            var ouser = await controller.AddUserAndReturnNewNews(user);
            DebugEx.WriteLine(ouser);
        }

        static async Task TestUserAccounts2()
        {
            //var blobClient = new SmartBlobClient("weaveuser", "GBzJEaV/B5JQTmLFj/N7VJoYGZBQcEhasXha3RKbd4BRUVN5aaJ01KMo0MNNtNHnVhzJmqlDgqEyk4CPEvX56A==", "user", false);
            //var userRepo = new UserRepository(blobClient);


            //var feeds = new MockFeeds();

            //var id = Guid.NewGuid();

            //var user = new Weave.UserFeedAggregator.DTOs.ServerIncoming.UserInfo
            //{
            //    Id = id,
            //    Feeds = feeds.Take(2).ToList(),
            //};

            //await userRepo.Save(user);

            //user = null;

            //user = await userRepo.Get(id);
            //Console.WriteLine(user.Feeds.Count);

            //user.Feeds.AddRange(feeds.Skip(3).Take(1));

            //await userRepo.Save(user);

            //user = null;

            //user = await userRepo.Get(id);
            //Console.WriteLine(user.Feeds.Count);
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

    class MockFeeds : List<Weave.UserFeedAggregator.DTOs.ServerIncoming.Feed>
    {
        public MockFeeds()
        {
            Add(new Weave.UserFeedAggregator.DTOs.ServerIncoming.Feed 
            { 
                FeedName = "Engadget", 
                Category = "Technology",
                ArticleViewingType = Weave.UserFeedAggregator.DTOs.ArticleViewingType.Mobilizer,
                FeedUri = "http://www.engadget.com/rss.xml",
            });
            Add(new Weave.UserFeedAggregator.DTOs.ServerIncoming.Feed
            {
                FeedName = "GigaOM",
                Category = null,
                ArticleViewingType = Weave.UserFeedAggregator.DTOs.ArticleViewingType.Mobilizer,
                FeedUri = "http://feeds.feedburner.com/ommalik",
            });

            Add(new Weave.UserFeedAggregator.DTOs.ServerIncoming.Feed
            {
                FeedName = "Mashable",
                Category = "Technology",
                ArticleViewingType = Weave.UserFeedAggregator.DTOs.ArticleViewingType.Mobilizer,
                FeedUri = "http://feeds.mashable.com/Mashable",
            });

            Add(new Weave.UserFeedAggregator.DTOs.ServerIncoming.Feed
            {
                FeedName = "The Verge",
                Category = "Technology",
                ArticleViewingType = Weave.UserFeedAggregator.DTOs.ArticleViewingType.Mobilizer,
                FeedUri = "http://www.theverge.com/rss/index.xml",
            });
        }
    }
}
