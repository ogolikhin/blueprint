import * as angular from "angular";
import {Models, AdminStoreModels, SearchServiceModels} from "../../main/models";

export enum ProjectServiceStatusCode {
    ResourceNotFound = 3000
}

export interface IProjectService {
    abort(): void;
    getFolders(id?: number): ng.IPromise<AdminStoreModels.IInstanceItem[]>;
    getArtifacts(projectId: number, artifactId?: number): ng.IPromise<Models.IArtifact[]>;
    getProject(id?: number): ng.IPromise<AdminStoreModels.IInstanceItem>;
    getProjectMeta(projectId?: number): ng.IPromise<Models.IProjectMeta>;
    getSubArtifactTree(artifactId: number): ng.IPromise<Models.ISubArtifactNode[]>;
    getProjectTree(projectId: number, artifactId: number, loadChildren?: boolean): ng.IPromise<Models.IArtifact[]>;
    searchProjects(searchCriteria: SearchServiceModels.ISearchCriteria,
                   resultCount?: number,
                   separatorString?: string): ng.IPromise<SearchServiceModels.IProjectSearchResultSet>;
    searchItemNames(searchCriteria: SearchServiceModels.IItemSearchCriteria,
                startOffset?: number,
                pageSize?: number): ng.IPromise<SearchServiceModels.IItemNameSearchResultSet>;
}

export class ProjectService implements IProjectService {
    public static $inject = ["$q", "$http"];

    private canceler: ng.IDeferred<any>;

    constructor(private $q: ng.IQService,
                private $http: ng.IHttpService) {
    }

    public abort(): void {
        this.canceler.resolve();
    }

    public getFolders(id?: number): ng.IPromise<AdminStoreModels.IInstanceItem[]> {
        const defer = this.$q.defer<any>();
        this.canceler = this.$q.defer<any>();

        const requestObj: ng.IRequestConfig = {
            url: `svc/adminstore/instance/folders/${id || 1}/children`,
            method: "GET",
            timeout: this.canceler.promise
        };

        this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<AdminStoreModels.IInstanceItem[]>) => defer.resolve(result.data),
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                const error = {
                    statusCode: errResult.status,
                    message: "Folder_NotFound"
                };
                defer.reject(error);
            }
        );
        return defer.promise;
    }

    public getProject(id?: number): ng.IPromise<AdminStoreModels.IInstanceItem> {
        const defer = this.$q.defer<any>();
        this.canceler = this.$q.defer<any>();

        const requestObj: ng.IRequestConfig = {
            url: `svc/adminstore/instance/projects/${id}`,
            method: "GET",
            timeout: this.canceler.promise
        };

        this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<AdminStoreModels.IInstanceItem>) => {
                defer.resolve(result.data);
            },
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                const error = {
                    statusCode: errResult.status,
                    message: "Project_NotFound"
                };
                defer.reject(error);
            }
        );
        return defer.promise;
    }

    public getArtifacts(projectId: number, artifactId?: number): ng.IPromise<Models.IArtifact[]> {
        if (projectId && projectId === artifactId) {
            artifactId = null;
        }

        const defer = this.$q.defer<any>();
        this.canceler = this.$q.defer<any>();

        const requestObj: ng.IRequestConfig = {
            url: `svc/artifactstore/projects/${projectId}` + (artifactId ? `/artifacts/${artifactId}` : ``) + `/children`,
            method: "GET",
            timeout: this.canceler.promise
        };

        this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IArtifact[]>) => {
                defer.resolve(result.data);
            },
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                const error = {
                    statusCode: errResult.status,
                    message: "Artifact_NotFound"
                };
                defer.reject(error);
            }
        );
        return defer.promise;
    }

    public getProjectTree(projectId: number, artifactId: number, loadChildren?: boolean) {
        if (angular.isUndefined(loadChildren)) {
            loadChildren = false;
        }

        const defer = this.$q.defer<any>();
        this.canceler = this.$q.defer<any>();

        const requestObj: ng.IRequestConfig = {
            url: `svc/artifactstore/projects/${projectId}/artifacts/?expandedToArtifactId=${artifactId}&includeChildren=${loadChildren}`,
            method: "GET",
            timeout: this.canceler.promise
        };

        this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IArtifact[]>) => {
                defer.resolve(result.data);
            },
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                const error = {
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
        const defer = this.$q.defer<any>();
        this.canceler = this.$q.defer<any>();

        const requestObj: ng.IRequestConfig = {
            url: `svc/artifactstore/projects/${projectId}/meta/customtypes`,
            method: "GET",
            timeout: this.canceler.promise
        };

        this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IProjectMeta>) => {
                defer.resolve(result.data);
            },
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                const error = {
                    statusCode: errResult.status,
                    message: "Project_NotFound"
                };
                defer.reject(error);
            }
        );
        return defer.promise;
    }

    public getSubArtifactTree(artifactId: number): ng.IPromise<Models.ISubArtifactNode[]> {
        const defer = this.$q.defer<any>();
        this.canceler = this.$q.defer<any>();

        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}/subartifacts`,
            method: "GET",
            timeout: this.canceler.promise
        };

        this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<Models.ISubArtifactNode[]>) => {
                defer.resolve(result.data);
            },
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                const error = {
                    statusCode: errResult.status,
                    message: (errResult.data ? errResult.data.message : "")
                };
                defer.reject(error);
            }
        );
        return defer.promise;
    }

    public searchProjects(searchCriteria: SearchServiceModels.ISearchCriteria,
                          resultCount: number = 100,
                          separatorString: string = " > "): ng.IPromise<SearchServiceModels.IProjectSearchResultSet> {
        this.canceler = this.$q.defer<any>();

        const requestObj: ng.IRequestConfig = {
            url: `/svc/searchservice/projectsearch/name`,
            params: {resultCount: resultCount, separatorString: separatorString},
            data: searchCriteria,
            method: "POST",
            timeout: this.canceler.promise
        };

        return this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<SearchServiceModels.IProjectSearchResultSet>) => {
                return result.data;
            },
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                return this.$q.reject(errResult ? {
                    statusCode: errResult.status,
                    message: (errResult.data ? errResult.data.message : "")
                } : undefined);
            }
        );
    }

    public searchItemNames(searchCriteria: SearchServiceModels.IItemSearchCriteria,
                           startOffset: number = 0,
                           pageSize: number = 100): ng.IPromise<SearchServiceModels.IItemNameSearchResultSet> {
        this.canceler = this.$q.defer<any>();

        const requestObj: ng.IRequestConfig = {
            url: `/svc/searchservice/itemsearch/name`,
            params: { startOffset: startOffset, pageSize: pageSize},
            data: searchCriteria,
            method: "POST",
            timeout: this.canceler.promise
        };

        return this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<SearchServiceModels.IItemNameSearchResultSet>) => {
                return result.data;
            },
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                return this.$q.reject(errResult ? {
                    statusCode: errResult.status,
                    message: (errResult.data ? errResult.data.message : "")
                } : undefined);
            }
        );
    }
}
