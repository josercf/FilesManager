using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace FilesManager.DataAccess.Storage.Models
{
    public sealed class User : EntityBase
    {
        public User()
        {
            PartitionKey = "User";
            RowKey = Guid.NewGuid().ToString();
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
