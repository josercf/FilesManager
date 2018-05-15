using FilesManager.DataAccess.Storage.BusinessContracts;
using FilesManager.DataAccess.Storage.Contracts;
using FilesManager.DataAccess.Storage.Models;

namespace FilesManager.DataAccess.Storage.Services
{
    public class DocumentService : ServiceBase<Document>, IDocumentService
    {
        public DocumentService(IAzureTableStorage<Document> tableStorage) : base(tableStorage)
        {
        }
    }
}
