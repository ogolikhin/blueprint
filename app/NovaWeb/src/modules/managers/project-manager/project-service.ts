import { Models } from "../../main/models";

export interface IProjectService {
    getFolders(id?: number): ng.IPromise<Models.IProjectNode[]>;
    getArtifacts(projectId: number, artifactId?: number): ng.IPromise<Models.IArtifact[]>;
    getProject(id?: number): ng.IPromise<Models.IProjectNode>;
    getProjectMeta(projectId?: number): ng.IPromise<Models.IProjectMeta>;
    getSubArtifactTree(artifactId: number): ng.IPromise<Models.ISubArtifactNode[]>;
}

export class ProjectService implements IProjectService {
    static $inject: [string] = ["$q", "$http"];

    constructor(
        private $q: ng.IQService,
        private $http: ng.IHttpService) {
    }

    public getFolders(id?: number): ng.IPromise<Models.IProjectNode[]> {
        var defer = this.$q.defer<any>();
        this.$http.get<Models.IProjectNode[]>(`svc/adminstore/instance/folders/${id || 1}/children`).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IProjectNode[]>) => defer.resolve(result.data),
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                var error = {
                    statusCode: errResult.status,
                    message: "Folder_NotFound"
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
                if (!errResult) {
                    defer.reject();
                    return;
                }
                var error = {
                    statusCode: errResult.status,
                    message: "Project_NotFound"
                };
                defer.reject(error);
            }
        );
        return defer.promise;
    } 

    public getArtifacts(projectId: number, artifactId?: number): ng.IPromise<Models.IArtifact[]> {
        var defer = this.$q.defer<any>();
        if (projectId && projectId === artifactId) {
            artifactId = null;
        }

        let url: string = `svc/artifactstore/projects/${projectId}` + (artifactId ? `/artifacts/${artifactId}` : ``) + `/children`;

        this.$http.get<Models.IArtifact[]>(url).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IArtifact[]>) => {
                defer.resolve(result.data);
            },
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                var error = {
                    statusCode: errResult.status,
                    message: "Artifact_NotFound"
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
                if (!errResult) {
                    defer.reject();
                    return;
                }
                var error = {
                    statusCode: errResult.status,
                    message: "Project_NotFound"
                };
                defer.reject(error);
            }
        );
        return defer.promise;
    }

    public getSubArtifactTree(artifactId: number): ng.IPromise<Models.ISubArtifactNode[]> {
        var defer = this.$q.defer<any>();
        let url = `/svc/artifactstore/artifacts/${artifactId}/subartifacts`;
        this.$http.get<Models.ISubArtifactNode[]>(url).then(
            (result: ng.IHttpPromiseCallbackArg<Models.ISubArtifactNode[]>) => {
                defer.resolve(result.data);
            },
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                var error = {
                    statusCode: errResult.status,
                    message: (errResult.data ? errResult.data.message : "")
                };
                defer.reject(error);
            }
        );
        return defer.promise;
    }
}
