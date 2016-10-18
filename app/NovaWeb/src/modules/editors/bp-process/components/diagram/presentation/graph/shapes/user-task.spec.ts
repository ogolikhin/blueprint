import * as angular from "angular";
import {UserTask, SystemTask, SystemDecision} from "./";
import {ShapesFactory} from "./shapes-factory";
import {ProcessGraph} from "../process-graph";
import {ProcessModel, ProcessShapeModel, ProcessLinkModel} from "../../../../../models/process-models";
import {ProcessShapeType, ProcessType} from "../../../../../models/enums";
import {createSystemDecisionForAddBranchTestModel} from "../../../../../models/test-model-factory";
import {ProcessViewModel, IProcessViewModel} from "../../../viewmodel/process-viewmodel";
import {ShapeModelMock, ArtifactReferenceLinkMock} from "./shape-model.mock";
import {DiagramNodeElement} from "./diagram-element";
import {NodeType, ElementType} from "../models/";
import {ISystemTask, IUserTask, IDiagramNode} from "../models/";
import {MessageServiceMock} from "../../../../../../../core/messages/message.mock";
import {IMessageService} from "../../../../../../../core/messages/message.svc";
import {ICommunicationManager, CommunicationManager} from "../../../../../../bp-process";
import {LocalizationServiceMock} from "../../../../../../../core/localization/localization.mock";
import {DialogService} from "../../../../../../../shared/widgets/bp-dialog";
import {ModalServiceMock} from "../../../../../../../shell/login/mocks.spec";
import {IStatefulArtifactFactory} from "../../../../../../../managers/artifact-manager/";
import {StatefulArtifactFactoryMock} from "../../../../../../../managers/artifact-manager/artifact/artifact.factory.mock";

