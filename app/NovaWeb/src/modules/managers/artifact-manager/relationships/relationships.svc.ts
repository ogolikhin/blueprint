import { ILocalizationService } from "../../../core";
import { Relationships } from "../../../main";

interface IArtifactRelationshipsResultSet {
    manualTraces: Relationships.IRelationship[];
    otherTraces: Relationships.IRelationship[];
}

export interface IArtifactRelationshipsService {
    getRelationships(artifactId: number, subArtifactId?: number, timeout?: ng.IPromise<void>): ng.IPromise<Relationships.IRelationship[]>;
}

export class ArtifactRelationshipsService implements IArtifactRelationshipsService {
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

    public getRelationships(
        artifactId: number,
        subArtifactId?: number,
        timeout?: ng.IPromise<void>): ng.IPromise<Relationships.IRelationship[]> {
        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}/relationships`,
            method: "GET",
            params: {
                subartifactId: subArtifactId
            }
        };

        this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<IArtifactRelationshipsResultSet>) => {
                    const manual = result.data.manualTraces || [];
                    const other = result.data.otherTraces || [];

                    defer.resolve(manual.concat(other));
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
