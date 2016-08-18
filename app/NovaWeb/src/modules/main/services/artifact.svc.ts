﻿import * as Models from "../models/models";


export interface IArtifactService {
    getArtifact(id: number): ng.IPromise<Models.IArtifact>;
    getArtifactOrSubArtifact(artifactId: number, subArtifactId: number): ng.IPromise<Models.IItem>;
}

export class ArtifactService implements IArtifactService {

    public static $inject = ["$http", "$q"];

    constructor(private $http: ng.IHttpService, private $q: ng.IQService, private fontNormalizer: any) {
    }

    public getArtifact(artifactId: number): ng.IPromise<Models.IArtifact> {
        var defer = this.$q.defer<any>();

        const request: ng.IRequestConfig = {
            url: `/svc/bpartifactstore/artifacts/${artifactId}`,
            method: "GET",
            //params: {
            //    types: true
            //}
        };

        this.$http(request).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IArtifact>) => defer.resolve(result.data),
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

    public getArtifactOrSubArtifact(artifactId: number, subArtifactId: number): ng.IPromise<Models.IItem> {
        var defer = this.$q.defer<any>();

        let rest = `/svc/bpartifactstore/artifacts/${artifactId}`;
        if (subArtifactId) {
            rest = rest + `/subartifacts/${subArtifactId}`;
        }

        const request: ng.IRequestConfig = {
            url: rest,
            method: "GET",            
        };

        this.$http(request).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IArtifact>) => defer.resolve(result.data),
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

}