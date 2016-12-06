import {IArtifact, IPublishResultSet} from "../../main/models/models";

export interface IUnpublishedArtifactsService {
    unpublishedArtifactsObservable: Rx.Observable<IArtifact[]>;
    processedArtifactsObservable: Rx.Observable<IArtifact[]>;
    publishAll(): ng.IPromise<IPublishResultSet>;
    discardAll(): ng.IPromise<IPublishResultSet>;
    getUnpublishedArtifacts(): ng.IPromise<IPublishResultSet>;
    publishArtifacts(artifactIds: number[]): ng.IPromise<IPublishResultSet>;
    discardArtifacts(artifactIds: number[]): ng.IPromise<IPublishResultSet>;
}

export class UnpublishedArtifactsService implements IUnpublishedArtifactsService {

    private _unpublishedSubject: Rx.Subject<IArtifact[]>;
    private _processedSubject: Rx.Subject<IArtifact[]>;
    private getUnpublishedArtifactsPromise: ng.IPromise<IPublishResultSet>;

    public static $inject = [
        "$http",
        "$q"
    ];

    constructor(private $http: ng.IHttpService,
                private $q: ng.IQService) {

        this._unpublishedSubject = new Rx.Subject<IArtifact[]>();
        this._processedSubject = new Rx.Subject<IArtifact[]>();
    }

    public get unpublishedArtifactsObservable(): Rx.Observable<IArtifact[]> {
        return this._unpublishedSubject.asObservable();
    }

    public get processedArtifactsObservable(): Rx.Observable<IArtifact[]> {
        return this._processedSubject.asObservable();
    }

    private notifyUnpublishedArtifacts(value: IArtifact[]) {
        this._unpublishedSubject.onNext(value);
    }

    private notifyProcessedArtifacts(value: IArtifact[]) {
        this._processedSubject.onNext(value);
    }

    public publishAll(): ng.IPromise<IPublishResultSet> {
        return this.$http.post(`/svc/bpartifactstore/artifacts/publish?all=true`, "")
            .then((result: ng.IHttpPromiseCallbackArg<IPublishResultSet>) => {
                this.notifyProcessedArtifacts(result.data.artifacts);
                return result.data;
            })
            .catch((result: ng.IHttpPromiseCallbackArg<any>) => this.$q.reject(result.data));
    }

    public getUnpublishedArtifacts(): ng.IPromise<IPublishResultSet> {
        if (this.getUnpublishedArtifactsPromise) {
            return this.getUnpublishedArtifactsPromise;
        }

        this.getUnpublishedArtifactsPromise = this.$http.get(`/svc/bpartifactstore/artifacts/unpublished`)
            .then((result: ng.IHttpPromiseCallbackArg<IPublishResultSet>) => {
                this.notifyUnpublishedArtifacts(result.data.artifacts);
                return result.data;
            })
            .catch((result: ng.IHttpPromiseCallbackArg<any>) => this.$q.reject(result.data))
            .finally(() => {
                this.getUnpublishedArtifactsPromise = null;
            });

        return this.getUnpublishedArtifactsPromise;
    }

    public publishArtifacts(artifactIds: number[]): ng.IPromise<IPublishResultSet> {
        return this.$http.post(`/svc/bpartifactstore/artifacts/publish?all=false`, artifactIds)
            .then((result: ng.IHttpPromiseCallbackArg<IPublishResultSet>) => {
                this.notifyProcessedArtifacts(result.data.artifacts);
                return result.data;
            })
            .catch((result: ng.IHttpPromiseCallbackArg<any>) => this.$q.reject(result.data));
    }

    public discardAll(): ng.IPromise<IPublishResultSet> {
        return this.$http.post(`/svc/bpartifactstore/artifacts/discard?all=true`, "")
            .then((result: ng.IHttpPromiseCallbackArg<IPublishResultSet>) => {
                this.notifyProcessedArtifacts(result.data.artifacts);
                return result.data;
            })
            .catch((result: ng.IHttpPromiseCallbackArg<any>) => this.$q.reject(result.data));
    }

    public discardArtifacts(artifactIds: number[]): ng.IPromise<IPublishResultSet> {
        return this.$http.post(`/svc/bpartifactstore/artifacts/discard?all=false`, artifactIds)
            .then((result: ng.IHttpPromiseCallbackArg<IPublishResultSet>) => {
                this.notifyProcessedArtifacts(result.data.artifacts);
                return result.data;
            })
            .catch((result: ng.IHttpPromiseCallbackArg<any>) => this.$q.reject(result.data));
    }
}
