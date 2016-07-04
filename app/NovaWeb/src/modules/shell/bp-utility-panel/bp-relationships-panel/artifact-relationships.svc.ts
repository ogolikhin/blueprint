import { ILocalizationService } from "../../../core";
import { Models } from "../../../main";

export interface IArtifactRelationship {
    artifactId: number;
    artifactName: string;
    artifactTypePrefix: string;
    itemId: number;
    itemName: string;
    itemTypePrefix: string;
    projectId: number;
    projectName: string;
    suspect: boolean;
    traceDirection: number;
    traceType: number;
}

export enum ITraceType {
    Trace = 0,
    Association = 1
}

export interface IArtifactRelationshipsResultSet {
    manualTraces: IArtifactRelationship[];
    otherTraces: IArtifactRelationship[];
  //  artifactId: number;
}

export interface IArtifactRelationships {
    getRelationships(artifactId: number, relationshipType: any): ng.IPromise<IArtifactRelationship[]>;
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
        artifactId: number, traceType: ITraceType): ng.IPromise<IArtifactRelationship[]> {
        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}/relationships`,           
            method: "GET"
        };

        this.$http(requestObj)
            .success((result: IArtifactRelationshipsResultSet) => {
                if (traceType === ITraceType.Trace) {
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
