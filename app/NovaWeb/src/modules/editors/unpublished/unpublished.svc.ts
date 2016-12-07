import {IPublishResultSet} from "../../main/models/models";

export interface IUnpublishedArtifactsService {
    unpublishedArtifactsObservable: Rx.Observable<IPublishResultSet>;
    processedArtifactsObservable: Rx.Observable<IPublishResultSet>;
    publishAll(): ng.IPromise<IPublishResultSet>;
    discardAll(): ng.IPromise<IPublishResultSet>;
    getUnpublishedArtifacts(): ng.IPromise<IPublishResultSet>;
    publishArtifacts(artifactIds: number[]): ng.IPromise<IPublishResultSet>;
    discardArtifacts(artifactIds: number[]): ng.IPromise<IPublishResultSet>;
}

export class UnpublishedArtifactsService implements IUnpublishedArtifactsService {

    private _unpublishedSubject: Rx.Subject<IPublishResultSet>;
    private _processedSubject: Rx.Subject<IPublishResultSet>;
    private getUnpublishedArtifactsPromise: ng.IPromise<IPublishResultSet>;

    public static $inject = [
        "$http",
        "$q"
    ];

    constructor(private $http: ng.IHttpService,
                private $q: ng.IQService) {

        this._unpublishedSubject = new Rx.Subject<IPublishResultSet>();
        this._processedSubject = new Rx.Subject<IPublishResultSet>();
    }

    public get unpublishedArtifactsObservable(): Rx.Observable<IPublishResultSet> {
        return this._unpublishedSubject.asObservable();
    }

    public get processedArtifactsObservable(): Rx.Observable<IPublishResultSet> {
        return this._processedSubject.asObservable();
    }

    private notifyUnpublishedArtifacts(value: IPublishResultSet) {
        this._unpublishedSubject.onNext(value);
    }

    private notifyProcessedArtifacts(value: IPublishResultSet) {
        this._processedSubject.onNext(value);
    }

    public publishAll(): ng.IPromise<IPublishResultSet> {
        return this.$http.post(`/svc/bpartifactstore/artifacts/publish?all=true`, "")
            .then((result: ng.IHttpPromiseCallbackArg<IPublishResultSet>) => {
                this.notifyProcessedArtifacts(result.data);
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
                this.notifyUnpublishedArtifacts(result.data);
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
                this.notifyProcessedArtifacts(result.data);
                return result.data;
            })
            .catch((result: ng.IHttpPromiseCallbackArg<any>) => this.$q.reject(result.data));
    }

    public discardAll(): ng.IPromise<IPublishResultSet> {
        return this.$http.post(`/svc/bpartifactstore/artifacts/discard?all=true`, "")
            .then((result: ng.IHttpPromiseCallbackArg<IPublishResultSet>) => {
                this.notifyProcessedArtifacts(result.data);
                return result.data;
            })
            .catch((result: ng.IHttpPromiseCallbackArg<any>) => this.$q.reject(result.data));
    }

    public discardArtifacts(artifactIds: number[]): ng.IPromise<IPublishResultSet> {
        return this.$http.post(`/svc/bpartifactstore/artifacts/discard?all=false`, artifactIds)
            .then((result: ng.IHttpPromiseCallbackArg<IPublishResultSet>) => {
                this.notifyProcessedArtifacts(result.data);
                return result.data;
            })
            .catch((result: ng.IHttpPromiseCallbackArg<any>) => this.$q.reject(result.data));
    }
}
