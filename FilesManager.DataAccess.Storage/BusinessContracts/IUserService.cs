using FilesManager.DataAccess.Storage.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FilesManager.DataAccess.Storage.BusinessContracts
{
    public interface IUserService
    {
        Task Insert(User entity);

        Task Update(User entity);

        Task<List<User>> Get(string query = null);

        Task<User> Authenticate(string email, string password);
    }
}
