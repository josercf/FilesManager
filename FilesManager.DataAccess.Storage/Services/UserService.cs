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
    public class UserService : IUserService
    {
        private readonly IAzureTableStorage<User> tableStorage;

        public UserService(IAzureTableStorage<User> tableStorage)
        {
            this.tableStorage = tableStorage;
            tableStorage.SetTableName(nameof(User));
        }

        public async Task<List<User>> Get(string query = null)
        {
           return await tableStorage.Retrieve(query);
        }

        public async Task Insert(User entity)
        {
            entity.CreatedAt = DateTime.Now;
            await tableStorage.Insert(entity);
        }

        public async Task<User> Authenticate(string email, string password)
        {
            var query = $@"Email eq '{email}' and Password eq '{password}'";
            var user = await Get(query);

            return user.FirstOrDefault();
        }

        public async Task Update(User entity)
        {
            await tableStorage.Insert(entity, false);
        }
    }
}
