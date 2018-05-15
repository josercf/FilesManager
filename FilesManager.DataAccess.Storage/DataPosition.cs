namespace FilesManager.DataAccess.Storage
{
    public class DataPosition
    {
        private const string NameTitle = "Nome";
        private const string DocumentTitle = "RG";
        private const string StartDateTitle = "Início";
        private const string EndDateTitle = "Término";
        private const string WorkLoadTitle = "Carga Horária Total";

        public string NameCol { get; private set; }
        public string DocumentCol { get; private set; }
        public string StartDateCol { get; private set; }
        public string EndDateCol { get; private set; }
        public string WorkLoadCol { get; private set; }

        public void GetDataPositions(string cellValue, string cellPosition)
        {
            switch (cellValue)
            {
                case NameTitle:
                    NameCol = cellPosition;
                    break;
                case DocumentTitle:
                    DocumentCol = cellPosition;
                    break;
                case StartDateTitle:
                    StartDateCol = cellPosition;
                    break;
                case EndDateTitle:
                    EndDateCol = cellPosition;
                    break;
                case WorkLoadTitle:
                    WorkLoadCol = cellPosition;
                    break;
                default:
                    break;
            }

        }

        public bool IsCompletedFields()
        {
            return !string.IsNullOrEmpty(NameCol) &&
                !string.IsNullOrEmpty(DocumentCol) &&
                !string.IsNullOrEmpty(StartDateCol) &&
                !string.IsNullOrEmpty(EndDateCol) &&
                !string.IsNullOrEmpty(WorkLoadCol);
        }

    }
}
