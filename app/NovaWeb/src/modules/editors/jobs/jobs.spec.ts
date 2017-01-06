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


});