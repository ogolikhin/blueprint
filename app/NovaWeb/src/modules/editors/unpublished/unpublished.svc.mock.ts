import {Models} from "../../main/models";
import {IUnpublishedArtifactsService} from "./unpublished.svc";

export class UnpublishedArtifactsServiceMock implements IUnpublishedArtifactsService {
    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public publishAll(): ng.IPromise<Models.IPublishResultSet> {
        const deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }
    public getUnpublishedArtifacts(): ng.IPromise<Models.IPublishResultSet> {
        return this.$q.when({artifacts: [], projects: []});
    }
    public publishArtifacts(artifactIds: number[]): ng.IPromise<Models.IPublishResultSet> {
        const deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }
    public discardArtifacts(artifactIds: number[]): ng.IPromise<Models.IPublishResultSet> {
        const deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }
    public discardAll(): ng.IPromise<Models.IPublishResultSet> {
        const deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }
}
