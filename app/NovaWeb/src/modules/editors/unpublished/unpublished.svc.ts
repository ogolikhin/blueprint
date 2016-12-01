import {Models} from "../../main/models";

export interface IUnpublishedArtifactsService {
    publishAll(): ng.IPromise<Models.IPublishResultSet>;
    discardAll(): ng.IPromise<Models.IPublishResultSet>;
    getUnpublishedArtifacts(): ng.IPromise<Models.IPublishResultSet>;
    publishArtifacts(artifactIds: number[]): ng.IPromise<Models.IPublishResultSet>;
    discardArtifacts(artifactIds: number[]): ng.IPromise<Models.IPublishResultSet>;
}

export class UnpublishedArtifactsService implements IUnpublishedArtifactsService {

    public static $inject = [
        "$http",
        "$q"
    ];

    constructor(private $http: ng.IHttpService,
                private $q: ng.IQService) {
    }

    public publishAll(): ng.IPromise<Models.IPublishResultSet> {
        return this.$http.post(`/svc/bpartifactstore/artifacts/publish?all=true`, "").then(
            (result: ng.IHttpPromiseCallbackArg<Models.IPublishResultSet>) => result.data,
            (result: ng.IHttpPromiseCallbackArg<any>) => this.$q.reject(result.data)
        );
    }

    public getUnpublishedArtifacts(): ng.IPromise<Models.IPublishResultSet> {
        return this.$http.get(`/svc/bpartifactstore/artifacts/unpublished`).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IPublishResultSet>) => result.data,
            (result: ng.IHttpPromiseCallbackArg<any>) => this.$q.reject(result.data)
        );
    }

    public publishArtifacts(artifactIds: number[]): ng.IPromise<Models.IPublishResultSet> {
        return this.$http.post(`/svc/bpartifactstore/artifacts/publish?all=false`, artifactIds).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IPublishResultSet>) => result.data,
            (result: ng.IHttpPromiseCallbackArg<any>) => this.$q.reject(result.data)
        );
    }

    public discardAll(): ng.IPromise<Models.IPublishResultSet> {
        return this.$http.post(`/svc/bpartifactstore/artifacts/discard?all=true`, "").then(
            (result: ng.IHttpPromiseCallbackArg<Models.IPublishResultSet>) => result.data,
            (result: ng.IHttpPromiseCallbackArg<any>) => this.$q.reject(result.data)
        );
    }

    public discardArtifacts(artifactIds: number[]): ng.IPromise<Models.IPublishResultSet> {
        return this.$http.post(`/svc/bpartifactstore/artifacts/discard?all=false`, artifactIds).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IPublishResultSet>) => result.data,
            (result: ng.IHttpPromiseCallbackArg<any>) => this.$q.reject(result.data)
        );
    }
}
