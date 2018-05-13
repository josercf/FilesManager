using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SheetProcess
{
    public static class OpenXmlSheetExtensions
    {
        public  static Cell FindCell(this IEnumerable<Cell> cells, string col, uint row)
        {
            col = Regex.Replace(col, @"[\d-]", string.Empty);
            return cells.FirstOrDefault(c => c.CellReference.Value == $"{col}{row}");
        }
    }
}
