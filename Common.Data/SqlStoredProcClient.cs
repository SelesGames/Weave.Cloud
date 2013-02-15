using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Data
{
    public class SqlStoredProcClient
    {
        string connectionString;

        public SqlStoredProcClient(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<IEnumerable<T>> GetAsync<T>(string storedProcName, Func<SqlDataReader, Task<T>> projection, CancellationToken cancelToken, params SqlParameter[] storedProcParameters)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(storedProcName, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                foreach (var parameter in storedProcParameters)
                    command.Parameters.Add(parameter);

                await connection.OpenAsync(cancelToken).ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(cancelToken).ConfigureAwait(false))
                {
                    List<T> results = new List<T>();

                    while (await reader.ReadAsync(cancelToken).ConfigureAwait(false))
                    {
                        var result = await projection(reader);
                        results.Add(result);
                    }

                    return results;
                }
            }
        }

        public Task<IEnumerable<T>> GetAsync<T>(string storedProcName, Func<SqlDataReader, Task<T>> projection, params SqlParameter[] storedProcParameters)
        {
            return GetAsync(storedProcName, projection, CancellationToken.None, storedProcParameters);
        }
    }
}
