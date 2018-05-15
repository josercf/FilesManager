using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using FilesManager.DataAccess.Storage;
using FilesManager.DataAccess.Storage.Infraestructure;
using FilesManager.DataAccess.Storage.Models;
using System.Linq;
using FilesManager.DataAccess.Storage.BusinessContracts;

namespace SheetProcess
{
    public static class Process
    {
        [FunctionName("Process")]
        public static async void Run([BlobTrigger("teste/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob,
                               string name,
                              [Queue("toprocess", Connection = "AzureWebJobsStorage")] ICollector<Document> queueCollector,
                              TraceWriter log)
        {
            var storageAccount = "cosmoshoroscopob34c";
            var storageKey = "ortdcoPVj90rv0GyPGDzMN/jN+5K0izumxFbIqvRM6MiDcXQwcNLSJomGeDQE3RdfhowIyH9MQw856fikwiIrw==";
            var containerName = "teste";

            var settings = new StorageAccountSettings(storageAccount, storageKey, containerName);
            IDocumentService documentService = null;
            if (name.StartsWith("uploads"))
            {
                try
                {
                    var sheetService = new SheetProcessService(queueCollector, documentService, log);
                    await sheetService.ProcessSheet(myBlob);
                }
                catch (Exception e)
                {
                    log.Error($"Ocorreu um erro: {e}");
                }
                log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            }
            else if (name.StartsWith("docs"))
            {
                try
                {
                    var tableName = "document";
                    var azureTableStorage = new AzureTableStorage<Document>(settings, tableName);

                    //Is necessary create new byte array here, because has timeout and 
                    //we can't edite the word file after
                    byte[] b;
                    using (BinaryReader br = new BinaryReader(myBlob))
                        b = br.ReadBytes((int)myBlob.Length);

                    var pk = ClearFileName(name);
                    var result = await azureTableStorage.Retrieve("");
                    var docData = result.FirstOrDefault();

                    if (docData?.Status == "Aguardando processamento")
                    {
                        docData.Status = "Processando";
                        await azureTableStorage.Insert(docData);
                        var docService = new DocProcessService(settings, documentService, log);
                        await docService.Process(b, docData, name);
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"Erro ao receber docx: {ex}");
                }
            }
        }
        public static string ClearFileName(string name) =>
            name.Substring(5, name.LastIndexOf('-') - 5).Trim();

    }
}
