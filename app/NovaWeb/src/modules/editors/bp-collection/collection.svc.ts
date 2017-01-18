import {ICollection} from "./collection-artifact";
import {ILocalizationService} from "../../core/localization/localization.service";

export interface ICollectionService {
    getCollection(id: number): ng.IPromise<ICollection>;
    addArtifactToCollection(artifactId: number, collectionId: number, addChildren: boolean): ng.IPromise<number>;
}

export class CollectionService implements ICollectionService {
    static $inject: [string] = [
        "$q",
        "$http",
        "$log",
        "localization"];

    private _collection: Rx.BehaviorSubject<ICollection>;

    constructor(private $q: ng.IQService,
                private $http: ng.IHttpService,
                private $log: ng.ILogService,
                private localization: ILocalizationService) {

        this._collection = new Rx.BehaviorSubject<ICollection>(null);
    }

    public get collection(): Rx.Observable<ICollection> {
        return this._collection.asObservable();
    }

    public getCollection(id: number): ng.IPromise<ICollection> {
        const defer = this.$q.defer<ICollection>();

        this.$http.get<ICollection>("/svc/bpartifactstore/collection/" + id)
            .then((result: ng.IHttpPromiseCallbackArg<ICollection>) => {
                this._collection.onNext(result.data);
                defer.resolve(result.data);

            }, (result: ng.IHttpPromiseCallbackArg<any>) => {
                defer.reject(result.data);
            });

        return defer.promise;
    }

    public addArtifactToCollection(artifactId: number, collectionId: number, addChildren: boolean): ng.IPromise<number> {
        const url = `svc/bpartifactstore/collection/${collectionId}/add/${artifactId}`;

        const requestObj: ng.IRequestConfig = {
            url: url,
            method: "POST",
            data: angular.toJson(addChildren)
        };

        return this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<number>) => {
                return result.data;
            },
            (result: ng.IHttpPromiseCallbackArg<any>) => {
                return this.$q.reject(result.data);
            }
        );
    }
}
