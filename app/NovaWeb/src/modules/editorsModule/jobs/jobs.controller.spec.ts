import * as angular from "angular";
import "angular-mocks";
import "rx";
import "lodash";
import {NavigationServiceMock} from "../../commonModule/navigation/navigation.service.mock";
import {LoadingOverlayServiceMock} from "../../commonModule/loadingOverlay/loadingOverlay.service.mock";
import {LocalizationServiceMock} from "../../commonModule/localization/localization.service.mock";
import {IJobsService} from "./jobs.service";
import {IJobInfo, IJobResult, JobStatus, JobType} from "./model/models";
import {JobsController} from "./jobs.controller";
import {JobsServiceMock} from "./jobs.service.mock";
import {MessageServiceMock} from "../../main/components/messages/message.mock";
import {DownloadServiceMock} from "../../commonModule/download/download.service.mock";
import {ProjectExplorerServiceMock} from "../../main/components/bp-explorer/project-explorer.service.mock";

xdescribe("Controller: Jobs", () => {
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
        $provide.service("projectExplorerService", ProjectExplorerServiceMock);
        $provide.service("downloadService", DownloadServiceMock);
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
        expect(controller.paginationData.page).toBe(1);
        expect(controller.paginationData.pageSize).toBe(10);
    });

    it("should initialize loading of jobs $onInit", () => {
        // arrange
        const testData: IJobResult = {jobInfos: [createJob(0, JobType.DocGen, JobStatus.Completed)], totalJobCount: 1};
        const jobsSpy = spyOn(jobsService, "getJobs")
            .and.returnValue($q.resolve(testData));

        // act
        controller.$onInit();
        $rootScope.$digest();

        // assert
        expect(jobsSpy).toHaveBeenCalled();
        expect(controller.isLoading).toBe(false);
        expect(controller.paginationData.page).toBe(1);
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
            const oldPage = controller.paginationData.page;
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
            const oldPage = controller.paginationData.page;
            spyOn(jobsService, "getJobs").and.returnValue($q.when());

            // act
            controller.loadNextPage();

            // assert
            expect(controller.paginationData.page).toBe(oldPage + 1);
        });
    });

    describe("loadPage", () => {
        it("updates total items retrieved from server call", () => {
            // arrange
            controller.paginationData.total = 0;

            spyOn(jobsService, "getJobs").and.returnValue($q.when({jobInfos: [], totalJobCount: 100}));

            // act
            controller.loadPage(1);
            $rootScope.$digest();

            // assert
            expect(controller.paginationData.total).toBe(100);
        });
        it("updates total items when returned value is null and on first page", () => {
            // arrange
            controller.paginationData.total = 100;

            spyOn(jobsService, "getJobs").and.returnValue($q.when({jobInfos: [], totalJobCount: 0}));

            // act
            controller.loadPage(1);
            $rootScope.$digest();

            // assert
            expect(controller.paginationData.total).toBe(0);
        });
        it("does not update total items when returned value is null and not on first page", () => {
            // arrange
            controller.paginationData.total = 100;

            spyOn(jobsService, "getJobs").and.returnValue($q.when({jobInfos: [], totalJobCount: 0}));

            // act
            controller.loadPage(2);
            $rootScope.$digest();

            // assert
            expect(controller.paginationData.total).toBe(100);
        });
    });

    describe("showPagination", () => {
        it("returns false when there are jobs but does not exceed page size and is not loading", () => {
            // arrange
            controller.jobs = [createJob(1, JobType.DocGen, JobStatus.Completed)];
            controller.isLoading = false;
            controller.paginationData.total = 1;
            controller.paginationData.pageSize = 10;
            // act
            const result = controller.canShowPagination();

            // assert
            expect(result).toBe(false);
        });
        it("returns true when there are no jobs but is not on first page", () => {
            // arrange
            controller.jobs = [];
            controller.isLoading = false;
            controller.paginationData.page = 2;

            // act
            const result = controller.canShowPagination();

            // assert
            expect(result).toBe(true);
        });
        it("returns false when there are jobs and is loading", () => {
            // arrange
            controller.jobs = [createJob(1, JobType.DocGen, JobStatus.Completed)];
            controller.isLoading = true;

            // act
            const result = controller.canShowPagination();

            // assert
            expect(result).toBe(false);
        });
        it("returns false when there are no jobs and on first page", () => {
            // arrange
            controller.jobs = [];
            controller.paginationData.page = 1;
            controller.isLoading = false;

            // act
            const result = controller.canShowPagination();

            // assert
            expect(result).toBe(false);
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
