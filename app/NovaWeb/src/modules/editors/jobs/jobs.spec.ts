import * as angular from "angular";
import "angular-mocks";
import "rx";
import "lodash";
import {NavigationServiceMock} from "../../core/navigation/navigation.svc.mock";
import {ProjectManagerMock} from "../../managers/project-manager/project-manager.mock";
import {LoadingOverlayServiceMock} from "../../core/loading-overlay/loading-overlay.svc.mock";
import {MessageServiceMock} from "../../core/messages/message.mock";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {IJobsService} from "./jobs.svc";
import {IJobInfo, JobStatus, JobType} from "./model/models";
import {JobsController} from "./jobs";
import {JobsServiceMock} from "./jobs.svc.mock";

describe("Controller: Jobs", () => {
    let controller: JobsController;
    let $q: ng.IQService;
    let $rootScope: ng.IRootScopeService;
    let $window: ng.IWindowService;
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
                       _$rootScope_: ng.IRootScopeService,
                       _$window_: ng.IWindowService) => {

        controller = $controller(JobsController);
        jobsService = _jobsService_;
        $q = _$q_;
        $rootScope = _$rootScope_;
        $window = _$window_;
        scheduler = new Rx.TestScheduler();
    }));

    it("should exist", () => {
        expect(controller).toBeDefined();
        expect(controller.toolbarActions.length).toBe(0);
        expect(controller.page).toBe(1);
        expect(controller.pageSize).toBe(10);
    });

    it("should initialize loading of jobs $onInit", () => {
        // arrange
        const testData: IJobInfo[] = [createJob(0, JobType.DocGen, JobStatus.Completed)];
        const jobsSpy = spyOn(jobsService, "getJobs")
            .and.returnValue($q.resolve(testData));

        // act
        controller.$onInit();
        $rootScope.$digest();

        // assert
        expect(jobsSpy).toHaveBeenCalled();
        expect(controller.isLoading).toBe(false);
        expect(controller.page).toBe(1);
        expect(controller.jobs.length).toBe(1);
    });

    describe("refreshJob", () => {
        it("doesn't refresh job if cannot refresh", () => {
            const job = createJob(1, JobType.DocGen, JobStatus.Completed);
            const getJobSpy = spyOn(jobsService, "getJob");

            controller.refreshJob(job);

            expect(getJobSpy).not.toHaveBeenCalled();
        });

        it("overwrites previous job status when status on server has changed, and leaves other jobs the same", () => {
            // arrange
            const refreshJobId = 1;
            const jobs = [
                createJob(refreshJobId, JobType.DocGen, JobStatus.Running),
                createJob(2, JobType.DocGen, JobStatus.Running)
            ];
            const testData: IJobInfo = createJob(refreshJobId, JobType.DocGen, JobStatus.Completed);

            spyOn(jobsService, "getJob").and.returnValue($q.resolve(testData));

            // act
            controller.jobs = jobs;
            controller.refreshJob(jobs[0]);
            $rootScope.$digest();

            // assert
            expect(controller.jobs[0].status).toBe(JobStatus.Completed);
            expect(controller.jobs[1].status).toBe(JobStatus.Running);
        });
    });

    describe("canRefresh", () => {
        it("true when JobStatus is Running", () => {
            const job = createJob(1, JobType.DocGen, JobStatus.Running);
            const result = controller.canRefresh(job);
            expect(result).toBe(true);
        });

        it("true when JobStatus is Scheduled", () => {
            const job = createJob(1, JobType.DocGen, JobStatus.Scheduled);
            const result = controller.canRefresh(job);
            expect(result).toBe(true);
        });

        it("true when JobStatus is Cancelling", () => {
            const job = createJob(1, JobType.DocGen, JobStatus.Cancelling);
            const result = controller.canRefresh(job);
            expect(result).toBe(true);
        });

        it("true when JobStatus is Suspending", () => {
            const job = createJob(1, JobType.DocGen, JobStatus.Suspending);
            const result = controller.canRefresh(job);
            expect(result).toBe(true);
        });

        it("false when JobStatus is Failed", () => {
            const job = createJob(1, JobType.DocGen, JobStatus.Failed);
            const result = controller.canRefresh(job);
            expect(result).toBe(false);
        });

        it("false when JobStatus is Completed", () => {
            const job = createJob(1, JobType.DocGen, JobStatus.Completed);
            const result = controller.canRefresh(job);
            expect(result).toBe(false);
        });
    });

    describe("canDownload", () => {
        it("returns true for Completed ProjectExport job", () => {
            const job = createJob(1, JobType.ProjectExport, JobStatus.Completed);
            const result = controller.canDownload(job);
            expect(result).toBe(true);
        });

        it("returns false for Scheduled ProjectExport job", () => {
            const job = createJob(1, JobType.ProjectExport, JobStatus.Scheduled);
            const result = controller.canDownload(job);
            expect(result).toBe(false);
        });

        it("returns false for Running ProjectExport job", () => {
            const job = createJob(1, JobType.ProjectExport, JobStatus.Running);
            const result = controller.canDownload(job);
            expect(result).toBe(false);
        });

        it("returns false for Failed ProjectExport job", () => {
            const job = createJob(1, JobType.ProjectExport, JobStatus.Failed);
            const result = controller.canDownload(job);
            expect(result).toBe(false);
        });

        it("returns false for Suspending ProjectExport job", () => {
            const job = createJob(1, JobType.ProjectExport, JobStatus.Suspending);
            const result = controller.canDownload(job);
            expect(result).toBe(false);
        });

        it("returns false for Suspended ProjectExport job", () => {
            const job = createJob(1, JobType.ProjectExport, JobStatus.Suspended);
            const result = controller.canDownload(job);
            expect(result).toBe(false);
        });

        it("returns false for Cancelling ProjectExport job", () => {
            const job = createJob(1, JobType.ProjectExport, JobStatus.Cancelling);
            const result = controller.canDownload(job);
            expect(result).toBe(false);
        });

        it("returns false for Terminated ProjectExport job", () => {
            const job = createJob(1, JobType.ProjectExport, JobStatus.Terminated);
            const result = controller.canDownload(job);
            expect(result).toBe(false);
        });

        it("returns false for Completed ProjectImport job", () => {
            const job = createJob(1, JobType.ProjectImport, JobStatus.Completed);
            const result = controller.canDownload(job);
            expect(result).toBe(false);
        });

        it("returns false for Completed DocGen job", () => {
            const job = createJob(1, JobType.DocGen, JobStatus.Completed);
            const result = controller.canDownload(job);
            expect(result).toBe(false);
        });
    });

    describe("downloadItem", () => {
        it("doesn't open new window if cannot download", () => {
            const job = createJob(1, JobType.ProjectExport, JobStatus.Running);
            const openSpy = spyOn($window, "open");

            controller.downloadItem(job);

            expect(openSpy).not.toHaveBeenCalled();
        });

        it("opens new window with correct url", () => {
            const job = createJob(1, JobType.ProjectExport, JobStatus.Completed);
            const openSpy = spyOn($window, "open");

            controller.downloadItem(job);

            expect(openSpy).toHaveBeenCalledWith(`/svc/adminstore/jobs/${job.jobId}/result/file`, "_blank");
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

            spyOn(jobsService, "getJobs").and.callFake((page: number = null, pageSize: number = null, timeout?: ng.IPromise<void>) => {
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

    function createJob(id: number, type: JobType, status: JobStatus): IJobInfo {
        return {
            jobId: 1,
            status: status,
            jobType: type,
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
    }
});
