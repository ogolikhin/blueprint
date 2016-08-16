import {ProcessService, IProcessService} from "./process.svc";
import { MessageServiceMock } from "../../../../core/messages/message.mock";

describe("Get process data model from the process model service", () => {

    let service: IProcessService, httpBackend;

    let testData = "{\"description\":\"< html > <head></head><body style=\'padding: 1px 0px 0px\'><div style=\'padding: 0px\'><p style=\'margin: 0px\'>This is a test process</p> </div></body> </html>\",\"type\":0,\"shapes\":null,\"links\":null,\"rawData\":null,\"thumbnail\":null,\"artifactInfoParentId\":null,\"typeId\":281,\"lockedByUserId\":null,\"versionId\":1,\"permissions\":0,\"artifactDisplayId\":0,\"typePrefix\":\"PRO\",\"id\":772,\"name\":\"Test Process\",\"parentId\":772,\"orderIndex\":35.0,\"connectionsAndStates\":0}";
   
    // Set up the module
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("processService", ProcessService);
        $provide.service("messageService", MessageServiceMock);
    }));

    beforeEach(inject((
        processService,
        $httpBackend: ng.IHttpBackendService
        ) => {
        service = processService;
        httpBackend = $httpBackend;
    }));

    afterEach(() => {
        httpBackend.verifyNoOutstandingExpectation();
        httpBackend.verifyNoOutstandingRequest();
    });

    describe("load", () => {
        describe("When process model is returned from the server", () => {
            it("should return data successfully through a promise", () => {
                // Arrange
                var successSpy = jasmine.createSpy("success"),
                    failureSpy = jasmine.createSpy("failure");
                httpBackend.expectGET("/svc/components/storyteller/processes/772")
                    .respond(JSON.parse(testData));

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
                    .respond(JSON.parse(testData));

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
                    .respond(JSON.parse(testData));

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
                    .respond(JSON.parse(testData));

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
                    .respond(JSON.parse(testData));

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
                    .respond(() => [500, {}, {}, "Internal Server Error"]);

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
                    .respond(() => [404, {}, {}, "Not Found"]);

                // Act
                service.load("772").then(successSpy, failureSpy);
                httpBackend.flush();

                // Assert
                expect(successSpy).not.toHaveBeenCalled();
                expect(failureSpy).toHaveBeenCalled();
            });
        });
    });
    
    describe("getProcesses", () => {
        describe("When processes collection is returned from the server", () => {
            it("should return data successfully through a promise", () => {
                // Arrange
                var successSpy = jasmine.createSpy("success"),
                    failureSpy = jasmine.createSpy("failure");
                var projectId = 1;
                var processes = [];
                processes.push({ projectId: 1, id: 2 });

                httpBackend.when("GET", `/svc/components/storyteller/projects/${projectId}/processes`)
                    .respond(processes);

                // Act
                service.getProcesses(projectId).then(successSpy, failureSpy);
                httpBackend.flush();

                // Assert
                expect(successSpy).toHaveBeenCalled();
                expect(failureSpy).not.toHaveBeenCalled();
            });
        });
        describe("When processes collection is not returned from the server", () => {
            it("should reject data through promise if data is not found", () => {
                // Arrange
                var successSpy = jasmine.createSpy("success"),
                    failureSpy = jasmine.createSpy("failure");
                var projectId = 0;
                httpBackend.when("GET", `/svc/components/storyteller/projects/${projectId}/processes`)
                    .respond(() => [404, {}, {}, "Not Found"]);

                // Act
                service.getProcesses(projectId).then(successSpy, failureSpy);
                httpBackend.flush();

                // Assert
                expect(successSpy).not.toHaveBeenCalled();
                expect(failureSpy).toHaveBeenCalled();
            });
        });
    });
    
});

