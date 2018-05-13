using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilesManager.Models
{
    public class FrontDocumentModel
    {
        public string StudentName { get; set; }
        public string StudentDocument { get; set; }
        public string Course { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string WorkLoad { get; set; }
        public string DateOfIssue { get; set; }

        public Dictionary<string, string> GetData()
        {
            var data = new Dictionary<string, string>();

            data.Add($"{nameof(StudentName)}", StudentName);
            data.Add($"{nameof(StudentDocument)}", StudentDocument);
            data.Add($"{nameof(Course)}", Course);
            data.Add($"{nameof(StartDate)}", StartDate);
            data.Add($"{nameof(EndDate)}", EndDate);
            data.Add($"{nameof(WorkLoad)}", WorkLoad);
            data.Add($"{nameof(DateOfIssue)}", DateOfIssue);

            return data;
        }


    }
}
