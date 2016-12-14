import * as angular from "angular";
import "angular-mocks";
import "script!mxClient";
import {LoadingOverlayServiceMock} from "./../../../../../core/loading-overlay/loading-overlay.svc.mock";
import {ProjectManagerMock} from "./../../../../../managers/project-manager/project-manager.mock";
import {ArtifactManagerMock} from "./../../../../../managers/artifact-manager/artifact-manager.mock";
import {ProcessDeleteAction} from "./process-delete-action";
import {NavigationServiceMock} from "./../../../../../core/navigation/navigation.svc.mock";
import {MessageServiceMock} from "./../../../../../core/messages/message.mock";
import {DialogServiceMock} from "./../../../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {StatefulArtifactFactoryMock} from "./../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {IProcessDiagramCommunication} from "./../../diagram/process-diagram-communication";
import {CommunicationManager} from "./../../../services/communication-manager";
import {LocalizationServiceMock} from "./../../../../../core/localization/localization.mock";
import {StatefulProcessArtifact, IStatefulProcessArtifact} from "./../../../process-artifact";
import {ProcessEvents} from "./../../diagram/process-diagram-communication";
import {ItemTypePredefined, RolePermissions} from "../../../../../main/models/enums";
import {IToolbarCommunication} from "./../toolbar-communication";
import * as TestShapes from "../../../models/test-shape-factory";
import * as TestModels from "../../../models/test-model-factory";

