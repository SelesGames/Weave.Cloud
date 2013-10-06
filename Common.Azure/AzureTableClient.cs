using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Azure
{
    public class AzureTableClient
    {
        TableServiceContext context;

        public TimeSpan ReadTimeout { get; set; }
        public TimeSpan WriteTimeout { get; set; }
        public string TableEndpoint { get; private set; }

        public AzureTableClient(string storageAccountName, string key, bool useHttps)
        {
            var blobCred = new StorageCredentialsAccountAndKey(storageAccountName, key);
            var account = new CloudStorageAccount(blobCred, useHttps);
            TableEndpoint = account.TableEndpoint.ToString();

            ReadTimeout = TimeSpan.FromSeconds(30);
            WriteTimeout = TimeSpan.FromMinutes(1);

            var client = account.CreateCloudTableClient();
            context = client.GetDataServiceContext();
        }

        public Task<IEnumerable<T>> Get<T>(CancellationToken cancelToken, string tableName, Func<IQueryable<T>, IQueryable<T>> operatorChain = null, RetryPolicy retryPolicy = null)
        {
            var query = context.CreateQuery<T>(tableName);
            if (operatorChain != null)
                query = (DataServiceQuery<T>)operatorChain(query);

            var tsQuery = query.AsTableServiceQuery();
            if (retryPolicy == null)
                retryPolicy = RetryPolicies.RetryExponential(3, TimeSpan.FromSeconds(10));
            tsQuery.RetryPolicy = retryPolicy;

            return tsQuery.GetResultsAsync(cancelToken);
        }

        public void Insert<T>(T obj, string tableName)
        {
            context.AddObject(tableName, obj);
        }

        public void Delete<T>(T obj)
        {
            context.DeleteObject(obj);
        }

        public Task SaveChanges(CancellationToken cancelToken)
        {
            var saveChangesOptions = SaveChangesOptions.Batch;// | SaveChangesOptions.ContinueOnError | SaveChangesOptions.ReplaceOnUpdate;
            return context.SaveChangesAsync(saveChangesOptions, cancelToken);
        }
    }
}