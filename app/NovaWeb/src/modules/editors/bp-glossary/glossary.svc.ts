import { ILocalizationService } from "../../core";
import { Models } from "../../main";

export interface IGlossaryService {
    getGlossary(id: number): ng.IPromise<IGlossaryDetals>;
}

export interface IGlossaryTerm {
    id: number;
    name: string;
    definition: string;
    typePrefix: string;
    predefined: Models.ItemTypePredefined;
    selected?: boolean;
}

export interface IGlossaryDetals {
    id: number;
    terms: IGlossaryTerm[];
}

export class GlossaryService implements IGlossaryService {
    static $inject: [string] = [
        "$q",
        "$http",
        "$log",
        "localization"];

    private _glossary: Rx.BehaviorSubject<IGlossaryDetals>;

    constructor(
        private $q: ng.IQService,
        private $http: ng.IHttpService,
        private $log: ng.ILogService,
        private localization: ILocalizationService) {

        this._glossary = new Rx.BehaviorSubject<IGlossaryDetals>(null);
    }

    public get glossary(): Rx.Observable<IGlossaryDetals> {
        return this._glossary.asObservable();
    }

    public getGlossary(id: number): ng.IPromise<IGlossaryDetals> {
        const defer = this.$q.defer<IGlossaryDetals>();

        this.$http.get("/svc/components/RapidReview/glossary/" + id + "?includeDraft=true")
            .success((result: IGlossaryDetals) => {
                this._glossary.onNext(result);
                defer.resolve(result);

            }).error((err: any, statusCode: number) => {
                const error = {
                    statusCode: statusCode,
                    message: (err ? err.message : "") || this.localization.get("Artifact_NotFound", "Error")
                };
                this.$log.error(error);
                defer.reject(error);
            });
            
        return defer.promise;
    }
}
