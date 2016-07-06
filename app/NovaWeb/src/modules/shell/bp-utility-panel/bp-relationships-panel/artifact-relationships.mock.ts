import { IArtifactRelationships, IArtifactRelationshipsResultSet } from "./artifact-relationships.svc";
import { Models, Relationships } from "../../../main";

export class ArtifactRelationshipsMock implements IArtifactRelationships {

    public static $inject = ["$q"];

    public artifactHistory;

    constructor(private $q: ng.IQService) { }

    public getRelationships(artifactId: number): ng.IPromise<IArtifactRelationshipsResultSet> {
        const deferred = this.$q.defer<IArtifactRelationshipsResultSet>();
        
        var artifactList = <IArtifactRelationshipsResultSet>{
            "manualTraces" :  [],
            "otherTraces": []
        }    

        deferred.resolve(artifactList);
        return deferred.promise;
    }

    public getRelationshipDetails(artifactId: number): ng.IPromise<Relationships.RelationshipExtendedInfo> {
        const deferred = this.$q.defer<any>();

        var artifactList = [
            {
                "id": 1

            },
            {
                "id": 2
            },
            {
                "id": 3
            }
        ];

        deferred.resolve(artifactList);
        return deferred.promise;
    }
}

