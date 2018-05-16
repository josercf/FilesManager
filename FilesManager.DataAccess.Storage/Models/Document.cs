using System;
using System.Collections.Generic;

namespace FilesManager.DataAccess.Storage.Models
{
    public class Document : EntityBase
    {
        public string StudentName { get; set; }
        public string StudentDocument { get; set; }
        public string Course { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string WorkLoad { get; set; }
        public string DateOfIssue { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DocumentFront { get { return $"{folderName}/{StudentName}-frente{documentExtension}"; } }
        public string DocumentBack { get { return $"{folderName}/{StudentName}-verso{documentExtension}"; } }
        public string Status { get; set; }
        public string CourseType { get; set; }

        private string folderName = "docs";
        private string documentExtension = ".docx";

        public Dictionary<string, string> GetData()
        {
            var data = new Dictionary<string, string>
            {
                { nameof(StudentName), StudentName },
                { nameof(StudentDocument), StudentDocument },
                { nameof(Course), Course },
                { nameof(StartDate), StartDate },
                { nameof(EndDate), EndDate },
                { nameof(WorkLoad), WorkLoad },
                { nameof(DateOfIssue), DateOfIssue }
            };

            return data;
        }

    }
}
