import "angular";

export interface IAuth {
    authenticated: ng.IPromise<boolean>;
}

export class AuthSvc implements IAuth {

    static $inject: [string] = ["$q", "$log"];
    constructor(private $q: ng.IQService, private $log: ng.ILogService) {
    }

    get authenticated(): ng.IPromise<boolean> {
        return this.$q.resolve(false);
    }
}