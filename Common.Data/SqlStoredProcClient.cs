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

        public async Task<IEnumerable<T>> GetAsync<T>(string commandText, Func<SqlDataReader, Task<T>> projection, CancellationToken cancelToken)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(commandText, connection))
            {
                command.CommandType = CommandType.StoredProcedure;

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

        public Task<IEnumerable<T>> GetAsync<T>(string commandText, Func<SqlDataReader, Task<T>> projection)
        {
            return GetAsync(commandText, projection, CancellationToken.None);
        }
    }
}
