using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FilesManager.Models.Home;
using FilesManager.Storage;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using S = DocumentFormat.OpenXml.Spreadsheet.Sheets;
using E = DocumentFormat.OpenXml.OpenXmlElement;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using System.Net.Http;
using FilesManager.Models;
using Newtonsoft.Json;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace FilesManager.Controllers
{
    public static class OpnXmlExtensions
    {
        public static Cell FindCell(this IEnumerable<Cell> cells, string col, uint row)
        {
            col = Regex.Replace(col, @"[\d-]", string.Empty);
            return cells.FirstOrDefault(c => c.CellReference.Value == $"{col}{row}");
        }
    }

    [Authorize]
    public class HomeController : Controller
    {
        private readonly IAzureBlobStorage blobStorage;
        private readonly IHostingEnvironment _environment;

        public HomeController(IAzureBlobStorage blobStorage, IHostingEnvironment environment)
        {
            this.blobStorage = blobStorage;
            this._environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var model = new FilesViewModel();
            foreach (var item in await blobStorage.ListAsync())
            {
                model.Files.Add(
                    new FileDetails { Name = item.Name, BlobName = item.BlobName });
            }
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

            try
            {
                using (SpreadsheetDocument mySpreadsheet = SpreadsheetDocument.Open(fileStream, false))
                {
                    S sheets = mySpreadsheet.WorkbookPart.Workbook.Sheets;

                    // For each sheet, display the sheet information.
                    foreach (E sheet in sheets)
                    {
                        try
                        {

                            var sheetName = sheet.GetAttributes().First(c => c.LocalName == "name").Value;
                            var sheetId = sheet.GetAttributes().First(c => c.LocalName == "id").Value;

                            //log.Info($"Sheet Founded: name: {sheetName}, id: {sheetId}");
                            await ReadExcelSheet(mySpreadsheet, sheetId, sheetName);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            catch (Exception e)
            {
                //log.Info($"Ocorreu um erro: {e}");
            }

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

        private async Task ReadExcelSheet(SpreadsheetDocument doc, string workSheetId,
                                               string sheetName)
        {
            var worksheet = (doc.WorkbookPart.GetPartById(workSheetId) as WorksheetPart).Worksheet;
            var rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>().Skip(5);

            //verificar se é pos ou extensão
            const string COURSE = "C3";
            string startDateCol = string.Empty, endDateCol = string.Empty, workLoadCol = string.Empty;

            var courseName = ExtractCourseName(
                                GetCellValue(doc, sheetName, COURSE));

            //var metaHeader = new Dictionary<string, string>();
            var header = rows.First().Descendants<Cell>();
            for (int i = 0; i < header.Count(); i++)
            {
                Cell celHeader = header.ElementAt(i);
                var cellValue = GetCellValue(doc, celHeader);
                //metaHeader.Add(celHeader.CellReference.Value, cellValue);

                if (cellValue is null) continue;
                if (cellValue == "Início") startDateCol = celHeader.CellReference.Value;
                if (cellValue == "Término") endDateCol = celHeader.CellReference.Value;
                if (cellValue == "Carga Horária Total") workLoadCol = celHeader.CellReference.Value;

                if (!string.IsNullOrWhiteSpace(startDateCol) &&
                   !string.IsNullOrWhiteSpace(endDateCol) &&
                    !string.IsNullOrWhiteSpace(workLoadCol)) break;
            }

            foreach (Row row in rows.Skip(1))
            {
                var cells = row.Descendants<Cell>();
                var document = ExtractFrontDocumentData(doc, cells, courseName, row.RowIndex.Value,
                                         startDateCol, endDateCol, workLoadCol);

                if (string.IsNullOrWhiteSpace(document.StudentName)) continue;

                var filePath = Path.Combine(_environment.ContentRootPath, "Uploads", "TemplatePosFrente.docx");

                var fileName = $"{document.StudentName.Trim()}-frente.docx";
                var destPath = Path.Combine(_environment.ContentRootPath, "Docs", fileName);

                await CreateNewFile(filePath, destPath);
                await SearchAndReplace(destPath, fileName, document);
            }
        }

        private FrontDocumentModel ExtractFrontDocumentData(SpreadsheetDocument doc, IEnumerable<Cell> cells,
                                                     string courseName, uint rowIndex,
                                                     string startDateCol, string endDateCol, string workLoadCol)
        {
            const string STUDENT_NAME = "B";
            const string STUDENT_DOCUMENT = "E";

            var documentFront = new FrontDocumentModel();
            documentFront.StudentName = GetCellValue(doc, cells.FindCell(STUDENT_NAME, rowIndex));
            documentFront.StudentDocument = GetCellValue(doc, cells.FindCell(STUDENT_DOCUMENT, rowIndex));
            documentFront.Course = courseName;

            //Find by previus header founded
            documentFront.StartDate = ParseDate(GetCellValue(doc, cells.FindCell(startDateCol, rowIndex)));
            documentFront.EndDate = ParseDate(GetCellValue(doc, cells.FindCell(endDateCol, rowIndex)));
            documentFront.WorkLoad = GetCellValue(doc, cells.FindCell(workLoadCol, rowIndex));

            documentFront.DateOfIssue = DateTime.Now.ToString("dd/MM/yyyy") + ".";

            return documentFront;
        }



        private static string ParseDate(string date)
        {
            if (string.IsNullOrEmpty(date)) return null;

            try
            {
                string[] monthsPTBR = { "janeiro", "fevereiro", "maarço", "abril", "maio", "junho", "julho", "agosto", "setembro", "outubro", "novembro", "dezembro" };
                var parts = date.Split(' ');

                return new DateTime(int.Parse(parts[4]), Array.IndexOf(monthsPTBR, parts[2].ToLower()) + 1, int.Parse(parts[0])).ToString("dd/MM/yyyy");
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static string ExtractCourseName(string courseName) => courseName.Substring("Pós-Graduação em ".Length);

        private static string GetCellValue(SpreadsheetDocument doc, Cell cell)
        {
            if (cell is null) return string.Empty;

            string value = cell.CellValue?.InnerText;
            if (cell?.DataType != null && cell?.DataType.Value == CellValues.SharedString)
                return doc.WorkbookPart?.SharedStringTablePart?.SharedStringTable?.ChildElements?.GetItem(int.Parse(value))?.InnerText;

            return value;
        }

        // Retrieve the value of a cell, given a file name, sheet name, 
        // and address name.
        public static string GetCellValue(SpreadsheetDocument document,
                                          string sheetName,
                                          string addressName)
        {
            string value = null;
            // Retrieve a reference to the workbook part.
            WorkbookPart wbPart = document.WorkbookPart;

            // Find the sheet with the supplied name, and then use that 
            // Sheet object to retrieve a reference to the first worksheet.
            Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().
              Where(s => s.Name == sheetName).FirstOrDefault();

            // Throw an exception if there is no sheet.
            if (theSheet == null)
                throw new ArgumentException("sheetName");

            // Retrieve a reference to the worksheet part.
            WorksheetPart wsPart =
                (WorksheetPart)(wbPart.GetPartById(theSheet.Id));

            // Use its Worksheet property to get a reference to the cell 
            // whose address matches the address you supplied.
            Cell theCell = wsPart.Worksheet.Descendants<Cell>().
              Where(c => c.CellReference == addressName).FirstOrDefault();

            // If the cell does not exist, return an empty string.
            if (theCell != null)
            {
                value = theCell.InnerText;

                // If the cell represents an integer number, you are done. 
                // For dates, this code returns the serialized value that 
                // represents the date. The code handles strings and 
                // Booleans individually. For shared strings, the code 
                // looks up the corresponding value in the shared string 
                // table. For Booleans, the code converts the value into 
                // the words TRUE or FALSE.
                if (theCell.DataType != null)
                {
                    switch (theCell.DataType.Value)
                    {
                        case CellValues.SharedString:

                            // For shared strings, look up the value in the
                            // shared strings table.
                            var stringTable =
                                wbPart.GetPartsOfType<SharedStringTablePart>()
                                .FirstOrDefault();

                            // If the shared string table is missing, something 
                            // is wrong. Return the index that is in
                            // the cell. Otherwise, look up the correct text in 
                            // the table.
                            if (stringTable != null)
                            {
                                value =
                                    stringTable.SharedStringTable
                                    .ElementAt(int.Parse(value)).InnerText;
                            }
                            break;

                        case CellValues.Boolean:
                            switch (value)
                            {
                                case "0":
                                    value = "FALSE";
                                    break;
                                default:
                                    value = "TRUE";
                                    break;
                            }
                            break;
                    }
                }
            }
            return value;
        }

        [NonAction]
        private static async Task CreateNewFile(string sourcePath, string doc)
        {
            await Task.Run(() => System.IO.File.Copy(sourcePath, doc, true));
        }

        [NonAction]
        private async Task SearchAndReplace(string document, string fileName, FrontDocumentModel data)
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
            //using (var fileStream = System.IO.File.OpenRead(document))
            //{
            //    await blobStorage.UploadAsync($@"docs/{fileName}", fileStream);
            //}
        }
    }
}