describe("ProcessDeleteAction", () => {
    let $rootScope: ng.IRootScopeService;
    let statefulProcess: IStatefulProcessArtifact;
    let localization: LocalizationServiceMock;
    let messageService: MessageServiceMock;
    let dialogService: DialogServiceMock;
    let artifactManager: ArtifactManagerMock;
    let projectManager: ProjectManagerMock;
    let loadingOverlayService: LoadingOverlayServiceMock;
    let navigationService: NavigationServiceMock;
    let toolbarCommunication: IToolbarCommunication;
    let processDiagramCommunication: IProcessDiagramCommunication;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("artifactManager", ArtifactManagerMock);
        $provide.service("projectManager", ProjectManagerMock);
        $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("communicationManager", CommunicationManager);
    }));

    beforeEach(inject((
        _$rootScope_: ng.IRootScopeService,
        _localization_: LocalizationServiceMock,
        _messageService_: MessageServiceMock,
        _dialogService_: DialogServiceMock,
        _artifactManager_: ArtifactManagerMock,
        _projectManager_: ProjectManagerMock,
        _loadingOverlayService_: LoadingOverlayServiceMock,
        _navigationService_: NavigationServiceMock,
        _communicationManager_: CommunicationManager
    ) => {
        $rootScope = _$rootScope_;
        $rootScope["config"] = {labels: []};

        const processModel = TestModels.createProcessModel();
        processModel["predefinedType"] = ItemTypePredefined.Process;
        processModel["permissions"] = RolePermissions.Edit | RolePermissions.Delete;
        statefulProcess = new StatefulProcessArtifact(processModel, null);

        localization = _localization_;
        messageService = _messageService_;
        dialogService = _dialogService_;
        artifactManager = _artifactManager_;
        projectManager = _projectManager_;
        loadingOverlayService = _loadingOverlayService_;
        navigationService = _navigationService_;
        toolbarCommunication = _communicationManager_.toolbarCommunicationManager;
        processDiagramCommunication = _communicationManager_.processDiagramCommunication;
    }));

    describe("constructor", () => {
        it("throws error if process diagram communication manager is not provided", () => {
            // arrange
            let error: Error;

            // act
            try {
                const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, null);
            } catch (ex) {
                error = ex;
            }

            // assert
            expect(error).toBeDefined();
            expect(error.message).toEqual("Process diagram communication is not provided or is null");
        });

        it("registers selection change listener", () => {
            // arrange
            const spy = spyOn(processDiagramCommunication, "register").and.callThrough();

            // act
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);

            // assert
            expect(spy).toHaveBeenCalledWith(ProcessEvents.SelectionChanged, jasmine.any(Function));
        });

        it("sets disabled to true for read-only process", () => {
            // arrange
            statefulProcess.artifactState.readonly = true;

            // act
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);

            // assert
            expect(action.disabled).toEqual(true);
        });
    });

    describe("on selection changed", () => {
        it("sets disabled to false when no shapes are selected", () => {
            // arrange
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, []);

            // assert
            expect(action.disabled).toEqual(false);
        });

        it("sets tooltip to artifact tooltip when no shapes are selected", () => {
            // arrange
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, []);

            // assert
            expect(action.tooltip).toEqual(localization.get("App_Toolbar_Delete"));
        });

        it("sets disabled to false when a single shapes is selected", () => {
            // arrange
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);
            const userTask = TestShapes.createUserTask(14, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask]);

            // assert
            expect(action.disabled).toEqual(false);
        });

        it("sets tooltip to shape tooltip when a single shapes is selected", () => {
            // arrange
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);
            const userTask = TestShapes.createUserTask(14, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask]);

            // assert
            expect(action.tooltip).toEqual(localization.get("ST_Shapes_Delete_Tooltip"));
        });

        it("sets disabled to true when multiple shapes are selected", () => {
            // arrange
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);
            const userTask = TestShapes.createUserTask(14, $rootScope);
            const userTask2 = TestShapes.createUserTask(15, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask, userTask2]);

            // assert
            expect(action.disabled).toEqual(true);
        });

        it("sets disabled to false when User Task is selected", () => {
            // arrange
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);
            const userTask = TestShapes.createUserTask(14, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask]);

            // assert
            expect(action.disabled).toEqual(false);
        });

        it("sets disabled to true when System Task is selected", () => {
            // arrange
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);
            const systemTask = TestShapes.createSystemTask(14, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [systemTask]);

            // assert
            expect(action.disabled).toEqual(true);
        });

        it("sets disabled to false when System Decision is selected", () => {
            // arrange
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);
            const systemDecision = TestShapes.createSystemDecision(14, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [systemDecision]);

            // assert
            expect(action.disabled).toEqual(false);
        });

        it("sets disabled to false when User Decision is selected", () => {
            // arrange
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);
            const userDecision = TestShapes.createUserDecision(14, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userDecision]);

            // assert
            expect(action.disabled).toEqual(false);
        });

        it("sets disabled to true when Start is selected", () => {
            // arrange
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);
            const start = TestShapes.createStart(14);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [start]);

            // assert
            expect(action.disabled).toEqual(true);
        });

        it("sets disabled to true when End is selected", () => {
            // arrange
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);
            const end = TestShapes.createEnd(14);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [end]);

            // assert
            expect(action.disabled).toEqual(true);
        });

        it("sets disabled to true when selection is valid for read-only process", () => {
            // arrange
            statefulProcess.artifactState.readonly = true;
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);
            const systemDecision = TestShapes.createSystemDecision(14, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [systemDecision]);

            // assert
            expect(action.disabled).toEqual(true);
        });
    });

    describe("execute", () => {
        it("deletes process if no shapes are selected", () => {
            // arrange
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, []);
            const spy = spyOn(loadingOverlayService, "beginLoading").and.callThrough();

            // act
            action.execute();

            // assert
            expect(spy).toHaveBeenCalled();
        });

        it("deletes User Task if User Task is selected", () => {
            // arrange
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);
            const userTask = TestShapes.createUserTask(14, $rootScope);
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask]);
            const spy = spyOn(processDiagramCommunication, "action");

            // act
            action.execute();

            // assert
            expect(spy).toHaveBeenCalledWith(ProcessEvents.DeleteShape, userTask);
        });

        it("deletes User Task if User has no delete permissions but has edit permissions", () => {
            // arrange
            const testModel = TestModels.createProcessModel();
            testModel["predefinedType"] = ItemTypePredefined.Process;
            testModel["permissions"] = RolePermissions.Edit;
            const testStatefulProcess = new StatefulProcessArtifact(testModel, null);

            const action = new ProcessDeleteAction(
                    testStatefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);
            const userTask = TestShapes.createUserTask(14, $rootScope);
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask]);
            const spy = spyOn(processDiagramCommunication, "action");

            // act
            action.execute();

            // assert
            expect(spy).toHaveBeenCalledWith(ProcessEvents.DeleteShape, userTask);
        });

        it("deletes User Decision if User Decision is selected", () => {
            // arrange
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);
            const userDecision = TestShapes.createUserDecision(14, $rootScope);
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userDecision]);
            const spy = spyOn(processDiagramCommunication, "action");

            // act
            action.execute();

            // assert
            expect(spy).toHaveBeenCalledWith(ProcessEvents.DeleteShape, userDecision);
        });

        it("deletes User Decision if User Decision is selected", () => {
            // arrange
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);
            const systemDecision = TestShapes.createSystemDecision(14, $rootScope);
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [systemDecision]);
            const spy = spyOn(processDiagramCommunication, "action");

            // act
            action.execute();

            // assert
            expect(spy).toHaveBeenCalledWith(ProcessEvents.DeleteShape, systemDecision);
        });

        it("doesn't delete System Task if System Task is selected", () => {
            // arrange
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);
            const systemTask = TestShapes.createSystemTask(14, $rootScope);
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [systemTask]);
            const spy = spyOn(processDiagramCommunication, "action");

            // act
            action.execute();

            // assert
            expect(spy).not.toHaveBeenCalled();
        });

        it("doesn't delete when multiple shapes are selected", () => {
            // arrange
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);
            const userTask = TestShapes.createUserTask(14, $rootScope);
            const userTask1 = TestShapes.createUserTask(15, $rootScope);
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask, userTask1]);
            const spy = spyOn(processDiagramCommunication, "action");

            // act
            action.execute();

            // assert
            expect(spy).not.toHaveBeenCalled();
        });

        it("doesn't delete when selection is valid for read-only process", () => {
            // arrange
            statefulProcess.artifactState.readonly = true;
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);
            const userTask = TestShapes.createUserTask(14, $rootScope);
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask]);
            const spy = spyOn(processDiagramCommunication, "action");

            // act
            action.execute();

            // assert
            expect(spy).not.toHaveBeenCalled();
        });
    });

    describe("dispose", () => {
        it("unregisters selection change listener", () => {
            // arrange
            const spy = spyOn(processDiagramCommunication, "unregister").and.callThrough();
            const action = new ProcessDeleteAction(
                    statefulProcess, localization, messageService, artifactManager, projectManager, 
                    loadingOverlayService, dialogService, navigationService, processDiagramCommunication);

            // act
            action.dispose();

            // assert
            expect(spy).toHaveBeenCalledWith(ProcessEvents.SelectionChanged, jasmine.any(String));
        });
    });
});