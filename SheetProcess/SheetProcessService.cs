using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using S = DocumentFormat.OpenXml.Spreadsheet.Sheets;
using E = DocumentFormat.OpenXml.OpenXmlElement;
using A = DocumentFormat.OpenXml.OpenXmlAttribute;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SheetProcess
{
    public class SheetProcessService
    {
        private readonly TraceWriter log;
        private readonly ICollector<FrontDocumentModel> queueCollector;

        public SheetProcessService(ICollector<FrontDocumentModel> queueCollector, TraceWriter log)
        {

        }
        public async Task ProcessSheet(Stream file)
        {
            using (SpreadsheetDocument mySpreadsheet = SpreadsheetDocument.Open(file, false))
            {
                S sheets = mySpreadsheet.WorkbookPart.Workbook.Sheets;

                // For each sheet, display the sheet information.
                foreach (E sheet in sheets)
                {

                    var sheetName = sheet.GetAttributes().First(c => c.LocalName == "name").Value;
                    var sheetId = sheet.GetAttributes().First(c => c.LocalName == "id").Value;

                    log.Info($"Sheet Founded: name: {sheetName}, id: {sheetId}");
                    await ReadExcelSheet(mySpreadsheet, sheetId, sheetName, queueCollector, log);
                    break;
                }
            }
        }
        private async Task ReadExcelSheet(SpreadsheetDocument doc, string workSheetId,
                                                string sheetName, ICollector<FrontDocumentModel> queueCollector, TraceWriter log)
        {
            var worksheet = (doc.WorkbookPart.GetPartById(workSheetId) as WorksheetPart).Worksheet;
            var rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>().Skip(5);

            //verificar se é pos ou extensão
            const string COURSE = "F3";
            string startDateCol = string.Empty, endDateCol = string.Empty, workLoadCol = string.Empty;
            var courseName = ExtractCourseName(GetCellValue(doc, sheetName, COURSE));

            //var metaHeader = new Dictionary<string, string>();
            var header = rows.First().Descendants<Cell>();
            for (int i = 0; i < header.Count(); i++)
            {
                Cell celHeader = header.ElementAt(i);
                var cellValue = GetCellValue(doc, celHeader);
                //metaHeader.Add(celHeader.CellReference.Value, cellValue);

                if (cellValue == "Início") startDateCol = celHeader.CellReference.Value;
                if (cellValue == "Término") endDateCol = celHeader.CellReference.Value;
                if (cellValue == "Carga Horária Total") workLoadCol = celHeader.CellReference.Value;
            }

            foreach (Row row in rows.Skip(1))
            {
                var cells = row.Descendants<Cell>();
                var document = await ExtractFrontDocumentData(doc, cells, courseName, row.RowIndex.Value,
                                                              startDateCol, endDateCol, workLoadCol);

                if (string.IsNullOrWhiteSpace(document.StudentName)) continue;
                log.Info($"Send data to queue: {document.StudentName}");
                queueCollector.Add(document);
                //using(var httpClient = new HttpClient())
                //{
                //    await httpClient.PostAsJsonAsync("http://localhost:16052/api/ProcessFile", document);
                //}  
            }
        }


        private async Task<FrontDocumentModel> ExtractFrontDocumentData(SpreadsheetDocument doc, IEnumerable<Cell> cells,
                                                     string courseName, uint rowIndex,
                                                     string startDateCol, string endDateCol, string workLoadCol)
        {
            const string STUDENT_NAME = "E";
            const string STUDENT_DOCUMENT = "H";

            return await Task.Factory.StartNew(() =>
            {
                var documentFront = new FrontDocumentModel();
                documentFront.StudentName = GetCellValue(doc, cells.FindCell(STUDENT_NAME, rowIndex));
                documentFront.StudentDocument = GetCellValue(doc, cells.FindCell(STUDENT_DOCUMENT, rowIndex));
                documentFront.Course = courseName;

                //Find by previus header founded
                documentFront.StartDate = GetCellValue(doc, cells.FindCell(startDateCol, rowIndex)).ParseDate();
                documentFront.EndDate = GetCellValue(doc, cells.FindCell(endDateCol, rowIndex)).ParseDate();
                documentFront.WorkLoad = GetCellValue(doc, cells.FindCell(workLoadCol, rowIndex)).ParseDate();

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
