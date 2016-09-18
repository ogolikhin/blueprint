import "angular";
import "angular-mocks";
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
                .respond(200, ArtifactServiceMock.createArtifact(100));

            // Act
            var error: any;
            var data: Models.IArtifact;
            artifactService.getArtifact(100).then((responce) => { data = responce; }, (err) => error = err);
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
                .respond(401);
                
            // Act
            var error: any;
            var data: Models.IArtifact;
            artifactService.getArtifact(100).then((responce) => { data = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeDefined();
            expect(error.statusCode).toEqual(401);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });

    describe("Update Artifact", () => {

        it("update artifact", inject(($httpBackend: ng.IHttpBackendService, artifactService: IArtifactService) => {
            // Arrange
            $httpBackend.expectPATCH("/svc/bpartifactstore/artifacts/100", angular.toJson(ArtifactServiceMock.createArtifact(100)))
                .respond(200, ArtifactServiceMock.createLightArtifact(100));

            // Act
            var error: any;
            var data: Models.IArtifact;
            artifactService.updateArtifact(ArtifactServiceMock.createArtifact(100)).then((responce) => { data = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(data).not.toBeUndefined();
            expect(data.id).toEqual(100);
            expect(data.version).toEqual(1);
            expect(data.lastSavedOn).toEqual('20160831T16:00:00');
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("update artifact unsuccessfully", inject(($httpBackend: ng.IHttpBackendService, artifactService: IArtifactService) => {
            // Arrange
            $httpBackend.expectPATCH("/svc/bpartifactstore/artifacts/100", angular.toJson(ArtifactServiceMock.createArtifact(100)))
                .respond(401);
                
            // Act
            var error: any;
            var data: Models.IArtifact;
            artifactService.updateArtifact(ArtifactServiceMock.createArtifact(100)).then((responce) => { data = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBeDefined();
            expect(error.statusCode).toEqual(401);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });

});