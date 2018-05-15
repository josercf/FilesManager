using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using S = DocumentFormat.OpenXml.Spreadsheet.Sheets;
using E = DocumentFormat.OpenXml.OpenXmlElement;
using FilesManager.DataAccess.Storage.BusinessContracts;
using Microsoft.Azure.WebJobs.Host;
using FilesManager.DataAccess.Storage;
using FilesManager.OficceTasks.Extensions;

namespace FilesManager.OficceTasks
{
    public class SheetProcess : ISheetReader
    {
        private readonly TraceWriter log;
        private readonly IDocumentService documentService;
        private readonly Action<DataAccess.Storage.Models.Document> onRowRead;

        public SheetProcess( IDocumentService documentService, Action<DataAccess.Storage.Models.Document> onRowRead, 
                             TraceWriter log)
        {
            this.documentService = documentService;
            this.onRowRead = onRowRead;
        }

        public async Task ProcessSheet(Stream file)
        {
            using (SpreadsheetDocument mySpreadsheet = SpreadsheetDocument.Open(file, false))
            {
                S sheets = mySpreadsheet.WorkbookPart.Workbook.Sheets;

                Task[] tasks = new Task[sheets.Count()];

                int i = 0;
                // For each sheet, display the sheet information.
                foreach (E sheet in sheets)
                {
                    var sheetName = string.Empty;
                    try
                    {
                        sheetName = sheet.GetAttributes().First(c => c.LocalName == "name").Value;
                        var sheetId = sheet.GetAttributes().First(c => c.LocalName == "id").Value;
                        log.Info($"Sheet Founded: name: {sheetName}, id: {sheetId}");

                        tasks[i] = ReadExcelSheet(mySpreadsheet, sheetId);
                        i++;
                        log.Info($"Sheet {sheetName} add to queue process");
                    }
                    catch (Exception ex)
                    {
                        log.Error($"Error on process sheet {sheetName} -> {ex}");
                    }
                }

                Task.WaitAll(tasks);
                log.Info($"End of file process");
            }
        }
        private async Task ReadExcelSheet(SpreadsheetDocument doc, string workSheetId)
        {
            var workpart = (doc.WorkbookPart.GetPartById(workSheetId) as WorksheetPart);
            var worksheet = workpart.Worksheet;

            if (workpart == null) return;

            var rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>().Skip(5);

            //TODO: verificar se é pos ou extensão
            string courseColPos = string.Empty, courseName = string.Empty;

            var cellsTitle = worksheet.GetFirstChild<SheetData>()
                                      .Descendants<Row>().ElementAt(2)
                                      .Descendants<Cell>();

            courseName = FindCourseName(doc, cellsTitle);

            var dataPos = new DataPosition();
            var header = rows.First().Descendants<Cell>();

            //Map data on sheet
            MapDataPosix(doc, dataPos, header);

            //Loop for all lines, each line a document will be created
            await ProcessData(doc, rows, courseName, dataPos);
        }

        private async Task ProcessData(SpreadsheetDocument doc, IEnumerable<Row> rows, string courseName, DataPosition dataPos)
        {
            foreach (Row row in rows.Skip(1))
            {
                var cells = row.Descendants<Cell>();
                var document = await ExtractFrontDocumentData(doc, cells, courseName,
                                                              row.RowIndex.Value, dataPos);

                if (string.IsNullOrWhiteSpace(document.StudentName)) continue;
                log.Info($"Send data to queue: {document.StudentName}");

                await documentService.Insert(document);
                onRowRead(document);
            }
        }

        private void MapDataPosix(SpreadsheetDocument doc, DataPosition dataPos, IEnumerable<Cell> header)
        {
            for (int i = 0; i < header.Count(); i++)
            {
                Cell celHeader = header.ElementAt(i);
                var cellValue =doc.GetCellValue(celHeader);
                dataPos.GetDataPositions(cellValue, celHeader.CellReference.Value);
            }
        }

        private string FindCourseName(SpreadsheetDocument doc, IEnumerable<Cell> cellsTitle)
        {
            for (int i = 0; i < cellsTitle.Count(); i++)
            {
                Cell celCourseTitle = cellsTitle.ElementAt(i);
                var cellValue = doc.GetCellValue(celCourseTitle);

                if (!string.IsNullOrWhiteSpace(cellValue) && cellValue.StartsWith("Pós-Graduação"))
                    return ExtractCourseName(cellValue);
            }
            return string.Empty;
        }

        private async Task<DataAccess.Storage.Models.Document> 
            ExtractFrontDocumentData(SpreadsheetDocument doc, IEnumerable<Cell> cells,
                                     string courseName, uint rowIndex, DataPosition data)
        {
            return await Task.Factory.StartNew(() =>
            {
                var documentFront = new DataAccess.Storage.Models.Document();

                documentFront.StudentName = doc.GetCellValue(cells.FindCell(data.NameCol, rowIndex))?.Trim();
                documentFront.StudentDocument = doc.GetCellValue(cells.FindCell(data.DocumentCol, rowIndex))?.Trim();

                documentFront.Course = courseName;

                //Find by previus header founded
                documentFront.StartDate = doc.GetCellValue(cells.FindCell(data.StartDateCol, rowIndex))?.ParseDate();
                documentFront.EndDate = doc.GetCellValue(cells.FindCell(data.EndDateCol, rowIndex))?.ParseDate();
                documentFront.WorkLoad = doc.GetCellValue(cells.FindCell(data.WorkLoadCol, rowIndex));
                //TODO: criar enum para status
                documentFront.Status = "Aguardando processamento";
                //TODO: colocar data por extenso
                documentFront.DateOfIssue = DateTime.Now.ToString("dd/MM/yyyy") + ".";
                return documentFront;
            });
        }

        //TODO: verificar document type
        private string ExtractCourseName(string courseName) => courseName.Substring("Pós-Graduação em ".Length);



        // Retrieve the value of a cell, given a file name, sheet name, 
        // and address name.

    }
}
