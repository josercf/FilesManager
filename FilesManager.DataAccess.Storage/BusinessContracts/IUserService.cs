using FilesManager.DataAccess.Storage.Models;
using System.Threading.Tasks;

namespace FilesManager.DataAccess.Storage.BusinessContracts
{
    public interface IUserService : IServiceBase<User>
    {
        Task<User> Authenticate(string email, string password);
    }
}
