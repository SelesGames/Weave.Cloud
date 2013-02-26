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
        //CloudStorageAccount account;
        string tableName;
        TableServiceContext context;

        public TimeSpan ReadTimeout { get; set; }
        public TimeSpan WriteTimeout { get; set; }
        public string TableEndpoint { get; private set; }

        public AzureTableClient(string storageAccountName, string key, bool useHttps, string tableName)
        {
            var blobCred = new StorageCredentialsAccountAndKey(storageAccountName, key);
            var account = new CloudStorageAccount(blobCred, useHttps);
            TableEndpoint = account.TableEndpoint.ToString();
            this.tableName = tableName;

            ReadTimeout = TimeSpan.FromSeconds(30);
            WriteTimeout = TimeSpan.FromMinutes(1);

            var client = account.CreateCloudTableClient();
            context = client.GetDataServiceContext();
        }

        public Task<IEnumerable<T>> Get<T>(CancellationToken cancelToken, Func<IQueryable<T>, IQueryable<T>> operatorChain = null, RetryPolicy retryPolicy = null)
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

        public void Insert<T>(T obj)
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
            //return context.SaveChangesAsync(cancelToken);
        }


        //public Task Save(string fileName, Stream obj)
        //{
        //    var client = account.CreateCloudBlobClient();
        //    var container = client.GetContainerReference(this.container);
        //    var blob = container.GetBlobReference(fileName);

        //    if (!string.IsNullOrEmpty(ContentType))
        //        blob.Properties.ContentType = ContentType;
        //    ////blob.Properties.ContentEncoding = "gzip";

        //    BlobRequestOptions options = new BlobRequestOptions();
        //    options.AccessCondition = AccessCondition.None;
        //    options.Timeout = WriteTimeout;

        //    //using (var ms = new MemoryStream())
        //    //{
        //    //    await obj.CopyToAsync(ms);
        //    //    ms.Position = 0;
        //    //    await blob.UploadFromStreamAsync(ms, options);
        //    //}
        //    return blob.UploadFromStreamAsync(obj, options);
        //}

        //public Task Delete(string fileName)
        //{
        //    var client = account.CreateCloudBlobClient();
        //    var container = client.GetContainerReference(this.container);
        //    var blob = container.GetBlobReference(fileName);

        //    return blob.DeleteAsync();
        //}
    }
}