﻿import { ILocalizationService } from "../../core";
import * as Models from "../models/models";

export {Models}

export interface IProjectRepository {
    getFolders(id?: number): ng.IPromise<any[]>;
    getArtifacts(projectId: number, artifactId?: number): ng.IPromise<Models.IArtifact[]>;
    getArtifactDetails(artifactId?: number): ng.IPromise<Models.IArtifactDetails>;
    getProjectMeta(projectId?: number): ng.IPromise<Models.IProjectMeta>;
}

export class ProjectRepository implements IProjectRepository {
    static $inject: [string] = ["$q", "$http", "$log", "localization"];

    constructor(
        private $q: ng.IQService,
        private $http: ng.IHttpService,
        private $log: ng.ILogService,
        private localization: ILocalizationService) {
    }

    public getFolders(id?: number): ng.IPromise<Models.IProjectNode[]> {
        var defer = this.$q.defer<any>();
        this.$http.get<any>(`svc/adminstore/instance/folders/${id || 1}/children`)
            .success((result: Models.IProjectNode[]) => {
                defer.resolve(result);
            }).error((err: any, statusCode: number) => {
                var error = {
                    statusCode: statusCode,
                    message: (err ? err.message : "") || this.localization.get("Folder_NotFound")
                };
                this.$log.error(error);
                defer.reject(error);
            });
        return defer.promise;
    } 

    public getArtifacts(projectId: number, artifactId?: number): ng.IPromise<Models.IArtifact[]> {
        var defer = this.$q.defer<any>();

        let url: string = `svc/artifactstore/projects/${projectId}` + (artifactId ? `/artifacts/${artifactId}` : ``) + `/children`;

        this.$http.get<any>(url)
            .success((result: Models.IArtifact[]) => {
                defer.resolve(result);
            }).error((err: any, statusCode: number) => {
                var error = {
                    statusCode: statusCode,
                    message: (err ? err.Message : "") || this.localization.get("Artifact_NotFound", "Error")
                };
                this.$log.error(error);
                defer.reject(error);
            });
        return defer.promise;
    }

    public getArtifactDetails(artifactId?: number): ng.IPromise<Models.IArtifactDetails> {
        var defer = this.$q.defer<any>();

        let url: string = `svc/artifactstore/artifact/${artifactId}`;

        this.$http.get<any>(url)
            .success((result: Models.IArtifactDetails[]) => {
                defer.resolve(result);
            }).error((err: any, statusCode: number) => {
                var error = {
                    statusCode: statusCode,
                    message: (err ? err.Message : "") || this.localization.get("Artifact_NotFound")
                };
                this.$log.error(error);
                defer.reject(error);
            });
        return defer.promise;
    }

    public getProjectMeta(projectId?: number): ng.IPromise<Models.IProjectMeta> {
        var defer = this.$q.defer<any>();

        let url: string = `svc/artifactstore/projects/${projectId}/meta/customtypes`;

        this.$http.get<any>(url)
            .success((result: Models.IProjectMeta) => {
                defer.resolve(result);
            }).error((err: any, statusCode: number) => {
                var error = {
                    statusCode: statusCode,
                    message: (err ? err.Message : "") || this.localization.get("Artifact_NotFound")
                };
                this.$log.error(error);
                defer.reject(error);
            });
        return defer.promise;
    }


}

