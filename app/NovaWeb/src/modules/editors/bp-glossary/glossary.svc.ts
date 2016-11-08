import {IArtifact} from "../../main/models/models";
import {ILocalizationService} from "../../core/localization/localizationService";

export interface IGlossaryService {
    getGlossary(id: number): ng.IPromise<IArtifact>;
}

export class GlossaryService implements IGlossaryService {
    static $inject: [string] = [
        "$q",
        "$http",
        "$log",
        "localization"];

    private _glossary: Rx.BehaviorSubject<IArtifact>;

    constructor(private $q: ng.IQService,
                private $http: ng.IHttpService,
                private $log: ng.ILogService,
                private localization: ILocalizationService) {

        this._glossary = new Rx.BehaviorSubject<IArtifact>(null);
    }

    public get glossary(): Rx.Observable<IArtifact> {
        return this._glossary.asObservable();
    }

    public getGlossary(id: number): ng.IPromise<IArtifact> {
        const defer = this.$q.defer<IArtifact>();

        this.$http.get<IArtifact>("/svc/bpartifactstore/glossary/" + id)
            .then((result: ng.IHttpPromiseCallbackArg<IArtifact>) => {
                this._glossary.onNext(result.data);
                defer.resolve(result.data);

            }, (result: ng.IHttpPromiseCallbackArg<any>) => {
                defer.reject(result.data);
            });

        return defer.promise;
    }
}
