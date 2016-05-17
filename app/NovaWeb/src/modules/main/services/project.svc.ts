/// <reference path="../../core/notification.ts" />
import {ILocalizationService} from "../../core/localization";
import {IProjectNotification} from "./project-notification";
import * as Data from "../repositories/artifacts";

export {Data}


export interface IProjectService {
    getFolders(id?: number): ng.IPromise<any[]>;
    getProject(projectId: number, artifactId?: number): ng.IPromise<Data.IProjectItem[]>;
}

export class ProjectService implements IProjectService {
    static $inject: [string] = ["$q", "$http", "localization", "projectNotification"];

    constructor(
        private $q: ng.IQService,
        private $http: ng.IHttpService,
        private localization: ILocalizationService,
        private notification: IProjectNotification) {
    }

    public getFolders(id?: number): ng.IPromise<Data.IProjectNode[]> {
        var defer = this.$q.defer<any>();
        this.$http.get<any>(`svc/adminstore/instance/folders/${id || 1}/children`)
            .success((result: Data.IProjectNode[]) => {
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

    public getProject(projectId: number, artifactId?: number): ng.IPromise<Data.IProjectItem[]> {
        var defer = this.$q.defer<any>();
        if (!projectId) {
            throw new Error("Inavlid parameter ");
        }

        let url: string = `svc/artifactstore/projects/${projectId}` + (artifactId ? `/artifacts/${artifactId}` : `` ) + `/children`;

        this.$http.get<any>(url)
            .success((result: Data.IProjectItem[]) => {
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

