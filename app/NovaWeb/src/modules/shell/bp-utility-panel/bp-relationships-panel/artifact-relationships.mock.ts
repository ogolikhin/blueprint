import { IArtifactRelationships } from "./artifact-relationships.svc";
import { Models } from "../../../main";

export class ArtifactRelationshipsMock implements IArtifactRelationships {

    public static $inject = ["$q"];

    public artifactHistory;

    constructor(private $q: ng.IQService) { }

    public getRelationships(artifactId: number): ng.IPromise<Models.IArtifactDetails[]> {
        const deferred = this.$q.defer<any[]>();
        
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

