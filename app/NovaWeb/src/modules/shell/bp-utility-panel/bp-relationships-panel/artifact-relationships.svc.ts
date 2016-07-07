import { ILocalizationService } from "../../../core";
import {Relationships} from "../../../main";

export interface IArtifactRelationshipsResultSet {
    manualTraces: Relationships.IRelationship[];
    otherTraces: Relationships.IRelationship[]; 
}

export interface IArtifactRelationships {
    getRelationships(artifactId: number): ng.IPromise<IArtifactRelationshipsResultSet>;
    getRelationshipDetails(artifactId: number): ng.IPromise<Relationships.IRelationshipExtendedInfo>;
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
        artifactId: number): ng.IPromise<IArtifactRelationshipsResultSet> {
        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}/relationships`,           
            method: "GET"
        };

        this.$http(requestObj)
            .success((result: IArtifactRelationshipsResultSet) => {               
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

    public getRelationshipDetails(
        artifactId: number): ng.IPromise<Relationships.IRelationshipExtendedInfo> {
        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}/relationshipdetails`,
            method: "GET"
        };

        this.$http(requestObj)
            .success((result: Relationships.IRelationshipExtendedInfo) => {                
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
