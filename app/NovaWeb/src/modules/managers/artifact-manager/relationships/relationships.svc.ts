import {ILocalizationService} from "../../../core";
import {Relationships} from "../../../main";

export interface IArtifactRelationshipsService {
    getRelationships(artifactId: number, subArtifactId?: number, timeout?: ng.IPromise<void>): ng.IPromise<Relationships.IArtifactRelationshipsResultSet>;
}

export class ArtifactRelationshipsService implements IArtifactRelationshipsService {
    static $inject: [string] = [
        "$q",
        "$http",
        "$log",
        "localization"];

    constructor(private $q: ng.IQService,
                private $http: ng.IHttpService,
                private $log: ng.ILogService,
                private localization: ILocalizationService) {
    }

    public getRelationships(artifactId: number,
                            subArtifactId?: number,
                            timeout?: ng.IPromise<void>): ng.IPromise<Relationships.IArtifactRelationshipsResultSet> {
        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}/relationships`,
            method: "GET",
            params: {
                subartifactId: subArtifactId
            }
        };

        this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<Relationships.IArtifactRelationshipsResultSet>) => {
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
