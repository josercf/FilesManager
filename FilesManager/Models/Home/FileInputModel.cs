using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilesManager.Models.Home
{
    public class FileInputModel
    {
        public string Folder { get; set; }
        public IFormFile File { get; set; }
    }
}
