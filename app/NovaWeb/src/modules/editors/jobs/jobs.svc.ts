import {IJobInfo, IJobResult} from "./model/models";

export interface IJobsService {
    getJobs(page?: number, pageSize?: number): ng.IPromise<IJobResult>;
    getJob(jobId: number): ng.IPromise<IJobInfo>;
}

export class JobsService implements IJobsService {
    static $inject = [
        "$q",
        "$http",
        "$log"
    ];

    constructor(
        private $q: ng.IQService,
        private $http: ng.IHttpService,
        private $log: ng.ILogService
    ) {
    }

    private getUrl(): string {
        return `/svc/adminstore/jobs/`;
    }

    public getJobs(page: number = null, pageSize: number = null, timeout?: ng.IPromise<void>): ng.IPromise<IJobResult> {
        this.$log.debug(`getting jobs page ${pageSize}, page ${pageSize}`);
        const deferred = this.$q.defer();
        const request: ng.IRequestConfig = {
            method: "GET",
            url: this.getUrl(),
            params: {page: page, pageSize: pageSize},
            timeout: timeout
        };

        this.$http(request)
            .then(
                (result) => {
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
            url: `${this.getUrl()}${jobId}`,
            params: {},
            timeout: timeout
        };

        this.$http(request)
            .then(
                (result) => {
                    deferred.resolve(result.data);
                },
                (error) => {
                    deferred.reject(error);
                }
            );

        return deferred.promise;
    }
}
