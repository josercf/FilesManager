using DocumentFormat.OpenXml.Packaging;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SheetProcess
{
    public class DocProcessService
    {
        private readonly AzureTableStorage azureTableStorage;
        private readonly AzureBlobStorage azureBlobStorage;
        private readonly TraceWriter log;

        public DocProcessService(AzureBlobSetings settings, AzureTableStorage azureTableStorage,  TraceWriter log)
        {
            this.azureTableStorage = azureTableStorage;
            this.azureBlobStorage = new AzureBlobStorage(settings);
            this.log = log;
        }

        public async Task Process(byte[] b, FrontDocumentModel docData, string fileName)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(b))
                {
                    using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(stream, true))
                    {
                        string docText = null;

                        using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
                        {
                            docText = sr.ReadToEnd();
                        }

                        foreach (var item in docData.GetData())
                        {
                            Regex regexText = new Regex(item.Key.ToUpper(), RegexOptions.CultureInvariant);
                            docText = regexText.Replace(docText, item.Value);
                        }

                        using (StreamWriter sw = new StreamWriter(wordDoc.MainDocumentPart.GetStream(FileMode.Create)))
                        {
                            sw.Write(docText);
                        }

                        wordDoc.Save();
                    }

                    await azureBlobStorage.UploadAsync(fileName, stream);
                     docData.Status = "Processado";
                    log.Info($"Documento criado!");
                    await azureTableStorage.Update(docData, "document");
                    log.Info($"Documento atualizado no storage");
                }
            }
            catch (Exception ex)
            {
                docData.Status = "Erro ao processar";
                await azureTableStorage.Update(docData, "document");
                log.Info($"Erro ao processar documento");
            }
        }

        public async Task<FrontDocumentModel> RetrieveRecord(CloudTable table, string partitionKey, string rowKey)
        {
            TableOperation tableOperation = TableOperation.Retrieve<FrontDocumentModel>(partitionKey, rowKey);
            TableResult tableResult = await table.ExecuteAsync(tableOperation);
            return tableResult.Result as FrontDocumentModel;
        }
    }
}
