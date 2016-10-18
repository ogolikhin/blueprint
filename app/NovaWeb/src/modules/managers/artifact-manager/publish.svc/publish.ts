//import * as angular from "angular";
import { Models, Enums } from "../../../main/models";
export {Models, Enums}

export interface IPublishService {
    publishAll(): ng.IPromise<Models.IPublishResultSet>;
    getUnpublishedArtifacts(): ng.IPromise<Models.IPublishResultSet>;
}

export class PublishService implements IPublishService {

    public static $inject = ["$http", "$q"];

    constructor(private $http: ng.IHttpService, private $q: ng.IQService, private fontNormalizer: any) {
    }

    public publishAll(): ng.IPromise<Models.IPublishResultSet>  {
        var defer = this.$q.defer<Models.IPublishResultSet>();

        this.$http.post(`/svc/bpartifactstore/artifacts/publish?all=true`, "").then(
            (result: ng.IHttpPromiseCallbackArg<Models.IPublishResultSet>) => defer.resolve(result.data),
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

    public getUnpublishedArtifacts(): ng.IPromise<Models.IPublishResultSet>  {
        var defer = this.$q.defer<Models.IPublishResultSet>();

        this.$http.get(`/svc/bpartifactstore/artifacts/unpublished`).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IPublishResultSet>) => defer.resolve(result.data),
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