import { ILocalizationService } from "../../core";
import { Models } from "../../main";

export interface IGlossaryService {
    getGlossary(id: number): ng.IPromise<IGlossaryDetails>;
}

export interface IGlossaryTerm {
    id: number;
    name: string;
    definition: string;
    typePrefix: string;
    predefined: Models.ItemTypePredefined;
    selected?: boolean;
}

export interface IGlossaryDetails {
    id: number;
    terms: IGlossaryTerm[];
}

export class GlossaryService implements IGlossaryService {
    static $inject: [string] = [
        "$q",
        "$http",
        "$log",
        "localization"];

    private _glossary: Rx.BehaviorSubject<IGlossaryDetails>;

    constructor(
        private $q: ng.IQService,
        private $http: ng.IHttpService,
        private $log: ng.ILogService,
        private localization: ILocalizationService) {

        this._glossary = new Rx.BehaviorSubject<IGlossaryDetails>(null);
    }

    public get glossary(): Rx.Observable<IGlossaryDetails> {
        return this._glossary.asObservable();
    }

    public getGlossary(id: number): ng.IPromise<IGlossaryDetails> {
        const defer = this.$q.defer<IGlossaryDetails>();

        this.$http.get("/svc/components/RapidReview/glossary/" + id + "?includeDraft=true")
            .success((result: IGlossaryDetails) => {
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
