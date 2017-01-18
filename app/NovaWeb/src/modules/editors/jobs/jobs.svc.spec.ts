import * as angular from "angular";
import "angular-mocks";
import "rx";
import {JobsService} from "./jobs.svc";
import {IJobInfo, JobStatus, JobType} from "./model/models";
import {HttpStatusCode} from "../../core/httpInterceptor/http-status-code";

describe("Jobs Service", () => {
    let service: JobsService, httpBackend;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("jobsService", JobsService);
    }));

    beforeEach(inject((jobsService,
                       $httpBackend: ng.IHttpBackendService) => {
        service = jobsService;
        httpBackend = $httpBackend;
    }));


    describe("Get Jobs", () => {
        it("successfully", inject(($httpBackend: ng.IHttpBackendService) => {
            // Arrange
            const testData: IJobInfo[] = [{
                jobId: 0,
                status: JobStatus.Completed,
                jobType: JobType.DocGen,
                project: "test project",
                submittedDateTime: null,
                jobStartDateTime: null,
                jobEndDateTime: null,
                userId: 1,
                userDisplayName: "user A",
                server: null,
                progress: 9,
                output: "successful",
                statusChanged: true,
                hasCancelJob: false,
                projectId: 1
            }];

            const successSpy = jasmine.createSpy("success"),
                failureSpy = jasmine.createSpy("failure");
            httpBackend.expectGET("/svc/adminstore/jobs/")
                    .respond(testData);
            // Act
            service.getJobs().then(successSpy, failureSpy);
            $httpBackend.flush();

            // Assert
            expect(successSpy).toHaveBeenCalled();
            expect(failureSpy).not.toHaveBeenCalled();
        }));

         it("failed", inject(($httpBackend: ng.IHttpBackendService) => {
            // Arrange
            const successSpy = jasmine.createSpy("success"),
                failureSpy = jasmine.createSpy("failure");
            httpBackend.expectGET("/svc/adminstore/jobs/")
                    .respond(() => [HttpStatusCode.ServerError, {}, {}, "Internal Server Error"]);
            // Act
            service.getJobs().then(successSpy, failureSpy);
            $httpBackend.flush();

            // Assert
            expect(failureSpy).toHaveBeenCalled();
            expect(successSpy).not.toHaveBeenCalled();
        }));
    });

        describe("Add Process test case generation jobs", () => {
        it("successfully", inject(($httpBackend: ng.IHttpBackendService) => {
            // Arrange
            const projectId = 1;
            const projectName = "test project";
            const processIds = [{processId: 1}];

            const successSpy = jasmine.createSpy("success"),
                failureSpy = jasmine.createSpy("failure");
            httpBackend.expectPOST("/svc/adminstore/jobs/process/testgen")
                    .respond(() => [HttpStatusCode.ServerError, {}, {}, "Internal Server Error"]);
            // Act
            service.addProcessTestsGenerationJobs(projectId, projectName, processIds).then(successSpy, failureSpy);
            $httpBackend.flush();

            // Assert
            expect(successSpy).not.toHaveBeenCalled();
            expect(failureSpy).toHaveBeenCalled();
        }));

         it("failed", inject(($httpBackend: ng.IHttpBackendService) => {
            // Arrange
            const response = {jobId: 1};
            const projectId = 1;
            const projectName = "test project";
            const processIds = [{processId: 1}];

            const successSpy = jasmine.createSpy("success"),
                failureSpy = jasmine.createSpy("failure");
            httpBackend.expectPOST("/svc/adminstore/jobs/process/testgen")
                    .respond(response);
            // Act
            service.addProcessTestsGenerationJobs(projectId, projectName, processIds).then(successSpy, failureSpy);
            $httpBackend.flush();

            // Assert
            expect(successSpy).toHaveBeenCalled();
            expect(failureSpy).not.toHaveBeenCalled();
        }));
    });
});
