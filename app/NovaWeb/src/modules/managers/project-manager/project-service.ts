import * as angular from "angular";
import {Models, AdminStoreModels, SearchServiceModels} from "../../main/models";

export enum ProjectServiceStatusCode {
    ResourceNotFound = 3000
}

export interface IProjectService {
    getFolders(id?: number, timeout?: ng.IPromise<void>): ng.IPromise<AdminStoreModels.IInstanceItem[]>;
    getArtifacts(projectId: number, artifactId?: number, timeout?: ng.IPromise<void>): ng.IPromise<Models.IArtifact[]>;
    getProject(id?: number, timeout?: ng.IPromise<void>): ng.IPromise<AdminStoreModels.IInstanceItem>;
    getProjectMeta(projectId?: number, timeout?: ng.IPromise<void>): ng.IPromise<Models.IProjectMeta>;
    getSubArtifactTree(artifactId: number, timeout?: ng.IPromise<void>): ng.IPromise<Models.ISubArtifactNode[]>;
    getProjectTree(projectId: number, artifactId: number, loadChildren?: boolean, timeout?: ng.IPromise<void>): ng.IPromise<Models.IArtifact[]>;
    searchProjects(searchCriteria: SearchServiceModels.ISearchCriteria,
                   resultCount?: number,
                   separatorString?: string,
                   timeout?: ng.IPromise<void>): ng.IPromise<SearchServiceModels.IProjectSearchResultSet>;
    searchItemNames(searchCriteria: SearchServiceModels.IItemNameSearchCriteria,
                    startOffset?: number,
                    pageSize?: number,                    
                    timeout?: ng.IPromise<void>): ng.IPromise<SearchServiceModels.IItemNameSearchResultSet>;
    getProjectNavigationPath(projectId: number, includeProjectItself: boolean, timeout?: ng.IPromise<void>): ng.IPromise<string[]>;
}

export class ProjectService implements IProjectService {
    public static $inject = ["$q", "$http"];

    constructor(private $q: ng.IQService,
        private $http: ng.IHttpService) {
    }

    public getFolders(id?: number, timeout?: ng.IPromise<void>): ng.IPromise<AdminStoreModels.IInstanceItem[]> {
        const defer = this.$q.defer<any>();

        const requestObj: ng.IRequestConfig = {
            url: `svc/adminstore/instance/folders/${id || 1}/children`,
            method: "GET",
            timeout: timeout
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

    public getProject(id?: number, timeout?: ng.IPromise<void>): ng.IPromise<AdminStoreModels.IInstanceItem> {
        const defer = this.$q.defer<any>();

        const requestObj: ng.IRequestConfig = {
            url: `svc/adminstore/instance/projects/${id}`,
            method: "GET",
            timeout: timeout
        };

        this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<AdminStoreModels.IInstanceItem>) => {
                defer.resolve(result.data);
            },
            (result: ng.IHttpPromiseCallbackArg<any>) => {
                result.data.message = "Project_NotFound";
                defer.reject(result.data);
            }
        );
        return defer.promise;
    }

    public getArtifacts(projectId: number, artifactId?: number, timeout?: ng.IPromise<void>): ng.IPromise<Models.IArtifact[]> {
        if (projectId && projectId === artifactId) {
            artifactId = null;
        }

        const defer = this.$q.defer<any>();

        const requestObj: ng.IRequestConfig = {
            url: `svc/artifactstore/projects/${projectId}` + (artifactId ? `/artifacts/${artifactId}` : ``) + `/children`,
            method: "GET",
            timeout: timeout
        };

        this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IArtifact[]>) => {
                defer.resolve(result.data);
            },
            (result: ng.IHttpPromiseCallbackArg<any>) => {
                result.data.message = "Artifact_NotFound";
                defer.reject(result.data);
            }
        );
        return defer.promise;
    }

    public getProjectTree(projectId: number, artifactId: number, loadChildren?: boolean, timeout?: ng.IPromise<void>) {
        if (angular.isUndefined(loadChildren)) {
            loadChildren = false;
        }

        const defer = this.$q.defer<any>();

        const requestObj: ng.IRequestConfig = {
            url: `svc/artifactstore/projects/${projectId}/artifacts/?expandedToArtifactId=${artifactId}&includeChildren=${loadChildren}`,
            method: "GET",
            timeout: timeout
        };

        this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IArtifact[]>) => {
                defer.resolve(result.data);
            },
            (result: ng.IHttpPromiseCallbackArg<any>) => {
                result.data.message = "Artifact_NotFound";
                defer.reject(result.data);
            }
        );
        return defer.promise;
    }

    public getProjectMeta(projectId?: number, timeout?: ng.IPromise<void>): ng.IPromise<Models.IProjectMeta> {
        const defer = this.$q.defer<any>();

        const requestObj: ng.IRequestConfig = {
            url: `svc/artifactstore/projects/${projectId}/meta/customtypes`,
            method: "GET",
            timeout: timeout
        };

        this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IProjectMeta>) => {
                defer.resolve(result.data);
            },
            (result: ng.IHttpPromiseCallbackArg<any>) => {
                result.data.message = "Project_NotFound";
                defer.reject(result.data);
            }
        );
        return defer.promise;
    }

    public getSubArtifactTree(artifactId: number, timeout?: ng.IPromise<void>): ng.IPromise<Models.ISubArtifactNode[]> {
        const defer = this.$q.defer<any>();

        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}/subartifacts`,
            method: "GET",
            timeout: timeout
        };

        this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<Models.ISubArtifactNode[]>) => {
                defer.resolve(result.data);
            },
            (result: ng.IHttpPromiseCallbackArg<any>) => {
                defer.reject(result.data);
            }
        );
        return defer.promise;
    }

    public searchProjects(searchCriteria: SearchServiceModels.ISearchCriteria,
                          resultCount: number = 100,
                          separatorString: string = " > ",
                          timeout?: ng.IPromise<void>): ng.IPromise<SearchServiceModels.IProjectSearchResultSet> {
        const requestObj: ng.IRequestConfig = {
            url: `/svc/searchservice/projectsearch/name`,
            params: {resultCount: resultCount, separatorString: separatorString},
            data: searchCriteria,
            method: "POST",
            timeout: timeout
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

    public searchItemNames(searchCriteria: SearchServiceModels.IItemNameSearchCriteria,
                          startOffset: number = 0,
                          pageSize: number = 100,                       
                          timeout?: ng.IPromise<void>): ng.IPromise<SearchServiceModels.IItemNameSearchResultSet> {
        const requestObj: ng.IRequestConfig = {
            url: `/svc/searchservice/itemsearch/name`,
            params: {startOffset: startOffset, pageSize: pageSize},
            data: searchCriteria,
            method: "POST",
            timeout: timeout
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

    public getProjectNavigationPath(projectId: number, includeProjectItself: boolean, timeout?: ng.IPromise<void>): ng.IPromise<string[]> {
        const deferred = this.$q.defer();

        const url = `/svc/adminstore/instance/projects/${projectId}/navigationPath?includeProjectItself=${includeProjectItself}`;
        const requestObj: ng.IRequestShortcutConfig = {
            timeout: timeout
        };

        this.$http.get(url, requestObj)
            .then((result) => {
                deferred.resolve(result.data);
            },
            (result: ng.IHttpPromiseCallbackArg<any>) => {
                deferred.reject(result.data);
            });

        return deferred.promise;
    }
}
