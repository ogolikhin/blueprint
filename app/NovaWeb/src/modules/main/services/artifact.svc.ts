﻿import * as Models from "../models/models";


export interface IArtifactService {
    getArtifact(id: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.IArtifact>;
    getSubArtifact(artifactId: number, subArtifactId: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.ISubArtifact>;
    lock(artifactId: number): ng.IPromise<Models.ILockResult[]>;
}

export class ArtifactService implements IArtifactService {

    public static $inject = ["$http", "$q"];

    constructor(private $http: ng.IHttpService, private $q: ng.IQService, private fontNormalizer: any) {
    }

    public getArtifact(artifactId: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.IArtifact> {
        var defer = this.$q.defer<any>();

        const request: ng.IRequestConfig = {
            url: `/svc/bpartifactstore/artifacts/${artifactId}`,
            method: "GET",
            timeout: timeout
        };

        this.$http(request).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IArtifact>) => defer.resolve(result.data),
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

    public getSubArtifact(artifactId: number, subArtifactId: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.ISubArtifact> {        
        var defer = this.$q.defer<any>();
        let rest = `/svc/bpartifactstore/artifacts/${artifactId}/subartifacts/${subArtifactId}`;
        
        const request: ng.IRequestConfig = {
            url: rest,
            method: "GET",
            timeout: timeout
        };

        this.$http(request).then(
            (result: ng.IHttpPromiseCallbackArg<Models.ISubArtifact>) => defer.resolve(result.data),
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                var error = {
                    statusCode: errResult.status,
                    message: (errResult.data ? errResult.data.message : "")
                };
                defer.reject(error);
            }
        );
        return defer.promise;
    }    


    public lock(artifactId: number): ng.IPromise<Models.ILockResult[]> {
        var defer = this.$q.defer<any>();

        const request: ng.IRequestConfig = {
            url: `/svc/shared/artifacts/lock`,
            method: "post",
            data: angular.toJson([artifactId])
        };

        this.$http(request).then(
            (result: ng.IHttpPromiseCallbackArg<Models.ILockResult>) => defer.resolve(result.data),
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