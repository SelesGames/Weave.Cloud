using Microsoft.WindowsAzure.StorageClient;
using System.Threading;

namespace Common.Azure.Logging
{
    public class LogClient
    {
        string storageAccountName, key;
        bool useHttps;

        public LogClient(string storageAccountName, string key, bool useHttps)
        {
            this.storageAccountName = storageAccountName;
            this.key = key;
            this.useHttps = useHttps;
        }

        public async void Log(TableServiceEntity entity, string tableName)
        {
            var tableClient = CreateTableClient();
            tableClient.Insert(entity, tableName);
            await tableClient.SaveChanges(CancellationToken.None);
        }

        AzureTableClient CreateTableClient()
        {
            return new AzureTableClient(storageAccountName, key, useHttps);
        }
    }
}
