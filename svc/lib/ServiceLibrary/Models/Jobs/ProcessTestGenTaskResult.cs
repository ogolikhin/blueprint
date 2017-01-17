using System;

namespace ServiceLibrary.Models.Jobs
{
    [Serializable]
    public class ProcessTestGenTaskResult
    {
        public int Id { get; set; }

        public Guid CsvFileGuid { get; set; }
    }
}
