import {ILicenseService} from "./license.svc";

export class LicenseServiceMock implements ILicenseService {

    static $inject: [string] = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public getServerLicenseValidity(): ng.IPromise<boolean> {
        return this.$q.resolve(true);
    }
}
