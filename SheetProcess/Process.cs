using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Queue;
using S = DocumentFormat.OpenXml.Spreadsheet.Sheets;
using E = DocumentFormat.OpenXml.OpenXmlElement;
using A = DocumentFormat.OpenXml.OpenXmlAttribute;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Net.Http;

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
            if (name.StartsWith("uploads"))
            {
                try
                {
                    var sheetService = new SheetProcessService(queueCollector, log);
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

            }
        }
    }
}
