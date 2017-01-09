import {IJobsService} from "./jobs.svc";
import {IJobInfo, IJobResult} from "./model/models";

export class JobsServiceMock implements IJobsService {
    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public getJobs(page?: number, pageSize?: number): ng.IPromise<IJobResult> {
        return this.$q.resolve<IJobResult>(<IJobResult>{});
    }
    
    public getJob(jobId: number): ng.IPromise<IJobInfo> {
        return this.$q.resolve<IJobInfo>(<IJobInfo>{});
    }
}
