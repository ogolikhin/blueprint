import "angular";
import {ILocalizationService} from "../core/localization";

export interface IProjectService {
    getFolders(id?: number): ng.IPromise<any[]>;

}
export class ProjectService implements IProjectService {
    static $inject: [string] = ["$q", "$http", "localization"];
    constructor(
        private $q: ng.IQService,
        private $http: ng.IHttpService,
        private localization: ILocalizationService) {
    }

    public getFolders(id?: number): ng.IPromise<any[]> {
        var defer = this.$q.defer<any>();
        this.$http.get<any>(`svc/adminstore/instance/folders/${id || 1}/children`)
            .success((result: any) => {
                angular.forEach(result, (it) => {
                    if (it.Type === "Folder") {
                        it.children = [];
                    }
                });
                defer.resolve(result);
            }).error((err: any, statusCode: number) => {
                var error = {
                    statusCode: statusCode,
                    message: (err ? err.Message : "") || this.localization.get("", "Error")
                };
                defer.reject(error);
            });

        return defer.promise;
    }
}
