using Common.Data;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.RssAggregator.LowFrequency
{
    public class DbClient
    {
        SqlStoredProcClient storedProcClient;

        static DbClient()
        {
            ProtoBuf.Serializer.PrepareSerializer<NewsItem>();
        }

        public DbClient(SqlStoredProcClient storedProcClient)
        {
            this.storedProcClient = storedProcClient;
        }

        public async Task<List<NewsItem>> GetLatestNNewsAsBlobList(Guid id, int topCount = 25)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var storedProcName = "GetLatestNNewsAsBlobList";

            var rows = await storedProcClient.GetAsync<byte[]>(
                storedProcName,
                o => o.GetFieldValueAsync<byte[]>(0),
                new SqlParameter("p1", id) { SqlDbType = System.Data.SqlDbType.UniqueIdentifier },
                new SqlParameter("p2", topCount) { SqlDbType = System.Data.SqlDbType.Int });

            var result = rows.OfType<byte[]>().Select(FromByteArray).ToList();

            sw.Stop();
            DebugEx.WriteLine("BINARY BLOB via stored proc SQL TOOK : {0} ms", sw.ElapsedMilliseconds);

            return result;
        }

        NewsItem FromByteArray(byte[] array)
        {
            using (var ms = new MemoryStream(array))
            {
                ms.Position = 0;
                return Serializer.Deserialize<NewsItem>(ms);
            }
        }
    }
}




#region deprecated


//public async Task<List<NewsItem>> GetLatestNewsForFeedId(Guid feedId, int topCount = 25)
//{
//    var sw = System.Diagnostics.Stopwatch.StartNew();

//    using (var client = dbClientProvider.Get())
//    {
//        var news = await client.Get<Sql.NewsItem, NewsItem>(x => x
//            .Where(o => feedId.Equals(o.FeedId))
//            .Where(o => o.NewsItemBlob != null)
//            .OrderByDescending(o => o.PublishDateTime)
//            .Take(25)
//            .Select(o => new NewsItem
//            {
//                Title = o.Title,
//                Link = o.Link,
//                PublishDateTime = o.UtcPublishDateTimeString,
//                ImageUrl = o.ImageUrl,
//                VideoUri = o.VideoUri,
//                YoutubeId = o.YoutubeId,
//                PodcastUri = o.PodcastUri,
//                ZuneAppId = o.ZuneAppId,
//            }));

//        var result = news.ToList();

//        sw.Stop();
//        DebugEx.WriteLine("REGULAR SQL TOOK : {0} ms", sw.ElapsedMilliseconds);

//        return result;
//    }
//}

//public async Task<List<NewsItem>> GetLatestNewsForFeedIdViaBinary(Guid feedId)
//{
//    var sw = System.Diagnostics.Stopwatch.StartNew();

//    using (var client = dbClientProvider.Get())
//    {
//        var news = await client.Get<Sql.NewsItem, System.Data.Linq.Binary>(x => x
//            .Where(o => feedId.Equals(o.FeedId))
//            .Where(o => o.NewsItemBlob != null)
//            .OrderByDescending(o => o.PublishDateTime)
//            .Take(25)
//            .Select(o => o.NewsItemBlob));

//        var binaries = news.ToList();
//        var byteArrays = binaries.Select(o => o.ToArray());
//        var result = byteArrays.Select(FromByteArray).ToList();

//        sw.Stop();
//        DebugEx.WriteLine("BINARY BLOB SQL TOOK : {0} ms", sw.ElapsedMilliseconds);

//        return result;
//    }
//}

#endregion
