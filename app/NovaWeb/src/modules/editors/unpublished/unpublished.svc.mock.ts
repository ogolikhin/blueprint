import {IUnpublishedArtifactsService} from "./unpublished.svc";
import {IPublishResultSet} from "../../main/models/models";

export class UnpublishedArtifactsServiceMock implements IUnpublishedArtifactsService {
    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public unpublishedArtifactsObservable: Rx.Observable<IPublishResultSet> = <any>{
            subscribeOnNext: () => { return; },
            dispose: () => { return; }
    };

    public processedArtifactsObservable: Rx.Observable<IPublishResultSet> = <any>{
        subscribeOnNext: () => { return; },
        dispose: () => { return; }
    };

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
