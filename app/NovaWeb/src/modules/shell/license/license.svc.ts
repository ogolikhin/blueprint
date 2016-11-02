import "angular";

export interface ILicenseService {
    getServerLicenseValidity(): ng.IPromise<boolean>;
}

export class LicenseService implements ILicenseService {

    static $inject: [string] = ["$q", "$http"];

    constructor(private $q: ng.IQService,
                private $http: ng.IHttpService) {
    }

    private _isServerLicenseValid: ng.IPromise<boolean>;
    public getServerLicenseValidity(): ng.IPromise<boolean> {
        if (!this._isServerLicenseValid) {
            const defer = this.$q.defer<boolean>();
            this.$http.post(`/svc/shared/licenses/verifyStorytellerAccess`, "")
                .then(() => defer.resolve(true))
                .catch(() => defer.resolve(false));
            this._isServerLicenseValid = defer.promise;
        }
        return this._isServerLicenseValid;
    }
}
