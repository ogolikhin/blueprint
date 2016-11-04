import * as angular from "angular";
import {Models, Enums} from "../../../main/models";
export {Models, Enums}

export interface IArtifactService {
    getArtifact(id: number, versionId?: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.IArtifact>;
    getArtifactModel<T extends Models.IArtifact>(url: string, id: number, versionId?: number, timeout?: ng.IPromise<any>): ng.IPromise<T>;
    getSubArtifact(artifactId: number, subArtifactId: number, versionId: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.ISubArtifact>;
    lock(artifactId: number): ng.IPromise<Models.ILockResult[]>;
    updateArtifact(artifact: Models.IArtifact);
    deleteArtifact(artifactId: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.IArtifact[]>;
    getChilden(projectId: number, artifactId?: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.IArtifact[]>;
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


    public updateArtifact(artifact: Models.IArtifact): ng.IPromise<Models.IArtifact> {
        const defer = this.$q.defer<Models.IArtifact>();

        this.$http.patch(`/svc/bpartifactstore/artifacts/${artifact.id}`, angular.toJson(artifact)).then(
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
                result.data.message = "Artifact_NotFound"; 
                defer.reject(result.data);
            }
        );
        return defer.promise;
    }
    public getChilden(projectId: number, artifactId?: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.IArtifact[]> {
        const defer = this.$q.defer<any>();

        const requestObj: ng.IRequestConfig = {
            url: `svc/artifactstore/projects/${projectId}/artifacts/${artifactId}/children`,
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
    
}
