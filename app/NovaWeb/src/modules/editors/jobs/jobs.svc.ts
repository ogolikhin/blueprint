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
    GenerateTests = 0x4000
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


export interface IJobsService {
    getJobs(page?: number, pageSize?: number): ng.IPromise<IJobInfo[]>;
    getJob(jobId: number): ng.IPromise<IJobInfo>;
}

export class JobsService implements IJobsService {
    static $inject = [
        "$q",
        "$http",
        "$log"
    ];

    constructor(private $q: ng.IQService,
                private $http: ng.IHttpService,
                private $log: ng.ILogService) {
    }

    private appendParameters(url: string, page: number, pageSize: number): string {
        if (page) {
            url = url + `?page=${page}`;
            if (pageSize) {
                url = url + `&pageSize=${pageSize}`;
            }
        } else if (pageSize) {
            url = url + `?pageSize=${pageSize}`;
        }
        return url;
    }

    private getUrl(page: number, pageSize: number): string {
        const url = `/svc/adminstore/jobs/`;
        return this.appendParameters(url, page, pageSize);
    }

    public getJobs(page: number = null, pageSize: number = null, timeout?: ng.IPromise<void>): ng.IPromise<IJobInfo[]> {
        this.$log.debug(`getting jobs page ${pageSize}, page ${pageSize}`);
        const deferred = this.$q.defer();
        const request: ng.IRequestConfig = {
            method: "GET",
            url: this.getUrl(page, pageSize),
            params: {},
            timeout: timeout
        };

        this.$http(request).then((result) => {
                deferred.resolve(result.data);
            },
            (error) => {
                deferred.reject(error);
            }
        );

        return deferred.promise;
    }

    public getJob(jobId: number, timeout?: ng.IPromise<void>): ng.IPromise<IJobInfo> {        
        this.$log.debug(`getting job info for job id ${jobId}`);
        const deferred = this.$q.defer();

        const request: ng.IRequestConfig = {
            method: "GET",
            url: this.getUrl(null, null) + jobId,
            params: {},
            timeout: timeout
        };
        

        this.$http(request).then((result) => {
                deferred.resolve(result.data);
            },
            (error) => {
                deferred.reject(error);
            }
        );

        return deferred.promise;
    }
}
