import * as angular from "angular";
require("script!mxClient");
import {ExecutionEnvironmentDetectorMock} from "../../../../../../commonModule/services/executionEnvironmentDetector.mock";
import {ProcessGraph} from "./process-graph";
import {ProcessViewModel} from "../../viewmodel/process-viewmodel";
import * as TestModels from "../../../../models/test-model-factory";
import {ICommunicationManager, CommunicationManager} from "../../../../../bp-process";
import {LocalizationServiceMock} from "../../../../../../commonModule/localization/localization.service.mock";
import {DialogService} from "../../../../../../shared/widgets/bp-dialog";
import {ModalServiceMock} from "../../../../../../shell/login/mocks.spec";
import {ShapesFactory} from "./shapes/shapes-factory";
import {IStatefulArtifactFactory} from "../../../../../../managers/artifact-manager/";
import {StatefulArtifactFactoryMock} from "../../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {StatefulSubArtifactCollection} from "../../../../../../managers/artifact-manager/sub-artifact";
import {ChangeSetCollector} from "../../../../../../managers/artifact-manager/changeset";
import {ProcessEvents} from "../../process-diagram-communication";
import {IMessageService} from "../../../../../../main/components/messages/message.svc";
import {MessageServiceMock} from "../../../../../../main/components/messages/message.mock";

