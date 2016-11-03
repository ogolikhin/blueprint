import * as angular from "angular";
require("script!mxClient");
import {ProcessGraph} from "./process-graph";
import {MessageServiceMock} from "../../../../../../core/messages/message.mock";
import {IMessageService} from "../../../../../../core/messages/message.svc";
import {ProcessViewModel} from "../../viewmodel/process-viewmodel";
import {NodeType} from "./models/";
import * as TestModels from "../../../../models/test-model-factory";
import {GRAPH_LEFT, GRAPH_TOP, GRAPH_COLUMN_WIDTH, GRAPH_ROW_HEIGHT} from "./models/";
import {IDiagramNode, IDecision} from "./models/";
import {CS_LEFT, CS_RIGHT, CS_VERTICAL} from "./shapes/connector-styles";
import {ProcessLinkModel} from "../../../../models/process-models";
import {DiagramLink} from "./shapes/";
import {ProcessValidator} from "./process-graph-validator";
import {ICommunicationManager, CommunicationManager} from "../../../../../bp-process";
import {LocalizationServiceMock} from "../../../../../../core/localization/localization.mock";
import {DialogService} from "../../../../../../shared/widgets/bp-dialog";
import {ModalServiceMock} from "../../../../../../shell/login/mocks.spec";
import {ProcessAddHelper} from "./process-add-helper";
import {ShapesFactory, ShapesFactoryMock} from "./shapes/shapes-factory";
import {IStatefulArtifactFactory} from "../../../../../../managers/artifact-manager/";
import {StatefulArtifactFactoryMock} from "../../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {StatefulSubArtifactCollection} from "../../../../../../managers/artifact-manager/sub-artifact";
import {ChangeSetCollector} from "../../../../../../managers/artifact-manager/changeset";

class ExecutionEnvironmentDetectorMock {
    private browserInfo: any;

    constructor() {
        this.browserInfo = { msie: false, firefox: false, version: 0 };
    }

    public getBrowserInfo(): any {
        return this.browserInfo;
    }
}

