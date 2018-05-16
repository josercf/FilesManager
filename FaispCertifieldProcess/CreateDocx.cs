using System;
using System.IO;
using FaispCertifieldProcess.DependencyInjection;
using FilesManager.DataAccess.Storage.Contracts;
using FilesManager.DataAccess.Storage.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace FaispCertifieldProcess
{
    public static class CreateDocx
    {
        [FunctionName("CreateDocx")]
        public static async void Run([QueueTrigger("toprocess", Connection = "AzureWebJobsStorage")]string myQueueItem,
            [Inject]IAzureBlobStorage storage, TraceWriter log)
        {
            try
            {
                //verificar se é extensão ou pós. Frente ou verso.


                var docData = JsonConvert.DeserializeObject<Document>(myQueueItem);
                await storage.CreateFromTemplate("templates/TemplatePosFrente.docx", docData.DocumentFront);
                log.Info($"document {docData.DocumentFront} created");
                await storage.CreateFromTemplate("templates/TemplatePosFrente.docx", docData.DocumentFront);
                log.Info($"document {docData.DocumentFront}  created");
            }
            catch (Exception ex)
            {
                log.Info($"Ocorreu um erro ao processar criar o novo arquivo \n{ex}");
            }
        }
    }
}
