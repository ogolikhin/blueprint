export interface IHeartbeatService {
    isSessionAlive(): ng.IPromise<void>;
}

export class HeartbeatService implements IHeartbeatService {

    static $inject: [string] = ["$q", "$log", "$http"];

    constructor(private $q: ng.IQService,
                private $log: ng.ILogService,
                private $http: ng.IHttpService) {
        // Nothing
    }

    public isSessionAlive(): ng.IPromise<void> {
        const deferred = this.$q.defer<void>();
        this.$http.get<any>("/svc/adminstore/sessions/alive")
            .then((result: ng.IHttpPromiseCallbackArg<void>) => {
                deferred.resolve();
            }, (result: ng.IHttpPromiseCallbackArg<any>) => {
                deferred.reject(result.data);
            });
        return deferred.promise;
    }
}
