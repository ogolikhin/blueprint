import * as angular from "angular";
import "angular-mocks";
import {LocalizationServiceMock} from "../../../core/localization/localization.mock";
import {IArtifactAttachmentsService, ArtifactAttachmentsService, IArtifactAttachmentsResultSet} from "./attachments.svc";

describe("Artifact Attachments Service", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactAttachments", ArtifactAttachmentsService);
        $provide.service("localization", LocalizationServiceMock);
    })); 

    it("get artifact attachments with default values", 
        inject(($httpBackend: ng.IHttpBackendService, artifactAttachments: IArtifactAttachmentsService) => {

        // Arrange
        $httpBackend.expectGET(`/svc/artifactstore/artifacts/306/attachment?addDrafts=true`)
            .respond(200, {
                "artifactId": 306,
                "subartifactId": null,
                "attachments": [
                    {
                    "userId": 1,
                    "userName": "admin",
                    "fileName": "gir_ride_a_pig_by_frequencyspark-d75th8v.png",
                    "attachmentId": 1093,
                    "uploadedDate": "2016-06-23T14:54:27.273Z"
                    }
                ],
                "documentReferences": [
                    {
                    "artifactName": "acc-wizard.d.ts",
                    "artifactId": 258,
                    "userId": 1,
                    "userName": "admin",
                    "referencedDate": "2016-06-23T14:54:27.273Z"
                    }
                ]
            });

        // Act
        let error: any;
        let data: IArtifactAttachmentsResultSet;
        artifactAttachments.getArtifactAttachments(306).then( (response) => {
            data = response;
        }, (err) => {
            error = err; 
        });

        $httpBackend.flush();

        // Assert
        expect(error).toBeUndefined();
        expect(data.attachments).toEqual(jasmine.any(Array));
        expect(data.documentReferences).toEqual(jasmine.any(Array));
        expect(data.artifactId).toEqual(306);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("gets an error if artifact id is invalid", 
        inject(($httpBackend: ng.IHttpBackendService, artifactAttachments: IArtifactAttachmentsService) => {

        // Arrange
        $httpBackend.expectGET(`/svc/artifactstore/artifacts/0/attachment?addDrafts=true`)
            .respond(404, {
                statusCode: 404,
                message: "Couldn't find the artifact"
            });

        // Act
        let error: any;
        let data: IArtifactAttachmentsResultSet;
        artifactAttachments.getArtifactAttachments(0).then( (response) => {
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
