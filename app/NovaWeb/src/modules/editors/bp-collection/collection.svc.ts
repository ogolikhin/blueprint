import {ILocalizationService} from "../../core";
import {ICollection, ICollectionArtifact} from "./models";

export interface ICollectionService {
    getCollection(id: number): ng.IPromise<ICollection>;
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
                if (!result) {
                    defer.reject();
                    return;
                }
                const error = {
                    statusCode: result.status,
                    message: result.data ? result.data.message : ""
                };
                defer.reject(error);
            });

        return defer.promise;
    }
}
