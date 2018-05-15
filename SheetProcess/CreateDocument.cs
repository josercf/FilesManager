using FilesManager.DataAccess.Storage;
using FilesManager.DataAccess.Storage.Infraestructure;
using FilesManager.DataAccess.Storage.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
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

                var storageAccount = "cosmoshoroscopob34c";
                var storageKey = "ortdcoPVj90rv0GyPGDzMN/jN+5K0izumxFbIqvRM6MiDcXQwcNLSJomGeDQE3RdfhowIyH9MQw856fikwiIrw==";
                var containerName = "teste";

                var settings = new StorageAccountSettings(storageAccount, storageKey, containerName);

                var storage = new AzureBlobStorage(settings);
                var docData = JsonConvert.DeserializeObject<Document>(myQueueItem);
                await storage.CreateFromTemplate("templates/TemplatePosFrente.docx", docData.DocumentFront);
            }
            catch (Exception ex)
            {
                log.Info($"Ocorreu um erro ao processar criar o novo arquivo \n{ex}");
            }
        }

        public static string GetEnvironmentVariable(string name)
        {
            return name + ": " +
                System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
