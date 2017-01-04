namespace ServiceLibrary.Models.Jobs
{
    public class ProjectExportTaskStatus
    {
        public int Id { get; set; }

        public ProjectExportResultDetails Details { get; set; }

        public string Message { get; set; }

    }
}
