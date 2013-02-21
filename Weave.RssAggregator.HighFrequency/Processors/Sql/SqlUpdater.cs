//using Common.Data;
//using SelesGames.Common;
using System;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.Client;
//using Sql = RssAggregator.Data.Sql;

namespace Weave.RssAggregator.HighFrequency
{
    public class SqlUpdater : ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>
    {
        SqlClient dbClient;
        //IProvider<ITransactionalDatabaseClient> transactClientProvider;

        public SqlUpdater(SqlClient dbClient)//, IProvider<ITransactionalDatabaseClient> transactClientProvider)
        {
            this.dbClient = dbClient;
            //this.transactClientProvider = transactClientProvider;
        }

        public bool IsHandledFully { get; private set; }

        public async Task ProcessAsync(HighFrequencyFeedUpdateDto update)
        {
            var entries = update.Entries;

            var latestNewsItemTimeStamp = await dbClient.GetLatestForFeedId(update.FeedId);
            //var latestNews = entries.Where(o => o.PublishDateTime > latestNewsItemTimeStamp).Select(Translate).ToList();
            var latestNews = entries.Where(o => o.PublishDateTime > latestNewsItemTimeStamp).ToList();

            if (!latestNews.Any())
            {
                IsHandledFully = true;
                return;
            }

            foreach (var newsItem in Enumerable.Reverse(latestNews))
            {
                await TryInsertAsync(newsItem);
            }
        }




        #region private helper functions

        async Task<bool> TryInsertAsync(Entry entry)
        {
            try
            {
                await dbClient.InsertNewsItemIfNotExists(entry);
                return true;
            }
            catch (Exception e)
            {
                DebugEx.WriteLine(e);
                return false;
            }
        }

        //async Task<bool> TryInsertAsync(Sql.NewsItem newsItem)
        //{
        //    try
        //    {
        //        using (var transactClient = transactClientProvider.Get())
        //        {
        //            transactClient.Insert(newsItem);
        //            await transactClient.SubmitChanges();
        //        }
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        DebugEx.WriteLine(e);
        //        return false;
        //    }
        //}

        //Sql.NewsItem Translate(Entry e)
        //{
        //    return new Sql.NewsItem
        //    {
        //        Id = e.Id,
        //        FeedId = e.FeedId,
        //        Title = e.Title,
        //        Link = e.Link,
        //        Description = e.Description,
        //        PublishDateTime = e.PublishDateTime,
        //        PublishDateTimeString = e.PublishDateTimeString,
        //        ImageUrl = e.ImageUrl,
        //        VideoUri = e.VideoUri,
        //        YoutubeId = e.YoutubeId,
        //        PodcastUri = e.PodcastUri,
        //        ZuneAppId = e.ZuneAppId,
        //        OriginalRssXml = e.OriginalRssXml,
        //        //NewsItemBlob = ConvertToBytes
        //    };
        //}

        #endregion
    }
}
