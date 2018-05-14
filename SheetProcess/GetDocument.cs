using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace SheetProcess
{
    public static class GetDocument
    {

        [FunctionName("GetDocument")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string blobName = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "blobname", true) == 0)
                .Value;

            var storageAccount = "cosmoshoroscopob34c";
            var storageKey = "ortdcoPVj90rv0GyPGDzMN/jN+5K0izumxFbIqvRM6MiDcXQwcNLSJomGeDQE3RdfhowIyH9MQw856fikwiIrw==";
            var containerName = "teste";
            var settings = new AzureBlobSetings(storageAccount, storageKey, containerName);

            
            if (blobName == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                blobName = data?.blobName;
            }

            var azureTableStorage = new AzureTableStorage(settings);

            var rowKey = blobName.Substring(5, blobName.Length - 17);
            var docData = await azureTableStorage.Get<FrontDocumentModel>("document", rowKey, rowKey);

            return docData == null
                ? req.CreateResponse(HttpStatusCode.NotFound)
                : req.CreateResponse(HttpStatusCode.OK, docData);
        }
    }
}
