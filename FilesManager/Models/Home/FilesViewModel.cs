using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilesManager.Models.Home
{
    public class FileDetails
    {
        public string Name { get; set; }
        public string BlobName { get; set; }
        public bool Processed { get; set; }
    }

    public class FilesViewModel
    {
        public List<FileDetails> Files { get; set; }
            = new List<FileDetails>();
    }
}
