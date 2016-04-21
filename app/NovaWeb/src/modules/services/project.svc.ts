import "angular";
import {ILocalizationService} from "../core/localization";

export interface IProjectService {
    getFolders(id?: number): ng.IPromise<any[]>;

}

export interface IProjectNode {
    Id: number,
    ParentFolderId: number,
    Type: string,
    Name: string,
    Description? : string,
    Children?: IProjectNode[]

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
            .success((result: IProjectNode[]) => {
                angular.forEach(result, (it) => {
                    if (it.Type === "Folder") {
                        it.Children = [];
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
