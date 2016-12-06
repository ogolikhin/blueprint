import {IUnpublishedArtifactsService} from "./unpublished.svc";
import {IArtifact, IPublishResultSet} from "../../main/models/models";

export class UnpublishedArtifactsServiceMock implements IUnpublishedArtifactsService {
    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public get unpublishedArtifactsObservable(): Rx.Observable<IArtifact[]> {
        return undefined;
    }

    public get processedArtifactsObservable(): Rx.Observable<IArtifact[]> {
        return undefined;
    }

    public publishAll(): ng.IPromise<IPublishResultSet> {
        const deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }

    public getUnpublishedArtifacts(): ng.IPromise<IPublishResultSet> {
        return this.$q.when({artifacts: [], projects: []});
    }

    public publishArtifacts(artifactIds: number[]): ng.IPromise<IPublishResultSet> {
        const deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }

    public discardArtifacts(artifactIds: number[]): ng.IPromise<IPublishResultSet> {
        const deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }

    public discardAll(): ng.IPromise<IPublishResultSet> {
        const deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }
}
