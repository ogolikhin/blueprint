import { IArtifactRelationships, IArtifactRelationshipsResultSet } from "./artifact-relationships.svc";
import { Models, Relationships } from "../../../main";

export class ArtifactRelationshipsMock implements IArtifactRelationships {

    public static $inject = ["$q"];

    public artifactHistory;

    constructor(private $q: ng.IQService) { }

    public getRelationships(artifactId: number): ng.IPromise<IArtifactRelationshipsResultSet> {
        const deferred = this.$q.defer<any>();

        
        var artifactList = {
            "manualTraces": [{
                "artifactId": "1",
                "artifactTypePrefix": "PRE",
                "artifactName": "Artifact1",
                "itemId": "2",
                "itemTypePrefix": "PRE",
                "itemName": "Item1",
                "projectId": "1",
                "projectName": "Project1",
                "rraceDirection": {},
                "traceType": {},
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
                    "rraceDirection": {},
                    "traceType": {},
                    "suspect": false,
                    "hasAccess": true,
                    "primitiveItemTypePredefined": 1
            }],
            "otherTraces": [{
                "artifactId": "1",
                "artifactTypePrefix": "PRE",
                "artifactName": "Artifact1",
                "itemId": "2",
                "itemTypePrefix": "PRE",
                "itemName": "Item1",
                "projectId": "1",
                "projectName": "Project1",
                "rraceDirection": {},
                "traceType": {},
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
                    "rraceDirection": {},
                    "traceType": {},
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
                "rraceDirection": {},
                "traceType": {},
                "suspect": false,
                "hasAccess": true,
                "primitiveItemTypePredefined": 1
            }]
        }    

        deferred.resolve(artifactList);
        return deferred.promise;
    }

    public getRelationshipDetails(artifactId: number): ng.IPromise<Relationships.IRelationshipExtendedInfo> {
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

