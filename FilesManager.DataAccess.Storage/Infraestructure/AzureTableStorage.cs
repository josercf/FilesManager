using FilesManager.DataAccess.Storage.Contracts;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FilesManager.DataAccess.Storage.Infraestructure
{
    public class AzureTableStorage<T> : IAzureTableStorage<T> where T : class, ITableEntity, new()
    {
        private readonly StorageAccountSettings settings;
        private CloudTable table;

        public AzureTableStorage(StorageAccountSettings settings, string tableName = null)
        {
            this.settings = settings;
            if (!string.IsNullOrEmpty(tableName)) SetTableName(tableName).Wait();
        }

        public async Task<bool> Delete(T entity)
        {
            try
            {
                var DeleteOperation = TableOperation.Delete(entity);
                await table.ExecuteAsync(DeleteOperation);
                return true;
            }
            catch (Exception ExceptionObj)
            {
                throw ExceptionObj;
            }
        }

        public async Task Insert(T entity, bool forInsert = true)
        {
            try
            {
                if (forInsert)
                {
                    var insertOperation = TableOperation.Insert(entity);
                    await table.ExecuteAsync(insertOperation);
                }
                else
                {
                    var insertOrMergeOperation = TableOperation.InsertOrReplace(entity);
                    await table.ExecuteAsync(insertOrMergeOperation);
                }
            }
            catch (Exception ExceptionObj)
            {
                throw ExceptionObj;
            }
        }

        public async Task<List<T>> Retrieve(string Query = null)
        {
            try
            {
                // Create the Table Query Object for Azure Table Storage  
                TableQuery<T> DataTableQuery = new TableQuery<T>();

                if (!string.IsNullOrEmpty(Query))
                    DataTableQuery = new TableQuery<T>().Where(Query);

                TableContinuationToken token = null;
                IEnumerable<T> IDataList = await table.ExecuteQuerySegmentedAsync(DataTableQuery, token);

                List<T> DataList = new List<T>();
                foreach (var singleData in IDataList)
                    DataList.Add(singleData);

                return DataList;
            }
            catch (Exception ExceptionObj)
            {
                throw ExceptionObj;
            }
        }

        public async Task SetTableName(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException(nameof(tableName));

            // Create the table client.
            var tableClient = GetContainer();

            table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
        }

        private CloudTableClient GetContainer()
        {
            //Account
            CloudStorageAccount storageAccount = new CloudStorageAccount(
                new StorageCredentials(settings.StorageAccount, settings.StorageKey), false);

            // Create the table client.
            return storageAccount.CreateCloudTableClient();
        }
    }
}
