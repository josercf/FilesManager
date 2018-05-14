using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using S = DocumentFormat.OpenXml.Spreadsheet.Sheets;
using E = DocumentFormat.OpenXml.OpenXmlElement;

namespace SheetProcess
{
    public class SheetProcessService
    {
        private readonly TraceWriter log;
        private readonly ICollector<FrontDocumentModel> queueCollector;
        private readonly AzureTableStorage azureTableStorage;

        public SheetProcessService(ICollector<FrontDocumentModel> queueCollector, AzureBlobSetings settings, TraceWriter log)
        {
            this.azureTableStorage = new AzureTableStorage(settings);
            this.queueCollector = queueCollector;
            this.log = log;

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

                await azureTableStorage.Insert(document, nameof(document));
                queueCollector.Add(document);
            }
        }

        private void MapDataPosix(SpreadsheetDocument doc, DataPosition dataPos, IEnumerable<Cell> header)
        {
            for (int i = 0; i < header.Count(); i++)
            {
                Cell celHeader = header.ElementAt(i);
                var cellValue = GetCellValue(doc, celHeader);
                dataPos.GetDataPositions(cellValue, celHeader.CellReference.Value);
            }
        }

        private string FindCourseName(SpreadsheetDocument doc, IEnumerable<Cell> cellsTitle)
        {
            for (int i = 0; i < cellsTitle.Count(); i++)
            {
                Cell celCourseTitle = cellsTitle.ElementAt(i);
                var cellValue = GetCellValue(doc, celCourseTitle);

                if (!string.IsNullOrWhiteSpace(cellValue) && 
                    cellValue.StartsWith("Pós-Graduação")) return ExtractCourseName(cellValue);
            }
            return string.Empty;
        }

        private async Task<FrontDocumentModel> ExtractFrontDocumentData(SpreadsheetDocument doc, IEnumerable<Cell> cells,
                                                     string courseName, uint rowIndex, DataPosition data)
        {
            return await Task.Factory.StartNew(() =>
            {
                var documentFront = new FrontDocumentModel(
                    GetCellValue(doc, cells.FindCell(data.NameCol, rowIndex))?.Trim(),
                    GetCellValue(doc, cells.FindCell(data.DocumentCol, rowIndex))?.Trim());

                documentFront.Course = courseName;

                //Find by previus header founded
                documentFront.StartDate = GetCellValue(doc, cells.FindCell(data.StartDateCol, rowIndex))?.ParseDate();
                documentFront.EndDate = GetCellValue(doc, cells.FindCell(data.EndDateCol, rowIndex))?.ParseDate();
                documentFront.WorkLoad = GetCellValue(doc, cells.FindCell(data.WorkLoadCol, rowIndex));
                documentFront.Status = "Aguardando processamento";
                documentFront.DateOfIssue = DateTime.Now.ToString("dd/MM/yyyy") + ".";
                return documentFront;
            });
        }

        private string ExtractCourseName(string courseName) => courseName.Substring("Pós-Graduação em ".Length);

        private string GetCellValue(SpreadsheetDocument doc, Cell cell)
        {
            if (cell is null) return string.Empty;

            string value = cell.CellValue?.InnerText;
            if (cell?.DataType != null && cell?.DataType.Value == CellValues.SharedString)
                return doc.WorkbookPart?.SharedStringTablePart?.SharedStringTable?.ChildElements?.GetItem(int.Parse(value))?.InnerText;

            return value;
        }

        // Retrieve the value of a cell, given a file name, sheet name, 
        // and address name.
        public string GetCellValue(SpreadsheetDocument document,
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
    }
}
