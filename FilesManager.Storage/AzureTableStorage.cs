using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FilesManager.Storage
{
    public class AzureTableStorage
    {
        private readonly AzureBlobSetings settings;


        public AzureTableStorage(AzureBlobSetings settings)
        {
            this.settings = settings;
        }

        public async Task Insert(ITableEntity model, string tableName)
        {
            // Create the table client.
            var tableClient = await GetContainerAsync();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.InsertOrReplace(model);

            // Execute the insert operation.
            await table.ExecuteAsync(insertOperation);
        }

        public async Task<T> Get<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity
        {
            // Create the table client.
            var tableClient = await GetContainerAsync();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            var tableOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var tableResult = await table.ExecuteAsync(tableOperation);
            return tableResult.Result as T;
        }

        
        public async Task Update(ITableEntity model, string tableName)
        {
            // Create the table client.
            var tableClient = await GetContainerAsync();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            var tableOperation = TableOperation.Replace(model);
            var tableResult = await table.ExecuteAsync(tableOperation);
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
