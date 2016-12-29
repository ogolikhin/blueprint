namespace Model.JobModel.Enums
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
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

        GenerateTests = 0x4000
    }

    public enum AlmJobType
    {
        ChangeSummary,
        AlmExport
    }
}
