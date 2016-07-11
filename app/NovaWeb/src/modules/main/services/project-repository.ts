import { ILocalizationService } from "../../core";
import * as Models from "../models/models";

export {Models}

export interface IProjectRepository {
    getFolders(id?: number): ng.IPromise<any[]>;
    getArtifacts(projectId: number, artifactId?: number): ng.IPromise<Models.IArtifact[]>;
    getArtifactDetails(projectId: number, artifactId: number): ng.IPromise<Models.IArtifact>;
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
                this.$log.error(err);
                var error = {
                    statusCode: statusCode,
                    message: (err ? err.message : "") || this.localization.get("Folder_NotFound")
                };
                defer.reject(error);
            });
        return defer.promise;
    } 
    public getProject(id?: number): ng.IPromise<Models.IProjectNode[]> {
        var defer = this.$q.defer<any>();
        this.$http.get<any>(`svc/adminstore/instance/projects/${id}`)
            .success((result: Models.IProjectNode[]) => {
                defer.resolve(result);
            }).error((err: any, statusCode: number) => {
                this.$log.error(err);
                var error = {
                    statusCode: statusCode,
                    message: (err ? err.message : "") || this.localization.get("Project_NotFound")
                };
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
                this.$log.error(err);
                var error = {
                    statusCode: statusCode,
                    message: (err ? err.message : "") || this.localization.get("Artifact_NotFound", "Error")
                };
                defer.reject(error);
            });
        return defer.promise;
    }

    public getArtifactDetails(projectId: number, artifactId: number): ng.IPromise<Models.IArtifact> {
        var defer = this.$q.defer<any>();

        const request: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}`,
            method: "GET",
            //params: {
            //    types: true
            //}
        };

        this.$http(request)
            .success((result: Models.IArtifact[]) => {
                defer.resolve(result);
            }).error((err: any, statusCode: number) => {
                this.$log.error(err);
                var error = {
                    statusCode: statusCode,
                    message: (err ? err.message : "") || this.localization.get("Artifact_NotFound")
                };
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
                this.$log.error(err);
                var error = {
                    statusCode: statusCode,
                    message: (err ? err.message : "") || this.localization.get("Project_NotFound")
                };
                defer.reject(error);
            });
        return defer.promise;
    }


}

