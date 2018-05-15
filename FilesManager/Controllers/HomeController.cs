using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FilesManager.Models.Home;
using FilesManager.Storage;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using FilesManager.Models;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace FilesManager.Controllers
{


    [Authorize]
    public class HomeController : Controller
    {
        private readonly IAzureBlobStorage blobStorage;
        private readonly AzureTableStorage azureTableStorage;
        private readonly IHostingEnvironment _environment;

        public HomeController(IAzureBlobStorage blobStorage,
                              AzureTableStorage azureTableStorage,
                              IHostingEnvironment environment)
        {
            this.azureTableStorage = azureTableStorage;
            this.blobStorage = blobStorage;
            this._environment = environment;
        }

        public async Task<IActionResult> Index()
        {

            var model = new FilesViewModel();
            var blobs = await blobStorage.ListAsync();
            foreach (var item in blobs.Where(c => c.Folder.Equals("docs")))
            {

                var http = new HttpClient();
                var url = string.Format("http://localhost:7071/api/GetDocument?blobname={0}", item.BlobName);
                var response = await http.GetAsync(url);
                var result = await response.Content.ReadAsStringAsync();

                var metaData = JsonConvert.DeserializeObject<Models.FrontDocumentModel>(result);

                var processed = metaData != null &&
                                metaData.Status.ToLower() == "processado";

                model.Files.Add(
                    new FileDetails
                    {
                        Name = item.Name,
                        BlobName = item.BlobName,
                        Processed = processed,
                        Status = metaData?.Status,
                        CreatedAt = metaData?.CreatedAt
                    });
            }

            ViewBag.Message = ViewBag?.Message ?? string.Empty;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(FileInputModel inputModel)
        {
            if (inputModel == null)
                return Content("Argument null");

            if (inputModel.File == null || inputModel.File.Length == 0)
                return Content("file not selected");

            var blobName = inputModel.File.GetFilename();
            var fileStream = await inputModel.File.GetFileStream();

            if (!string.IsNullOrEmpty(inputModel.Folder))
                blobName = string.Format(@"{0}\{1}", inputModel.Folder, blobName);

            await blobStorage.UploadAsync(blobName, fileStream);

            ViewBag.Message = "Estamos processando seus arquivos, por favor aguarde!";
            return RedirectToAction("Index");

        }

        public async Task<IActionResult> Download(string blobName, string name)
        {
            if (string.IsNullOrEmpty(blobName))
                return Content("Blob Name not present");

            var stream = await blobStorage.DownloadAsync(blobName);
            return File(stream.ToArray(), "application/octet-stream", name);
        }

        public async Task<IActionResult> Delete(string blobName)
        {
            if (string.IsNullOrEmpty(blobName))
                return Content("Blob Name not present");

            await blobStorage.DeleteAsync(blobName);

            return RedirectToAction("Index");
        }
    }
}
