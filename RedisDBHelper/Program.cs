using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Weave.Services.Redis.Ambient;
using Weave.Updater.Azure;
using Weave.User.Service.Redis;

namespace RedisDBHelper
{
    public class Program
    {
        public Task<string> ProcessInput(string input)
        {
            var parameters = input.Split(' ');
            var command = parameters.First();
            if (command.Equals("testfeed", StringComparison.OrdinalIgnoreCase))
            {
                var url = parameters.Skip(1).First();
                return new RedisFeedUpdaterTest(url).Execute();
            }

            else if (command.Equals("testentry", StringComparison.OrdinalIgnoreCase))
            {
                var id = parameters.Skip(1).First();
                return new RedisExpandedEntryTest(id).Execute();
            }

            else if (command.Equals("remfeed"))
            {
                var url = parameters.Skip(1).First();
                return new RedisDeleteFeedUpdater(url).Execute();
            }

            else if (command.Equals("remdb"))
            {
                var dbNum = parameters.Skip(1).First();
                return new RedisDeleteDatabase(dbNum).Execute();
            }

            else return Task.FromResult("unrecognized command");
        }

        static async Task TestFeedUpdateToAzure()
        {
            //var feedUrl = "http://www.polygon.com/rss/index.xml";

            //var cm = Settings.StandardConnection;
            //var db = cm.GetDatabase(DatabaseNumbers.FEED_UPDATER);
            //var redisCache = new Weave.User.Service.Redis.FeedUpdaterCache(db);
            //var blobClient = new Weave.Updater.Azure.FeedUpdaterCache(
            //    "weaveuser2",
            //    "JO5kSIOr+r3NdM45gfzb1szHe/hPx6f+MS7YOWogr8VDqSikiIP//OMUbOxCCMTFTcJgldVhl+Y0zP9WpvQV5g==",
            //    "updaterfeeds");
            //var blobUpdater = new FeedBlobUpdater(blobClient, redisCache);

            //var wasUpdated = await blobUpdater.Update(feedUrl);
            //Debug.WriteLine(wasUpdated);
        }
    }
}