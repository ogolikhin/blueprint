import { ILocalizationService } from "../../../core";
import { Models } from "../../../main";

export interface IArtifactRelationship {
    //fields
}

export interface IArtifactRelationshipsResultSet {
    artifactRelationships: IArtifactRelationship[];
    artifactId: number;
}

export interface IArtifactRelationships {
    getRelationships(artifactId: number): ng.IPromise<IArtifactRelationship[]>;
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
        artifactId: number): ng.IPromise<IArtifactRelationship[]> {
        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}/version`, 
            method: "GET"
        };

        this.$http(requestObj)
            .success((result: IArtifactRelationshipsResultSet) => {
                //defer.resolve(result.artifactRelationships);

                var a = [];
                a.push(<IArtifactRelationship>{ "id": "1" });
                a.push(<IArtifactRelationship>{ "id": "2" });
                a.push(<IArtifactRelationship>{ "id": "3" });
                a.push(<IArtifactRelationship>{ "id": "4" });
                a.push(<IArtifactRelationship>{ "id": "5" });
                defer.resolve(a);

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
