import { Models } from "../../main/models";

export interface IProjectService {
    abort(): void;
    getFolders(id?: number): ng.IPromise<Models.IProjectNode[]>;
    getArtifacts(projectId: number, artifactId?: number): ng.IPromise<Models.IArtifact[]>;
    getProject(id?: number): ng.IPromise<Models.IProjectNode>;
    getProjectMeta(projectId?: number): ng.IPromise<Models.IProjectMeta>;
    getSubArtifactTree(artifactId: number): ng.IPromise<Models.ISubArtifactNode[]>;
    getProjectTree(projectId: number, artifactId: number, loadChildren?: boolean): ng.IPromise<Models.IArtifact[]>;
}

export class ProjectService implements IProjectService {
    static $inject: [string] = ["$q", "$http"];

    private canceler: ng.IDeferred<any>;

    constructor(
        private $q: ng.IQService,
        private $http: ng.IHttpService) {
    }

    public abort(): void {
        this.canceler.resolve();
    }

    public getFolders(id?: number): ng.IPromise<Models.IProjectNode[]> {
        var defer = this.$q.defer<any>();
        this.canceler = this.$q.defer<any>();

        let url: string = `svc/adminstore/instance/folders/${id || 1}/children`;
        this.$http.get<Models.IProjectNode[]>(url, { timeout: this.canceler.promise }).then(
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
        this.canceler = this.$q.defer<any>();

        let url: string = `svc/adminstore/instance/projects/${id}`;
        this.$http.get<Models.IProjectNode>(url, { timeout: this.canceler.promise }).then(
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
        this.canceler = this.$q.defer<any>();

        let url: string = `svc/artifactstore/projects/${projectId}` + (artifactId ? `/artifacts/${artifactId}` : ``) + `/children`;
        this.$http.get<Models.IArtifact[]>(url, { timeout: this.canceler.promise }).then(
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

    public getProjectTree(projectId: number, artifactId: number, loadChildren?: boolean) {
        var defer = this.$q.defer<any>();
        if (projectId && projectId === artifactId) {
            artifactId = null;
        }
        if (loadChildren === undefined) {
            loadChildren = false;
        }
        this.canceler = this.$q.defer<any>();

        let url: string = `svc/artifactstore/projects/${projectId}/artifacts/?expandedToArtifactId=${artifactId}&includeChildren=${loadChildren}`;
        this.$http.get<Models.IArtifact[]>(url, { timeout: this.canceler.promise }).then(
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
                    message: "Artifact_NotFound",
                    errorCode: errResult.data.errorCode
                };
                defer.reject(error);
            }
        );
        return defer.promise;
    }

    public getProjectMeta(projectId?: number): ng.IPromise<Models.IProjectMeta> {
        var defer = this.$q.defer<any>();
        this.canceler = this.$q.defer<any>();

        let url: string = `svc/artifactstore/projects/${projectId}/meta/customtypes`;
        this.$http.get<Models.IProjectMeta>(url, { timeout: this.canceler.promise }).then(
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
        this.canceler = this.$q.defer<any>();

        let url = `/svc/artifactstore/artifacts/${artifactId}/subartifacts`;
        this.$http.get<Models.ISubArtifactNode[]>(url, { timeout: this.canceler.promise }).then(
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
