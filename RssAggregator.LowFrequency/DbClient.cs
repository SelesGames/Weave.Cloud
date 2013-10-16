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