using Microsoft.WindowsAzure.StorageClient;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Azure
{
    public static class CloudTableQueryExtensions
    {
        public static Task<DataServiceResponse> SaveChangesAsync(this TableServiceContext context, CancellationToken cancelToken)
        {
            return Task.Factory.FromAsync<DataServiceResponse>(context.BeginSaveChangesWithRetries, context.EndSaveChangesWithRetries, null);
        }

        public static Task<DataServiceResponse> SaveChangesAsync(this TableServiceContext context, SaveChangesOptions saveOptions, CancellationToken cancelToken)
        {
            return Task.Factory.FromAsync<SaveChangesOptions, DataServiceResponse>(context.BeginSaveChangesWithRetries, context.EndSaveChangesWithRetries, saveOptions, null);
        }

        public static async Task<IEnumerable<T>> GetResultsAsync<T>(this CloudTableQuery<T> query, CancellationToken cancelToken)
        {
            List<T> results = new List<T>();

            ResultSegment<T> resultSegment;
            resultSegment = await query.ExecuteSegmentedAsync().ConfigureAwait(false);
            cancelToken.ThrowIfCancellationRequested();

            results.AddRange(resultSegment.Results);

            while (resultSegment.HasMoreResults)
            {
                resultSegment = await resultSegment.GetNextAsync().ConfigureAwait(false);
                cancelToken.ThrowIfCancellationRequested();
                results.AddRange(resultSegment.Results);
            }

            return results;
        }

        public static Task<ResultSegment<T>> ExecuteSegmentedAsync<T>(this CloudTableQuery<T> query)
        {
            return Task.Factory.FromAsync<ResultSegment<T>>(query.BeginExecuteSegmented, query.EndExecuteSegmented, null);
        }

        public static Task<ResultSegment<T>> ExecuteSegmentedAsync<T>(this CloudTableQuery<T> query, ResultContinuation continuationToken)
        {
            return Task.Factory.FromAsync<ResultContinuation, ResultSegment<T>>(query.BeginExecuteSegmented, query.EndExecuteSegmented, continuationToken, null);
        }

        public static Task<ResultSegment<T>> GetNextAsync<T>(this ResultSegment<T> resultSegment)
        {
            return Task.Factory.FromAsync<ResultSegment<T>>(resultSegment.BeginGetNext, resultSegment.EndGetNext, null);
        }
    }
}
