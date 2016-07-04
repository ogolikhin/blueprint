import { ILocalizationService } from "../../../core";
import * as Models from "../../../main/models/models";


export enum ITraceType {
    Manual = 0,
    Other = 1
}

export interface IArtifactRelationshipsResultSet {
    manualTraces: Models.IArtifactDetails[];
    otherTraces: Models.IArtifactDetails[];
  //  artifactId: number;
}

export interface IArtifactRelationships {
    getRelationships(artifactId: number, relationshipType: any): ng.IPromise<Models.IArtifactDetails[]>;
}

export class ArtifactRelationships implements IArtifactRelationships {
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
        artifactId: number, traceType: ITraceType): ng.IPromise<Models.IArtifactDetails[]> {
        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}/relationships`,           
            method: "GET"
        };

        this.$http(requestObj)
            .success((result: IArtifactRelationshipsResultSet) => {
                if (traceType === ITraceType.Manual) {
                    defer.resolve(result.manualTraces);
                } else {
                    defer.resolve(result.otherTraces);
                }
            }).error((err: any, statusCode: number) => {
                const error = {
                    statusCode: statusCode,
                    message: (err ? err.Message : "") || this.localization.get("Artifact_NotFound", "Error")
                };
                this.$log.error(error);
                defer.reject(error);
            });
            
        return defer.promise;
    }
}
