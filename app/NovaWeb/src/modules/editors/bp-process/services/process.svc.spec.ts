import * as angular from "angular";
import {ProcessService, IProcessService} from "./process.svc";
import {MessageServiceMock} from "../../../core/messages/message.mock";
import {HttpStatusCode} from "../../../core/http";
import {createDefaultProcessModel} from "../models/test-model-factory";

describe("Get process data model from the process model service", () => {

    let service: IProcessService, httpBackend;

    // Set up the module
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("processService", ProcessService);
        $provide.service("messageService", MessageServiceMock);
    }));

    beforeEach(inject((processService,
                       $httpBackend: ng.IHttpBackendService) => {
        service = processService;
        httpBackend = $httpBackend;
    }));

    afterEach(() => {
        httpBackend.verifyNoOutstandingExpectation();
        httpBackend.verifyNoOutstandingRequest();
    });

    describe("load", () => {
        describe("When process model is returned from the server", () => {
            let testData = createDefaultProcessModel();
            it("should return data successfully through a promise", () => {
                // Arrange
                var successSpy = jasmine.createSpy("success"),
                    failureSpy = jasmine.createSpy("failure");
                httpBackend.expectGET("/svc/components/storyteller/processes/772")
                    .respond(testData);

                // Act
                service.load("772").then(successSpy, failureSpy);
                httpBackend.flush();

                // Assert
                expect(successSpy).toHaveBeenCalled();
                expect(failureSpy).not.toHaveBeenCalled();

            });

            it("should return data successfully through a promise when proper versionid is supplied", () => {
                // Arrange
                let mockArtifactId = "772";
                let mockVersionId = 123;

                let successSpy = jasmine.createSpy("success"),
                    failureSpy = jasmine.createSpy("failure");
                httpBackend.when("GET", "/svc/components/storyteller/processes/" + mockArtifactId + "?versionId=" + mockVersionId)
                    .respond(testData);

                // Act
                service.load(mockArtifactId, mockVersionId).then(successSpy, failureSpy);
                httpBackend.flush();

                // Assert
                expect(successSpy).toHaveBeenCalled();
                expect(failureSpy).not.toHaveBeenCalled();
            });

            it("should return data successfully through a promise when proper revisionId is supplied", () => {
                // Arrange
                let mockArtifactId = "772";
                let mockRevisionId = 123;

                let successSpy = jasmine.createSpy("success"),
                    failureSpy = jasmine.createSpy("failure");
                httpBackend.when("GET", "/svc/components/storyteller/processes/" + mockArtifactId + "?revisionId=" + mockRevisionId)
                    .respond(testData);

                // Act
                service.load(mockArtifactId, null, mockRevisionId).then(successSpy, failureSpy);
                httpBackend.flush();

                // Assert
                expect(successSpy).toHaveBeenCalled();
                expect(failureSpy).not.toHaveBeenCalled();
            });

            it("should return data successfully through a promise when proper baselineId is supplied", () => {
                // Arrange
                let mockArtifactId = "772";
                let mockBaselineId = 123;

                let successSpy = jasmine.createSpy("success"),
                    failureSpy = jasmine.createSpy("failure");
                httpBackend.when("GET", "/svc/components/storyteller/processes/" + mockArtifactId + "?baselineId=" + mockBaselineId)
                    .respond(testData);

                // Act
                service.load(mockArtifactId, null, null, mockBaselineId).then(successSpy, failureSpy);
                httpBackend.flush();

                // Assert
                expect(successSpy).toHaveBeenCalled();
                expect(failureSpy).not.toHaveBeenCalled();
            });

            it("should return data successfully through a promise when readonly property is supplied", () => {
                // Arrange
                let mockArtifactId = "772";
                let isReadOnly = true;

                let successSpy = jasmine.createSpy("success"),
                    failureSpy = jasmine.createSpy("failure");
                httpBackend.when("GET", "/svc/components/storyteller/processes/" + mockArtifactId + "?readOnly=" + isReadOnly)
                    .respond(testData);

                // Act
                service.load(mockArtifactId, null, null, null, isReadOnly).then(successSpy, failureSpy);
                httpBackend.flush();

                // Assert
                expect(successSpy).toHaveBeenCalled();
                expect(failureSpy).not.toHaveBeenCalled();
            });
        });

        describe("When process model is not returned from the server", () => {
            it("should reject data through promise if the server is broken", () => {
                // Arrange
                var successSpy = jasmine.createSpy("success"),
                    failureSpy = jasmine.createSpy("failure");
                httpBackend.when("GET", "/svc/components/storyteller/processes/772")
                    .respond(() => [HttpStatusCode.ServerError, {}, {}, "Internal Server Error"]);

                // Act
                service.load("772").then(successSpy, failureSpy);
                httpBackend.flush();

                // Assert
                expect(successSpy).not.toHaveBeenCalled();
                expect(failureSpy).toHaveBeenCalled();
            });

            it("should reject data through promise if data is not found", () => {
                // Arrange
                var successSpy = jasmine.createSpy("success"),
                    failureSpy = jasmine.createSpy("failure");
                httpBackend.when("GET", "/svc/components/storyteller/processes/772")
                    .respond(() => [HttpStatusCode.NotFound, {}, {}, "Not Found"]);

                // Act
                service.load("772").then(successSpy, failureSpy);
                httpBackend.flush();

                // Assert
                expect(successSpy).not.toHaveBeenCalled();
                expect(failureSpy).toHaveBeenCalled();
            });
        });
    });    
});
