import * as angular from "angular";
import "rx";
import "angular-mocks";
import "script!mxClient";
import {ExecutionEnvironmentDetectorMock} from "../../../../../../core/services/execution-environment-detector.mock";
import {ArtifactReference} from "../../../../models/process-models";
import {ProcessGraph} from "./process-graph";
import {IProcessGraph} from "./models/";
import {ShapesFactory} from "./shapes/shapes-factory";
import {ProcessCopyPasteHelper} from "./process-copy-paste-helper";
import {IProcessViewModel, ProcessViewModel} from "../../viewmodel/process-viewmodel";
import {IProcessGraphModel, ProcessGraphModel} from "../../viewmodel/process-graph-model";
import {ICommunicationManager, CommunicationManager} from "../../../../../bp-process";
import {MessageServiceMock} from "../../../../../../core/messages/message.mock";
import {IMessageService} from "../../../../../../core/messages/message.svc";
import {DialogService} from "../../../../../../shared/widgets/bp-dialog";
import {LocalizationServiceMock} from "../../../../../../core/localization/localization.service.mock";
import {IStatefulArtifactFactory} from "../../../../../../managers/artifact-manager/";
import {StatefulArtifactFactoryMock} from "../../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ModalServiceMock} from "../../../../../../shell/login/mocks.spec";
import {IClipboardService, ClipboardService} from "../../../../services/clipboard.svc";
import {IFileUploadService, ICopyImageResult} from "../../../../../../core/fileUpload/";
import {FileUploadServiceMock} from "../../../../../../core/fileUpload/fileUpload.service.mock";
import {ILoadingOverlayService} from "../../../../../../core/loadingOverlay/loadingOverlay.service";
import {LoadingOverlayServiceMock} from "../../../../../../core/loadingOverlay/loadingOverlay.service.mock";
import {IHttpError} from "../../../../../../core/services/users-and-groups.svc";
import * as ProcessModels from "../../../../models/process-models";
import * as ProcessEnums from "../../../../models/enums";
import * as TestModels from "../../../../models/test-model-factory";
import * as TestShapes from "../../../../models/test-shape-factory";

