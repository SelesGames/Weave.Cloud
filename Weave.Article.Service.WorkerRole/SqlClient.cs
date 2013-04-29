using Common.Data;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Weave.Article.Service.DTOs.ServerOutgoing;

namespace Weave.Article.Service.WorkerRole
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




        #region Read/Unread status

        /// <summary>
        /// Marks a news item as read only if it does not already exist.
        /// </summary>
        /// <param name="userId">The id of the User</param>
        /// <param name="n">The NewsItem to be added</param>
        /// <returns>True if the NewsItem was marked as read, False if the NewsItem was already marked read.</returns>
        public async Task<bool> MarkNewsItemRead(Guid userId, SavedNewsItem n)
        {
            var storedProcName = "MarkNewsItemRead";

            byte[] blob;
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, n);
                ms.Position = 0;
                blob = ms.ToArray();
            }

            var pubDate = DateTime.Parse(n.UtcPublishDateTime, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AdjustToUniversal);

            var rows = await storedProcClient.GetAsync<bool>(
                storedProcName,
                o => o.GetFieldValueAsync<bool>(0),
                new SqlParameter("p1", n.Id) {      SqlDbType = SqlDbType.UniqueIdentifier },
                new SqlParameter("p2", userId) {    SqlDbType = SqlDbType.UniqueIdentifier },
                new SqlParameter("p3", pubDate) {   SqlDbType = SqlDbType.DateTime },
                new SqlParameter("p4", n.SourceName) { SqlDbType = SqlDbType.VarChar },
                new SqlParameter("p5", n.Title) { SqlDbType = SqlDbType.VarChar },
                new SqlParameter("p6", n.Link) { SqlDbType = SqlDbType.VarChar },
                new SqlParameter("p7", blob) {      SqlDbType = SqlDbType.VarBinary }
                );

            return rows.FirstOrDefault();
        }

        public async Task RemoveNewsItemRead(Guid userId, Guid newsItemId)
        {
            var storedProcName = "RemoveNewsItemRead";

            await storedProcClient.GetAsync<object>(
                storedProcName,
                o => null,
                new SqlParameter("p1", userId) {        SqlDbType = SqlDbType.UniqueIdentifier },
                new SqlParameter("p2", newsItemId) {    SqlDbType = SqlDbType.UniqueIdentifier }
                );
        }

        public async Task<List<SavedNewsItem>> GetNewsItemRead(Guid userId, int take, int skip = 0)
        {
            var storedProcName = "GetNewsItemRead";

            var rows = await storedProcClient.GetAsync<byte[]>(
                storedProcName,
                o => o.GetFieldValueAsync<byte[]>(2),
                new SqlParameter("p1", userId) { SqlDbType = SqlDbType.UniqueIdentifier },
                new SqlParameter("p2", take) { SqlDbType = SqlDbType.Int },
                new SqlParameter("p3", skip) { SqlDbType = SqlDbType.Int }
                );

            var result = rows.OfType<byte[]>().Select(FromByteArray).ToList();
            return result;
        }

        #endregion




        #region Favorite

        public async Task<bool> AddNewsItemFavorite(Guid userId, SavedNewsItem n)
        {
            var storedProcName = "AddNewsItemFavorite";

            byte[] blob;
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, n);
                ms.Position = 0;
                blob = ms.ToArray();
            }

            var pubDate = DateTime.Parse(n.UtcPublishDateTime, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AdjustToUniversal);

            var rows = await storedProcClient.GetAsync<bool>(
                storedProcName,
                o => o.GetFieldValueAsync<bool>(0),
                new SqlParameter("p1", n.Id) { SqlDbType = SqlDbType.UniqueIdentifier },
                new SqlParameter("p2", userId) { SqlDbType = SqlDbType.UniqueIdentifier },
                new SqlParameter("p3", pubDate) { SqlDbType = SqlDbType.DateTime },
                new SqlParameter("p4", n.SourceName) { SqlDbType = SqlDbType.VarChar },
                new SqlParameter("p5", n.Title) { SqlDbType = SqlDbType.VarChar },
                new SqlParameter("p6", n.Link) { SqlDbType = SqlDbType.VarChar },
                new SqlParameter("p7", blob) { SqlDbType = SqlDbType.VarBinary }
                );

            return rows.FirstOrDefault();
        }

        public async Task RemoveNewsItemFavorite(Guid userId, Guid newsItemId)
        {
            var storedProcName = "RemoveNewsItemFavorite";

            await storedProcClient.GetAsync<object>(
                storedProcName,
                o => null,
                new SqlParameter("p1", userId) { SqlDbType = SqlDbType.UniqueIdentifier },
                new SqlParameter("p2", newsItemId) { SqlDbType = SqlDbType.UniqueIdentifier }
                );
        }

        public async Task<List<SavedNewsItem>> GetNewsItemFavorites(Guid userId, int take, int skip = 0)
        {
            var storedProcName = "GetNewsItemFavorites";

            var rows = await storedProcClient.GetAsync<byte[]>(
                storedProcName,
                o => o.GetFieldValueAsync<byte[]>(2),
                new SqlParameter("p1", userId) { SqlDbType = SqlDbType.UniqueIdentifier },
                new SqlParameter("p2", take) { SqlDbType = SqlDbType.Int },
                new SqlParameter("p3", skip) { SqlDbType = SqlDbType.Int }
                );

            var result = rows.OfType<byte[]>().Select(FromByteArray).ToList();
            return result;
        }

        #endregion



        SavedNewsItem FromByteArray(byte[] array)
        {
            using (var ms = new MemoryStream(array))
            {
                ms.Position = 0;
                return Serializer.Deserialize<SavedNewsItem>(ms);
            }
        }
    }
}
