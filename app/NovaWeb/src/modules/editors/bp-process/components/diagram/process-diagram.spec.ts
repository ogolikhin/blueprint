import * as angular from "angular";
import {MessageServiceMock} from "../../../../core/messages/message.mock";
import {IMessageService} from "../../../../core/messages/message.svc";
import {INavigationService} from "../../../../core/navigation/navigation.svc";
import {NavigationServiceMock} from "../../../../core/navigation/navigation.svc.mock";
import {ProcessDiagram} from "./process-diagram";
import {ICommunicationManager, CommunicationManager} from "../../../bp-process"; 
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {DialogService} from "../../../../shared/widgets/bp-dialog";
import {ProcessType} from "../../models/enums";
import * as TestModels from "../../models/test-model-factory";
import { ModalServiceMock } from "../../../../shell/login/mocks.spec";

describe("ProcessDiagram Tests", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("messageService", MessageServiceMock);
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("dialogService", DialogService);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("navigationService", NavigationServiceMock);
    }));
    let rootScope: ng.IRootScopeService,
        scope,
        timeout: ng.ITimeoutService,
        q: ng.IQService,
        log: ng.ILogService,
        messageService: IMessageService;
    let communicationManager: ICommunicationManager,
        dialogService: DialogService,
        localization: LocalizationServiceMock,
        navigationService: INavigationService;

    let container: HTMLElement,
        wrapper: HTMLElement;

    beforeEach(inject((
        $rootScope: ng.IRootScopeService,
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        $log: ng.ILogService,
        _messageService_: IMessageService, 
        _communicationManager_: ICommunicationManager,
        _dialogService_: DialogService,
        _localization_: LocalizationServiceMock,
        _navigationService_: INavigationService) => {

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
        navigationService = _navigationService_;

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
            navigationService
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
            navigationService
        );

        let model = TestModels.createDefaultProcessModel();

        // act
        diagram.createDiagram(model, container);
        rootScope.$apply();

        diagram.destroy();

        // assert
        expect(diagram.processViewModel).toBeNull(null);
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
            navigationService
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
            navigationService
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
            navigationService
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
            navigationService
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
            navigationService
        );
        let navigateToArtifactSpy = spyOn(navigationService, "navigateToArtifact");

        let model = TestModels.createDefaultProcessModel();
        model.propertyValues["clientType"].value = ProcessType.BusinessProcess;

        diagram.createDiagram(model, container);
        rootScope.$apply();

        // act
        communicationManager.processDiagramCommunication.navigateToAssociatedArtifact(artifactId);

        // assert
        expect(navigateToArtifactSpy).toHaveBeenCalledWith(artifactId, undefined);
    });
});