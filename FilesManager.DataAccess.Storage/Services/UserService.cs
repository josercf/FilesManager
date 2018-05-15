using FilesManager.DataAccess.Storage.BusinessContracts;
using FilesManager.DataAccess.Storage.Contracts;
using FilesManager.DataAccess.Storage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilesManager.DataAccess.Storage.Services
{
    public class UserService : ServiceBase<User>, IUserService
    {

        public UserService(IAzureTableStorage<User> tableStorage)
            : base(tableStorage)
        {
            tableStorage.SetTableName(nameof(User));
        }

        public override Task Insert(User entity)
        {
            entity.CreatedAt = DateTime.Now;
            return base.Insert(entity);
        }

        public async Task<User> Authenticate(string email, string password)
        {
            var query = $@"Email eq '{email}' and Password eq '{password}'";
            var user = await Get(query);

            return user.FirstOrDefault();
        }
    }
}
