using Common.Data;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.Client;

namespace Weave.RssAggregator.HighFrequency
{
    public class SqlClient
    {
        SqlStoredProcClient storedProcClient;

        public SqlClient(SqlStoredProcClient storedProcClient)
        {
            this.storedProcClient = storedProcClient;
        }

        public async Task<DateTime> GetLatestForFeedId(Guid feedId)
        {
            //var command = string.Format(
            //    "EXEC GetLatestPublicationDateTimeForFeedId @p1='{0}'", feedId);
            var storedProcName = "GetLatestPublicationDateTimeForFeedId";

            var rows = await storedProcClient.GetAsync<DateTime>(
                storedProcName, 
                o => o.GetFieldValueAsync<DateTime>(0), 
                new SqlParameter("p1", feedId) { SqlDbType = System.Data.SqlDbType.UniqueIdentifier });

            return rows.FirstOrDefault();
        }

        /// <summary>
        /// Inserts an Entry into the database only if it does not already exist (as determined by Entry.Id).
        /// </summary>
        /// <param name="entry">The Entry to be added</param>
        /// <returns>True if the Entry was added to the database, False if the Entry already existed.</returns>
        public async Task<bool> InsertNewsItemIfNotExists(Entry entry)
        {
            var storedProcName = "InsertNewsItemIfNotExists";

            var rows = await storedProcClient.GetAsync<bool>(
                storedProcName,
                o => o.GetFieldValueAsync<bool>(0),
                new SqlParameter("p1", entry.Id) { SqlDbType = System.Data.SqlDbType.UniqueIdentifier },
                new SqlParameter("p2", entry.FeedId) { SqlDbType = System.Data.SqlDbType.UniqueIdentifier },
                new SqlParameter("p3", entry.PublishDateTime) { SqlDbType = System.Data.SqlDbType.DateTime },
                new SqlParameter("p4", entry.Title) { SqlDbType = System.Data.SqlDbType.NVarChar },
                new SqlParameter("p5", entry.Link) { SqlDbType = System.Data.SqlDbType.NVarChar },
                new SqlParameter("p6", entry.Description) { SqlDbType = System.Data.SqlDbType.NVarChar },
                new SqlParameter("p7", entry.PublishDateTimeString) { SqlDbType = System.Data.SqlDbType.NVarChar },

                new SqlParameter("p8", (object)entry.ImageUrl ?? DBNull.Value) { SqlDbType = System.Data.SqlDbType.NVarChar, IsNullable = true },
                new SqlParameter("p9", (object)entry.VideoUri ?? DBNull.Value) { SqlDbType = System.Data.SqlDbType.NVarChar, IsNullable = true },
                new SqlParameter("p10", (object)entry.YoutubeId ?? DBNull.Value) { SqlDbType = System.Data.SqlDbType.NVarChar, IsNullable = true },
                new SqlParameter("p11", (object)entry.PodcastUri ?? DBNull.Value) { SqlDbType = System.Data.SqlDbType.NVarChar, IsNullable = true },
                new SqlParameter("p12", (object)entry.ZuneAppId ?? DBNull.Value) { SqlDbType = System.Data.SqlDbType.NVarChar, IsNullable = true },
                new SqlParameter("p13", (object)entry.OriginalRssXml ?? DBNull.Value) { SqlDbType = System.Data.SqlDbType.NVarChar, IsNullable = true },
                new SqlParameter("p14", (object)entry.NewsItemBlob ?? DBNull.Value) { SqlDbType = System.Data.SqlDbType.VarBinary, IsNullable = true });

            return rows.FirstOrDefault();
        }
    }
}
