import { ILocalizationService } from "../../../core";
import {Relationships} from "../../../main";




export interface IArtifactRelationshipsResultSet {
    manualTraces: Relationships.Relationship[];
    otherTraces: Relationships.Relationship[];
  //  artifactId: number;
}

export interface IArtifactRelationships {
    getRelationships(artifactId: number, relationshipType: any): ng.IPromise<Relationships.Relationship[]>;
    getRelationshipDetails(artifactId: number): ng.IPromise<Relationships.RelationshipExtendedInfo>;
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
        artifactId: number, traceType: Relationships.ITraceType): ng.IPromise<Relationships.Relationship[]> {
        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}/relationships`,           
            method: "GET"
        };

        this.$http(requestObj)
            .success((result: IArtifactRelationshipsResultSet) => {
                if (traceType === Relationships.ITraceType.Manual) {
                    defer.resolve(result.manualTraces);
                } else {
                    defer.resolve(result.otherTraces);
                }
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

    public getRelationshipDetails(
        artifactId: number): ng.IPromise<Relationships.RelationshipExtendedInfo> {
        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}/relationshipdetails`,
            method: "GET"
        };

        this.$http(requestObj)
            .success((result: Relationships.RelationshipExtendedInfo) => {                
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
