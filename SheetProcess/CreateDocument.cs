using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace SheetProcess
{
    public static class CreateDocument
    {
        [FunctionName("CreateDocument")]
        public static async void RunAsync([QueueTrigger("toprocess", Connection = "AzureWebJobsStorage")]string myQueueItem,
            CloudTable outputTable,
            TraceWriter log)
        {
            try
            {
                log.Info($"C# Queue trigger function processed: {myQueueItem}");
                var settings = new AzureBlobSetings("cosmoshoroscopob34c",
                    "ortdcoPVj90rv0GyPGDzMN/jN+5K0izumxFbIqvRM6MiDcXQwcNLSJomGeDQE3RdfhowIyH9MQw856fikwiIrw==",
                    "teste");

                var storage = new AzureBlobStorage(settings);
                var docData = JsonConvert.DeserializeObject<FrontDocumentModel>(myQueueItem);
                await storage.CreateFromTemplate("templates/TemplatePosFrente.docx", $"docs/{docData.StudentName}-frente.docx");
            }
            catch (System.Exception ex)
            {
                log.Info($"Ocorreu um erro ao processar criar o novo arquivo");
            }
        }
    }
}
