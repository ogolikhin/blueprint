using System;

namespace ServiceLibrary.Models.Jobs
{
    public class ProjectExportResultDetails
    {
        public Guid FileGuid { get; set; }

        public string Name { get; set; }

        public long Size { get; set; }

        public int Artifacts { get; set; }
    }
}
