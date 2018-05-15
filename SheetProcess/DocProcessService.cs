using DocumentFormat.OpenXml.Packaging;
using FilesManager.DataAccess.Storage;
using FilesManager.DataAccess.Storage.BusinessContracts;
using FilesManager.DataAccess.Storage.Contracts;
using FilesManager.DataAccess.Storage.Infraestructure;
using FilesManager.DataAccess.Storage.Models;
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
        private readonly IDocumentService documentService;
        private readonly IAzureBlobStorage azureBlobStorage;
        private readonly TraceWriter log;

        public DocProcessService(StorageAccountSettings settings, IDocumentService documentService,  TraceWriter log)
        {
            this.documentService = documentService;
            this.azureBlobStorage = new AzureBlobStorage(settings);
            this.log = log;
        }

        public async Task Process(byte[] b, Document docData, string fileName)
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
                    await documentService.Update(docData);
                    log.Info($"Documento atualizado no storage");
                }
            }
            catch (Exception ex)
            {
                docData.Status = "Erro ao processar";
                await documentService.Update(docData);
                log.Info($"Erro ao processar documento");
            }
        }
    }
}
