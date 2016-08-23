import { ILocalizationService } from "../../../core";
import {Relationships} from "../../../main";

export interface IArtifactRelationshipsResultSet {
    manualTraces: Relationships.IRelationship[];
    otherTraces: Relationships.IRelationship[]; 
}

export interface IArtifactRelationships {
    getRelationships(artifactId: number, subArtifactId?: number, timeout?: ng.IPromise<void>): ng.IPromise<IArtifactRelationshipsResultSet>;
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
        artifactId: number,
        subArtifactId?: number,
        timeout?: ng.IPromise<void>): ng.IPromise<IArtifactRelationshipsResultSet> {
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
                    defer.resolve(result.data);               
            },
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                const error = {
                    statusCode: errResult.status,
                    message: (errResult.data ? errResult.data.message : "") || this.localization.get("Artifact_NotFound", "Error")
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

        this.$http(requestObj).then(
            (result: ng.IHttpPromiseCallbackArg<Relationships.IRelationshipExtendedInfo>) => {                
                    defer.resolve(result.data);               
            },
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                const error = {
                    statusCode: errResult.status,
                    message: (errResult.data ? errResult.data.message : "") || this.localization.get("Artifact_NotFound", "Error")
                };
                this.$log.error(error);
                defer.reject(error);
            });

        return defer.promise;
    }
}
