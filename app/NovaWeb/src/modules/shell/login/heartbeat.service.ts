export interface IHeartbeatService {
    isSessionAlive(): ng.IHttpPromise<void>;
}

export class HeartbeatService implements IHeartbeatService {

    static $inject: [string] = ["$http"];

    constructor(private $http: ng.IHttpService) {
        // Nothing
    }

    public isSessionAlive(): ng.IHttpPromise<void> {
        return this.$http.get<any>("/svc/adminstore/sessions/alive");
    }
}
