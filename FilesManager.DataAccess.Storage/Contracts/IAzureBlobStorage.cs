﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FilesManager.DataAccess.Storage.Contracts
{
    public interface IAzureBlobStorage
    {
        Task CreateFromTemplate(string templateName, string fileName);
        Task UploadAsync(string blobName, string filePath);
        Task UploadAsync(string blobName, Stream stream);
        Task<MemoryStream> DownloadAsync(string blobName);
        Task DownloadAsync(string blobName, string path);
        Task DeleteAsync(string blobName);
        Task<bool> ExistsAsync(string blobName);
        Task<List<AzureBlobItem>> ListAsync();
        Task<List<AzureBlobItem>> ListAsync(string rootFolder);
        Task<List<string>> ListFoldersAsync();
        Task<List<string>> ListFoldersAsync(string rootFolder);
    }
}
