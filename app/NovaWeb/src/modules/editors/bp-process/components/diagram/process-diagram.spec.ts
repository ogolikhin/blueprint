import {ISelectionManager} from "../../../../managers/selection-manager/selection-manager";
import * as angular from "angular";
import * as TestModels from "../../models/test-model-factory";
import {ExecutionEnvironmentDetectorMock} from "../../../../core/services/execution-environment-detector.mock";
import {LoadingOverlayServiceMock} from "../../../../core/loadingOverlay/loadingOverlay.service.mock";
import {ILoadingOverlayService} from "../../../../core/loadingOverlay/loadingOverlay.service";
import {INavigationService} from "../../../../core/navigation/navigation.service";
import {NavigationServiceMock} from "../../../../core/navigation/navigation.service.mock";
import {ProcessDiagram} from "./process-diagram";
import {ICommunicationManager, CommunicationManager} from "../../../bp-process";
import {LocalizationServiceMock} from "../../../../core/localization/localization.service.mock";
import {DialogService} from "../../../../shared/widgets/bp-dialog";
import {ProcessType} from "../../models/enums";
import {ModalServiceMock} from "../../../../shell/login/mocks.spec";
import {IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ProcessEvents} from "./process-diagram-communication";
import {ShapesFactory, ShapesFactoryMock} from "./presentation/graph/shapes/shapes-factory";
import {IClipboardService, ClipboardService} from "../../services/clipboard.svc";
import {UtilityPanelService} from "../../../../shell/bp-utility-panel/utility-panel.svc";
import {FileUploadServiceMock} from "../../../../core/fileUpload/fileUpload.service.mock";
import {IMessageService} from "../../../../main/components/messages/message.svc";
import {MessageServiceMock} from "../../../../main/components/messages/message.mock";

