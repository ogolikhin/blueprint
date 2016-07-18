import "angular";
import "angular-mocks";
import {Models, IArtifactService, ArtifactService} from "../../main/";

import {LocalizationServiceMock} from "../../core/localization.mock";
import {ArtifactServiceMock} from "./artifact.svc.mock";

describe("Artifact Repository", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("artifactService", ArtifactService);
    }));

    describe("Get Artifact", () => {

        it("get artifact", inject(($httpBackend: ng.IHttpBackendService, artifactService: IArtifactService) => {
            // Arrange
            $httpBackend.expectGET("/svc/artifactstore/artifacts/100")
                .respond(200, ArtifactServiceMock.createArtifact(100));

            // Act
            var error: any;
            var data: Models.IArtifact;
            artifactService.getArtifact(100).then((responce) => { data = responce; }, (err) => error = err);
            $httpBackend.flush();
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
            $httpBackend.expectGET("/svc/artifactstore/artifacts/100")
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

});