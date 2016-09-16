import "angular";
import "angular-mocks";
import {LocalizationServiceMock} from "../../../core/localization/localization.mock";
import {IArtifactRelationshipsResultSet, IArtifactRelationships, ArtifactRelationships} from "./relationships.svc";
import {Relationships} from "../../../main";

describe("Artifact Relationships Service", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactRelationships", ArtifactRelationships);
        $provide.service("localization", LocalizationServiceMock);
    }));

    it("get artifact relationships with default values", inject(($httpBackend: ng.IHttpBackendService, artifactRelationships: IArtifactRelationships) => {
        // Arrange
        $httpBackend.expectGET(`/svc/artifactstore/artifacts/5/relationships`)
            .respond(200, {
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
            });

        // Act
        let error: any;
        let data: IArtifactRelationshipsResultSet;
        artifactRelationships.getRelationships(5).then((response) => {
            data = response;
        }, (err) => {
            error = err;
        });

        $httpBackend.flush();

        // Assert
        expect(error).toBeUndefined();
        expect(data).not.toBeUndefined();
        expect(data.manualTraces.length).toEqual(2);
        expect(data.otherTraces.length).toEqual(3);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("gets an error if artifact id is invalid", inject(($httpBackend: ng.IHttpBackendService, artifactRelationships: IArtifactRelationships) => {
        // Arrange
        $httpBackend.expectGET(`/svc/artifactstore/artifacts/5/relationships`)
            .respond(404, {
                statusCode: 404,
                message: "Couldn't find the artifact"
            });

        // Act
        let error: any;
        let data: IArtifactRelationshipsResultSet;
        artifactRelationships.getRelationships(5).then((response) => {
            data = response;
        }, (err) => {
            error = err;
        });

        $httpBackend.flush();

        // Assert
        expect(data).toBeUndefined();
        expect(error.statusCode).toEqual(404);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("get artifact details with default values", inject(($httpBackend: ng.IHttpBackendService, artifactRelationships: IArtifactRelationships) => {
        // Arrange
        $httpBackend.expectGET(`/svc/artifactstore/artifacts/5/relationshipdetails`)
            .respond(200,            
                {
                    "artifactId": "1",
                    "description": "desc",
                    "pathToProject": [{ "itemId": 1, "itemName": "Item1", "parentId": 0 }]
                }
            );

        // Act
        let error: any;
        let data: Relationships.IRelationshipExtendedInfo;
        artifactRelationships.getRelationshipDetails(5).then((response) => {
            data = response;
        }, (err) => {
            error = err;
        });

        $httpBackend.flush();

        // Assert
        expect(error).toBeUndefined();
        expect(data).not.toBeUndefined();
        expect(data.artifactId).toEqual("1");
        expect(data.description).toEqual("desc");
        expect(data.pathToProject).toEqual([{ "itemId": 1, "itemName": "Item1", "parentId": 0 }]);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("gets an error if artifact id is invalid", inject(($httpBackend: ng.IHttpBackendService, artifactRelationships: IArtifactRelationships) => {
        // Arrange
        $httpBackend.expectGET(`/svc/artifactstore/artifacts/5/relationshipdetails`)
            .respond(404, {
                statusCode: 404,
                message: "Couldn't find the artifact"
            });

        // Act
        let error: any;
        let data: Relationships.IRelationshipExtendedInfo;
        artifactRelationships.getRelationshipDetails(5).then((response) => {
            data = response;
        }, (err) => {
            error = err;
        });

        $httpBackend.flush();

        // Assert
        expect(data).toBeUndefined();
        expect(error.statusCode).toEqual(404);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));
});
