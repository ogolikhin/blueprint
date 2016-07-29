import * as Models from "../models/models";
import { ILocalizationService } from "../../core";
export { Models }

export interface IProjectRepository {
    getFolders(id?: number): ng.IPromise<any[]>;
    getArtifacts(projectId: number, artifactId?: number): ng.IPromise<Models.IArtifact[]>;
    getProject(id?: number): ng.IPromise<Models.IProjectNode>;
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
        this.$http.get<Models.IProjectNode[]>(`svc/adminstore/instance/folders/${id || 1}/children`).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IProjectNode[]>) => defer.resolve(result.data),
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                this.$log.error(errResult.data);
                var error = {
                    statusCode: errResult.status,
                    message: (errResult.data ? errResult.data.message : "") || this.localization.get("Folder_NotFound")
                };
                defer.reject(error);
            }
        );
        return defer.promise;
    } 
    public getProject(id?: number): ng.IPromise<Models.IProjectNode> {
        var defer = this.$q.defer<any>();
        this.$http.get<Models.IProjectNode>(`svc/adminstore/instance/projects/${id}`).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IProjectNode>) => {
                defer.resolve(result.data);
            },
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                this.$log.error(errResult.data);
                var error = {
                    statusCode: errResult.status,
                    message: (errResult.data ? errResult.data.message : "") || this.localization.get("Project_NotFound")
                };
                defer.reject(error);
            }
        );
        return defer.promise;
    } 

    public getArtifacts(projectId: number, artifactId?: number): ng.IPromise<Models.IArtifact[]> {
        var defer = this.$q.defer<any>();

        let url: string = `svc/artifactstore/projects/${projectId}` + (artifactId ? `/artifacts/${artifactId}` : ``) + `/children`;

        this.$http.get<Models.IArtifact[]>(url).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IArtifact[]>) => {
                defer.resolve(result.data);
            },
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                this.$log.error(errResult.data);
                var error = {
                    statusCode: errResult.status,
                    message: (errResult.data ? errResult.data.message : "") || this.localization.get("Artifact_NotFound", "Error")
                };
                defer.reject(error);
            }
        );
        return defer.promise;
    }



    public getProjectMeta(projectId?: number): ng.IPromise<Models.IProjectMeta> {
        var defer = this.$q.defer<any>();

        let url: string = `svc/artifactstore/projects/${projectId}/meta/customtypes`;

        this.$http.get<Models.IProjectMeta>(url).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IProjectMeta>) => {
                defer.resolve(result.data);
            },
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                this.$log.error(errResult.data);
                var error = {
                    statusCode: errResult.status,
                    message: (errResult.data ? errResult.data.message : "") || this.localization.get("Project_NotFound")
                };
                defer.reject(error);
            }
        );
        return defer.promise;
    }


}

