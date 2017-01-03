import * as angular from "angular";
import "angular-mocks";
import "rx";
import {IJobInfo, JobsService, JobStatus, JobType} from "./jobs.svc";
import {HttpStatusCode} from "../../core/http/http-status-code";

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
});