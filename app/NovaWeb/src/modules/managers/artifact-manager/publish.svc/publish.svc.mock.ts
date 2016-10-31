import {IPublishService} from "./publish.svc";
import { Models, Enums } from "../../../main/models";

export class PublishServiceMock implements IPublishService {
    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public publishAll(): ng.IPromise<Models.IPublishResultSet> {
        const deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }
    public getUnpublishedArtifacts(): ng.IPromise<Models.IPublishResultSet> {
        const deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
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
