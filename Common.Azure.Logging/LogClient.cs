using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Azure.Logging
{
    public class LogClient
    {
        AzureTableClient tableClient;

        public LogClient(string storageAccountName, string key, bool useHttps, string tableName)
        {
            tableClient = new AzureTableClient(storageAccountName, key, useHttps, tableName);
        }

        public async void Log(
    }
}
