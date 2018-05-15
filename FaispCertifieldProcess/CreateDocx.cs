using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace FaispCertifieldProcess
{
    public static class CreateDocx
    {
        [FunctionName("CreateDocx")]
        public static void Run([QueueTrigger("toprocess", Connection = "AzureWebJobsStorage")]string myQueueItem,
                               TraceWriter log)
        {
            log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}
