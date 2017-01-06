import {NavigationServiceMock} from "../../core/navigation/navigation.svc.mock";
import {ProjectManagerMock} from "../../managers/project-manager/project-manager.mock";
import {LoadingOverlayServiceMock} from "../../core/loading-overlay/loading-overlay.svc.mock";
import {MessageServiceMock} from "../../core/messages/message.mock";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {IJobInfo, IJobsService, JobStatus, JobType} from "./jobs.svc";
import {JobsController} from "./jobs";
import * as angular from "angular";
import "angular-mocks";
import "rx";
import "lodash";
import {JobsServiceMock} from "./jobs.svc.mock";


describe("Controller: Jobs", () => {
    let controller: JobsController;
    let $q: ng.IQService;
    let $rootScope: ng.IRootScopeService;
    let scheduler: Rx.TestScheduler;
    let jobsService: IJobsService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("jobsService", JobsServiceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("projectManager", ProjectManagerMock);
    }));

    beforeEach(inject(($controller: ng.IControllerService,
                        _jobsService_: IJobsService,
                       _$q_: ng.IQService,
                       _$rootScope_: ng.IRootScopeService) => {

        controller = $controller(JobsController);
        jobsService = _jobsService_;
        $q = _$q_;
        $rootScope = _$rootScope_;
        scheduler = new Rx.TestScheduler();
    }));

    

    it("should exist", () => {
        expect(controller).toBeDefined();
        expect(controller.toolbarActions.length).toBe(0);
        expect(controller.page).toBe(1);
        expect(controller.pageLength).toBe(10);
    });

    
    it("should initialize loading of jobs $onInit", () => {
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
        const jobsSpy = spyOn(jobsService, "getJobs")
            .and.returnValue($q.resolve(testData));

        // Act
        controller.$onInit();
        $rootScope.$digest();

        // Assert
        expect(jobsSpy).toHaveBeenCalled();
        expect(controller.isLoading).toBe(false);
        expect(controller.page).toBe(1);
        expect(controller.jobs.length).toBe(1);
    });

    describe("refreshJob", () => {
        it("overwrites previous job status when status on server has changed, and leaves other jobs the same", () => {
            const refreshJobId = 1;
            const jobs = [];
            const job1 = {jobId: refreshJobId, status: JobStatus.Running};
            const job2 = {jobId: 2, status: JobStatus.Running};
            jobs.push(job1);
            jobs.push(job2);
            const testData: IJobInfo = {
                jobId: refreshJobId, 
                status: JobStatus.Completed, 
                jobType: JobType.DocGen, 
                project: "test project", 
                submittedDateTime: null,  
                jobStartDateTime: null, 
                jobEndDateTime: null,  
                userId: 1,  
                userDisplayName: "user A", 
                server: null,  
                progress: 100,  
                output: "successful",  
                statusChanged: true,  
                hasCancelJob: false, 
                projectId: 1
            };

            spyOn(jobsService, "getJob").and.returnValue($q.resolve(testData));

            // Act
            controller.jobs = jobs;
            controller.refreshJob(refreshJobId);
            $rootScope.$digest();

            expect(controller.jobs[0].status).toBe(JobStatus.Completed);
            expect(controller.jobs[1].status).toBe(JobStatus.Running);
        });
    });

    describe("canRefresh", () => {
        it("true when JobStatus is Running", () => {
            const result = controller.canRefresh(JobStatus.Running);
            expect(result).toBe(true);
        });
        it("true when JobStatus is Scheduled", () => {
            const result = controller.canRefresh(JobStatus.Scheduled);
            expect(result).toBe(true);
        });
        it("true when JobStatus is Cancelling", () => {
            const result = controller.canRefresh(JobStatus.Cancelling);
            expect(result).toBe(true);
        });
        it("true when JobStatus is Suspending", () => {
            const result = controller.canRefresh(JobStatus.Suspending);
            expect(result).toBe(true);
        });
        it("false when JobStatus is Completed", () => {
            const result = controller.canRefresh(JobStatus.Completed);
            expect(result).toBe(false);
        });
    });

    describe("isJobRunning", () => {
        it("true when JobStatus is Running", () => {
            const result = controller.isJobRunning(JobStatus.Running);
            expect(result).toBe(true);
        });
        it("false when JobStatus is Scheduled", () => {
            const result = controller.isJobRunning(JobStatus.Scheduled);
            expect(result).toBe(false);
        });
        it("false when JobStatus is Completed", () => {
            const result = controller.isJobRunning(JobStatus.Completed);
            expect(result).toBe(false);
        });
        it("false when JobStatus is Terminated", () => {
            const result = controller.isJobRunning(JobStatus.Terminated);
            expect(result).toBe(false);
        });
    });

    describe("loadNextPage", () => {
        
        it("calls service with incremented page", () => {
            // arrange
            const oldPage = controller.page;
            let requestedPage: number;

            spyOn(jobsService, "getJobs").and.callFake( (page: number = null, pageSize: number = null, timeout?: ng.IPromise<void>) => {
                requestedPage = page;
                return $q.when();
            });

            // act
            controller.loadNextPage();

            // assert
            expect(requestedPage).toBe(oldPage + 1);
        });

        it("increments page size by 1", () => {
            // arrange
            const oldPage = controller.page;
            spyOn(jobsService, "getJobs").and.returnValue($q.when());

            // act
            controller.loadNextPage();

            // assert
            expect(controller.page).toBe(oldPage + 1);
        });
    });
});