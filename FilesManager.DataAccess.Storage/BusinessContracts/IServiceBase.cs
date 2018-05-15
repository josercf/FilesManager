using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FilesManager.DataAccess.Storage.BusinessContracts
{
    public interface IServiceBase<T> where T : class, ITableEntity, new()
    {
        Task Insert(T entity);

        Task Update(T entity);

        Task<List<T>> Get(string query = null);
    }
}
