using System;
using System.IO;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace SheetProcess
{
    public static class FillDocx
    {
        [FunctionName("FillDocx")]
        public static async void RunAsync([QueueTrigger("toprocess", Connection = "AzureWebJobsStorage")]string myQueueItem,
                                 TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            var settings = new AzureBlobSetings("cosmoshoroscopob34c",
                "ortdcoPVj90rv0GyPGDzMN/jN+5K0izumxFbIqvRM6MiDcXQwcNLSJomGeDQE3RdfhowIyH9MQw856fikwiIrw==", 
                "teste");

            var storage = new AzureBlobStorage(settings);
            
             var data = await storage.DownloadAsync("templates/TemplatePosFrente.docx");

            var docData = JsonConvert.DeserializeObject<FrontDocumentModel>(myQueueItem);
            //var docToFill = await storage.CreateFromTemplate("templates/TemplatePosFrente.docx", $"{docData.StudentName.Trim()}.docx");

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(data, true))
            {
                string docText = null;
                using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
                {
                    docText = sr.ReadToEnd();
                }

                foreach (var item in docData.GetData())
                {
                    Regex regexText = new Regex(item.Key);
                    docText = regexText.Replace(docText, item.Value);
                }


                using (StreamWriter sw = new StreamWriter(wordDoc.MainDocumentPart.GetStream(FileMode.Create)))
                {
                    sw.Write(docText);

                    sw.Close();
                }
            }

            log.Info($"Documento criado!");
        }
    }
}
