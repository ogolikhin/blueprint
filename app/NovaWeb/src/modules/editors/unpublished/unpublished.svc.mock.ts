import {Models} from "../../main/models";
import {IUnpublishedArtifactsService} from "./unpublished.svc";
import {IArtifact} from "../../main/models/models";

export class UnpublishedArtifactsServiceMock implements IUnpublishedArtifactsService {
    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public get unpublishedArtifacts(): IArtifact[] {
        return undefined;
    }

    public get unpublishedArtifactsObservable(): Rx.Observable<IArtifact[]> {
        return undefined;
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
