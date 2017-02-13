import * as angular from "angular";
import {Models, Enums} from "../../../main/models";
export {Models, Enums}

export interface IArtifactService {
    getArtifact(id: number, versionId?: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.IArtifact>;
    getArtifactModel<T extends Models.IArtifact>(url: string, id: number, versionId?: number, timeout?: ng.IPromise<any>): ng.IPromise<T>;
    getSubArtifact(artifactId: number, subArtifactId: number, versionId: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.ISubArtifact>;
    lock(artifactId: number): ng.IPromise<Models.ILockResult[]>;
    updateArtifact(url: string, artifact: Models.IArtifact, config?: ng.IRequestShortcutConfig);
    deleteArtifact(artifactId: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.IArtifact[]>;
    create(name: string, projectId: number, parentId: number, itemTypeId: number, orderIndex?: number): ng.IPromise<Models.IArtifact>;
    getArtifactNavigationPath(artifactId: number): ng.IPromise<Models.IArtifact[]>;
    moveArtifact(artifactId: number, newParentId: number, orderIndex?: number): ng.IPromise<Models.IArtifact>;
    copyArtifact(artifactId: number, newParentId: number, orderIndex?: number): ng.IPromise<Models.ICopyResultSet>;
}

export class ArtifactService implements IArtifactService {

    public static $inject = ["$http", "$q"];

    constructor(private $http: ng.IHttpService, private $q: ng.IQService, private fontNormalizer: any) {
    }

    public getArtifact(artifactId: number, versionId?: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.IArtifact> {
        const url = `/svc/bpartifactstore/artifacts/${artifactId}`;
        return this.getArtifactModel<Models.IArtifact>(url, artifactId, versionId, timeout);
    }

    // public getCollectionArtifact(artifactId: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.IArtifact> {
    //     const url = `/svc/bpartifactstore/collection/${artifactId}`;
    //     return this.getArtifactInternal<Models.Co>(url, artifactId, undefined, timeout);
    // }

    public getArtifactModel<T extends Models.IArtifact>(url: string, artifactId: number, versionId?: number, timeout?: ng.IPromise<any>): ng.IPromise<T> {
        const defer = this.$q.defer<any>();
        const config: ng.IRequestShortcutConfig = {
            timeout: timeout,
            params: {
                versionId: versionId
            }
        };

        this.$http.get(url, config).then(
            (result: ng.IHttpPromiseCallbackArg<T>) => defer.resolve(result.data),
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                defer.reject(errResult.data);
            }
        );
        return defer.promise;
    }

    public getSubArtifact(artifactId: number, subArtifactId: number, versionId: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.ISubArtifact> {
        const defer = this.$q.defer<any>();
        const url = `/svc/bpartifactstore/artifacts/${artifactId}/subartifacts/${subArtifactId}`;
        const config: ng.IRequestShortcutConfig = {
            timeout: timeout,
            params: {
                versionId: versionId
            }
        };

        this.$http.get(url, config).then(
            (result: ng.IHttpPromiseCallbackArg<Models.ISubArtifact>) => defer.resolve(result.data),
            (result: ng.IHttpPromiseCallbackArg<any>) => {
                defer.reject(result.data);
            }
        );
        return defer.promise;
    }


    public lock(artifactId: number): ng.IPromise<Models.ILockResult[]> {
        const defer = this.$q.defer<any>();

        const request: ng.IRequestConfig = {
            url: `/svc/shared/artifacts/lock`,
            method: "post",
            data: angular.toJson([artifactId])
        };

        this.$http(request).then(
            (result: ng.IHttpPromiseCallbackArg<Models.ILockResult>) => defer.resolve(result.data),
            (result: ng.IHttpPromiseCallbackArg<any>) => {
                defer.reject(result.data);
            }
        );
        return defer.promise;
    }


    public updateArtifact(url: string, artifact: Models.IArtifact, config?: ng.IRequestShortcutConfig): ng.IPromise<Models.IArtifact> {
        const defer = this.$q.defer<Models.IArtifact>();

        this.$http.patch(url, angular.toJson(artifact), config).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IArtifact>) => defer.resolve(result.data),
            (result: ng.IHttpPromiseCallbackArg<any>) => {
                defer.reject(result.data);
            }
        );
        return defer.promise;
    }

    public deleteArtifact(artifactId: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.IArtifact[]> {
        const defer = this.$q.defer<any>();

        const requestObj: ng.IRequestConfig = {
            url: `svc/bpartifactstore/artifacts/${artifactId}`,
            method: "DELETE",
            timeout: timeout
        };

        this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IArtifact[]>) => {
                defer.resolve(result.data);
            },
            (result: ng.IHttpPromiseCallbackArg<any>) => {
                defer.reject(result.data);
            }
        );
        return defer.promise;
    }

    public create(name: string, projectId: number, parentId: number, itemTypeId: number, orderIndex?: number): ng.IPromise<Models.IArtifact> {
        const defer = this.$q.defer<any>();

        const request: ng.IRequestConfig = {
            url: `/svc/bpartifactstore/artifacts/create`,
            method: "POST",
            data: {
                name: name,
                projectId: projectId,
                parentId: parentId,
                itemTypeId: itemTypeId,
                orderIndex: orderIndex ? orderIndex : undefined
            }
        };

        this.$http(request).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IArtifact>) => defer.resolve(result.data),
            (result: ng.IHttpPromiseCallbackArg<any>) => {
                defer.reject(result.data);
            }
        );
        return defer.promise;
    }

    public getArtifactNavigationPath(artifactId: number): ng.IPromise<Models.IArtifact[]> {
        const deferred = this.$q.defer();

        const url = `/svc/artifactstore/artifacts/${artifactId}/navigationPath`;

        this.$http.get(url)
            .then((result) => {
                deferred.resolve(result.data);
            },
            (result: ng.IHttpPromiseCallbackArg<any>) => {
                deferred.reject(result.data);
            });

        return deferred.promise;
    }

    public moveArtifact(artifactId: number, newParentId: number, orderIndex?: number): ng.IPromise<Models.IArtifact> {
        const url = orderIndex ?
            `/svc/bpartifactstore/artifacts/${artifactId}/moveTo/${newParentId}?orderIndex=${orderIndex}` :
            `/svc/bpartifactstore/artifacts/${artifactId}/moveTo/${newParentId}`;

        const requestObj: ng.IRequestConfig = {
            url: url,
            method: "POST"
        };

        return this.$http(requestObj)
            .then((result: ng.IHttpPromiseCallbackArg<Models.IArtifact>) => {
                return result.data;
            })
            .catch((result: ng.IHttpPromiseCallbackArg<any>) => {
                return this.$q.reject(result.data);
            });
    }

    public copyArtifact(artifactId: number, newParentId: number, orderIndex?: number): ng.IPromise<Models.ICopyResultSet> {
        const url = orderIndex ?
            `/svc/bpartifactstore/artifacts/${artifactId}/copyTo/${newParentId}?orderIndex=${orderIndex}` :
            `/svc/bpartifactstore/artifacts/${artifactId}/copyTo/${newParentId}`;

        const requestObj: ng.IRequestConfig = {
            url: url,
            method: "POST"
        };

        return this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<Models.ICopyResultSet>) => {
                return result.data;
            },
            (result: ng.IHttpPromiseCallbackArg<any>) => {
                return this.$q.reject(result.data);
            }
        );
    }
}