describe("UserTask test", () => {

    let LABEL_EDIT_MAXLENGTH = 40;
    let PERSONA_EDIT_MAXLENGTH = 40;
    let LABEL_VIEW_MAXLENGTH = 40;
    let PERSONA_VIEW_MAXLENGTH = 16;
    //let graph: ProcessGraph;
    let localScope, rootScope, shapesFactory, wrapper, container;
    let viewModel: ProcessViewModel;
    let msgService: IMessageService;
    let communicationManager: ICommunicationManager,
        dialogService: DialogService,
        localization: LocalizationServiceMock;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("messageService", MessageServiceMock);
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("dialogService", DialogService);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
    }));

    beforeEach(inject((_$window_: ng.IWindowService,
                       $rootScope: ng.IRootScopeService,
                       messageService: IMessageService,
                       _communicationManager_: ICommunicationManager,
                       _dialogService_: DialogService,
                       _localization_: LocalizationServiceMock,
                       statefulArtifactFactory: IStatefulArtifactFactory) => {
        rootScope = $rootScope;
        communicationManager = _communicationManager_;
        dialogService = _dialogService_;
        localization = _localization_;
        wrapper = document.createElement("DIV");
        container = document.createElement("DIV");
        wrapper.appendChild(container);
        document.body.appendChild(wrapper);
        msgService = messageService;

        $rootScope["config"] = {};
        $rootScope["config"].labels = {
            "ST_Persona_Label": "Persona",
            "ST_Colors_Label": "Color",
            "ST_Comments_Label": "Comments"
        };
        shapesFactory = new ShapesFactory($rootScope, statefulArtifactFactory);
        localScope = {graphContainer: container, graphWrapper: wrapper, isSpa: false};


        let processModel = new ProcessModel();
        viewModel = new ProcessViewModel(processModel, communicationManager);
        viewModel.isReadonly = false;
    }));

    it("Test UserTask class", () => {
        // Arrange
        let testUserTask = ShapeModelMock.instance().UserTaskMock();
        let testArtifactReferenceLink = new ArtifactReferenceLinkMock(1);
        testUserTask.propertyValues["label"] = {
            propertyName: "label", value: "", typeId: 0, typePredefined: 0
        };
        testUserTask.propertyValues["clientType"] = {
            propertyName: "clientType", value: NodeType.UserTask.toString(), typeId: 1, typePredefined: 0
        };
        testUserTask.propertyValues["persona"] = {
            propertyName: "persona", value: "Persona", typeId: 2, typePredefined: 0
        };
        testUserTask.propertyValues["description"] = {
            propertyName: "description", value: "Description", typeId: 3, typePredefined: 0
        };
        testUserTask.propertyValues["objective"] = {
            propertyName: "objective", value: "Objective", typeId: 4, typePredefined: 0
        };
        testUserTask.propertyValues["associatedArtifact"] = {
            propertyName: "associatedArtifact", value: new ArtifactReferenceLinkMock(2), typeId: 5, typePredefined: 0
        };
        testUserTask.propertyValues["storyLinks"] = shapesFactory.createStoryLinksValue(testArtifactReferenceLink);

        // Act
        let graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization);

        let node = new UserTask(testUserTask, rootScope, null, shapesFactory);
        node.render(graph, 80, 80, false);
        node.renderLabels();

        node.label = "test label";
        node.persona = "test persona";
        node.description = "test description";
        node.objective = "test objective";
        node.associatedArtifact = testArtifactReferenceLink;

        //Assert
        expect(graph.getNodeById("30").getNodeType()).toEqual(NodeType.UserTask);
        expect(node.userStoryId).toEqual(1);
        expect(node.label).toEqual("test label");
        expect(node.persona).toEqual("test persona");
        expect(node.description).toEqual("test description");
        expect(node.objective).toEqual("test objective");
        expect(node.associatedArtifact).toEqual(testArtifactReferenceLink);
    });

    it("Test getSourceSystemTasks", () => {
        // Arrange
        let testSytemTask = ShapeModelMock.instance().SystemTaskMock();
        let testUserTask = ShapeModelMock.instance().UserTaskMock();

        let graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization);
        let node = new UserTask(testUserTask, rootScope, null, shapesFactory);
        spyOn(node, "getSources").and.returnValue([new SystemTask(testSytemTask, rootScope, "", null, shapesFactory)]);

        // Act
        let systemTasks: ISystemTask[] = node.getPreviousSystemTasks(graph);

        //Assert
        expect(systemTasks.length).toEqual(1);
    });

    it("Test getNextSystemTasks simple case", () => {
        // Arrange
        let testSytemTask = ShapeModelMock.instance().SystemTaskMock();
        let testUserTask = ShapeModelMock.instance().UserTaskMock();

        let graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization);
        let node = new UserTask(testUserTask, rootScope, null, shapesFactory);
        spyOn(node, "getTargets").and.returnValue([new SystemTask(testSytemTask, rootScope, "", null, shapesFactory)]);

        // Act
        let systemTasks: ISystemTask[] = node.getNextSystemTasks(graph);

        //Assert
        expect(systemTasks.length).toEqual(1);
    });

    it("Test getNextSystemTasks next is a system decision", () => {
        // Arrange
        let testSytemTask = ShapeModelMock.instance().SystemTaskMock();
        let testUserTask = ShapeModelMock.instance().UserTaskMock();
        let testSystemDecision = ShapeModelMock.instance().SystemDecisionmock();

        let graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization);
        let UserTaskNode = new UserTask(testUserTask, rootScope, null, shapesFactory);
        let SystemDecisionNode = new SystemDecision(testSystemDecision, rootScope);
        spyOn(UserTaskNode, "getTargets").and.returnValue([SystemDecisionNode]);
        spyOn(SystemDecisionNode, "getTargets").and.returnValue(
            [
                new SystemTask(testSytemTask, rootScope, "", null, shapesFactory),
                new SystemTask(testSytemTask, rootScope, "", null, shapesFactory)
            ]
        );

        // Act
        let systemTasks: ISystemTask[] = UserTaskNode.getNextSystemTasks(graph);

        //Assert
        expect(systemTasks.length).toEqual(2);
    });

    it("Test getNextSystemTasks next is a system decision and another system decision is on the branch", () => {
        // Arrange
        let testSytemTask = ShapeModelMock.instance().SystemTaskMock();
        let testUserTask = ShapeModelMock.instance().UserTaskMock();
        let testSystemDecision = ShapeModelMock.instance().SystemDecisionmock();

        let graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization);
        let UserTaskNode = new UserTask(testUserTask, rootScope, null, shapesFactory);
        let SystemDecisionNode1 = new SystemDecision(testSystemDecision, rootScope);
        let SystemDecisionNode2 = new SystemDecision(testSystemDecision, rootScope);
        spyOn(UserTaskNode, "getTargets").and.returnValue([SystemDecisionNode1]);
        spyOn(SystemDecisionNode1, "getTargets").and.returnValue(
            [
                SystemDecisionNode2,
                new SystemTask(testSytemTask, rootScope, "", null, shapesFactory)
            ]
        );
        spyOn(SystemDecisionNode2, "getTargets").and.returnValue(
            [
                new SystemTask(testSytemTask, rootScope, "", null, shapesFactory),
                new SystemTask(testSytemTask, rootScope, "", null, shapesFactory)
            ]
        );

        // Act
        let systemTasks: ISystemTask[] = UserTaskNode.getNextSystemTasks(graph);

        //Assert
        expect(systemTasks.length).toEqual(3);
    });

    it("Test setUserStoryId", () => {
        // Arrange
        let testUserTask = ShapeModelMock.instance().UserTaskMock();

        let node = new UserTask(testUserTask, rootScope, null, shapesFactory);


        // Act
        node.userStoryId = 3;

        //Assert
        expect(node.userStoryId).toEqual(3);
    });

    it("Test setUserStoryId", () => {
        // Arrange
        let testUserTask = ShapeModelMock.instance().UserTaskMock();

        let node = new UserTask(testUserTask, rootScope, null, shapesFactory);

        // Act
        node.userStoryId = 99;

        //Assert
        expect(node.userStoryId).toEqual(99);
    });

    describe("Test text elements", () => {

        it("Test formatElementText - label overflow", () => {
            // Arrange
            let testUserTask = ShapeModelMock.instance().UserTaskMock();

            let node = new UserTask(testUserTask, rootScope, null, shapesFactory);
            let textInput = "0123456789,0123456789,0123456789,0123456789";
            let expectedText = textInput.substr(0, LABEL_VIEW_MAXLENGTH) + " ...";
            // Act
            let actualText = node.formatElementText(node, textInput);

            //Assert
            expect(actualText).toEqual(expectedText);
        });

        it("Test formatElementText - persona overflow", () => {
            // Arrange
            let testUserTask = ShapeModelMock.instance().UserTaskMock();

            let node = new UserTask(testUserTask, rootScope, null, shapesFactory);
            let editNode = new DiagramNodeElement("H1", ElementType.UserTaskHeader, "", new mxGeometry(), "");

            let textInput = "01234567890123456789";
            let expectedText = textInput.substr(0, PERSONA_VIEW_MAXLENGTH) + " ...";
            // Act
            let actualText = node.formatElementText(editNode, textInput);

            //Assert
            expect(actualText).toEqual(expectedText);
        });
        it("Test getElementTextLength - label", () => {
            // Arrange
            let testUserTask = ShapeModelMock.instance().UserTaskMock();

            let node = new UserTask(testUserTask, rootScope, null, shapesFactory);

            // Act
            let textLength = node.getElementTextLength(node);

            //Assert
            expect(textLength).toEqual(LABEL_EDIT_MAXLENGTH);
        });

        it("Test getElementTextLength - persona", () => {
            // Arrange
            let testUserTask = ShapeModelMock.instance().UserTaskMock();

            let node = new UserTask(testUserTask, rootScope, null, shapesFactory);
            let editNode = new DiagramNodeElement("H1", ElementType.UserTaskHeader, "", new mxGeometry(), "");

            // Act
            let textLength = node.getElementTextLength(editNode);

            //Assert
            expect(textLength).toEqual(PERSONA_EDIT_MAXLENGTH);
        });

        it("Test setElementText - label", () => {
            // Arrange
            let testUserTask = ShapeModelMock.instance().UserTaskMock();

            let node = new UserTask(testUserTask, rootScope, null, shapesFactory);

            let testLabelText = "test label";


            // Act
            node.setElementText(node, testLabelText);

            //Assert
            expect(node.label).toEqual(testLabelText);
        });

        it("Test setElementText - persona", () => {
            // Arrange
            let testUserTask = ShapeModelMock.instance().UserTaskMock();

            let node = new UserTask(testUserTask, rootScope, null, shapesFactory);
            
            let editNode = new DiagramNodeElement("H1", ElementType.UserTaskHeader, "", new mxGeometry(), "");

            let testLabelText = "test label";

            // Act
            node.setElementText(editNode, testLabelText);

            //Assert
            expect(node.persona).toEqual(testLabelText);
        });

        it("Test latest persona value reuse", () => {
            // Arrange
            let testUserTask = ShapeModelMock.instance().UserTaskMock();

            let node = new UserTask(testUserTask, rootScope, null, shapesFactory);
            node.persona = "12345";

            // Act
            let node1 = new UserTask(testUserTask, rootScope, null, shapesFactory);

            //Assert
            expect(node1.persona).toEqual(node.persona);
        });
    });

    describe("when using default process", () => {
        let testModel;
        let processModel: IProcessViewModel;
        let graph: ProcessGraph;

        beforeEach(() => {

            let startModel = new ProcessShapeModel(11);
            startModel.propertyValues = shapesFactory.createPropertyValuesForSystemTaskShape();
            startModel.propertyValues["clientType"].value = ProcessShapeType.Start;
            startModel.propertyValues["x"].value = 0;
            let preconditionModel = new ProcessShapeModel(22);
            preconditionModel.propertyValues = shapesFactory.createPropertyValuesForSystemTaskShape();
            preconditionModel.propertyValues["clientType"].value = ProcessShapeType.PreconditionSystemTask;
            preconditionModel.propertyValues["x"].value = 1;
            let userTask = shapesFactory.createModelUserTaskShape(2, 1, 33, 2, 0);
            let systemTask = shapesFactory.createModelSystemTaskShape(2, 1, 44, 3, 0);
            let endModel = new ProcessShapeModel(55);
            endModel.propertyValues = shapesFactory.createPropertyValuesForSystemTaskShape();
            endModel.propertyValues["clientType"].value = ProcessShapeType.Start;
            endModel.propertyValues["x"].value = 4;

            testModel = new ProcessModel();
            testModel.propertyValues = {};
            testModel.propertyValues["clientType"] = shapesFactory.createClientTypeValueForProcess(ProcessType.UserToSystemProcess);
            testModel.shapes = [];
            testModel.shapes.push(startModel);
            testModel.shapes.push(preconditionModel);
            testModel.shapes.push(userTask);
            testModel.shapes.push(systemTask);
            testModel.shapes.push(endModel);
            testModel.links = [];
            testModel.links.push(new ProcessLinkModel(null, 11, 22));
            testModel.links.push(new ProcessLinkModel(null, 22, 33));
            testModel.links.push(new ProcessLinkModel(null, 33, 44));
            testModel.links.push(new ProcessLinkModel(null, 44, 55));
            processModel = new ProcessViewModel(testModel, communicationManager);

            graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization);

            graph.render(false, null);
        });

        it("return system task when attempting to retrieve system task for user task", () => {
            // Arrange
            let node: IUserTask = <IUserTask>graph.getNodeById("33");

            // Act
            let systemTask = node.getNextSystemTasks(graph)[0];

            //Assert
            expect(systemTask).not.toBeNull();
            expect(systemTask.model).toEqual(processModel.shapes[3]);
        });
    });

    describe("Delete User Task Messages", () => {

        let userTaskMessage = "user task only";
        let userTaskAndSystemDecisionMessage = "user task with system decision";
        let processGraph: ProcessGraph;
        beforeEach(() => {
            rootScope["config"].labels = {
                "ST_Confirm_Delete_User_Task": userTaskMessage,
                "ST_Confirm_Delete_User_Task_System_Decision": userTaskAndSystemDecisionMessage
            };
            /*
             start -> PRE -> UT1 -> SD ->  ST2 -> UT4 -> ST4 -> END
             ->  ST3 -> END
             Ut1Id = 20
             Ut4Id = 40
             */

            let testModel = createSystemDecisionForAddBranchTestModel();
            let processModel = new ProcessViewModel(testModel, communicationManager);
            processGraph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, msgService);

        });
        afterEach(() => {
            processGraph = null;
        });
        it("delete message simple user task", () => {

            //Arrange

            let simpleUtId = 40;
            processGraph.render(false, null);

            let ut4Node: IDiagramNode = processGraph.getNodeById(simpleUtId.toString());

            //Act
            let actualMessage = ut4Node.getDeleteDialogParameters().message;

            //Assert
            expect(actualMessage).toBe(userTaskMessage);
        });
        it("delete message user task with system decision and conditions", () => {

            //Arrange

            let simpleUtId = 20;
            processGraph.render(false, null);

            let ut1Node: IDiagramNode = processGraph.getNodeById(simpleUtId.toString());

            //Act
            let actualMessage = ut1Node.getDeleteDialogParameters().message;

            //Assert
            expect(actualMessage).toBe(userTaskAndSystemDecisionMessage);
        });

    });
});
