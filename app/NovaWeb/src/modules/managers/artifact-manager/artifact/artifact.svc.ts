import * as angular from "angular";
import {Models, Enums} from "../../../main/models";
export {Models, Enums}

export interface IArtifactService {
    getArtifact(id: number, versionId?: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.IArtifact>;
    getSubArtifact(artifactId: number, subArtifactId: number, versionId: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.ISubArtifact>;
    lock(artifactId: number): ng.IPromise<Models.ILockResult[]>;
    updateArtifact(artifact: Models.IArtifact);
}

export class ArtifactService implements IArtifactService {

    public static $inject = ["$http", "$q"];

    constructor(private $http: ng.IHttpService, private $q: ng.IQService, private fontNormalizer: any) {
    }

    public getArtifact(artifactId: number, versionId?: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.IArtifact> {
        const defer = this.$q.defer<any>();
        const url = `/svc/bpartifactstore/artifacts/${artifactId}`;
        const config: ng.IRequestShortcutConfig = {
            timeout: timeout,
            params: {
                versionId: versionId
            }
        };

        this.$http.get(url, config).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IArtifact>) => defer.resolve(result.data),
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
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                defer.reject(errResult.data);
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
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                defer.reject(errResult.data);
            }
        );
        return defer.promise;
    }


    public updateArtifact(artifact: Models.IArtifact): ng.IPromise<Models.IArtifact> {
        const defer = this.$q.defer<Models.IArtifact>();

        this.$http.patch(`/svc/bpartifactstore/artifacts/${artifact.id}`, angular.toJson(artifact)).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IArtifact>) => defer.resolve(result.data),
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                const error = {
                    statusCode: errResult.status,
                    errorCode: errResult.data ? errResult.data.errorCode : -1,
                    message: (errResult.data ? errResult.data.message : "")
                };
                defer.reject(error);
            }
        );
        return defer.promise;
    }
}
