using Common.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
