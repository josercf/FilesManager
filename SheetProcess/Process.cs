using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using Microsoft.Extensions.Configuration;

namespace SheetProcess
{
    public static class Process
    {
        [FunctionName("Process")]
        public static async void Run([BlobTrigger("teste/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob,
                               string name,
                              [Queue("toprocess", Connection = "AzureWebJobsStorage")] ICollector<FrontDocumentModel> queueCollector,
                              TraceWriter log)
        {
            var storageAccount = "cosmoshoroscopob34c";
            var storageKey = "ortdcoPVj90rv0GyPGDzMN/jN+5K0izumxFbIqvRM6MiDcXQwcNLSJomGeDQE3RdfhowIyH9MQw856fikwiIrw==";
            var containerName = "teste";

            var settings = new AzureBlobSetings(storageAccount, storageKey, containerName);

            if (name.StartsWith("uploads"))
            {
                try
                {
                    var sheetService = new SheetProcessService(queueCollector, settings, log);
                    await sheetService.ProcessSheet(myBlob);
                }
                catch (Exception e)
                {
                    log.Info($"Ocorreu um erro: {e}");
                }
                log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            }
            else if (name.StartsWith("docs"))
            {
                var tableName = "document";
                var azureTableStorage = new AzureTableStorage(settings);

                //Is necessary create new byte array here, because has timeout and 
                //we can't edite the word file after
                byte[] b;
                using (BinaryReader br = new BinaryReader(myBlob))
                    b = br.ReadBytes((int)myBlob.Length);

                var pk = ClearFileName(name);
                var docData = await azureTableStorage.Get<FrontDocumentModel>(tableName, pk, pk);

                if (docData?.Status == "Aguardando processamento")
                {
                    docData.Status = "Processando";
                    await azureTableStorage.Update(docData, tableName);
                    var docService = new DocProcessService(settings, azureTableStorage, log);
                    await docService.Process(b, docData, name);
                }
            }
        }
        public static string ClearFileName(string name) =>
            name.Substring(5, name.LastIndexOf('-') - 5).Trim();

    }
}
