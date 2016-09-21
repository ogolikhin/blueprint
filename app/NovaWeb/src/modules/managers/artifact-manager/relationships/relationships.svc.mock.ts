import { IArtifactRelationshipsService } from "./relationships.svc";
import { Relationships } from "../../../main";

export class ArtifactRelationshipsMock implements IArtifactRelationshipsService {

    public static $inject = ["$q"];

    public artifactHistory;

    constructor(private $q: ng.IQService) { }

    public getRelationships(artifactId: number): ng.IPromise<Relationships.IRelationship[]> {
        const deferred = this.$q.defer<any>();
        const artifactList = [
            {
                "artifactId": "1",
                "artifactTypePrefix": "PRE",
                "artifactName": "Artifact1",
                "itemId": "2",
                "itemTypePrefix": "PRE",
                "itemName": "Item1",
                "projectId": "1",
                "projectName": "Project1",
                "traceDirection": {},
                "traceType": 2,
                "suspect": false,
                "hasAccess": true,
                "primitiveItemTypePredefined": "1"
            }, {
                "artifactId": "3",
                "artifactTypePrefix": "PRE",
                "artifactName": "Artifact1",
                "itemId": "4",
                "itemTypePrefix": "PRE",
                "itemName": "Item1",
                "projectId": "1",
                "projectName": "Project1",
                "traceDirection": {},
                "traceType": 2,
                "suspect": false,
                "hasAccess": true,
                "primitiveItemTypePredefined": 1
            },
            {
                "artifactId": "1",
                "artifactTypePrefix": "PRE",
                "artifactName": "Artifact1",
                "itemId": "2",
                "itemTypePrefix": "PRE",
                "itemName": "Item1",
                "projectId": "1",
                "projectName": "Project1",
                "traceDirection": {},
                "traceType": 16,
                "suspect": false,
                "hasAccess": true,
                "primitiveItemTypePredefined": "1"
            }, {
                "artifactId": "3",
                "artifactTypePrefix": "PRE",
                "artifactName": "Artifact1",
                "itemId": "4",
                "itemTypePrefix": "PRE",
                "itemName": "Item1",
                "projectId": "1",
                "projectName": "Project1",
                "traceDirection": {},
                "traceType": 32,
                "suspect": false,
                "hasAccess": true,
                "primitiveItemTypePredefined": 1
            },
            {
                "artifactId": "3",
                "artifactTypePrefix": "PRE",
                "artifactName": "Artifact1",
                "itemId": "4",
                "itemTypePrefix": "PRE",
                "itemName": "Item1",
                "projectId": "1",
                "projectName": "Project1",
                "traceDirection": {},
                "traceType": 64,
                "suspect": false,
                "hasAccess": true,
                "primitiveItemTypePredefined": 1
            }
        ];

        deferred.resolve(artifactList);
        return deferred.promise;
    }

    public getRelationshipDetails(artifactId: number): ng.IPromise<Relationships.IRelationshipExtendedInfo> {
        const deferred = this.$q.defer<any>();

        var details = {

            "artifactId" : "1",
            "description": "desc",
            "pathToProject": [{ "itemId": 1, "itemName": "Item1", "parentId": 0}]
        };

        deferred.resolve(details);
        return deferred.promise;
    }
}

