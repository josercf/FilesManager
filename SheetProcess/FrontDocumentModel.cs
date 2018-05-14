using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace SheetProcess
{
    public class FrontDocumentModel : TableEntity
    {
        public FrontDocumentModel()
        {

        }
        public FrontDocumentModel(string studentName, string studentDocument)
        {
            if(string.IsNullOrWhiteSpace(studentName) || 
                string.IsNullOrWhiteSpace(studentDocument))
                return;

            this.PartitionKey = studentName;
            this.RowKey = studentName;

            StudentName = studentName;
            StudentDocument = studentDocument;
            CreatedAt = DateTime.Now;
        }

        public string StudentName { get; set; }
        public string StudentDocument { get; set; }
        public string Course { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string WorkLoad { get; set; }
        public string DateOfIssue { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }



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
