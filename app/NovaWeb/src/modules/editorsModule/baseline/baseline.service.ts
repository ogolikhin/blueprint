import {IBaseline} from "../configuration/classes/baseline-artifact";
import {ILocalizationService} from "../../commonModule/localization/localization.service";

export interface IBaselineService {
    getBaseline(id: number): ng.IPromise<IBaseline>;
    addArtifactToBaseline(artifactId: number, baselineId: number, addChildren: boolean): ng.IPromise<number>;
}

export class BaselineService implements IBaselineService {
    static $inject: [string] = [
        "$q",
        "$http",
        "$log",
        "localization"];

    private _baseline: Rx.BehaviorSubject<IBaseline>;

    constructor(private $q: ng.IQService,
                private $http: ng.IHttpService,
                private $log: ng.ILogService,
                private localization: ILocalizationService) {

        this._baseline = new Rx.BehaviorSubject<IBaseline>(null);
    }

    public get baseline(): Rx.Observable<IBaseline> {
        return this._baseline.asObservable();
    }

    public getBaseline(id: number): ng.IPromise<IBaseline> {
        const defer = this.$q.defer<IBaseline>();

        this.$http.get<IBaseline>("/svc/bpartifactstore/baseline/" + id)
            .then((result: ng.IHttpPromiseCallbackArg<IBaseline>) => {
                this._baseline.onNext(result.data);
                defer.resolve(result.data);

            }, (result: ng.IHttpPromiseCallbackArg<any>) => {
                defer.reject(result.data);
            });

        return defer.promise;
    }

    public addArtifactToBaseline(artifactId: number, baselineId: number, addChildren: boolean): ng.IPromise<number> {
        const url = `svc/bpartifactstore/baseline/${baselineId}/add/${artifactId}`;

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
