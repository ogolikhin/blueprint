import {Models} from "../../main/models";
import {IArtifact} from "../../main/models/models";

export interface IUnpublishedArtifactsService {
    unpublishedArtifacts: IArtifact[];
    unpublishedArtifactsObservable: Rx.Observable<IArtifact[]>;
    publishAll(): ng.IPromise<Models.IPublishResultSet>;
    discardAll(): ng.IPromise<Models.IPublishResultSet>;
    getUnpublishedArtifacts(): ng.IPromise<Models.IPublishResultSet>;
    publishArtifacts(artifactIds: number[]): ng.IPromise<Models.IPublishResultSet>;
    discardArtifacts(artifactIds: number[]): ng.IPromise<Models.IPublishResultSet>;
}

export class UnpublishedArtifactsService implements IUnpublishedArtifactsService {

    private _unpublishedArtifacts: IArtifact[];
    private _subject: Rx.Subject<IArtifact[]>;

    public static $inject = [
        "$http",
        "$q"
    ];

    constructor(private $http: ng.IHttpService,
                private $q: ng.IQService) {

        this._subject = new Rx.Subject<IArtifact[]>();
        this._unpublishedArtifacts = [];
    }

    public get unpublishedArtifacts(): IArtifact[] {
        return this._unpublishedArtifacts;
    }

    public get unpublishedArtifactsObservable(): Rx.Observable<IArtifact[]> {
        return this._subject.asObservable();
    }

    public set unpublishedArtifacts(value: IArtifact[]) {
        this._unpublishedArtifacts = value;
        this._subject.onNext(this.unpublishedArtifacts);
    }

    private removeFromUnpublishedArtifacts(valuesToRemove: IArtifact[]) {
        return _.differenceBy(this._unpublishedArtifacts, valuesToRemove, "id");
    }

    public publishAll(): ng.IPromise<Models.IPublishResultSet> {
        return this.$http.post(`/svc/bpartifactstore/artifacts/publish?all=true`, "")
            .then((result: ng.IHttpPromiseCallbackArg<Models.IPublishResultSet>) => {
                this.unpublishedArtifacts = this.removeFromUnpublishedArtifacts(result.data.artifacts);
                return result.data;
            })
            .catch((result: ng.IHttpPromiseCallbackArg<any>) => this.$q.reject(result.data));
    }

    public getUnpublishedArtifacts(): ng.IPromise<Models.IPublishResultSet> {
        return this.$http.get(`/svc/bpartifactstore/artifacts/unpublished`)
            .then((result: ng.IHttpPromiseCallbackArg<Models.IPublishResultSet>) => {
                this.unpublishedArtifacts = result.data.artifacts;
                return result.data;
            })
            .catch((result: ng.IHttpPromiseCallbackArg<any>) => this.$q.reject(result.data));
    }

    public publishArtifacts(artifactIds: number[]): ng.IPromise<Models.IPublishResultSet> {
        return this.$http.post(`/svc/bpartifactstore/artifacts/publish?all=false`, artifactIds)
            .then((result: ng.IHttpPromiseCallbackArg<Models.IPublishResultSet>) => {
                this.unpublishedArtifacts = this.removeFromUnpublishedArtifacts(result.data.artifacts);
                return result.data;
            })
            .catch((result: ng.IHttpPromiseCallbackArg<any>) => this.$q.reject(result.data));
    }

    public discardAll(): ng.IPromise<Models.IPublishResultSet> {
        return this.$http.post(`/svc/bpartifactstore/artifacts/discard?all=true`, "")
            .then((result: ng.IHttpPromiseCallbackArg<Models.IPublishResultSet>) => {
                this.unpublishedArtifacts = this.removeFromUnpublishedArtifacts(result.data.artifacts);
                return result.data;
            })
            .catch((result: ng.IHttpPromiseCallbackArg<any>) => this.$q.reject(result.data));
    }

    public discardArtifacts(artifactIds: number[]): ng.IPromise<Models.IPublishResultSet> {
        return this.$http.post(`/svc/bpartifactstore/artifacts/discard?all=false`, artifactIds)
            .then((result: ng.IHttpPromiseCallbackArg<Models.IPublishResultSet>) => {
                this.unpublishedArtifacts = this.removeFromUnpublishedArtifacts(result.data.artifacts);
                return result.data;
            })
            .catch((result: ng.IHttpPromiseCallbackArg<any>) => this.$q.reject(result.data));
    }
}
