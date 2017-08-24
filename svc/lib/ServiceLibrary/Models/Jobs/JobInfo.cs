using System;
using Newtonsoft.Json;

namespace ServiceLibrary.Models.Jobs
{
    public enum JobStatus
    {
        Scheduled = 0,

        Terminated = 1,

        Running = 2,

        Completed = 3,

        Failed = 4,

        //Warning = 5,

        Cancelling = 6,

        Suspending = 7,

        Suspended = 8
    }

    /// <summary>
    /// Job type enumeration.
    /// </summary>
    [Flags]
    public enum JobType : long
    {
        None = 0x00,

        System = 0x01,

        DocGen = 0x02,

        TfsExport = 0x04,

        QcExport = 0x08,

        HpAlmRestExport = 0x10,

        TfsChangeSummary = 0x20,

        QcChangeSummary = 0x40,

        HpAlmRestChangeSummary = 0x80,

        TfsExportTests = 0x100,

        QcExportTests = 0x200,

        HpAlmRestExportTests = 0x400,

        ExcelImport = 0x800,

        ProjectImport = 0x1000,

        ProjectExport = 0x2000,

        GenerateTests = 0x4000,

        GenerateProcessTests = 0x8000,

        GenerateUserStories = 0x10000
    }

    public class JobInfo
    {
        public int JobId { get; set; }

        public JobStatus Status { get; set; }

        public JobType JobType { get; set; }

        public string Project { get; set; }

        public DateTime SubmittedDateTime { get; set; }

        public DateTime? JobStartDateTime { get; set; }

        public DateTime? JobEndDateTime { get; set; }

        public int? UserId { get; set; }

        public string UserDisplayName { get; set; }

        public string Server { get; set; }

        public decimal Progress { get; set; }

        public string Output { get; set; }

        public bool StatusChanged { get; set; }

        public bool HasCancelJob { get; set; }

        public int? ProjectId { get; set; }

        [JsonIgnore]
        public string Result { get; set; }
    }
}
