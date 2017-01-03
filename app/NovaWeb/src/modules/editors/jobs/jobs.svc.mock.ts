import {IJobInfo, IJobsService} from "./jobs.svc";
export class JobsServiceMock implements IJobsService {
    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public getJobs(page?: number, pageSize?: number): ng.IPromise<IJobInfo[]> {
        return this.$q.resolve<IJobInfo[]>(<IJobInfo[]>{});
    }
}
