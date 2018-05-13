using Microsoft.AspNetCore.Http;

namespace FilesManager.Models.Home
{
    public class FileInputModel
    {
        public string Folder { get; set; }
        public IFormFile File { get; set; }
    }
}