describe("ProcessDiagram Tests", () => {
    let rootScope: ng.IRootScopeService,
        scope,
        timeout: ng.ITimeoutService,
        q: ng.IQService,
        log: ng.ILogService,
        messageService: IMessageService,
        statefulArtifactFactory: IStatefulArtifactFactory;

    let communicationManager: ICommunicationManager,
        dialogService: DialogService,
        localization: LocalizationServiceMock,
        clipboard: IClipboardService,
        navigationService: INavigationService,
        utilityPanelService: UtilityPanelService,
        shapesFactory: ShapesFactory,
        selectionManager: ISelectionManager,
        fileUploadService: FileUploadServiceMock,
        loadingOverlayService: ILoadingOverlayService;

    let container: HTMLElement,
        wrapper: HTMLElement;

    let _window: any = window;
    _window.executionEnvironmentDetector = ExecutionEnvironmentDetectorMock;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("messageService", MessageServiceMock);
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("dialogService", DialogService);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("shapesFactory", ShapesFactoryMock);
        $provide.service("clipboardService", ClipboardService);
        $provide.service("utilityPanelService", UtilityPanelService);
        $provide.service("fileUploadService", FileUploadServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
    }));

    beforeEach(inject(($rootScope: ng.IRootScopeService,
                       $timeout: ng.ITimeoutService,
                       $q: ng.IQService,
                       $log: ng.ILogService,
                       _messageService_: IMessageService,
                       _communicationManager_: ICommunicationManager,
                       _dialogService_: DialogService,
                       _localization_: LocalizationServiceMock,
                       _navigationService_: INavigationService,
                       _statefulArtifactFactory_: IStatefulArtifactFactory,
                       _shapesFactory_: ShapesFactory,
                       _utilityPanelService_: UtilityPanelService,
                       _clipboardService_: ClipboardService,
                       _fileUploadService_: FileUploadServiceMock,
                       _loadingOverlayService_: ILoadingOverlayService) => {

        $rootScope["config"] = {
            settings: {
                ProcessShapeLimit: 100,
                StorytellerIsSMB: "false"
            }
        };

        rootScope = $rootScope;
        scope = {};
        timeout = $timeout;
        q = $q;
        log = $log;
        messageService = _messageService_;
        communicationManager = _communicationManager_;
        dialogService = _dialogService_;
        localization = _localization_;
        clipboard = _clipboardService_;
        navigationService = _navigationService_;
        statefulArtifactFactory = _statefulArtifactFactory_;
        utilityPanelService = _utilityPanelService_;
        shapesFactory = _shapesFactory_;
        fileUploadService = _fileUploadService_;

        wrapper = document.createElement("DIV");
        container = document.createElement("DIV");
        wrapper.appendChild(container);

    }));

    it("Load process - Success", () => {
        // arrange
        let diagram = new ProcessDiagram(
            rootScope,
            scope,
            timeout,
            q,
            log,
            messageService,
            communicationManager,
            dialogService,
            localization,
            navigationService,
            statefulArtifactFactory,
            shapesFactory,
            utilityPanelService,
            clipboard,
            selectionManager,
            fileUploadService,
            loadingOverlayService
        );

        let model = TestModels.createDefaultProcessModel();

        // act
        diagram.createDiagram(model, container);
        rootScope.$apply();

        // assert
        expect(diagram.processViewModel).not.toBeNull(null);
        expect(diagram.processViewModel.shapes.length).toBe(5);
    });
    it("Load process - Destroy ", () => {
        // arrange
        let diagram = new ProcessDiagram(
            rootScope,
            scope,
            timeout,
            q,
            log,
            messageService,
            communicationManager,
            dialogService,
            localization,
            navigationService,
            statefulArtifactFactory,
            shapesFactory,
            utilityPanelService,
            clipboard,
            selectionManager,
            fileUploadService,
            loadingOverlayService
        );

        let model = TestModels.createDefaultProcessModel();

        // act
        diagram.createDiagram(model, container);
        rootScope.$apply();

        diagram.destroy();

        // assert
        expect(diagram.processViewModel).not.toBeDefined();
        expect(container.childElementCount).toBe(0);
    });

    it("creatediagram - Invalid Process Id", () => {
        // arrange
        let diagram = new ProcessDiagram(
            rootScope,
            scope,
            timeout,
            q,
            log,
            messageService,
            communicationManager,
            dialogService,
            localization,
            navigationService,
            statefulArtifactFactory,
            shapesFactory,
            utilityPanelService,
            clipboard,
            selectionManager,
            fileUploadService,
            loadingOverlayService
        );

        let model = TestModels.createDefaultProcessModel();
        model.id = -1;
        let error: Error;
        // act
        try {
            diagram.createDiagram(model, container);
        } catch (err) {
            error = err;
        }

        // assert
        expect(error.message).toBe("Process id '-1' is invalid.");
    });
    it("creatediagram - Null element", () => {
        // arrange
        let diagram = new ProcessDiagram(
            rootScope,
            scope,
            timeout,
            q,
            log,
            messageService,
            communicationManager,
            dialogService,
            localization,
            navigationService,
            statefulArtifactFactory,
            shapesFactory,
            utilityPanelService,
            clipboard,
            selectionManager,
            fileUploadService,
            loadingOverlayService
        );

        let model = TestModels.createDefaultProcessModel();
        let error: Error;

        // act
        try {
            diagram.createDiagram(model, null);
        } catch (err) {
            error = err;
        }

        // assert
        expect(error.message).toBe("There is no html element for the diagram");
    });

    it("responds to change of process type to Business", () => {
        // arrange
        let diagram = new ProcessDiagram(
            rootScope,
            scope,
            timeout,
            q,
            log,
            messageService,
            communicationManager,
            dialogService,
            localization,
            navigationService,
            statefulArtifactFactory,
            shapesFactory,
            utilityPanelService,
            clipboard,
            selectionManager,
            fileUploadService,
            loadingOverlayService
        );

        let model = TestModels.createDefaultProcessModel();
        model.propertyValues["clientType"].value = ProcessType.UserToSystemProcess;

        diagram.createDiagram(model, container);
        rootScope.$apply();

        // act
        communicationManager.toolbarCommunicationManager.toggleProcessType(ProcessType.BusinessProcess);

        // assert
        expect(diagram.processViewModel.processType).toBe(ProcessType.BusinessProcess);
    });

    it("responds to change of process type to User-System", () => {
        // arrange
        let diagram = new ProcessDiagram(
            rootScope,
            scope,
            timeout,
            q,
            log,
            messageService,
            communicationManager,
            dialogService,
            localization,
            navigationService,
            statefulArtifactFactory,
            shapesFactory,
            utilityPanelService,
            clipboard,
            selectionManager,
            fileUploadService,
            loadingOverlayService
        );

        let model = TestModels.createDefaultProcessModel();
        model.propertyValues["clientType"].value = ProcessType.BusinessProcess;

        diagram.createDiagram(model, container);
        rootScope.$apply();

        // act
        communicationManager.toolbarCommunicationManager.toggleProcessType(ProcessType.UserToSystemProcess);

        // assert
        expect(diagram.processViewModel.processType).toBe(ProcessType.UserToSystemProcess);
    });

    it("calls navigationService to navigation to associated artifact", () => {
        // arrange
        let artifactId = 14;
        let diagram = new ProcessDiagram(
            rootScope,
            scope,
            timeout,
            q,
            log,
            messageService,
            communicationManager,
            dialogService,
            localization,
            navigationService,
            statefulArtifactFactory,
            shapesFactory,
            utilityPanelService,
            clipboard,
            selectionManager,
            fileUploadService,
            loadingOverlayService
        );
        let navigateToArtifactSpy = spyOn(navigationService, "navigateTo");

        let model = TestModels.createDefaultProcessModel();
        model.propertyValues["clientType"].value = ProcessType.BusinessProcess;

        diagram.createDiagram(model, container);
        rootScope.$apply();

        // act
        communicationManager.processDiagramCommunication.action(ProcessEvents.NavigateToAssociatedArtifact,
        {id: artifactId, isAccessible: true});

        // assert
        expect(navigateToArtifactSpy).toHaveBeenCalledWith({id: artifactId, version: undefined, enableTracking: undefined});
    });
});
