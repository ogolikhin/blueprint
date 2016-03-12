﻿import "angular";

export interface IAuth {
    authenticated: ng.IPromise<boolean>;
}

export class AuthSvc implements IAuth {

    static $inject: [string] = ["$q"];
    constructor(private $q: ng.IQService) {
    }

    get authenticated(): ng.IPromise<boolean> {
        return this.$q.resolve(false);
    }
}