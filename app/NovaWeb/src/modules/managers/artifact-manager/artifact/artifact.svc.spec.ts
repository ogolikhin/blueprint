import * as angular from "angular";
import "angular-mocks";
import { ApplicationError, HttpStatusCode} from "../../../core";
import {LocalizationServiceMock} from "../../../core/localization/localization.mock";
import {Models, IArtifactService, ArtifactService} from "./artifact.svc";
import {ArtifactServiceMock} from "./artifact.svc.mock";

describe("Artifact Repository", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("artifactService", ArtifactService);
    }));

    describe("Get Artifact", () => {

        it("get artifact", inject(($httpBackend: ng.IHttpBackendService, artifactService: IArtifactService) => {
            // Arrange
            $httpBackend.expectGET("/svc/bpartifactstore/artifacts/100")
                .respond(HttpStatusCode.Success, ArtifactServiceMock.createArtifact(100));

            // Act
            let error: any;
            let data: Models.IArtifact;
            artifactService.getArtifact(100).then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(data).not.toBeUndefined();
            expect(data.id).toEqual(100);
            expect(data.name).toEqual("Artifact 100");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("get one folder unsuccessfully", inject(($httpBackend: ng.IHttpBackendService, artifactService: IArtifactService) => {
            // Arrange
            $httpBackend.expectGET("/svc/bpartifactstore/artifacts/100")
                .respond(HttpStatusCode.Unauthorized, new ApplicationError({
                        statusCode: HttpStatusCode.Unauthorized
                    }) 
                );

            // Act
            let error: any;
            let data: Models.IArtifact;
            artifactService.getArtifact(100).then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeDefined();
            expect(error.statusCode).toEqual(HttpStatusCode.Unauthorized);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });

    describe("Update Artifact", () => {

        it("update artifact", inject(($httpBackend: ng.IHttpBackendService, artifactService: IArtifactService) => {
            // Arrange
            $httpBackend.expectPATCH("/svc/bpartifactstore/artifacts/100", angular.toJson(ArtifactServiceMock.createArtifact(100)))
                .respond(HttpStatusCode.Success, ArtifactServiceMock.createLightArtifact(100));

            // Act
            let error: any;
            let data: Models.IArtifact;
            artifactService.updateArtifact(ArtifactServiceMock.createArtifact(100)).then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(data).not.toBeUndefined();
            expect(data.id).toEqual(100);
            expect(data.version).toEqual(1);
            expect(data.lastSavedOn).toEqual("20160831T16:00:00");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("update artifact unsuccessfully", inject(($httpBackend: ng.IHttpBackendService, artifactService: IArtifactService) => {
            // Arrange
            $httpBackend.expectPATCH("/svc/bpartifactstore/artifacts/100", angular.toJson(ArtifactServiceMock.createArtifact(100)))
                .respond(HttpStatusCode.Unauthorized, {
                    statusCode: HttpStatusCode.Unauthorized
                });

            // Act
            let error: any;
            let data: Models.IArtifact;
            artifactService.updateArtifact(ArtifactServiceMock.createArtifact(100)).then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeDefined();
            expect(error.statusCode).toEqual(HttpStatusCode.Unauthorized);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });

///---

    describe("Get Artifact dependants:", () => {

        it("5 children -> successful", inject(($httpBackend: ng.IHttpBackendService, artifactService: IArtifactService) => {
            // Arrange
            
            $httpBackend.expectGET("svc/artifactstore/projects/1/artifacts/100/children")
                .respond(HttpStatusCode.Success, ArtifactServiceMock.createChildren(100, 5));

            // Act
            let error: any;
            let data: Models.IArtifact[];
            artifactService.getChilden(1, 100).then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(data).toEqual(jasmine.any(Array));
            expect(data.length).toEqual(5);
            expect(data[0].id).toEqual(101);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("5 children -> unsuccessful ", inject(($httpBackend: ng.IHttpBackendService, artifactService: IArtifactService) => {
            // Arrange
            $httpBackend.expectGET("svc/artifactstore/projects/1/artifacts/100/children")
                .respond(HttpStatusCode.NotFound, {
                    statusCode: HttpStatusCode.NotFound
                });

            // Act
            let error: any;
            let data: Models.IArtifact[];
            artifactService.getChilden(1, 100).then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeDefined();
            expect(error.statusCode).toEqual(HttpStatusCode.NotFound);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });


    describe("Delete Artifact:", () => {

        it("Single -> successful", inject(($httpBackend: ng.IHttpBackendService, artifactService: IArtifactService) => {
            // Arrange
            
            $httpBackend.expectDELETE("svc/bpartifactstore/artifacts/100")
                .respond(HttpStatusCode.Success, ArtifactServiceMock.createChildren(99, 1));

            // Act
            let error: any;
            let data: Models.IArtifact[];
            artifactService.deleteArtifact(100).then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(data).toEqual(jasmine.any(Array));
            expect(data.length).toEqual(1);
            expect(data[0].id).toEqual(100);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
        it("Single -> unsuccessful", inject(($httpBackend: ng.IHttpBackendService, artifactService: IArtifactService) => {
            // Arrange
            $httpBackend.expectDELETE("svc/bpartifactstore/artifacts/100")
                .respond(HttpStatusCode.NotFound, {
                    statusCode: HttpStatusCode.NotFound
                });

            // Act
            let error: any;
            let data: Models.IArtifact[];
            artifactService.deleteArtifact(100).then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeDefined();
            expect(error.statusCode).toEqual(HttpStatusCode.NotFound);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
        

        it("5 artifact -> successful", inject(($httpBackend: ng.IHttpBackendService, artifactService: IArtifactService) => {
            // Arrange
            
            $httpBackend.expectDELETE("svc/bpartifactstore/artifacts/200")
                .respond(HttpStatusCode.Success, ArtifactServiceMock.createChildren(200, 5));

            // Act
            let error: any;
            let data: Models.IArtifact[];
            artifactService.deleteArtifact(200).then((responce) => {
                data = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(data).toEqual(jasmine.any(Array));
            expect(data.length).toEqual(5);
            expect(data[0].id).toEqual(201);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });



});