describe("DragDropHandler test", () => {
    let msgService: IMessageService,
        localScope,
        rootScope,
        wrapper,
        container,
        communicationManager: ICommunicationManager,
        shapesFactoryService: ShapesFactory,
        dialogService: DialogService,
        localization: LocalizationServiceMock,
        statefulArtifactFactory: IStatefulArtifactFactory,
        shapesFactory: ShapesFactory;

    let _window: any = window;
    _window.executionEnvironmentDetector = ExecutionEnvironmentDetectorMock;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("messageService", MessageServiceMock);
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("dialogService", DialogService);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("shapesFactory", ShapesFactory);
    }));

    let setProcessViewModel = function (model) {
        model.subArtifactCollection = new StatefulSubArtifactCollection(model, null);
        model.changeset = new ChangeSetCollector(model);
        model.artifactState = {dirty: false};
        model.lock = () => undefined;
        const processModel = new ProcessViewModel(model, communicationManager);
        return processModel;
    };

    beforeEach(inject((_$window_: ng.IWindowService,
        $rootScope: ng.IRootScopeService,
        messageService: IMessageService,
        _communicationManager_: ICommunicationManager,
        _dialogService_: DialogService,
        _localization_: LocalizationServiceMock,
        _statefulArtifactFactory_: IStatefulArtifactFactory,
        _shapesFactory_: ShapesFactory
    ) => {
        rootScope = $rootScope;
        msgService = messageService;
        wrapper = document.createElement("DIV");
        container = document.createElement("DIV");
        wrapper.appendChild(container);
        document.body.appendChild(wrapper);
        communicationManager = _communicationManager_;
        dialogService = _dialogService_;
        localization = _localization_;
        statefulArtifactFactory = _statefulArtifactFactory_;
        shapesFactory = _shapesFactory_;

        $rootScope["config"] = {
            labels: {
                "ST_Persona_Label": "Persona",
                "ST_Colors_Label": "Color",
                "ST_Comments_Label": "Comments",
                "ST_New_User_Task_Label": "New User Task",
                "ST_New_User_Task_Persona": "User",
                "ST_New_User_Decision_Label": "New Decision",
                "ST_New_System_Task_Label": "New System Task",
                "ST_New_System_Task_Persona": "System",
                "ST_Eighty_Percent_of_Shape_Limit_Reached": "The Process now has {0} of the maximum {1} shapes",
                "ST_Shape_Limit_Exceeded": "The Process will exceed the maximum {0} shapes",
                "ST_Shape_Limit_Exceeded_Initial_Load": "The Process will exceed the maximum {0} shapes"
            },
            settings: {
                "StorytellerShapeLimit": 100,
                "StorytellerIsSMB": "false"
            }
        };

        localScope = {graphContainer: container, graphWrapper: wrapper, isSpa: false};

        localScope["vm"] = {
            "$rootScope": rootScope
        };

        shapesFactoryService = new ShapesFactory(rootScope, statefulArtifactFactory);
    }));

    describe("isValidDropSource", () => {

        it("returns true when drag and drop is enabled and drop source is User Task", () => {
            // Arrange
            const testModel = TestModels.createLargeTestModel();
            const processModel = setProcessViewModel(testModel);
            const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            const userTaskId = "36";
            spyOn(graph.dragDropHandler, "isEnabled").and.returnValue(true);

            // Act
            graph.render(false, null);
            const isValid = graph.dragDropHandler.isValidDropSource(graph.getNodeById(userTaskId));
            //Assert
            expect(isValid).toBe(true);
        });

        it("returns false when drag and drop is disabled and drop source is User Task", () => {
            // Arrange
            const testModel = TestModels.createLargeTestModel();
            const processModel = setProcessViewModel(testModel);
            const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            const userTaskId = "36";
            spyOn(graph.dragDropHandler, "isEnabled").and.returnValue(false);

            // Act
            graph.render(false, null);
            const isValid = graph.dragDropHandler.isValidDropSource(graph.getNodeById(userTaskId));
            //Assert
            expect(isValid).toBe(false);
        });

        it("returns false when drag and drop is enabled and drop source is System Task", () => {
            // Arrange
            const testModel = TestModels.createLargeTestModel();
            const processModel = setProcessViewModel(testModel);
            const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            const systemTask = "37";
            spyOn(graph.dragDropHandler, "isEnabled").and.returnValue(true);

            // Act
            graph.render(false, null);
            const isValid = graph.dragDropHandler.isValidDropSource(graph.getNodeById(systemTask));
            //Assert
            expect(isValid).toBe(false);
        });
    });

    it("createDragPreview - preview is created with predetermined width and height", () => {
        // Arrange
        let testModel = TestModels.createLargeTestModel();
        const processModel = setProcessViewModel(testModel);
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

        // Act
        graph.render(true, null);
        let preview: HTMLDivElement = graph.dragDropHandler.createDragPreview();

        //Assert
        expect(preview.style.width).toEqual("60px");
        expect(preview.style.height).toEqual("75px");
    });

    it("highlightDropTarget - returns that getDropEdgeState method was called", () => {
        // Arrange
        let testModel = TestModels.createLargeTestModel();
        const processModel = setProcessViewModel(testModel);
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(true, null);
        let pt = {
            getX: () => 130,
            getY: () => 120
        };

        graph.dragDropHandler.moveCell = graph.getNodeById("20");
        graph.dragDropHandler.createDragPreview();
        let getDropEdgeStateSpy = spyOn(graph.layout, "getDropEdgeState");

        // Act
        graph.dragDropHandler.highlightDropTarget(pt);

        // Assert
        expect(getDropEdgeStateSpy).toHaveBeenCalled();
    });

    it("Reset - nulls out the cell that we are dragging.", () => {
        // Arrange
        let testModel = TestModels.createLargeTestModel();
        const processModel = setProcessViewModel(testModel);
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(true, null);
        graph.dragDropHandler.moveCell = graph.getNodeById("20");

        // Act
        graph.dragDropHandler.reset();

        // Assert
        expect(graph.dragDropHandler.moveCell).toBeNull();
    });

    describe("isEnabled", () => {

        it("returns isEnabled equal to 'false' when multiple shapes are selected through selection event", () => {
            // Arrange
            const testModel = TestModels.createLargeTestModel();
            const processModel = setProcessViewModel(testModel);
            const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            const userTaskId1 = "36";
            const userTaskId2 = "20";

            // Act
            graph.render(false, null);
            const userTaskNode1 = graph.getNodeById(userTaskId1);
            const userTaskNode2 = graph.getNodeById(userTaskId2);
            graph.processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTaskId1, userTaskId2]);

            const isEnabled = graph.dragDropHandler.isEnabled();
            //Assert
            expect(isEnabled).toBe(false);
        });

        it("returns isEnabled equal to 'true' when one shape is selected through selection event", () => {
            // Arrange
            const testModel = TestModels.createLargeTestModel();
            const processModel = setProcessViewModel(testModel);
            const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            const userTaskId1 = "36";

            // Act
            graph.render(false, null);
            const userTaskNode1 = graph.getNodeById(userTaskId1);
            graph.processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTaskId1]);

            const isEnabled = graph.dragDropHandler.isEnabled();
            //Assert
            expect(isEnabled).toBe(true);
        });
    });
});