describe("ProcessCopyPasteHelper tests", () => {
    let localScope, timeout, wrapper, container;
    let shapesFactory: ShapesFactory;
    let statefulArtifactFactory: IStatefulArtifactFactory;
    let communicationManager: ICommunicationManager;
    let dialogService: DialogService;
    let localization: LocalizationServiceMock;
    let process: ProcessModels.IProcess;
    let clientModel: IProcessGraphModel;
    let viewModel: IProcessViewModel;
    let graph: IProcessGraph;
    let copyPasteHelper: ProcessCopyPasteHelper;
    let selectedNodes = [];
    let clipboard: IClipboardService;
    let messageService: IMessageService;
    let $log: ng.ILogService;
    let fileUploadService: IFileUploadService;
    let $q: ng.IQService;
    let loadingOverlayService: ILoadingOverlayService;
    let $rootScope: ng.IRootScopeService;

    let _window: any = window;
    _window.executionEnvironmentDetector = ExecutionEnvironmentDetectorMock;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("dialogService", DialogService);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("fileUploadService", FileUploadServiceMock);
        $provide.service("shapesFactory", ShapesFactory);
        $provide.service("clipboardService", ClipboardService);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
    }));

    beforeEach(inject((_$window_: ng.IWindowService,
                       _$rootScope_: ng.IRootScopeService,
                       $timeout: ng.ITimeoutService,
                       _communicationManager_: ICommunicationManager,
                       _dialogService_: DialogService,
                       _$log_: ng.ILogService,
                       _$q_: ng.IQService,
                       _localization_: LocalizationServiceMock,
                       _statefulArtifactFactory_: IStatefulArtifactFactory,
                       _loadingOverlayService_: ILoadingOverlayService,
                       _fileUploadService_: IFileUploadService,
                       _clipboardService_: IClipboardService,
                       _messageService_: IMessageService,
                       _shapesFactory_: ShapesFactory) => {
        $rootScope = _$rootScope_;
        timeout = $timeout;
        communicationManager = _communicationManager_;
        dialogService = _dialogService_;
        localization = _localization_;
        wrapper = document.createElement("DIV");
        container = document.createElement("DIV");
        wrapper.appendChild(container);
        document.body.appendChild(wrapper);
        statefulArtifactFactory = _statefulArtifactFactory_;
        shapesFactory = _shapesFactory_;
        messageService = _messageService_;
        $log = _$log_;
        $q = _$q_;
        fileUploadService = _fileUploadService_;
        loadingOverlayService = _loadingOverlayService_;
        clipboard = _clipboardService_;

        $rootScope["config"] = {};
        $rootScope["config"].labels = {
            "ST_Persona_Label": "Persona",
            "ST_Colors_Label": "Color",
            "ST_Comments_Label": "Comments",
            "ST_New_User_Task_Label": "New User Task",
            "ST_New_User_Task_Persona": "User",
            "ST_New_User_Decision_Label": "New User Decision",
            "ST_New_System_Task_Label": "New System Task",
            "ST_New_System_Task_Persona": "System",
            "ST_Delete_CannotDelete_UD_AtleastTwoConditions": "Decision points should have at least two conditions",
            "ST_Add_CannotAdd_MaximumConditionsReached": "Cannot add any more conditions because the maximum number of conditions has been reached.",
            "ST_Auto_Insert_Task": "The task and its associated shapes have been moved. Another task has been created at the old location."
        };
        localScope = {graphContainer: container, graphWrapper: wrapper, isSpa: false};
        shapesFactory = new ShapesFactory($rootScope, _statefulArtifactFactory_);
    }));

    function initializeCopyPasteHelperAndRenderGraph() {
        clientModel = new ProcessGraphModel(process);
        viewModel = new ProcessViewModel(clientModel, communicationManager);
        graph = new ProcessGraph($rootScope, localScope, container, viewModel, dialogService,
        localization, shapesFactory, messageService, $log, statefulArtifactFactory, clipboard, fileUploadService, $q, loadingOverlayService);
        copyPasteHelper = new ProcessCopyPasteHelper(graph, clipboard,
        shapesFactory, messageService, $log, fileUploadService, $q, loadingOverlayService, localization);
        graph.render(true, null);
    };

    describe("copy selected shapes", () => {
        it("copy 1 shape succeeded", () => {
            // Arrange
            const userTaskId = "20";
            let userTaskNode;
            let resultModel: ProcessModels.IProcess;
            process = TestModels.createDefaultProcessModel();
            initializeCopyPasteHelperAndRenderGraph();
            userTaskNode = graph.getNodeById(userTaskId);
            spyOn(graph, "getSelectedNodes").and.returnValue([userTaskNode]);

            // Act
            copyPasteHelper.copySelectedShapes();
            $rootScope.$digest();
            resultModel = (<ProcessModels.ProcessClipboardData>clipboard.getData()).getData();

            // Assert
            expect(resultModel.decisionBranchDestinationLinks.length).toEqual(0);
            expect(resultModel.shapes.length).toEqual(2);
            expect(resultModel.links.length).toEqual(2);
        });

        /*
        copy 2 user tasks(UT1 + UT2), both are the first UT on a decision branch, belongs to the same user decision
        start -> pre -> UD -> UT1 -> ST1 -> end
                           -> UT2 -> ST2 -> end
        result model:
        UD -> UT1 -> ST1 -> end
           -> UT2 -> ST2 -> end
        */
        it("copy 2 User Task in decision branch 1st position succeeded", () => {
            // Arrange
            const userDecisionId = "4";
            const userTaskId1 = "5";
            const systemTaskId1 = "6";
            const userTaskId2 = "7";
            const systemTaskId2 = "8";
            let userDecisionNode, userTaskNode1, userTaskNode2;
            let resultModel: ProcessModels.IProcess;
            process = TestModels.createUserDecisionWithTwoBranchesModel();
            initializeCopyPasteHelperAndRenderGraph();
            userDecisionNode = graph.getNodeById(userDecisionId);
            userTaskNode1 = graph.getNodeById(userTaskId1);
            userTaskNode2 = graph.getNodeById(userTaskId2);
            spyOn(graph, "getSelectedNodes").and.returnValue([userDecisionNode, userTaskNode1, userTaskNode2]);

            // Act
            copyPasteHelper.copySelectedShapes();
            $rootScope.$digest();
            resultModel = (<ProcessModels.ProcessClipboardData>clipboard.getData()).getData();

            // Assert
            expect(resultModel.decisionBranchDestinationLinks.length).toEqual(1);
            expect(resultModel.decisionBranchDestinationLinks[0].destinationId).toEqual(parseInt(ProcessCopyPasteHelper.treeEndId, 10));
            const copiedlinks1 = resultModel.links.filter(
                l => l.sourceId === parseInt(systemTaskId1, 10) && l.destinationId === parseInt(ProcessCopyPasteHelper.treeEndId, 10)
            );
            const copiedlinks2 = resultModel.links.filter(
                l => l.sourceId === parseInt(systemTaskId2, 10) && l.destinationId === parseInt(ProcessCopyPasteHelper.treeEndId, 10)
            );
            expect(copiedlinks1.length).toEqual(1);
            expect(copiedlinks2.length).toEqual(1);
            expect(resultModel.shapes.length).toEqual(5);
            expect(resultModel.links.length).toEqual(6);
        });

        /*
        copy 2 user tasks(UT1 + UT2), one is the first UT on a decision branch,
        the other one is not on the branch
        start -> pre -> ud -> ---------- -> ut2 -> st2 -> end
                           -> ut1 -> st1 ->
        result model:
        UD -> UT1 -> ST1 -> end
           -> UT2 -> ST2 -> end
        */
        it("copy 2 User Task not in decision branch 1st position succeeded", () => {
            // Arrange
            const userDecisionId = "30";
            const userTaskId1 = "40";
            const systemTaskId1 = "50";
            const userTaskId2 = "60";
            const systemTaskId2 = "70";
            let userDecisionNode, userTaskNode1, userTaskNode2;
            let resultModel: ProcessModels.IProcess;
            process = TestModels.createDecisionWithFirstBranchEmptyNoXAndY();
            initializeCopyPasteHelperAndRenderGraph();

            userDecisionNode = graph.getNodeById(userDecisionId);
            userTaskNode1 = graph.getNodeById(userTaskId1);
            userTaskNode2 = graph.getNodeById(userTaskId2);
            spyOn(graph, "getSelectedNodes").and.returnValue([userDecisionNode, userTaskNode1, userTaskNode2]);

            // Act
            copyPasteHelper.copySelectedShapes();
            $rootScope.$digest();
            resultModel = (<ProcessModels.ProcessClipboardData>clipboard.getData()).getData();

            // Assert
            expect(resultModel.decisionBranchDestinationLinks.length).toEqual(1);
            expect(resultModel.decisionBranchDestinationLinks[0].destinationId).toEqual(parseInt(ProcessCopyPasteHelper.treeEndId, 10));
            const copiedlinks1 = resultModel.links.filter(
                l => l.sourceId === parseInt(systemTaskId1, 10) && l.destinationId === parseInt(ProcessCopyPasteHelper.treeEndId, 10)
            );
            const copiedlinks2 = resultModel.links.filter(
                l => l.sourceId === parseInt(systemTaskId2, 10) && l.destinationId === parseInt(ProcessCopyPasteHelper.treeEndId, 10)
            );
            expect(copiedlinks1.length).toEqual(1);
            expect(copiedlinks2.length).toEqual(1);
            expect(resultModel.shapes.length).toEqual(5);
            expect(resultModel.links.length).toEqual(6);
        });

        /*
        copy 2 user tasks(UT2 + UT3) in a infinite loop
        Start -> Pre -> UT1 -> ST1 -> UD -> UT2 -> ST2 -> End
                                         -> UT3 -> ST3 -> UT1
        result model:
        UD -> UT1 -> ST1 -> end
           -> UT2 -> ST2 -> end
        */
        it("copy 2 User Task where one of them in a loop succeeded", () => {
            // Arrange
            const userDecisionId = "50";
            const userTaskId1 = "60";
            const systemTaskId1 = "70";
            const userTaskId2 = "80";
            const systemTaskId2 = "90";
            let userDecisionNode, userTaskNode1, userTaskNode2;
            let resultModel: ProcessModels.IProcess;
            process = TestModels.createUserDecisionLoopModelWithoutXAndY();
            initializeCopyPasteHelperAndRenderGraph();

            userDecisionNode = graph.getNodeById(userDecisionId);
            userTaskNode1 = graph.getNodeById(userTaskId1);
            userTaskNode2 = graph.getNodeById(userTaskId2);
            spyOn(graph, "getSelectedNodes").and.returnValue([userDecisionNode, userTaskNode1, userTaskNode2]);

            // Act
            copyPasteHelper.copySelectedShapes();
            $rootScope.$digest();
            resultModel = (<ProcessModels.ProcessClipboardData>clipboard.getData()).getData();
            // Assert
            expect(resultModel.decisionBranchDestinationLinks.length).toEqual(1);
            expect(resultModel.decisionBranchDestinationLinks[0].destinationId).toEqual(parseInt(ProcessCopyPasteHelper.treeEndId, 10));
            const copiedlinks1 = resultModel.links.filter(
                l => l.sourceId === parseInt(systemTaskId1, 10) && l.destinationId === parseInt(ProcessCopyPasteHelper.treeEndId, 10)
            );
            const copiedlinks2 = resultModel.links.filter(
                l => l.sourceId === parseInt(systemTaskId2, 10) && l.destinationId === parseInt(ProcessCopyPasteHelper.treeEndId, 10)
            );
            expect(copiedlinks1.length).toEqual(1);
            expect(copiedlinks2.length).toEqual(1);
            expect(resultModel.shapes.length).toEqual(5);
            expect(resultModel.links.length).toEqual(6);
        });

        /*
        copy 2 user tasks(UT3 + UT5) in a infinite loop
        Start -> Pre -> UD -> UT1 -> ST1 -> End
                           -> UT2 -> ST2 -> UT3 -> ST3 -> UT5
                           -> UT4 -> ST4 -> UT5 -> ST5 -> UT3
        result model:
        UT2 -> ST2 -> UT1 -> ST1 -> end
        */
        it("copy 2 User Task in inifinite loop succeeded", () => {
            // Arrange
            const userTaskId1 = "80";
            const systemTaskId1 = "90";
            const userTaskId2 = "120";
            const systemTaskId2 = "130";
            let userTaskNode1, userTaskNode2;
            let resultModel: ProcessModels.IProcess;
            process = TestModels.createUserDecisionInfiniteLoopModelWithoutXAndY();
            initializeCopyPasteHelperAndRenderGraph();

            userTaskNode1 = graph.getNodeById(userTaskId1);
            userTaskNode2 = graph.getNodeById(userTaskId2);
            spyOn(graph, "getSelectedNodes").and.returnValue([userTaskNode1, userTaskNode2]);

            // Act
            copyPasteHelper.copySelectedShapes();
            $rootScope.$digest();
            resultModel = (<ProcessModels.ProcessClipboardData>clipboard.getData()).getData();

            // Assert
            expect(resultModel.decisionBranchDestinationLinks.length).toEqual(0);
            const copiedlinks1 = resultModel.links.filter(
                l => l.sourceId === parseInt(systemTaskId2, 10) && l.destinationId === parseInt(userTaskId1, 10)
            );
            const copiedlinks2 = resultModel.links.filter(
                l => l.sourceId === parseInt(systemTaskId1, 10) && l.destinationId === parseInt(ProcessCopyPasteHelper.treeEndId, 10)
            );
            expect(copiedlinks1.length).toEqual(1);
            expect(copiedlinks2.length).toEqual(1);
            expect(resultModel.shapes.length).toEqual(4);
            expect(resultModel.links.length).toEqual(4);
        });

        /*
        copy 2 user tasks(UT2 + UT4) in a loop from different decisions
        Start -> Pre -> UD1 -> UT1 -> ST1 -> UD2 -> UT3 -> ST3 -> end
                            -> UT2 ->ST2 ->UD2    -> UT4 -> ST4 -> UT2
        result model:
        UT2 -> ST2 -> UT1 -> ST1 -> end
        */
        it("copy 2 User Task in different loop", () => {
            // Arrange
            const userTaskId1 = "60";
            const systemTaskId1 = "70";
            const userTaskId2 = "110";
            const systemTaskId2 = "120";
            let userTaskNode1, userTaskNode2;
            let resultModel: ProcessModels.IProcess;
            process = TestModels.createLoopFromDIfferentUserDecisionModelWithoutXAndY();
            initializeCopyPasteHelperAndRenderGraph();

            userTaskNode1 = graph.getNodeById(userTaskId1);
            userTaskNode2 = graph.getNodeById(userTaskId2);
            spyOn(graph, "getSelectedNodes").and.returnValue([userTaskNode1, userTaskNode2]);

            // Act
            copyPasteHelper.copySelectedShapes();
            $rootScope.$digest();
            resultModel = (<ProcessModels.ProcessClipboardData>clipboard.getData()).getData();

            // Assert
            expect(resultModel.decisionBranchDestinationLinks.length).toEqual(0);
            const copiedlinks1 = resultModel.links.filter(
                l => l.sourceId === parseInt(systemTaskId1, 10) && l.destinationId === parseInt(userTaskId2, 10)
            );
            const copiedlinks2 = resultModel.links.filter(
                l => l.sourceId === parseInt(systemTaskId2, 10) && l.destinationId === parseInt(ProcessCopyPasteHelper.treeEndId, 10)
            );
            expect(copiedlinks1.length).toEqual(1);
            expect(copiedlinks2.length).toEqual(1);
            expect(resultModel.shapes.length).toEqual(4);
            expect(resultModel.links.length).toEqual(4);
        });
    });

    describe("insert shapes tests", () => {
        it("insert single shapes succeeded", () => {
            // Arrange
            const userTaskId1 = "20";
            const systemTaskId = 25;
            const tempUserTaskId = -1;
            const tempSystemTaskId = -2;
            const endId = 30;

            process = TestModels.createDefaultProcessModel();
            initializeCopyPasteHelperAndRenderGraph();
            graph.layout.setTempShapeId(0);
            const userTaskNode = graph.getNodeById(userTaskId1);
            spyOn(graph, "getSelectedNodes").and.returnValue([userTaskNode]);
            spyOn(viewModel, "addToSubArtifactCollection");

            // Act
            copyPasteHelper.copySelectedShapes();
            $rootScope.$digest();
            copyPasteHelper.insertSelectedShapes([systemTaskId], endId);
            $rootScope.$digest();

            // Assert
            expect(viewModel.shapes.length).toEqual(7);
            expect(viewModel.links.length).toEqual(6);
            expect(viewModel.decisionBranchDestinationLinks.length).toEqual(0);

            const insertedLinks1 = viewModel.links.filter(
                l => l.sourceId === systemTaskId && l.destinationId === tempUserTaskId
            );
            const insertedLinks2 = viewModel.links.filter(
                l => l.sourceId === tempSystemTaskId && l.destinationId === endId
            );
            expect(insertedLinks1.length).toEqual(1);
            expect(insertedLinks2.length).toEqual(1);
        });

        /*
        copy 2 user tasks(UT2 + UT4) in a loop from different decisions
        and insert before second user decision
        original:
        Start -> Pre -> UD1 -> UT1 -> ST1 -> UD2 -> UT3 -> ST3 -> end
                            -> UT2 ->ST2 ->UD2    -> UT4 -> ST4 -> UT2
        result:
        Start -> Pre  -> UD1 -> UT1 -> ST1 -> tempUT1 -> tempST1 -> tempUT2 -> tempST2 -> UD2 -> UT3 -> ST3 -> end
                             -> UT2 ->ST2 ->tempUT1                                               -> UT4 -> ST4 -> UT2

        */
        it("copy 2 User Task in different loop and insert before second user decision", () => {
            // Arrange
            const userTaskId1 = "60";
            const userTaskId2 = "110";
            const systemTaskId1 = 50;
            const systemTaskId2 = 70;
            const userDecisionId2 = 80;
            const tempUserTaskId1 = -1;
            const tempSystemTaskId2 = -4;

            let userTaskNode1, userTaskNode2;
            process = TestModels.createLoopFromDIfferentUserDecisionModelWithoutXAndY();
            initializeCopyPasteHelperAndRenderGraph();
            graph.layout.setTempShapeId(0);

            userTaskNode1 = graph.getNodeById(userTaskId1);
            userTaskNode2 = graph.getNodeById(userTaskId2);
            spyOn(graph, "getSelectedNodes").and.returnValue([userTaskNode1, userTaskNode2]);
            spyOn(viewModel, "addToSubArtifactCollection");

            // Act
            copyPasteHelper.copySelectedShapes();
            $rootScope.$digest();
            copyPasteHelper.insertSelectedShapes([systemTaskId1, systemTaskId2], userDecisionId2);
            $rootScope.$digest();

            // Assert
            expect(viewModel.shapes.length).toEqual(17);
            expect(viewModel.links.length).toEqual(18);
            expect(viewModel.decisionBranchDestinationLinks.length).toEqual(2);
            const insertedLinks1 = viewModel.links.filter(
                l => l.sourceId === systemTaskId1 && l.destinationId === tempUserTaskId1
            );
            const insertedLinks2 = viewModel.links.filter(
                l => l.sourceId === systemTaskId2 && l.destinationId === tempUserTaskId1
            );
            const insertedLinks3 = viewModel.links.filter(
                l => l.sourceId === tempSystemTaskId2 && l.destinationId === userDecisionId2
            );
            expect(insertedLinks1.length).toEqual(1);
            expect(insertedLinks2.length).toEqual(1);
            expect(insertedLinks3.length).toEqual(1);
        });

        /*
        copy 2 user tasks(UT1 + UT2) in a loop from different decisions
        and insert before UD2
        original:
        Start -> Pre -> UD1 -> UT1 -> ST1 -> UD2 -> UT3 -> ST3 -> end
                            -> UT2 ->ST2 ->UD2    -> UT4 -> ST4 -> UT2
        result:
        Start -> Pre  -> UD1 -> UT1 -> ST1                  -> tempUD1 -> tempUT1 -> tempST1 -> UD2 -> UT3 -> ST3 -> end
                                       -> UT2 ->ST2 ->tempUT1                         tempUT2 -> tempST2 ->        -> UT4 -> ST4 -> UT2

        */
        it("copy 2 User Task and insert before second user decision", () => {
            // Arrange
            const userDecisionId = "30";
            const userTaskId1 = "60";
            const userTaskId2 = "40";
            const systemTaskId1 = 50;
            const systemTaskId2 = 70;
            const userDecisionId2 = 80;
            const tempUserDecisionId1 = -1;
            const tempUserTaskId1 = -2;
            const tempSystemTaskId2 = -5;

            let userDecisionNode, userTaskNode1, userTaskNode2;
            process = TestModels.createLoopFromDIfferentUserDecisionModelWithoutXAndY();
            initializeCopyPasteHelperAndRenderGraph();
            graph.layout.setTempShapeId(0);

            userDecisionNode = graph.getNodeById(userDecisionId);
            userTaskNode1 = graph.getNodeById(userTaskId1);
            userTaskNode2 = graph.getNodeById(userTaskId2);
            spyOn(graph, "getSelectedNodes").and.returnValue([userDecisionNode, userTaskNode1, userTaskNode2]);
            spyOn(viewModel, "addToSubArtifactCollection");

            // Act
            copyPasteHelper.copySelectedShapes();
            $rootScope.$digest();
            copyPasteHelper.insertSelectedShapes([systemTaskId1, systemTaskId2], userDecisionId2);
            $rootScope.$digest();

            // Assert
            expect(viewModel.shapes.length).toEqual(18);
            expect(viewModel.links.length).toEqual(20);
            expect(viewModel.decisionBranchDestinationLinks.length).toEqual(3);

            const insertedLinks1 = viewModel.links.filter(
                l => l.sourceId === systemTaskId1 && l.destinationId === tempUserDecisionId1
            );
            const insertedLinks2 = viewModel.links.filter(
                l => l.sourceId === systemTaskId2 && l.destinationId === tempUserDecisionId1
            );
            const insertedLinks3 = viewModel.links.filter(
                l => l.sourceId === tempSystemTaskId2 && l.destinationId === userDecisionId2
            );
            expect(insertedLinks1.length).toEqual(1);
            expect(insertedLinks2.length).toEqual(1);
            expect(insertedLinks3.length).toEqual(1);
        });

        it("copy User Task with System Decision and insert before end", () => {
            // Arrange
            const userTaskId = "30";
            const systemDecisionId = "40";
            const systemTaskId1 = 50;
            const systemTaskId2 = 60;
            const endId = 70;
            const tempUserTaskId = -2;
            const tempSystemTaskId1 = -3;
            const tempSystemTaskId2 = -4;

            let userTaskNode, systemDecisionNode;
            process = TestModels.createSystemDecisionLoopModelWithoutXAndY();
            initializeCopyPasteHelperAndRenderGraph();
            graph.layout.setTempShapeId(0);

            userTaskNode = graph.getNodeById(userTaskId);
            systemDecisionNode = graph.getNodeById(systemDecisionId);
            spyOn(graph, "getSelectedNodes").and.returnValue([userTaskNode, systemDecisionNode]);
            spyOn(viewModel, "addToSubArtifactCollection");

            // Act
            copyPasteHelper.copySelectedShapes();
            $rootScope.$digest();
            copyPasteHelper.insertSelectedShapes([systemTaskId1, systemTaskId2], endId);
            $rootScope.$digest();

            // Assert
            expect(viewModel.shapes.length).toEqual(11);
            expect(viewModel.links.length).toEqual(12);
            expect(viewModel.decisionBranchDestinationLinks.length).toEqual(1);

            const insertedLinks1 = viewModel.links.filter(
                l => l.sourceId === tempSystemTaskId1 && l.destinationId === endId
            );
            const insertedLinks2 = viewModel.links.filter(
                l => l.sourceId === tempSystemTaskId2 && l.destinationId === endId
            );
            expect(insertedLinks1.length).toEqual(1);
            expect(insertedLinks2.length).toEqual(1);
        });

        it("copy User Task with nested System Decisions and insert before end", () => {
            // Arrange
            const userTaskId = "3";
            const systemTaskId1 = 5;
            const systemTaskId3 = 9;
            const endId = 14;
            const tempUserTaskId = -2;
            const tempSystemTaskId1 = -3;
            const tempSystemTaskId2 = -4;

            let userTaskNode;
            process = TestModels.createNestedSystemDecisionsWithLoopModelWithoutXAndY();
            initializeCopyPasteHelperAndRenderGraph();
            graph.layout.setTempShapeId(0);

            userTaskNode = graph.getNodeById(userTaskId);
            spyOn(graph, "getSelectedNodes").and.returnValue([userTaskNode]);
            spyOn(viewModel, "addToSubArtifactCollection");

            // Act
            copyPasteHelper.copySelectedShapes();
            $rootScope.$digest();
            copyPasteHelper.insertSelectedShapes([systemTaskId3], endId);
            $rootScope.$digest();

            // Assert
            expect(viewModel.shapes.length).toEqual(19);
            expect(viewModel.links.length).toEqual(23);
            expect(viewModel.decisionBranchDestinationLinks.length).toEqual(3);
            const insertedLinks1 = viewModel.links.filter(
                l => l.sourceId === tempSystemTaskId1 && l.destinationId === endId
            );
            const insertedLinks2 = viewModel.links.filter(
                l => l.sourceId === tempSystemTaskId2 && l.destinationId === endId
            );
            expect(insertedLinks1.length).toEqual(1);
            expect(insertedLinks2.length).toEqual(1);
        });

        it("copy User Task with System Decision and insert into a branch", () => {
            // Arrange
            const userTaskId = "30";
            const systemDecisionId = "40";
            const systemTaskId2 = 60;
            const endId = 70;
            const tempUserTaskId = -2;
            const tempSystemTaskId1 = -3;
            const tempSystemTaskId2 = -4;

            let userTaskNode, systemDecisionNode;
            process = TestModels.createSystemDecisionLoopModelWithoutXAndY();
            initializeCopyPasteHelperAndRenderGraph();
            graph.layout.setTempShapeId(0);

            userTaskNode = graph.getNodeById(userTaskId);
            systemDecisionNode = graph.getNodeById(systemDecisionId);
            spyOn(graph, "getSelectedNodes").and.returnValue([userTaskNode, systemDecisionNode]);
            spyOn(viewModel, "addToSubArtifactCollection");

            // Act
            copyPasteHelper.copySelectedShapes();
            $rootScope.$digest();
            copyPasteHelper.insertSelectedShapes([systemTaskId2], endId);
            $rootScope.$digest();

            // Assert
            expect(viewModel.shapes.length).toEqual(11);
            expect(viewModel.links.length).toEqual(12);
            expect(viewModel.decisionBranchDestinationLinks.length).toEqual(1);
            const insertedLinks1 = viewModel.links.filter(
                l => l.sourceId === tempSystemTaskId1 && l.destinationId === endId
            );
            const insertedLinks2 = viewModel.links.filter(
                l => l.sourceId === tempSystemTaskId2 && l.destinationId === endId
            );
            expect(insertedLinks1.length).toEqual(1);
            expect(insertedLinks2.length).toEqual(1);
        });

        it("clears out include if target process is same as include.", () => {
            // Arrange
            const userTaskId = "20";
            process = TestModels.createDefaultProcessModel();
            initializeCopyPasteHelperAndRenderGraph();
            const userTaskNode = graph.getNodeById(userTaskId);
            spyOn(graph, "getSelectedNodes").and.returnValue([userTaskNode]);
            spyOn(viewModel, "addToSubArtifactCollection");

            const include = new ArtifactReference();
            include.id = viewModel.id;
            userTaskNode.model.associatedArtifact = include;

            // Act
            const systemTaskId = 25;
            const endId = 30;
            copyPasteHelper.copySelectedShapes();
            $rootScope.$digest();
            copyPasteHelper.insertSelectedShapes([systemTaskId], endId);
            $rootScope.$digest();

            // Assert
            const newlyAddedUserTask = viewModel.shapes.filter(
                a => a.id < 0 && a.propertyValues[shapesFactory.ClientType.key].value === ProcessEnums.ProcessShapeType.UserTask)[0];
            expect(newlyAddedUserTask.associatedArtifact).toBeNull();
        });

        it("clears out story links for copied user tasks.", () => {
            // Arrange
            const userTaskId = "20";
            process = TestModels.createDefaultProcessModel();
            initializeCopyPasteHelperAndRenderGraph();
            const userTaskNode = graph.getNodeById(userTaskId);
            spyOn(graph, "getSelectedNodes").and.returnValue([userTaskNode]);
            spyOn(viewModel, "addToSubArtifactCollection");

            userTaskNode.model.propertyValues[shapesFactory.StoryLinks.key].value = 1;

            // Act
            const systemTaskId = 25;
            const endId = 30;
            copyPasteHelper.copySelectedShapes();
            $rootScope.$digest();
            copyPasteHelper.insertSelectedShapes([systemTaskId], endId);
            $rootScope.$digest();

            // Assert
            const newlyAddedUserTask = viewModel.shapes.filter(
                a => a.id < 0 && a.propertyValues[shapesFactory.ClientType.key].value === ProcessEnums.ProcessShapeType.UserTask)[0];
            expect(newlyAddedUserTask.propertyValues[shapesFactory.StoryLinks.key].value).toBeNull();
        });
    });

    describe("copy images tests", () => {
        beforeEach(() => {
            let userTaskNode;
            process = TestModels.createDefaultProcessModel();
            initializeCopyPasteHelperAndRenderGraph();
        });

        it("does not call filestore service when detects no system tasks with saved images", () => {
            //Arrange
            const copyPasteHelper = new ProcessCopyPasteHelper(
                graph, clipboard, shapesFactory, messageService, $log, fileUploadService, $q, loadingOverlayService, localization);

            spyOn(copyPasteHelper, "createDecisionPointRefs");
            spyOn(copyPasteHelper, "addTasksAndDecisionsToClipboardData").and.callFake(
                (data, baseNodes, decisionPointRefs) => {
                    data.systemShapeImageIds = [];
            });
            spyOn(copyPasteHelper, "connectAllSubtrees");
            spyOn(copyPasteHelper, "addBranchLinks");
            spyOn(copyPasteHelper, "createProcessModel");
            spyOn(copyPasteHelper, "isPastableAfterUserDecision");
            const copySpy = spyOn(copyPasteHelper, "copySystemTaskSavedImages").and.callThrough();
            const fileStoreSpy = spyOn(fileUploadService, "copyArtifactImagesToFilestore");

            //Assert
            copyPasteHelper.copySelectedShapes();

            //Act
            expect(copySpy).toHaveBeenCalled();
            expect(fileStoreSpy).not.toHaveBeenCalled();
        });

        it("calls filestore service when detects system tasks with saved images", () => {
            //Arrange
            const copyPasteHelper = new ProcessCopyPasteHelper(
                graph, clipboard, shapesFactory, messageService, $log, fileUploadService, $q, loadingOverlayService, localization);
            spyOn(copyPasteHelper, "createDecisionPointRefs");
            spyOn(copyPasteHelper, "addTasksAndDecisionsToClipboardData").and.callFake(
                (data, baseNodes, decisionPointRefs) => {
                    data.systemShapeImageIds = [1];
            });
            spyOn(copyPasteHelper, "connectAllSubtrees");
            spyOn(copyPasteHelper, "addBranchLinks");
            spyOn(copyPasteHelper, "createProcessModel");
            spyOn(copyPasteHelper, "isPastableAfterUserDecision");
            const copySpy = spyOn(copyPasteHelper, "copySystemTaskSavedImages").and.callThrough();
            const fileStoreSpy = spyOn(fileUploadService, "copyArtifactImagesToFilestore");

            //Act
            copyPasteHelper.copySelectedShapes();

            //Assert
            expect(copySpy).toHaveBeenCalled();
            expect(fileStoreSpy).toHaveBeenCalled();
        });

        it("does not send to filestore when detects system tasks with only unsaved images", () => {
            //Arrange
            const userTaskId = 20;
            const systemTaskId = 25;
            const systemTaskShape = process.shapes.filter(a => a.id === systemTaskId)[0];
            systemTaskShape.propertyValues[shapesFactory.AssociatedImageUrl.key].value = "a/b/c";
            systemTaskShape.propertyValues[shapesFactory.ImageId.key].value = "some file guid";
            graph.render(true, 20);
            const userTaskNode = graph.getNodeById(userTaskId.toString());
            spyOn(graph, "getSelectedNodes").and.returnValue([userTaskNode]);
            const copyPasteHelper = new ProcessCopyPasteHelper(
                graph, clipboard, shapesFactory, messageService, $log, fileUploadService, $q, loadingOverlayService, localization);
            spyOn(copyPasteHelper, "copySystemTaskSavedImages").and.callThrough();

            const fileStoreSpy = spyOn(fileUploadService, "copyArtifactImagesToFilestore");

            //Act
            copyPasteHelper.copySelectedShapes();
            $rootScope.$digest();

            //Assert
            expect(fileStoreSpy).not.toHaveBeenCalled();
        });

        it("correctly detects system tasks with saved images", () => {
            //Arrange
            const userTaskId = 20;
            const systemTaskId = 25;
            const systemTaskShape = process.shapes.filter(a => a.id === systemTaskId)[0];
            systemTaskShape.propertyValues[shapesFactory.AssociatedImageUrl.key].value = "a/b/c";
            systemTaskShape.propertyValues[shapesFactory.ImageId.key].value = 1;
            graph.render(true, 20);
            const userTaskNode = graph.getNodeById(userTaskId.toString());
            spyOn(graph, "getSelectedNodes").and.returnValue([userTaskNode]);

            const copyPasteHelper = new ProcessCopyPasteHelper(
                graph, clipboard, shapesFactory, messageService, $log, fileUploadService, $q, loadingOverlayService, localization);
            spyOn(copyPasteHelper, "copySystemTaskSavedImages").and.callThrough();

            const copyResult: ICopyImageResult = {
                originalId: systemTaskId, newImageId: "some new guid", newImageUrl: "some/new/url"
            };
            let detectedSystemTaskIds: number [] = [];
            spyOn(fileUploadService, "copyArtifactImagesToFilestore")
                .and.callFake((systemTaskIds, expirationDate) => {
                    detectedSystemTaskIds = systemTaskIds;
                    return $q.when([copyResult]);
            });

            //Act
            copyPasteHelper.copySelectedShapes();
            $rootScope.$digest();

            //Assert
            expect(detectedSystemTaskIds.length).toBe(1);
        });

        it("sets clipboard data after sucessful filestore call", () => {
            //Arrange
            const userTaskId = 20;
            const systemTaskId = 25;
            const systemTaskShape = process.shapes.filter(a => a.id === systemTaskId)[0];
            systemTaskShape.propertyValues[shapesFactory.AssociatedImageUrl.key].value = "a/b/c";
            systemTaskShape.propertyValues[shapesFactory.ImageId.key].value = 1;
            graph.render(true, 20);
            const userTaskNode = graph.getNodeById(userTaskId.toString());
            spyOn(graph, "getSelectedNodes").and.returnValue([userTaskNode]);

            const copyPasteHelper = new ProcessCopyPasteHelper(
                graph, clipboard, shapesFactory, messageService, $log, fileUploadService, $q, loadingOverlayService, localization);
            const copySpy = spyOn(copyPasteHelper, "copySystemTaskSavedImages").and.callThrough();
            const copyResult: ICopyImageResult = {
                originalId: systemTaskId, newImageId: "some new guid", newImageUrl: "some/new/url"
            };
            const fileStoreSpy = spyOn(fileUploadService, "copyArtifactImagesToFilestore")
                .and.callFake((systemTaskIds, expirationDate) => {
                    return $q.when([copyResult]);
            });

            //Act
            copyPasteHelper.copySelectedShapes();
            $rootScope.$digest();
            const data  = (<ProcessModels.ProcessClipboardData>clipboard.getData()).getData();

            //Assert
            expect(data).not.toBeNull();
            const clipboardSystemTask = data.shapes.filter(a => a.id === systemTaskId)[0];
            expect(clipboardSystemTask.propertyValues[shapesFactory.AssociatedImageUrl.key].value).toBe(copyResult.newImageUrl);
            expect(clipboardSystemTask.propertyValues[shapesFactory.ImageId.key].value).toBe(copyResult.newImageId);
        });

        it("sets clipboard data after failed filestore call", () => {
             //Arrange
            const userTaskId = 20;
            const systemTaskId = 25;
            const systemTaskShape = process.shapes.filter(a => a.id === systemTaskId)[0];
            systemTaskShape.propertyValues[shapesFactory.AssociatedImageUrl.key].value = "a/b/c";
            systemTaskShape.propertyValues[shapesFactory.ImageId.key].value = 1;
            graph.render(true, 20);
            const userTaskNode = graph.getNodeById(userTaskId.toString());
            spyOn(graph, "getSelectedNodes").and.returnValue([userTaskNode]);

            const copyPasteHelper = new ProcessCopyPasteHelper(
                graph, clipboard, shapesFactory, messageService, $log, fileUploadService, $q, loadingOverlayService, localization);
            const copySpy = spyOn(copyPasteHelper, "copySystemTaskSavedImages").and.callThrough();

            const copyResult: ICopyImageResult = {
                originalId: systemTaskId, newImageId: "some new guid", newImageUrl: "some/new/url"
            };
            const fileStoreSpy = spyOn(fileUploadService, "copyArtifactImagesToFilestore")
                .and.callFake((systemTaskIds, expirationDate) => {
                    const error: IHttpError = {message: "ERROR", errorCode: 404, statusCode: null};
                    return $q.reject(error);
            });

            //Act
            copyPasteHelper.copySelectedShapes();
            $rootScope.$digest();
            const data  = (<ProcessModels.ProcessClipboardData>clipboard.getData()).getData();

            //Assert
            expect(data).not.toBeNull();
            const clipboardSystemTask = data.shapes.filter(a => a.id === systemTaskId)[0];
            expect(clipboardSystemTask.propertyValues[shapesFactory.AssociatedImageUrl.key].value).toBeNull();
            expect(clipboardSystemTask.propertyValues[shapesFactory.ImageId.key].value).toBeNull();
        });

        it("adds error message to display to user after 404 filestore error", () => {
             //Arrange
            const userTaskId = 20;
            const systemTaskId = 25;
            const systemTaskShape = process.shapes.filter(a => a.id === systemTaskId)[0];
            systemTaskShape.propertyValues[shapesFactory.AssociatedImageUrl.key].value = "a/b/c";
            systemTaskShape.propertyValues[shapesFactory.ImageId.key].value = 1;
            graph.render(true, 20);
            const userTaskNode = graph.getNodeById(userTaskId.toString());
            spyOn(graph, "getSelectedNodes").and.returnValue([userTaskNode]);

            const copyPasteHelper = new ProcessCopyPasteHelper(
                graph, clipboard, shapesFactory, messageService, $log, fileUploadService, $q, loadingOverlayService, localization);
            const copySpy = spyOn(copyPasteHelper, "copySystemTaskSavedImages").and.callThrough();
            const copyResult: ICopyImageResult = {
                originalId: systemTaskId, newImageId: "some new guid", newImageUrl: "some/new/url"
            };
            const serverError = "ERROR";
            const fileStoreSpy = spyOn(fileUploadService, "copyArtifactImagesToFilestore")
                .and.callFake((systemTaskIds, expirationDate) => {
                    const error: IHttpError = {message: serverError, errorCode: 404, statusCode: null};
                    return $q.reject(error);
            });
            const errorMessageSpy = spyOn(messageService, "addError");
            const expectedErrorMessage = "Copy_Images_Failed" + " " + serverError;
            //Act
            copyPasteHelper.copySelectedShapes();
            $rootScope.$digest();

            //Assert
            expect(errorMessageSpy).toHaveBeenCalledTimes(1);
            expect(errorMessageSpy).toHaveBeenCalledWith(expectedErrorMessage);
        });
    });

    describe("getCommonUserDecisions", () => {
        beforeEach(() => {
            process = TestModels.createDefaultProcessModel();
            clientModel = new ProcessGraphModel(process);
            viewModel = new ProcessViewModel(clientModel, communicationManager);
            graph = new ProcessGraph(
                $rootScope, localScope, container, viewModel, dialogService, localization, shapesFactory,
                messageService, $log, statefulArtifactFactory, clipboard, fileUploadService, $q, loadingOverlayService);
            copyPasteHelper = new ProcessCopyPasteHelper(
                graph, clipboard, shapesFactory, messageService, $log, fileUploadService, $q, loadingOverlayService,
                localization);
        });

        it("returns empty array if no user task is selected", () => {
            // act
            const result = copyPasteHelper.getCommonUserDecisions([]);

            // assert
            expect(result).toEqual([]);
        });

        it("returns empty array if a single user task is selected", () => {
            // arrange
            spyOn(graph, "getMxGraphModel").and.returnValue({});
            const decision = TestShapes.createUserDecision(999, $rootScope);
            const userTask1 = TestShapes.createUserTask(888, $rootScope);
            spyOn(userTask1, "getSources").and.returnValue([decision]);
            const userTasks = [userTask1];

            // act
            const result = copyPasteHelper.getCommonUserDecisions(userTasks);

            // assert
            expect(result).toEqual([]);
        });

        it("returns empty array when selected user tasks don't share common user decision", () => {
            // arrange
            spyOn(graph, "getMxGraphModel").and.returnValue({});
            const decision1 = TestShapes.createSystemTask(111, $rootScope);
            const decision2 = TestShapes.createUserDecision(222, $rootScope);
            const userTask1 = TestShapes.createUserTask(333, $rootScope);
            spyOn(userTask1, "getSources").and.returnValue([decision1]);
            const userTask2 = TestShapes.createUserTask(444, $rootScope);
            spyOn(userTask2, "getSources").and.returnValue([decision2]);
            const userTasks = [userTask1, userTask2];

            // act
            const result = copyPasteHelper.getCommonUserDecisions(userTasks);

            // assert
            expect(result).toEqual([]);
        });

        it("returns user decision when selected user tasks share common user decision", () => {
            // arrange
            spyOn(graph, "getMxGraphModel").and.returnValue({});
            const decision = TestShapes.createUserDecision(222, $rootScope);
            const userTask1 = TestShapes.createUserTask(333, $rootScope);
            spyOn(userTask1, "getSources").and.returnValue([decision]);
            const userTask2 = TestShapes.createUserTask(444, $rootScope);
            spyOn(userTask2, "getSources").and.returnValue([decision]);
            const userTask3 = TestShapes.createUserTask(555, $rootScope);
            spyOn(userTask3, "getSources").and.returnValue([decision]);
            const userTasks = [userTask1, userTask2, userTask3];

            // act
            const result = copyPasteHelper.getCommonUserDecisions(userTasks);

            // assert
            expect(result).toEqual([decision]);
        });
    });
});
