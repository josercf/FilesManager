using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FilesManager.DataAccess.Storage.Contracts
{
    public interface IAzureTableStorage<T> where T : class, ITableEntity, new()
    {
        Task SetTableName(string tableName);

        Task Insert(T entity, bool forInsert = true);

        Task<List<T>> Retrieve(string Query = null);

        Task<bool> Delete(T entity);
    }
}
