import { ILocalizationService } from "../../../../core";
import { Relationships } from "../../../../main";

export interface IRelationshipDetailsService {
    getRelationshipDetails(artifactId: number): ng.IPromise<Relationships.IRelationshipExtendedInfo>;
}

export class RelationshipDetailsService implements IRelationshipDetailsService {
    static $inject: [string] = [
        "$q",
        "$http",
        "$log",
        "localization"];

    constructor(
        private $q: ng.IQService,
        private $http: ng.IHttpService,
        private $log: ng.ILogService,
        private localization: ILocalizationService) {
    }

    public getRelationshipDetails(
        artifactId: number): ng.IPromise<Relationships.IRelationshipExtendedInfo> {
        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}/relationshipdetails`,
            method: "GET"
        };

        this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<Relationships.IRelationshipExtendedInfo>) => {
                    defer.resolve(result.data);
            }, (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                const error = {
                    statusCode: errResult.status,
                    message: errResult.data ? errResult.data.message : "Artifact_NotFound"
                };
                defer.reject(error);
            });

        return defer.promise;
    }
}
