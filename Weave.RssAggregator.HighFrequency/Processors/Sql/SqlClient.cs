using Common.Data;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Weave.RssAggregator.HighFrequency
{
    public class SqlClient
    {
        SqlStoredProcClient storedProcClient;

        public SqlClient(SqlStoredProcClient storedProcClient)
        {
            this.storedProcClient = storedProcClient;
            storedProcClient.CommandTimeout = TimeSpan.FromMinutes(2);
#if DEBUG
            storedProcClient.CommandTimeout = TimeSpan.FromMinutes(10);
#endif
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
        public async Task<bool> InsertNewsItemIfNotExists(EntryWithPostProcessInfo entry)
        {
            var storedProcName = "InsertNewsItemIfNotExists";

            var rows = await storedProcClient.GetAsync<bool>(
                storedProcName,
                o => o.GetFieldValueAsync<bool>(0),
                new SqlParameter("p1", entry.Id) { SqlDbType = SqlDbType.UniqueIdentifier },
                new SqlParameter("p2", entry.FeedId) { SqlDbType = SqlDbType.UniqueIdentifier },
                new SqlParameter("p3", entry.UtcPublishDateTime) { SqlDbType = SqlDbType.DateTime },
                new SqlParameter("p4", entry.Title) { SqlDbType = SqlDbType.NVarChar },
                new SqlParameter("p5", entry.Link) { SqlDbType = SqlDbType.NVarChar },
                new SqlParameter("p6", entry.Description) { SqlDbType = SqlDbType.NVarChar },
                new SqlParameter("p7", entry.OriginalPublishDateTimeString) { SqlDbType = SqlDbType.NVarChar },
                new SqlParameter("p8", entry.UtcPublishDateTimeString) { SqlDbType = SqlDbType.NVarChar },

                new SqlParameter("p9", (object)entry.Image.OriginalUrl ?? DBNull.Value) { SqlDbType = SqlDbType.NVarChar, IsNullable = true },
                new SqlParameter("p10", (object)entry.VideoUri ?? DBNull.Value) { SqlDbType = SqlDbType.NVarChar, IsNullable = true },
                new SqlParameter("p11", (object)entry.YoutubeId ?? DBNull.Value) { SqlDbType = SqlDbType.NVarChar, IsNullable = true },
                new SqlParameter("p12", (object)entry.PodcastUri ?? DBNull.Value) { SqlDbType = SqlDbType.NVarChar, IsNullable = true },
                new SqlParameter("p13", (object)entry.ZuneAppId ?? DBNull.Value) { SqlDbType = SqlDbType.NVarChar, IsNullable = true },
                new SqlParameter("p14", (object)entry.OriginalRssXml ?? DBNull.Value) { SqlDbType = SqlDbType.NVarChar, IsNullable = true },
                new SqlParameter("p15", entry.NewsItemBlob) { SqlDbType = SqlDbType.VarBinary },
                new SqlParameter("p16", (object)entry.Image.Width ?? DBNull.Value) { SqlDbType = SqlDbType.Int, IsNullable = true },
                new SqlParameter("p17", (object)entry.Image.Height ?? DBNull.Value) { SqlDbType = SqlDbType.Int, IsNullable = true },
                new SqlParameter("p18", (object)entry.Image.BaseResizedUrl ?? DBNull.Value) { SqlDbType = SqlDbType.VarChar, IsNullable = true },
                new SqlParameter("p19", (object)entry.Image.SupportedFormats ?? DBNull.Value) { SqlDbType = SqlDbType.VarChar, IsNullable = true }
            );

            return rows.FirstOrDefault();
        }
    }
}
