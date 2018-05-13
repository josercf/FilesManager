using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;

namespace SheetProcess
{
    public static class CreateDocument
    {
        [FunctionName("CreateDocument")]
        public static async void RunAsync([QueueTrigger("toprocess", Connection = "AzureWebJobsStorage")]string myQueueItem,
            TraceWriter log)
        {
            try
            {
                log.Info($"C# Queue trigger function processed: {myQueueItem}");

                var storageAccount = GetEnvironmentVariable("Blob_StorageAccount");
                var storageKey = GetEnvironmentVariable("Blob_StorageKey");
                var containerName = GetEnvironmentVariable("Blob_ContainerName");

                storageAccount = "cosmoshoroscopob34c";
                storageKey = "ortdcoPVj90rv0GyPGDzMN/jN+5K0izumxFbIqvRM6MiDcXQwcNLSJomGeDQE3RdfhowIyH9MQw856fikwiIrw==";
                containerName = "teste";

                var settings = new AzureBlobSetings(storageAccount, storageKey, containerName);

                var storage = new AzureBlobStorage(settings);
                var docData = JsonConvert.DeserializeObject<FrontDocumentModel>(myQueueItem);
                await storage.CreateFromTemplate("templates/TemplatePosFrente.docx", $"docs/{docData.StudentName}-frente.docx");
            }
            catch (System.Exception ex)
            {
                log.Info($"Ocorreu um erro ao processar criar o novo arquivo");
            }
        }

        public static string GetEnvironmentVariable(string name)
        {
            return name + ": " +
                System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
