import { Models, Enums } from "../../../main/models";
export {Models, Enums}

export interface IPublishService {
    publishAll(): ng.IPromise<Models.IPublishResultSet>;
    getUnpublishedArtifacts(): ng.IPromise<Models.IPublishResultSet>;
    publishArtifacts(artifactIds: number[]): ng.IPromise<Models.IPublishResultSet>;
}

export class PublishService implements IPublishService {

    public static $inject = ["$http", "$q"];

    constructor(private $http: ng.IHttpService, private $q: ng.IQService, private fontNormalizer: any) {
    }

    public publishAll(): ng.IPromise<Models.IPublishResultSet>  {
        let defer = this.$q.defer<Models.IPublishResultSet>();

        this.$http.post(`/svc/bpartifactstore/artifacts/publish?all=true`, "").then(
            (result: ng.IHttpPromiseCallbackArg<Models.IPublishResultSet>) => defer.resolve(result.data),
            (result: ng.IHttpPromiseCallbackArg<any>) => {
                defer.reject(result.data);
            }
        );
        return defer.promise;
    }

    public getUnpublishedArtifacts(): ng.IPromise<Models.IPublishResultSet>  {
        let defer = this.$q.defer<Models.IPublishResultSet>();

        this.$http.get(`/svc/bpartifactstore/artifacts/unpublished`).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IPublishResultSet>) => defer.resolve(result.data),
            (result: ng.IHttpPromiseCallbackArg<any>) => {
                defer.reject(result.data);
            }
        );
        return defer.promise;
    }

    public publishArtifacts(artifactIds: number[]): ng.IPromise<Models.IPublishResultSet> {
        let defer = this.$q.defer<Models.IPublishResultSet>();

        this.$http.post(`/svc/bpartifactstore/artifacts/publish?all=false`, artifactIds).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IPublishResultSet>) => defer.resolve(result.data),
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                let error = {
                    content: errResult.data ? errResult.data.errorContent : null,
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