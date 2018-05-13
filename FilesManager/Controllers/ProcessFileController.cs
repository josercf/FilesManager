using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using FilesManager.Models;
using FilesManager.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FilesManager.Controllers
{
    [Produces("application/json")]
    [Route("api/ProcessFile")]
    public class ProcessFileController : Controller
    {
        private readonly IHostingEnvironment _environment;
        private readonly IAzureBlobStorage blobStorage;

        public ProcessFileController(IHostingEnvironment environment, IAzureBlobStorage blobStorage)
        {
            _environment = environment;
            this.blobStorage = blobStorage;
        }

        public async Task<IActionResult> Post([FromBody] FrontDocumentModel model)
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "Uploads", "TemplatePosFrente.docx");

            var fileName = $"{model.StudentName.Trim()}-frente.docx";
            var destPath = Path.Combine(_environment.ContentRootPath, "Docs", fileName);

            await CreateNewFile(filePath, destPath);
            await SearchAndReplace(destPath, fileName, model);

            return await Task.FromResult(Ok());
        }

        [NonAction]
        private static async Task CreateNewFile(string sourcePath, string doc)
        {
            // To copy a file to another location and 
            // overwrite the destination file if it already exists.
            await Task.Run(() => System.IO.File.Copy(sourcePath, doc, true));
        }

        [NonAction]
        private async Task SearchAndReplace(string document,string fileName, FrontDocumentModel data)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(document, true))
            {
                string docText = null;
                using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
                {
                    docText = sr.ReadToEnd();
                }

                foreach (var item in data.GetData())
                {
                    Regex regexText = new Regex(item.Key.ToUpper());
                    docText = regexText.Replace(docText, item.Value);
                }

                using (StreamWriter sw = new StreamWriter(wordDoc.MainDocumentPart.GetStream(FileMode.Create)))
                {
                    sw.Write(docText);
                    sw.Close();
                }
            }

            Task.Factory.StartNew(async () =>
            {
                using (var fileStream = System.IO.File.OpenRead(document))
                {
                    await blobStorage.UploadAsync($@"docs/{fileName}", fileStream);
                }
            });
           

            
        }
    }
}