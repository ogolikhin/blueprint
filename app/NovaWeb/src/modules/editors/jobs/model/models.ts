export enum JobType {
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
    HpAlmRetExportTests = 0x400,
    ExcelImport = 0x800,
    ProjectImport = 0x1000,
    ProjectExport = 0x2000,
    GenerateTests = 0x4000,
    GenerateProcessTests = 0x8000,
}

export enum JobStatus {
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

export interface IJobInfo {
    jobId: number;
    status: JobStatus;
    jobType: JobType;
    project: string;
    submittedDateTime: Date;
    jobStartDateTime: Date;
    jobEndDateTime: Date;
    userId: number;
    userDisplayName: string;
    server: string;
    progress: number;
    output: string;
    statusChanged: boolean;
    hasCancelJob: boolean;
    projectId: number;
}

export interface IJobResult {
    jobInfos: IJobInfo[];
    totalJobCount: number;
}

export interface IAddJobResult {
    jobMessageId: number;
}