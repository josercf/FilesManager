using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SheetProcess
{
    public class AzureTableStorage
    {
        private readonly AzureBlobSetings settings;
        // Create the table client.
        private readonly CloudTableClient tableClient;

        public AzureTableStorage(AzureBlobSetings settings)
        {
            this.settings = settings;
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));

            // Create the table client.
            var tableClient = storageAccount.CreateCloudTableClient();
        }

        public async Task Insert(ITableEntity model, string tableName)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(model);

            // Execute the insert operation.
            await table.ExecuteAsync(insertOperation);
        }

        public async Task<T> Get<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity
        {
            // Create the table client.
            var tableClient = GetContainerAsync();

            // Create the CloudTable object that represents the "people" table.
           // CloudTable table = tableClient.GetTableReference(tableName);


            var tableOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var tableResult = await table.ExecuteAsync(tableOperation);
            return tableResult.Result as T ;
        }

        private async Task<CloudTableClient> GetContainerAsync()
        {
            //Account
            CloudStorageAccount storageAccount = new CloudStorageAccount(
                new StorageCredentials(settings.StorageAccount, settings.StorageKey), false);

            // Create the table client.
            var tableClient = storageAccount.CreateCloudTableClient();

            return tableClient;
        }
    }
}