describe("Layout test", () => {
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
        model.lock = function (){};
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

    it("Test default process without system tasks", () => {
        // Arrange
        const testModel = TestModels.createModelWithoutSystemTask();
        const processModel = setProcessViewModel(testModel);

        // Act
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(false, null);

        //Assert
        expect(graph.getNodeById("10").getNodeType()).toEqual(NodeType.ProcessStart);
        expect(graph.getNodeById("20").getNodeType()).toEqual(NodeType.UserTask);
        expect(graph.getNodeById("30").getNodeType()).toEqual(NodeType.ProcessEnd);
    });

    it("Test default process", () => {
        // Arrange
        const testModel = TestModels.createDefaultProcessModel();
        const processModel = setProcessViewModel(testModel);

        // Act
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(false, null);

        //Assert
        expect(graph.getNodeById("10").getNodeType()).toEqual(NodeType.ProcessStart);
        expect(graph.getNodeById("15").getNodeType()).toEqual(NodeType.SystemTask);
        expect(graph.getNodeById("20").getNodeType()).toEqual(NodeType.UserTask);
        expect(graph.getNodeById("25").getNodeType()).toEqual(NodeType.SystemTask);
        expect(graph.getNodeById("30").getNodeType()).toEqual(NodeType.ProcessEnd);
    });

    it("Test insert task", () => {
        // Arrange
        const testModel = TestModels.createDefaultProcessModel();
        const processModel = setProcessViewModel(testModel);
        let graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

        //bypass testing adding stateful shapes logic here
        spyOn(processModel, "addToSubArtifactCollection").and.returnValue(null);

        // Act
        graph.render(false, null);
        ProcessAddHelper.insertTaskWithUpdate(graph.getNodeById("15").getConnectableElement().edges[1], graph.layout, shapesFactoryService);

        graph.destroy();
        graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(true, null);

        //Assert
        expect(graph.getNodeById("10").getNodeType()).toEqual(NodeType.ProcessStart);
        expect(graph.getNodeById("10").getX()).toEqual(GRAPH_LEFT);
        expect(graph.getNodeById("15").getNodeType()).toEqual(NodeType.SystemTask);
        expect(graph.getNodeById("15").getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH);
        expect(graph.getNodeById("20").getNodeType()).toEqual(NodeType.UserTask);
        expect(graph.getNodeById("20").getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH * 4);
        expect(graph.getNodeById("25").getNodeType()).toEqual(NodeType.SystemTask);
        expect(graph.getNodeById("25").getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH * 5);
        expect(graph.getNodeById("30").getNodeType()).toEqual(NodeType.ProcessEnd);
        expect(graph.getNodeById("30").getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH * 6);
    });

    it("Test insert user decision in the middle of the diagram", () => {
        // Arrange
        const testModel = TestModels.createDefaultProcessModel();
        const processModel = setProcessViewModel(testModel);
        let graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

        //bypass testing adding stateful shapes logic here
        spyOn(processModel, "addToSubArtifactCollection").and.returnValue(null);

        // Act
        graph.render(false, null);

        ProcessAddHelper.insertUserDecision(graph.getNodeById("15").getConnectableElement().edges[1], graph.layout, shapesFactoryService);

        graph.destroy();
        graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(true, null);

        //Assert
        expect(graph.getNodeById("10").getNodeType()).toEqual(NodeType.ProcessStart);
        expect(graph.getNodeById("10").getX()).toEqual(GRAPH_LEFT);
        expect(graph.getNodeById("15").getNodeType()).toEqual(NodeType.SystemTask);
        expect(graph.getNodeById("15").getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH);
        expect(graph.getNodeById("20").getNodeType()).toEqual(NodeType.UserTask);
        expect(graph.getNodeById("20").getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH * 3);
        expect(graph.getNodeById("25").getNodeType()).toEqual(NodeType.SystemTask);
        expect(graph.getNodeById("25").getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH * 4);
        expect(graph.getNodeById("30").getNodeType()).toEqual(NodeType.ProcessEnd);
        expect(graph.getNodeById("30").getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH * 6);
    });

    it("Test insert user decision at the end of the diagram", () => {
        // Arrange
        const processModel = setProcessViewModel(TestModels.createDefaultProcessModel());
        let graph = new ProcessGraph(rootScope, {graphContainer: container, graphWrapper: wrapper},
            container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.layout.setTempShapeId(0);

        //bypass testing adding stateful shapes logic here
        spyOn(processModel, "addToSubArtifactCollection").and.returnValue(null);

        // Act
        graph.render(false, null);

        ProcessAddHelper.insertUserDecision(graph.getNodeById("25").getConnectableElement().edges[1], graph.layout, shapesFactoryService);

        graph.destroy();
        graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(true, null);

        //Assert
        let node = graph.getNodeById("-1");
        expect(node.getNodeType()).toEqual(NodeType.UserDecision);
        expect(node.getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH * 4);
        expect(node.getConnectableElement().getY()).toEqual(GRAPH_TOP);

        node = graph.getNodeById("-2");
        expect(node.getNodeType()).toEqual(NodeType.UserTask);
        expect(node.getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH * 5);
        expect(node.getY()).toEqual(GRAPH_TOP);
        expect(node.getConnectableElement().edges[0].getStyle()).toContain(CS_RIGHT);

        node = graph.getNodeById("-3");
        expect(node.getNodeType()).toEqual(NodeType.SystemTask);
        expect(node.getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH * 6);
        expect(node.getY()).toEqual(GRAPH_TOP);
        expect(node.getConnectableElement().edges[0].getStyle()).toContain(CS_RIGHT);

        node = graph.getNodeById("-4");
        expect(node.getNodeType()).toEqual(NodeType.UserTask);
        expect(node.getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH * 5);
        expect(node.getY()).toEqual(GRAPH_TOP + GRAPH_ROW_HEIGHT);
        expect(node.getConnectableElement().edges[0].getStyle()).toContain(CS_VERTICAL);

        node = graph.getNodeById("-5");
        expect(node.getNodeType()).toEqual(NodeType.SystemTask);
        expect(node.getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH * 6);
        expect(node.getY()).toEqual(GRAPH_TOP + GRAPH_ROW_HEIGHT);
        expect(node.getConnectableElement().edges[0].getStyle()).toContain(CS_RIGHT);

        node = graph.getNodeById("30");
        expect(node.getNodeType()).toEqual(NodeType.ProcessEnd);
        expect(node.getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH * 8);
        expect(node.getY()).toEqual(GRAPH_TOP);
    });

    it("Test insert System decision in the middle of the diagram", () => {
        // Arrange
        const testModel = TestModels.createDefaultProcessModel();
        const processModel = setProcessViewModel(testModel);
        let graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

        const link = new ProcessLinkModel(0, 20, 25);
        const diagramLink = new DiagramLink(link, null);

        //bypass testing adding stateful shapes logic here
        spyOn(processModel, "addToSubArtifactCollection").and.returnValue(null);

        // Act
        graph.render(false, null);

        ProcessAddHelper.insertSystemDecision(diagramLink, graph.layout, shapesFactoryService);

        graph.destroy();
        graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(true, null);

        //Assert
        expect(graph.getNodeById("10").getNodeType()).toEqual(NodeType.ProcessStart);
        expect(graph.getNodeById("10").getX()).toEqual(GRAPH_LEFT);
        expect(graph.getNodeById("15").getNodeType()).toEqual(NodeType.SystemTask);
        expect(graph.getNodeById("15").getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH);
        expect(graph.getNodeById("20").getNodeType()).toEqual(NodeType.UserTask);
        expect(graph.getNodeById("20").getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH * 2);
        expect(graph.getNodeById("25").getNodeType()).toEqual(NodeType.SystemTask);
        expect(graph.getNodeById("25").getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH * 4);
        expect(graph.getNodeById("30").getNodeType()).toEqual(NodeType.ProcessEnd);
        expect(graph.getNodeById("30").getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH * 6);
    });

    it("Test rendering with the adding merging point", () => {
        // Arrange
        const testModel = TestModels.createLargeTestModel();
        const processModel = setProcessViewModel(testModel);

        // Act
        let graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.layout.setTempShapeId(0);
        graph.render(true, null);

        //Assert
        expect(graph.getNodeById("-1").getNodeType()).toEqual(NodeType.MergingPoint);
        expect(graph.getNodeById("-1").getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH * 7);
        expect(graph.getNodeById("30").getNodeType()).toEqual(NodeType.ProcessEnd);
        expect(graph.getNodeById("30").getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH * 8);
    });

    it("Test rendering large model with inserting two new user decisions: should not change Y position of branch in the previous subtree", () => {
        // Arrange
        const testModel = TestModels.createLargeTestModel();
        const processModel = setProcessViewModel(testModel);
        let graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.layout.setTempShapeId(0);

        //bypass testing adding stateful shapes logic here
        spyOn(processModel, "addToSubArtifactCollection").and.returnValue(null);

        // Act
        graph.render(true, null);
        ProcessAddHelper.insertUserDecision(graph.getNodeById("27").getConnectableElement().edges[1], graph.layout, shapesFactoryService);

        graph.destroy();
        graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(true, null);

        ProcessAddHelper.insertUserDecision(graph.getNodeById("30").getConnectableElement().edges[0], graph.layout, shapesFactoryService);

        graph.destroy();
        graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(true, null);

        //Assert
        //inserted condition should not change Y positions of branch in previous subtree
        expect(graph.getNodeById("-5").getNodeType()).toEqual(NodeType.UserTask);
        expect(graph.getNodeById("-5").getY()).toEqual(GRAPH_TOP + GRAPH_ROW_HEIGHT);
        expect(graph.getNodeById("37").getNodeType()).toEqual(NodeType.SystemTask);
        expect(graph.getNodeById("37").getY()).toEqual(GRAPH_TOP + GRAPH_ROW_HEIGHT * 2);

        // end process moved 4 columns to the right
        expect(graph.getNodeById("30").getNodeType()).toEqual(NodeType.ProcessEnd);
        expect(graph.getNodeById("30").getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH * 15);
    });

    it("Test rendering large model with inserting two new user decisions: should not change Y position of branch in the next subtree", () => {
        // Arrange
        const testModel = TestModels.createLargeTestModel();
        const processModel = setProcessViewModel(testModel);
        let graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.layout.setTempShapeId(0);

        //bypass testing adding stateful shapes logic here
        spyOn(processModel, "addToSubArtifactCollection").and.returnValue(null);

        // Act
        graph.render(true, null);
        ProcessAddHelper.insertUserDecision(graph.getNodeById("30").getConnectableElement().edges[0], graph.layout, shapesFactoryService);

        graph.destroy();
        graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(true, null);

        ProcessAddHelper.insertUserDecision(graph.getNodeById("27").getConnectableElement().edges[1], graph.layout, shapesFactoryService);

        graph.destroy();
        graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(true, null);

        //Assert
        //inserted condition should not change Y positions of branch in next subtree
        expect(graph.getNodeById("-6").getNodeType()).toEqual(NodeType.SystemTask);
        expect(graph.getNodeById("-6").getY()).toEqual(GRAPH_TOP + GRAPH_ROW_HEIGHT);

        // end process moved 4 columns to the right
        expect(graph.getNodeById("30").getNodeType()).toEqual(NodeType.ProcessEnd);
        expect(graph.getNodeById("30").getX()).toEqual(GRAPH_LEFT + GRAPH_COLUMN_WIDTH * 15);
    });

    it("Gap between precondition and next user task is same as between any other system task and next user task", () => {
        // Arrange
        const testModel = TestModels.createLargeTestModel();
        const processModel = setProcessViewModel(testModel);
        let graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.layout.setTempShapeId(0);

        //bypass testing adding stateful shapes logic here
        spyOn(processModel, "addToSubArtifactCollection").and.returnValue(null);

        // Act
        graph.render(true, null);
        ProcessAddHelper.insertTaskWithUpdate(graph.getNodeById("25").getConnectableElement().edges[1], graph.layout, shapesFactoryService);

        graph.destroy();
        graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(true, null);

        //Assert
        const firstSystemToUserTaskGap = graph.getNodeById("15").geometry.getCenterX() - graph.getNodeById("20").geometry.getCenterX();
        const otherSystemToUserTaskGap = graph.getNodeById("25").geometry.getCenterX() - graph.getNodeById("-2").geometry.getCenterX();

        expect(firstSystemToUserTaskGap).toEqual(otherSystemToUserTaskGap);
    });

    xit("Inserted user task is selected", () => {
        // Arrange
        const testModel = TestModels.createLargeTestModel();
        const processModel = setProcessViewModel(testModel);
        let graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

        const unregProcesssModelUpdate = rootScope.$on("processModelUpdate", (event: any, selectedNodeId: number) => {
            graph.destroy();
            graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            graph.render(true, selectedNodeId);
        });

        //bypass testing adding stateful shapes logic here
        spyOn(processModel, "addToSubArtifactCollection").and.returnValue(null);

        // Act
        graph.render(true, null);
        ProcessAddHelper.insertTaskWithUpdate(graph.getNodeById("25").getConnectableElement().edges[1],
            graph.layout, shapesFactoryService);
        unregProcesssModelUpdate();

        //Assert
        expect(graph.getMxGraph().getSelectionCell().getId()).toEqual("-2");
    });

    xit("Inserted decision point is selected", () => {
        // Arrange
        const testModel = TestModels.createLargeTestModel();
        const processModel = setProcessViewModel(testModel);
        let graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

        const unregProcesssModelUpdate = rootScope.$on("processModelUpdate", (event: any, selectedNodeId: number) => {
            graph.destroy();
            graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            graph.render(true, selectedNodeId);
        });

        //bypass testing adding stateful shapes logic here
        spyOn(processModel, "addToSubArtifactCollection").and.returnValue(null);

        // Act
        graph.render(true, null);
        ProcessAddHelper.insertUserDecision(graph.getNodeById("30").getConnectableElement().edges[0], graph.layout, shapesFactoryService);
        unregProcesssModelUpdate();

        //Assert
        expect(graph.getMxGraph().getSelectionCell().getId()).toEqual("-2");
    });

    it("Insert condition negative test.", () => {
        // Arrange
        const testModel = TestModels.createLargeTestModel();
        const processModel = setProcessViewModel(testModel);
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(false, null);

        //bypass testing adding stateful shapes logic here
        spyOn(processModel, "addToSubArtifactCollection").and.returnValue(null);

        // Act
        //Assert
        expect(() => {
            ProcessAddHelper.insertUserDecision(graph.getNodeById("30"), graph.layout, shapesFactoryService);
        }).toThrowError();
    });

    it("Insert task negative test.", () => {
        // Arrange
        const testModel = TestModels.createLargeTestModel();
        const processModel = setProcessViewModel(testModel);
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(false, null);

        //bypass testing adding stateful shapes logic here
        spyOn(processModel, "addToSubArtifactCollection").and.returnValue(null);

        // Act
        //Assert
        expect(() => {
            ProcessAddHelper.insertTaskWithUpdate(graph.getNodeById("30"), graph.layout, shapesFactoryService);
        }).toThrowError();
    });

    it("Test setSystemTasksVisible method.", () => {
        // Arrange
        const testModel = TestModels.createLargeTestModel();
        const processModel = setProcessViewModel(testModel);
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(false, null);

        // Act
        graph.setSystemTasksVisible(false);

        //Assert
        expect(graph.getNodeById("37").children[1].isVisible()).not.toBeTruthy();
    });

    it("Test auto-layout Default Process", () => {
        // Arrange && Act
        const testModel = TestModels.createDefaultProcessModelWithoutXAndY();
        const processModel = setProcessViewModel(testModel);
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(true, null);

        // Act
        const expectedModel = TestModels.createDefaultProcessModel();

        // Assert
        for (let i in processModel.shapes) {
            expect(processModel.shapes[i].propertyValues["x"].value).toEqual(expectedModel.shapes[i].propertyValues["x"].value);
            expect(processModel.shapes[i].propertyValues["y"].value).toEqual(expectedModel.shapes[i].propertyValues["y"].value);
        }
    });

    it("Test auto-layout simple case", () => {
        // Arrange && Act
        const testModel = TestModels.createSimpleCaseModelWithoutXandY();
        const processModel = setProcessViewModel(testModel);
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(true, null);

        // Act
        const expectedModel = TestModels.createSimpleCaseModelAfterAutoLayout();

        // Assert
        for (let i in processModel.shapes) {
            expect(processModel.shapes[i].propertyValues["x"].value).toEqual(expectedModel.shapes[i].propertyValues["x"].value);
            expect(processModel.shapes[i].propertyValues["y"].value).toEqual(expectedModel.shapes[i].propertyValues["y"].value);
        }
    });

    it("Test auto-layout decision with multiple branches case", () => {
        // Arrange && Act
        const testModel = TestModels.createMultiDecisionBranchModelWithoutXAndY();
        const processModel = setProcessViewModel(testModel);
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(true, null);

        // Act
        const expectedModel = TestModels.createMultiDecisionBranchModel();

        // Assert
        for (let i in processModel.shapes) {
            expect(processModel.shapes[i].propertyValues["x"].value).toEqual(expectedModel.shapes[i].propertyValues["x"].value);
            expect(processModel.shapes[i].propertyValues["y"].value).toEqual(expectedModel.shapes[i].propertyValues["y"].value);
        }
    });

    it("Test auto-layout two merge points case", () => {
        // Arrange && Act
        const testModel = TestModels.createTwoMergePointsModelWithoutXAndY();
        const processModel = setProcessViewModel(testModel);
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(true, null);

        // Act
        const expectedModel = TestModels.createTwoMergePointsModel();

        // Assert
        for (let i in processModel.shapes) {
            expect(processModel.shapes[i].propertyValues["x"].value).toEqual(expectedModel.shapes[i].propertyValues["x"].value);
            expect(processModel.shapes[i].propertyValues["y"].value).toEqual(expectedModel.shapes[i].propertyValues["y"].value);
        }
    });

    it("Test auto-layout multiple merge points with multiple branches case", () => {
        // Arrange && Act
        const testModel = TestModels.createMultipleMergePointsWithMultipleBranchesModelWithoutXAndY();
        const processModel = setProcessViewModel(testModel);
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(true, null);

        // Act
        const expectedModel = TestModels.createMultipleMergePointsWithMultipleBranchesModel();

        // Assert
        for (let i in processModel.shapes) {
            expect(processModel.shapes[i].propertyValues["x"].value).toEqual(expectedModel.shapes[i].propertyValues["x"].value);
            expect(processModel.shapes[i].propertyValues["y"].value).toEqual(expectedModel.shapes[i].propertyValues["y"].value);
        }
    });

    it("Test auto-layout system decision before user decision in a branch", () => {
        // Arrange && Act
        const testModel = TestModels.createSystemDecisionBeforeUserDecisionInBranchModelWithoutXAndY();
        const processModel = setProcessViewModel(testModel);
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(true, null);

        // Act
        const expectedModel = TestModels.createSystemDecisionBeforeUserDecisionInBranchModel();

        // Assert
        for (let i in processModel.shapes) {
            expect(processModel.shapes[i].propertyValues["x"].value).toEqual(expectedModel.shapes[i].propertyValues["x"].value);
            expect(processModel.shapes[i].propertyValues["y"].value).toEqual(expectedModel.shapes[i].propertyValues["y"].value);
        }
    });

    describe("Test handleUserTaskDragDrop method", () => {

        it("with system task as the next shape.", () => {
            // Arrange
            const testModel = TestModels.createLargeTestModel();
            const processModel = setProcessViewModel(testModel);

            let graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            let modelUpdateHandler = graph.viewModel.communicationManager.processDiagramCommunication.registerModelUpdateObserver((selectedNodeId: number) => {
                graph.destroy();
                graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
                graph.render(true, null);
            });

            // Act
            graph.render(true, null);
            graph.layout.handleUserTaskDragDrop(26, graph.getNodeById("37").getConnectableElement().edges[0]);
            graph.viewModel.communicationManager.processDiagramCommunication.removeModelUpdateObserver(modelUpdateHandler);

            //Assert
            // System task ('37') connected to the moved user task ('26')
            expect((<IDiagramNode>(graph.getNodeById("37").getConnectableElement().edges[0].target)).getId()).toEqual("26");
        });

        it("system decision as the next shape.", () => {
            // Arrange
            const testModel = TestModels.createSystemDecisionBeforeUserDecisionInBranchModel();
            const processModel = setProcessViewModel(testModel);
            let graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            graph.layout.setTempShapeId(0);
            let modelUpdateHandler = graph.viewModel.communicationManager.processDiagramCommunication.registerModelUpdateObserver((selectedNodeId: number) => {
                graph.destroy();
                graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
                graph.render(true, null);
            });

            // Act
            graph.render(true, null);
            ProcessAddHelper.insertTaskWithUpdate(graph.getNodeById("80").getConnectableElement().edges[0],
                graph.layout, shapesFactoryService);
            graph.layout.handleUserTaskDragDrop(20, graph.getNodeById("80").getConnectableElement().edges[0]);
            graph.viewModel.communicationManager.processDiagramCommunication.removeModelUpdateObserver(modelUpdateHandler);

            //Assert
            // 1. new user task + system task created
            expect(graph.getNodeById("-3").getNodeType()).toEqual(NodeType.UserTask);
            expect(graph.getNodeById("-4").getNodeType()).toEqual(NodeType.SystemTask);
            // 2. Sytem task ('-4') connected to the moved user task ('20')
            expect((<IDiagramNode>(graph.getNodeById("-4").getConnectableElement().edges[0].target)).getId()).toEqual("20");
        });

        it("drag and drop causing loop, success", () => {
            /*
             start -> PRE -> UT1 -> SD ->  ST2 -> UT4 -> ST4 -> END
             ->  ST3 -> UT4
             */
            // Arrange
            const testModel = TestModels.createSystemDecisionForDnDTestModel();
            const processModel = setProcessViewModel(testModel);
            const ut1Id = 20;
            const ut4Id = 40;

            const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

            // Act
            graph.render(false, null);
            const st4ToEnd = graph.getNodeById("45").getOutgoingLinks(graph.getMxGraphModel())[0];
            graph.layout.handleUserTaskDragDrop(ut1Id, st4ToEnd);

            // Assert
            processModel.updateTree();
            const decisionDestinationBranch = processModel.decisionBranchDestinationLinks[0];
            const utDragPrevId = processModel.getPrevShapeIds(ut1Id)[0];
            const prevNodeCountUt4 = processModel.getPrevShapeIds(ut4Id).length;

            expect(utDragPrevId).toBe(45); // source to be ST4 now
            expect(prevNodeCountUt4).toBe(2); // still 2 nodes incoming to UT4
            expect(decisionDestinationBranch.destinationId).toBe(ut4Id); // destination branch to be UT4 still
        });

        describe("complicated DnD loop cases", () => {
            let graph: ProcessGraph, processModel: ProcessViewModel;
            const preId = 15;
            const ut1Id = 20;
            const ut2Id = 30;
            const st2AId = 40;
            const st2BId = 45;
            const st4Id = 60;
            const st5Id = 70;
            const ut6Id = 75;

            beforeEach(() => {
                // Arrange
                const testModel = TestModels.createDnDComplicatedModel();
                processModel = setProcessViewModel(testModel);
                graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
                /*
                 start -> pre -> ut1 -> st1 -> ut2 -> sd2 -> st2A ---------> ud3 -> ut4 -> st4 -> ut6 -> st6 -> end
                 -> st2B -> ut1         -> ut5 -> st5 -> ut1                        
                 */
            });

            afterEach(() => {
                processModel = null;
                graph = null;
            });

            it("multiple successive drag and drop, success", () => {
                // Act
                graph.render(true, null);

                let mergeToUt1 = graph.getNodeById(ut1Id.toString()).getIncomingLinks(graph.getMxGraphModel())[0];
                graph.layout.handleUserTaskDragDrop(ut2Id, mergeToUt1);
                graph.destroy();
                graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
                graph.render(true, null);

                let st4ToUt6 = graph.getNodeById(ut6Id.toString()).getIncomingLinks(graph.getMxGraphModel())[0];
                graph.layout.handleUserTaskDragDrop(ut2Id, st4ToUt6);
                processModel.updateTree();

                // Assert
                processModel.decisionBranchDestinationLinks.forEach((link) => {
                    expect(link.destinationId).toBe(ut1Id);
                });

                expect(processModel.getPrevShapeIds(ut1Id).length).toBe(3);
                expect(processModel.getPrevShapeIds(ut1Id).indexOf(preId)).toBeGreaterThan(-1);
                expect(processModel.getPrevShapeIds(ut1Id).indexOf(st2BId)).toBeGreaterThan(-1);
                expect(processModel.getPrevShapeIds(ut1Id).indexOf(st5Id)).toBeGreaterThan(-1);
                expect(processModel.getPrevShapeIds(ut2Id).length).toBe(1);
                expect(processModel.getPrevShapeIds(ut2Id).indexOf(st4Id)).toBeGreaterThan(-1);
            });
            it("drop target where destination is same as child systemTask in condition, success", () => {
                // Act
                graph.render(true, null);

                let st5ToUt1 = graph.getNodeById(st5Id.toString()).getOutgoingLinks(graph.getMxGraphModel())[0];
                graph.layout.handleUserTaskDragDrop(ut2Id, st5ToUt1);

                processModel.updateTree();

                // Assert
                processModel.decisionBranchDestinationLinks.forEach((link) => {
                    expect(link.destinationId).toBe(ut1Id);
                });

                expect(processModel.getPrevShapeIds(ut1Id).length).toBe(3);
                expect(processModel.getPrevShapeIds(ut1Id).indexOf(preId)).toBeGreaterThan(-1);
                expect(processModel.getPrevShapeIds(ut1Id).indexOf(st2BId)).toBeGreaterThan(-1);
                expect(processModel.getPrevShapeIds(ut1Id).indexOf(st2AId)).toBeGreaterThan(-1);
                expect(processModel.getPrevShapeIds(ut2Id).length).toBe(1);
                expect(processModel.getPrevShapeIds(ut2Id).indexOf(st5Id)).toBeGreaterThan(-1);
            });

            it("drop target where multiple sources contains child system task (from system condition), success", () => {
                // Act
                graph.render(true, null);

                let mergeToUt1 = graph.getNodeById(ut1Id.toString()).getIncomingLinks(graph.getMxGraphModel())[0];
                graph.layout.handleUserTaskDragDrop(ut2Id, mergeToUt1);

                processModel.updateTree();

                // Assert
                processModel.decisionBranchDestinationLinks.forEach((link) => {
                    expect(link.destinationId).toBe(ut2Id);
                });

                expect(processModel.getPrevShapeIds(ut2Id).length).toBe(3);
                expect(processModel.getPrevShapeIds(ut2Id).indexOf(preId)).toBeGreaterThan(-1);
                expect(processModel.getPrevShapeIds(ut2Id).indexOf(st2BId)).toBeGreaterThan(-1);
                expect(processModel.getPrevShapeIds(ut2Id).indexOf(st5Id)).toBeGreaterThan(-1);
            });
        });

        it("infinite loop, drag outside, success", () => {
            // Arrange
            let testModel = TestModels.createUserDecisionInfiniteLoopModel();
            // Start -> Pre -> UD -> UT1 -> ST1 -> End
            //                       UT2 -> ST2 -> UT3 -> ST3 -> UT5
            //                       UT4 -> ST4 -> UT5 -> ST5 -> UT3
            let UT3 = 80;
            let ST1 = 50;
            let processModel = setProcessViewModel(testModel);
            let graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            graph.render(false, null);
            let st1ToEnd = graph.getNodeById(ST1.toString()).getOutgoingLinks(graph.getMxGraphModel())[0];

            //Act
            graph.layout.handleUserTaskDragDrop(UT3, st1ToEnd);

            // Assert

            processModel.updateTree();
            let errorMessages: string[] = [];
            let validator = new ProcessValidator();
            validator.isValid(processModel, rootScope, errorMessages);

            expect(errorMessages.length).toBe(0);
            expect(processModel.getNextShapeIds(ST1).length).toBe(1);
            expect(processModel.getNextShapeIds(ST1)[0]).toBe(UT3);
        });

        //Bug 1086
        it("infinite loop, drag loop task, incoming edge before merge point same condition, success", () => {
            // Arrange
            let testModel = TestModels.createUserDecisionInfiniteLoopModel();
            // Start -> Pre -> UD -> UT1 -> ST1 -> End
            //                       UT2 -> ST2 -> UT3 -> ST3 -> UT5
            //                       UT4 -> ST4 -> UT5 -> ST5 -> UT3
            let UT3 = 80;
            let ST2 = 70;
            let processModel = setProcessViewModel(testModel);
            let graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            graph.render(false, null);
            let st2ToMergeUt3 = graph.getNodeById(ST2.toString()).getOutgoingLinks(graph.getMxGraphModel())[0];

            //Act
            graph.layout.handleUserTaskDragDrop(UT3, st2ToMergeUt3);

            // Assert
            processModel.updateTree();
            let errorMessages: string[] = [];
            let validator = new ProcessValidator();
            validator.isValid(processModel, rootScope, errorMessages);

            expect(errorMessages.length).toBe(0);
            expect(processModel.getNextShapeIds(ST2).length).toBe(1);
            expect(processModel.getNextShapeIds(ST2)[0]).toBe(UT3);
        });

        //Bug 1086
        it("infinite loop, drag loop task, incoming edge before merge point different condition, success", () => {
            // Arrange
            let testModel = TestModels.createUserDecisionInfiniteLoopModel();
            // Start -> Pre -> UD -> UT1 -> ST1 -> End
            //                       UT2 -> ST2 -> UT3 -> ST3 -> UT5
            //                       UT4 -> ST4 -> UT5 -> ST5 -> UT3
            let UT3 = 80;
            let ST5 = 130;
            let processModel = setProcessViewModel(testModel);
            let graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            graph.render(false, null);
            let st5ToMergeUt3 = graph.getNodeById(ST5.toString()).getOutgoingLinks(graph.getMxGraphModel())[0];

            //Act
            graph.layout.handleUserTaskDragDrop(UT3, st5ToMergeUt3);

            // Assert
            processModel.updateTree();
            let errorMessages: string[] = [];
            let validator = new ProcessValidator();
            validator.isValid(processModel, rootScope, errorMessages);

            expect(errorMessages.length).toBe(0);
            expect(processModel.getNextShapeIds(ST5).length).toBe(1);
            expect(processModel.getNextShapeIds(ST5)[0]).toBe(UT3);
        });

        it("With simple system decision family, drag to edge right after family's merge point, success", () => {
            //Arrange
            let testModel = TestModels.createUserDecisionWithUserTaskWithSimpleSystemDecisioFamily();

            let processModel = setProcessViewModel(testModel);
            let graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            graph.render(true, null);
            let ut2Id = 60;
            let endId = 120;
            let mergeNodeToEnd = graph.getNodeById(endId.toString()).getIncomingLinks(graph.getMxGraphModel())[0];

            //Act
            graph.layout.handleUserTaskDragDrop(ut2Id, mergeNodeToEnd);

            // Assert
            processModel.updateTree();
            let errorMessages: string[] = [];
            let validator = new ProcessValidator();
            validator.isValid(processModel, rootScope, errorMessages);

            expect(errorMessages.length).toBe(0);
            expect(processModel.getPrevShapeIds(endId).length).toBe(1);
        });
    });

    describe("isValidForDrop method", () => {
        it("returns correct results for large model", () => {
            // Arrange
            const testModel = TestModels.createLargeTestModel();
            const processModel = setProcessViewModel(testModel);
            const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

            // Act
            graph.render(true, null);

            //Assert
            expect(graph.layout.isValidForDrop(26, graph.getNodeById("37").getConnectableElement().edges[0])).toBeTruthy();
            expect(graph.layout.isValidForDrop(26, graph.getNodeById("15").getConnectableElement().edges[1])).toBeTruthy();
            expect(graph.layout.isValidForDrop(26, graph.getNodeById("26").getConnectableElement().edges[0])).toBeFalsy();
            expect(graph.layout.isValidForDrop(26, graph.getNodeById("27").getConnectableElement().edges[1])).toBeFalsy();
            expect(graph.layout.isValidForDrop(26, graph.getNodeById("27").getConnectableElement().edges[1])).toBeFalsy();
            expect(graph.layout.isValidForDrop(26, graph.getNodeById("30").getConnectableElement().edges[0])).toBeTruthy();
        });

        it("returns false for system decision loop", () => {
            // Arrange
            const process = TestModels.createSystemDecisionLoopModel();
            const viewModel = setProcessViewModel(process);
            const graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization, shapesFactory, null, null, null);
            graph.render(true, null);

            const userTaskId = 30;
            const systemTask2Id = 60;
            const dropEdge = graph.getNodeById(systemTask2Id.toString()).getConnectableElement().edges[0];

            // Act
            const result = graph.layout.isValidForDrop(userTaskId, dropEdge);

            // Assert
            expect(result).toBe(false);
        });
    });

    it("Test getDropEdgeState method.", () => {
        // Arrange
        const testModel = TestModels.createLargeTestModel();
        const processModel = setProcessViewModel(testModel);
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        const p1: MxPoint = new mxPoint(607, 388);
        const p2: MxPoint = new mxPoint(520, 420);

        // Act
        graph.render(true, null);

        //Assert
        expect(graph.layout.getDropEdgeState(p1)).not.toBeNull();
        expect(graph.layout.getDropEdgeState(p2)).toBeNull();
    });

    it("Test end arrows appearance in the diagram edges", () => {
        // Arrange && Act
        const testModel = TestModels.createSystemDecisionBeforeUserDecisionInBranchModelWithoutXAndY();
        const processModel = setProcessViewModel(testModel);
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

        // Act
        graph.render(true, null);

        // Assert
        expect(graph.getNodeById("10").getConnectableElement().edges[0].getStyle()).toContain("endArrow=none");
        expect(graph.getNodeById("15").getConnectableElement().edges[1].getStyle()).toContain("endArrow=none");
        expect(graph.getNodeById("20").getConnectableElement().edges[1].getStyle()).toContain("endArrow=none");
        expect(graph.getNodeById("30").getConnectableElement().edges[1].getStyle()).toContain("endArrow=open");
        expect(graph.getNodeById("40").getConnectableElement().edges[1].getStyle()).toContain("endArrow=none");
        expect(graph.getNodeById("40").getConnectableElement().edges[2].getStyle()).toContain("endArrow=none");
        expect(graph.getNodeById("45").getConnectableElement().edges[1].getStyle()).toContain("endArrow=open");
        expect(graph.getNodeById("50").getConnectableElement().edges[1].getStyle()).toContain("endArrow=open");
        expect(graph.getNodeById("55").getConnectableElement().edges[1].getStyle()).toContain("endArrow=open");
        expect(graph.getNodeById("55").getConnectableElement().edges[2].getStyle()).toContain("endArrow=open");
    });

    it("Test loop connector appears correctly", () => {
        // Arrange
        const process = TestModels.createUserDecisionLoopModelWithoutXAndY();
        const model = setProcessViewModel(process);
        const graph = new ProcessGraph(rootScope, localScope, container, model, dialogService, localization, shapesFactory, null, null, null);

        // Act
        graph.render(true, null);

        // Assert
        expect(graph.getNodeById("90").getConnectableElement().edges[0].getStyle()).toContain("edgeStyle=" + CS_LEFT);
    });

    describe("Test insert branches", () => {
        describe("insert user decision branch", () => {
            it("insert user decision branch, destination is correct", () => {
                const testModel = TestModels.createUserDecisionForAddBranchTestModel();
                const processModel = setProcessViewModel(testModel);
                const ut4Id = 50;
                const udId = 25;
                const endId = 60;

                //bypass testing adding stateful shapes logic here,cannot extract this spyOn logic out since
                //test models used in each test are different and only generated in each test
                spyOn(processModel, "addToSubArtifactCollection").and.returnValue(null);

                const processGraph = new ProcessGraph(
                    rootScope, localScope, container, processModel, dialogService, 
                    localization, shapesFactory, null, null, null
                );
                processGraph.render(true, false);

                const ud: IDecision = processGraph.getMxGraphModel().getCell(udId.toString());

                ProcessAddHelper.insertUserDecisionCondition(ud.model.id, processGraph.layout, shapesFactoryService);

                const conditionDestinations = processModel.getBranchDestinationIds(ud.model.id);

                expect(conditionDestinations.length).toBe(2);
                expect(conditionDestinations[0]).toBe(endId);
                expect(conditionDestinations[1]).toBe(ut4Id);
            });

            it("succeeds if no user task exist in first condition", () => {
                // Arrange
                const process = TestModels.createUserDecisionWithoutUserTaskInFirstConditionModel();
                const processModel = setProcessViewModel(process);
                const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
                const decisionId = 40;
                const expectedConditions = 3;

                //bypass testing adding stateful shapes logic here
                spyOn(processModel, "addToSubArtifactCollection").and.returnValue(null);

                // Act
                ProcessAddHelper.insertUserDecisionCondition(decisionId, graph.layout, shapesFactoryService);

                // Assert
                expect(process.links.filter(link => link.sourceId === decisionId).length).toBe(expectedConditions);
            });
        });

        describe("insert system decision branch", () => {
            it("simple, destination is correct", () => {
                const testModel = TestModels.createSystemDecisionForAddBranchTestModel();
                const processModel = setProcessViewModel(testModel);
                const ut4Id = 40;
                const sdId = 25;
                const endId = 50;

                //bypass testing adding stateful shapes logic here
                spyOn(processModel, "addToSubArtifactCollection").and.returnValue(null);

                const processGraph = new ProcessGraph(
                    rootScope, localScope, container, processModel, dialogService, 
                    localization, shapesFactory, null, null, null
                );
                processGraph.render(true, false);

                ProcessAddHelper.insertSystemDecisionCondition(sdId, processGraph.layout, shapesFactoryService);

                const conditionDestinations = processModel.getBranchDestinationIds(sdId);

                expect(conditionDestinations.length).toBe(2);
                expect(conditionDestinations[0]).toBe(endId);
                expect(conditionDestinations[1]).toBe(ut4Id);
            });

            it("back to back system decisions different end points, first system decision destination is correct", () => {
                const testModel = TestModels.createBackToBackSystemDecisionWithLoopTestModel();
                const processModel = setProcessViewModel(testModel);
                const ut1Id = 20;
                const sd1Id = 25;
                const ut4Id = 50;

                //bypass testing adding stateful shapes logic here
                spyOn(processModel, "addToSubArtifactCollection").and.returnValue(null);

                const processGraph = new ProcessGraph(
                    rootScope, localScope, container, processModel, dialogService, 
                    localization, shapesFactory, null, null, null
                );
                processGraph.render(true, false);

                ProcessAddHelper.insertSystemDecisionCondition(sd1Id, processGraph.layout, shapesFactoryService);

                const conditionDestinations = processModel.getBranchDestinationIds(sd1Id);

                expect(conditionDestinations.length).toBe(2);
                expect(conditionDestinations[0]).toBe(ut1Id);
                expect(conditionDestinations[1]).toBe(ut4Id);
            });

            it("back to back system decisions different end points, second system decision destination is correct", () => {
                const testModel = TestModels.createBackToBackSystemDecisionWithLoopTestModel();
                const processModel = setProcessViewModel(testModel);
                const sd2Id = 35;
                const ut4Id = 50;
                const endId = 60;

                //bypass testing adding stateful shapes logic here
                spyOn(processModel, "addToSubArtifactCollection").and.returnValue(null);

                const processGraph = new ProcessGraph(
                    rootScope, localScope, container, processModel, dialogService, 
                    localization, shapesFactory, null, null, null
                );
                processGraph.render(true, false);

                ProcessAddHelper.insertSystemDecisionCondition(sd2Id, processGraph.layout, shapesFactoryService);

                const conditionDestinations = processModel.getBranchDestinationIds(sd2Id);

                expect(conditionDestinations.length).toBe(2);
                expect(conditionDestinations[0]).toBe(endId);
                expect(conditionDestinations[1]).toBe(ut4Id);
            });
        });
    });

    describe("Test shape limit", () => {
        it("Should not insert additional shapes when limit is reached", () => {
            // Arrange
            const testModel = TestModels.createDefaultProcessModel();
            const processModel = new ProcessViewModel(testModel, communicationManager, rootScope, localScope, msgService);
            let graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

            //bypass testing adding stateful shapes logic here
            spyOn(processModel, "addToSubArtifactCollection").and.returnValue(null);

            // Act
            graph.render(false, null);
            const edge = graph.getNodeById("15").getConnectableElement().edges[1];
            // the limit is 5 shapes
            graph.viewModel.shapeLimit = 5;
            const spyInsertTask = spyOn(ProcessAddHelper, "insertTask");
            ProcessAddHelper.insertTaskWithUpdate(edge, graph.layout, shapesFactoryService);

            graph.destroy();
            graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            graph.render(true, null);

            //Assert
            const msg = msgService.messages;
            expect(spyInsertTask).not.toHaveBeenCalled();
            expect(msg[0].messageText).toBe("The Process will exceed the maximum 5 shapes");
        });

        it("Should show a warning when eighty percent of the limit is reached", () => {
            // Arrange
            const testModel = TestModels.createDefaultProcessModel();
            const processModel = new ProcessViewModel(testModel, communicationManager, rootScope, localScope, msgService);
            let graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

            //bypass testing adding stateful shapes logic here
            spyOn(processModel, "addToSubArtifactCollection").and.returnValue(null);

            // Act
            graph.render(false, null);
            const edge = graph.getNodeById("15").getConnectableElement().edges[1];
            // we start out with 5 shapes
            // the limit is 9 shapes
            // we will add two extra shapes to trigger warning
            graph.viewModel.shapeLimit = 9;
            const spyInsertTask = spyOn(ProcessAddHelper, "insertTask");
            ProcessAddHelper.insertTaskWithUpdate(edge, graph.layout, shapesFactoryService);

            graph.destroy();
            graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            graph.render(true, null);

            //Assert
            const msg = msgService.messages;
            expect(spyInsertTask).toHaveBeenCalled();
            expect(msg[0].messageText).toBe("The Process now has 7 of the maximum 9 shapes");
        });
    });
});
