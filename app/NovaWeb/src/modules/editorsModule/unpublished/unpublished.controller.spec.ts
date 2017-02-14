import "angular-mocks";
import "rx";
import {LoadingOverlayServiceMock} from "../../commonModule/loadingOverlay/loadingOverlay.service.mock";
import {LocalizationServiceMock} from "../../commonModule/localization/localization.service.mock";
import {NavigationServiceMock} from "../../commonModule/navigation/navigation.service.mock";
import {MessageServiceMock} from "../../main/components/messages/message.mock";
import {ItemTypePredefined} from "../../main/models/itemTypePredefined.enum";
import {ProjectManagerMock} from "../../managers/project-manager/project-manager.mock";
import {DialogServiceMock} from "../../shared/widgets/bp-dialog/bp-dialog.mock";
import {BPButtonGroupAction} from "../../shared/widgets/bp-toolbar/actions/bp-button-group-action";
import {UnpublishedController} from "./unpublished.controller";
import {IUnpublishedArtifactsService} from "./unpublished.service";
import {UnpublishedArtifactsServiceMock} from "./unpublished.service.mock";
import * as angular from "angular";
import createSpy = jasmine.createSpy;


describe("Controller: Unpublished", () => {
    let controller: UnpublishedController;
    let publishService: IUnpublishedArtifactsService;
    let $q: ng.IQService;
    let $rootScope: ng.IRootScopeService;
    let scheduler: Rx.TestScheduler;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("publishService", UnpublishedArtifactsServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("projectManager", ProjectManagerMock);
        $provide.service("dialogService", DialogServiceMock);
    }));

    beforeEach(inject(($controller: ng.IControllerService,
                       _publishService_: IUnpublishedArtifactsService,
                       _$q_: ng.IQService,
                       _$rootScope_: ng.IRootScopeService) => {

        controller = $controller(UnpublishedController);
        publishService = _publishService_;
        $q = _$q_;
        $rootScope = _$rootScope_;
        scheduler = new Rx.TestScheduler();
    }));

    it("should exist", () => {
        expect(controller).toBeDefined();
        expect(controller.selectedArtifacts.length).toBe(0);
        expect(controller.unpublishedArtifacts.length).toBe(0);
        expect(controller.toolbarActions.length).toBe(0);
    });

    it("should initialize loading of unpublished artifacts $onInit", () => {
        // Arrange
        const unpublishedSpy = spyOn(publishService, "getUnpublishedArtifacts")
            .and.returnValue($q.resolve({
                artifacts: [],
                projects: []
            }));

        // Act
        controller.$onInit();
        $rootScope.$digest();

        // Assert
        expect(unpublishedSpy).toHaveBeenCalled();
        expect(controller.toolbarActions.length).toBe(1);
        expect((<BPButtonGroupAction>controller.toolbarActions[0]).actions.length).toBe(2);
        expect(controller.isLoading).toBe(false);
    });

    describe("toggleAll", () => {
        beforeEach(() => {
            controller.$onInit();
            $rootScope.$digest();

            controller.unpublishedArtifacts = <any>[
                {id: 1}, {id: 2}, {id: 3}, {id: 4}, {id: 5}
            ];
        });

        it("should toggle all artifacts from unselected to selected", () => {
            // Arrange
            controller.selectedArtifacts = [];

            // Act
            controller.toggleAll();

            // Assert
            expect(controller.selectedArtifacts.length).toBe(5);
            expect(controller.isGroupToggleChecked()).toBe(true);
        });

        it("should toggle all artifacts from unselected to selected if only some are selected", () => {
            // Arrange
            controller.selectedArtifacts = [controller.unpublishedArtifacts[1], controller.unpublishedArtifacts[3]];

            // Act
            controller.toggleAll();

            // Assert
            expect(controller.selectedArtifacts.length).toBe(5);
            expect(controller.isGroupToggleChecked()).toBe(true);
        });

        it("should toggle all artifacts from selected to unselected", () => {
            // Arrange
            controller.selectedArtifacts = controller.unpublishedArtifacts.slice(0);

            // Act
            controller.toggleAll();

            // Assert
            expect(controller.selectedArtifacts.length).toBe(0);
            expect(controller.isGroupToggleChecked()).toBe(false);
        });
    });

    describe("toggleSelection", () => {
        beforeEach(() => {
            controller.$onInit();
            $rootScope.$digest();
        });

        it("should select artifact if it's not selected", () => {
            // Arrange
            const artifact = <any>{id: 1};
            controller.selectedArtifacts = [];

            // Act
            controller.toggleSelection(artifact);

            // Assert
            expect(controller.selectedArtifacts.length).toBe(1);
        });

        it("should de-select artifact if it's not selected", () => {
            // Arrange
            const artifact = <any>{id: 1};
            controller.selectedArtifacts = [artifact];

            // Act
            controller.toggleSelection(artifact);

            // Assert
            expect(controller.selectedArtifacts.length).toBe(0);
        });
    });

    it("isSelected", () => {
        // Arrange
        const artifact1 = <any>{id: 1};
        const artifact2 = <any>{id: 2};
        const artifact3 = <any>{id: 3};
        controller.selectedArtifacts = [artifact1, artifact2];

        // Assert
        expect(controller.isSelected(artifact1)).toBe(true);
        expect(controller.isSelected(artifact2)).toBe(true);
        expect(controller.isSelected(artifact3)).toBe(false);
    });

    it("isNavigatable", () => {
        // Act
        const artifact1 = <any>{id: 1, predefinedType: ItemTypePredefined.ArtifactBaseline};
        const artifact2 = <any>{id: 1, predefinedType: ItemTypePredefined.Baseline};
        const artifact3 = <any>{id: 1, predefinedType: ItemTypePredefined.BaselineFolder};
        const artifact4 = <any>{id: 1, predefinedType: ItemTypePredefined.ArtifactReviewPackage};

        const navigatable1 = controller.isNavigatable(artifact1);
        const navigatable2 = controller.isNavigatable(artifact2);
        const navigatable3 = controller.isNavigatable(artifact3);
        const navigatable4 = controller.isNavigatable(artifact4);

        // Assert
        expect(navigatable1).toBe(false);
        expect(navigatable2).toBe(false);
        expect(navigatable3).toBe(false);
        expect(navigatable4).toBe(false);
    });

    it("unpublishedArtifactObservable", () => {
        // Arrange
        const unpublishedSubject = new Rx.Subject();
        publishService.unpublishedArtifactsObservable = <any>unpublishedSubject.asObservable();

        spyOn(publishService, "getUnpublishedArtifacts").and.returnValue($q.resolve());
        controller.$onInit();
        $rootScope.$digest();

        // Act
        const result = {
            artifacts: [{id: 1, projectId: 1}, {id: 2, projectId: 1}],
            projects: [{id: 1}]
        };
        unpublishedSubject.onNext(result);
        scheduler.start();

        // Assert
        expect(controller.unpublishedArtifacts.length).toBe(2);
        expect(controller.selectedArtifacts.length).toBe(0);
    });

    it("processedArtifactObservable", () => {
        // Arrange
        const processedSubject = new Rx.Subject();
        publishService.processedArtifactsObservable = <any>processedSubject.asObservable();

        spyOn(publishService, "getUnpublishedArtifacts").and.returnValue($q.resolve());
        controller.$onInit();
        $rootScope.$digest();
        const artifact1 = {id: 1, projectId: 1};
        const artifact2 = {id: 2, projectId: 1};
        const artifact3 = {id: 3, projectId: 1};
        controller.unpublishedArtifacts = <any>[artifact1, artifact2, artifact3];
        controller.selectedArtifacts = <any>[artifact1, artifact2];

        // Act
        const result = {
            artifacts: [artifact1, artifact2],
            projects: [{id: 1}]
        };
        processedSubject.onNext(result);
        scheduler.start();

        // Assert
        expect(controller.unpublishedArtifacts.length).toBe(1);
        expect(controller.selectedArtifacts.length).toBe(0);
    });

    it("$onDestroy", () => {
        // Arrange
        const processedSubject = new Rx.Subject();
        const unpublishedSubject = new Rx.Subject();
        publishService.unpublishedArtifactsObservable = <any>unpublishedSubject.asObservable();
        publishService.processedArtifactsObservable = <any>processedSubject.asObservable();

        spyOn(publishService, "getUnpublishedArtifacts").and.returnValue($q.resolve());
        controller.$onInit();
        $rootScope.$digest();
        scheduler.start();

        // Act
        controller.$onDestroy();
        scheduler.start();

        // Assert
        expect(processedSubject.hasObservers()).toBe(false);
        expect(unpublishedSubject.hasObservers()).toBe(false);
    });
});
