/// <reference path="../../core/notification.ts" />
import {ILocalizationService} from "../../core/localization";
import {IProjectNotification} from "./project-notification";
import * as Data from "../repositories/artifacts";

export {Data}


export interface IProjectService {
    getFolders(id?: number): ng.IPromise<any[]>;
    getProject(id?: number): ng.IPromise<Data.IProjectItem[]>;
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
                //angular.forEach(result, (it) => {
                //    if (it.Type === "Folder") {
                //        it.Children = new Array<Data.IProjectNode>();
                //    }
                //});
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

    public getProject(id?: number): ng.IPromise<Data.IProjectItem[]> {
        var defer = this.$q.defer<any>();
        this.$http.get<any>(`svc/artifactstore/projects/${id}/children`)
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

