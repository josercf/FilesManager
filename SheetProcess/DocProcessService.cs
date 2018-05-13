using DocumentFormat.OpenXml.Packaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SheetProcess
{
    public class DocProcessService
    {
        public DocProcessService()
        {

        }

        public async Task Process(Stream doc, string fileName)
        {

            try
            {
                var docData = await RetrieveRecord("", "");
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(doc, true))
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
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {

                throw;
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
