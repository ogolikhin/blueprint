﻿import "angular-mocks";
import {LocalizationServiceMock} from "../../../../commonModule/localization/localization.service.mock";
import {IRelationshipDetailsService, RelationshipDetailsService} from "./relationship-details.svc";
import {Relationships} from "../../../../main";
import {HttpStatusCode} from "../../../../commonModule/httpInterceptor/http-status-code";

describe("Artifact Relationships Service", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactRelationships", RelationshipDetailsService);
        $provide.service("localization", LocalizationServiceMock);
    }));

    it("get artifact details with default values", inject(($httpBackend: ng.IHttpBackendService, artifactRelationships: IRelationshipDetailsService) => {
        // Arrange
        $httpBackend.expectGET(`/svc/artifactstore/artifacts/5/relationshipdetails`)
            .respond(HttpStatusCode.Success,
                {
                    "artifactId": "1",
                    "description": "desc",
                    "pathToProject": [{"itemId": 1, "itemName": "Item1", "parentId": 0}]
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
        expect(data.pathToProject).toEqual([{"itemId": 1, "itemName": "Item1", "parentId": 0}]);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("gets an error if artifact id is invalid", inject(($httpBackend: ng.IHttpBackendService, artifactRelationships: IRelationshipDetailsService) => {
        // Arrange
        $httpBackend.expectGET(`/svc/artifactstore/artifacts/5/relationshipdetails`)
            .respond(HttpStatusCode.NotFound, {
                statusCode: HttpStatusCode.NotFound,
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
        expect(error.statusCode).toEqual(HttpStatusCode.NotFound);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));
});
