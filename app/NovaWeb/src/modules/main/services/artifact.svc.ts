import * as Models from "../models/models";


export interface IArtifactService {
    getArtifact(id: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.IArtifact>;
    getSubArtifact(artifactId: number, subArtifactId: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.ISubArtifact>;
    lock(artifactId: number): ng.IPromise<Models.ILockResult[]>;
    updateArtifact(artifact: Models.IArtifact);
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
            (result: ng.IHttpPromiseCallbackArg<Models.IArtifact>) => {
                //DEBUG
                result.data.specificPropertyValues.push({
                    propertyTypeId: 1,
                    propertyTypeVersionId: 1,
                    propertyTypePredefined: Models.PropertyTypePredefined.Image,
                    isReuseReadOnly: false,
                    value: "https://www.google.ca/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png"
                }, {
                    propertyTypeId: 1,
                    propertyTypeVersionId: 1,
                    propertyTypePredefined: Models.PropertyTypePredefined.ActorInheritance,
                    isReuseReadOnly: false,
                    value: {
                        path: "Path",
                        id: 1,
                        name: "Actor Name",
                        prefix: "ACT",
                        hasAccess: true
                    }
                });
                //END DEBUG

                defer.resolve(result.data);
            }, (errResult: ng.IHttpPromiseCallbackArg<any>) => {
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


    public updateArtifact(artifact: Models.IArtifact): ng.IPromise<Models.IArtifact>  {
        var defer = this.$q.defer<Models.IArtifact>();

        this.$http.patch(`/svc/bpartifactstore/artifacts/${artifact.id}`, angular.toJson(artifact)).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IArtifact>) => defer.resolve(result.data),
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                var error = {
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