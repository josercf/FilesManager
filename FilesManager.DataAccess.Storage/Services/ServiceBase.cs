using FilesManager.DataAccess.Storage.BusinessContracts;
using FilesManager.DataAccess.Storage.Contracts;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FilesManager.DataAccess.Storage.Services
{
    public class ServiceBase<T> : IServiceBase<T> where T : class, ITableEntity, new()
    {
        protected readonly IAzureTableStorage<T> tableStorage;

        public ServiceBase(IAzureTableStorage<T> tableStorage)
        {
            this.tableStorage = tableStorage;
        }

        public async virtual Task<List<T>> Get(string query = null)
        {
            return await tableStorage.Retrieve(query);
        }

        public async virtual Task Insert(T entity)
        {
            await tableStorage.Insert(entity);
        }

        public async virtual Task Update(T entity)
        {
            await tableStorage.Insert(entity, false);
        }
    }
}
