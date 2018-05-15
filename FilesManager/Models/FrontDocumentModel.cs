using System.Collections.Generic;
using System.Threading.Tasks;
using FilesManager.Storage;

namespace FilesManager.Models
{
    public class FrontDocumentModel
    {

        private AzureTableStorage azureTableStorage;

        public string StudentName { get; set; }
        public string StudentDocument { get; set; }
        public string Course { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string WorkLoad { get; set; }
        public string DateOfIssue { get; set; }


        public async Task<FrontDocumentModel> GetData()
        {
            try
            {
                return await azureTableStorage.GetAll<FrontDocumentModel>("document");

            }
            catch (System.Exception ex)
            {

                throw;
            }

            //var data = new Dictionary<string, string>
            //{
            //    { nameof(StudentName), StudentName },
            //    { nameof(StudentDocument), StudentDocument },
            //    { nameof(Course), Course },
            //    { nameof(StartDate), StartDate },
            //    { nameof(EndDate), EndDate },
            //    { nameof(WorkLoad), WorkLoad },
            //    { nameof(DateOfIssue), DateOfIssue }
            //};

            //return data;
        }


        public Dictionary<string, string> Get()
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
