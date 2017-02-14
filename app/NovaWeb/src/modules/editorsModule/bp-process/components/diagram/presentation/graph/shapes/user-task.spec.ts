import {LocalizationServiceMock} from "../../../../../../../commonModule/localization/localization.service.mock";
import {MessageServiceMock} from "../../../../../../../main/components/messages/message.mock";
import {IMessageService} from "../../../../../../../main/components/messages/message.svc";
import {Models} from "../../../../../../../main/models/";
import {PropertyTypePredefined} from "../../../../../../../main/models/enums";
import {ItemTypePredefined} from "../../../../../../../main/models/item-type-predefined";
import {IStatefulArtifactFactoryMock, StatefulArtifactFactoryMock} from "../../../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ArtifactServiceMock} from "../../../../../../../managers/artifact-manager/artifact/artifact.svc.mock";
import {DialogService} from "../../../../../../../shared/widgets/bp-dialog";
import {ModalServiceMock} from "../../../../../../../shell/login/mocks.spec";
import {CommunicationManager, ICommunicationManager} from "../../../../../../bp-process";
import {ProcessShapeType, ProcessType} from "../../../../../models/enums";
import {ArtifactReference, ProcessLinkModel, ProcessModel, ProcessShapeModel} from "../../../../../models/process-models";
import {createSystemDecisionForAddBranchTestModel} from "../../../../../models/test-model-factory";
import {StatefulProcessArtifact} from "../../../../../process-artifact";
import {StatefulProcessSubArtifact} from "../../../../../process-subartifact";
import {IProcessViewModel, ProcessViewModel} from "../../../viewmodel/process-viewmodel";
import {IUserTaskShape, NodeType} from "../models/";
import {IDiagramNode, ISystemTask, IUserTask} from "../models/";
import {ProcessGraph} from "../process-graph";
import {SystemDecision, SystemTask, UserTask} from "./";
import {ArtifactReferenceLinkMock, ShapeModelMock} from "./shape-model.mock";
import {ShapesFactory} from "./shapes-factory";
import * as angular from "angular";

describe("UserTask test", () => {
    const LABEL_EDIT_MAXLENGTH = 40;
    const PERSONA_EDIT_MAXLENGTH = 40;
    const LABEL_VIEW_MAXLENGTH = 40;
    const PERSONA_VIEW_MAXLENGTH = 16;

    //const graph: ProcessGraph;
    let localScope, rootScope, shapesFactory, wrapper, container;
    let viewModel: ProcessViewModel;
    let msgService: IMessageService;
    let communicationManager: ICommunicationManager,
        dialogService: DialogService,
        localization: LocalizationServiceMock,
        statefulArtifactFactory: IStatefulArtifactFactoryMock;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("messageService", MessageServiceMock);
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("dialogService", DialogService);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("shapesFactory", ShapesFactory);
    }));

    beforeEach(inject((_$window_: ng.IWindowService,
        $rootScope: ng.IRootScopeService,
        messageService: IMessageService,
        _communicationManager_: ICommunicationManager,
        _dialogService_: DialogService,
        _localization_: LocalizationServiceMock,
        _statefulArtifactFactory_: IStatefulArtifactFactoryMock,
        _shapesFactory_: ShapesFactory) => {

        rootScope = $rootScope;
        communicationManager = _communicationManager_;
        dialogService = _dialogService_;
        localization = _localization_;
        statefulArtifactFactory = _statefulArtifactFactory_;
        shapesFactory = _shapesFactory_;

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

        const processModel = new ProcessModel();
        viewModel = new ProcessViewModel(processModel, communicationManager);
    }));

    it("Test UserTask class", () => {
        // Arrange
        const testUserTask = ShapeModelMock.instance().UserTaskMock();
        const testArtifactReference = new ArtifactReference();
        const testArtifactReferenceLink = new ArtifactReferenceLinkMock(1);
        testUserTask.propertyValues["label"] = {
            propertyName: "label", value: "", typeId: 0, typePredefined: 0
        };
        testUserTask.propertyValues["clientType"] = {
            propertyName: "clientType", value: NodeType.UserTask.toString(), typeId: 1, typePredefined: 0
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
        const graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization, shapesFactory, null, null, null);

        const node = new UserTask(testUserTask, rootScope, null, shapesFactory);
        node.render(graph, 80, 80, false);
        node.renderLabels();

        node.label = "test label";
        node.description = "test description";
        node.objective = "test objective";
        node.associatedArtifact = testArtifactReference;

        //Assert
        expect(graph.getNodeById("30").getNodeType()).toEqual(NodeType.UserTask);
        expect(node.userStoryId).toEqual(1);
        expect(node.label).toEqual("test label");
        expect(node.description).toEqual("test description");
        expect(node.objective).toEqual("test objective");
        expect(node.associatedArtifact).toEqual(testArtifactReference);
    });

    it("Test getSourceSystemTasks", () => {
        // Arrange
        const testSytemTask = ShapeModelMock.instance().SystemTaskMock();
        const testUserTask = ShapeModelMock.instance().UserTaskMock();

        const graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization, shapesFactory, null, null, null);
        const node = new UserTask(testUserTask, rootScope, null, shapesFactory);
        spyOn(node, "getSources").and.returnValue([new SystemTask(testSytemTask, rootScope, null, null, shapesFactory)]);

        // Act
        const systemTasks: ISystemTask[] = node.getPreviousSystemTasks(graph);

        //Assert
        expect(systemTasks.length).toEqual(1);
    });

    it("Test getNextSystemTasks simple case", () => {
        // Arrange
        const testSytemTask = ShapeModelMock.instance().SystemTaskMock();
        const testUserTask = ShapeModelMock.instance().UserTaskMock();

        const graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization, shapesFactory, null, null, null);
        const node = new UserTask(testUserTask, rootScope, null, shapesFactory);
        spyOn(node, "getTargets").and.returnValue([new SystemTask(testSytemTask, rootScope, null, null, shapesFactory)]);

        // Act
        const systemTasks: ISystemTask[] = node.getNextSystemTasks(graph);

        //Assert
        expect(systemTasks.length).toEqual(1);
    });

    it("Test getNextSystemTasks next is a system decision", () => {
        // Arrange
        const testSytemTask = ShapeModelMock.instance().SystemTaskMock();
        const testUserTask = ShapeModelMock.instance().UserTaskMock();
        const testSystemDecision = ShapeModelMock.instance().SystemDecisionmock();

        const graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization, shapesFactory, null, null, null);
        const UserTaskNode = new UserTask(testUserTask, rootScope, null, shapesFactory);
        const SystemDecisionNode = new SystemDecision(testSystemDecision, rootScope);
        spyOn(UserTaskNode, "getTargets").and.returnValue([SystemDecisionNode]);
        spyOn(SystemDecisionNode, "getTargets").and.returnValue(
            [
                new SystemTask(testSytemTask, rootScope, null, null, shapesFactory),
                new SystemTask(testSytemTask, rootScope, null, null, shapesFactory)
            ]
        );

        // Act
        const systemTasks: ISystemTask[] = UserTaskNode.getNextSystemTasks(graph);

        //Assert
        expect(systemTasks.length).toEqual(2);
    });

    it("Test getNextSystemTasks next is a system decision and another system decision is on the branch", () => {
        // Arrange
        const testSytemTask = ShapeModelMock.instance().SystemTaskMock();
        const testUserTask = ShapeModelMock.instance().UserTaskMock();
        const testSystemDecision = ShapeModelMock.instance().SystemDecisionmock();

        const graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization, shapesFactory, null, null, null);
        const UserTaskNode = new UserTask(testUserTask, rootScope, null, shapesFactory);
        const SystemDecisionNode1 = new SystemDecision(testSystemDecision, rootScope);
        const SystemDecisionNode2 = new SystemDecision(testSystemDecision, rootScope);
        spyOn(UserTaskNode, "getTargets").and.returnValue([SystemDecisionNode1]);
        spyOn(SystemDecisionNode1, "getTargets").and.returnValue(
            [
                SystemDecisionNode2,
                new SystemTask(testSytemTask, rootScope, null, null, shapesFactory)
            ]
        );
        spyOn(SystemDecisionNode2, "getTargets").and.returnValue(
            [
                new SystemTask(testSytemTask, rootScope, null, null, shapesFactory),
                new SystemTask(testSytemTask, rootScope, null, null, shapesFactory)
            ]
        );

        // Act
        const systemTasks: ISystemTask[] = UserTaskNode.getNextSystemTasks(graph);

        //Assert
        expect(systemTasks.length).toEqual(3);
    });

    it("Test setUserStoryId", () => {
        // Arrange
        const testUserTask = ShapeModelMock.instance().UserTaskMock();

        const node = new UserTask(testUserTask, rootScope, null, shapesFactory);


        // Act
        node.userStoryId = 3;

        //Assert
        expect(node.userStoryId).toEqual(3);
    });

    it("Test setUserStoryId", () => {
        // Arrange
        const testUserTask = ShapeModelMock.instance().UserTaskMock();

        const node = new UserTask(testUserTask, rootScope, null, shapesFactory);

        // Act
        node.userStoryId = 99;

        //Assert
        expect(node.userStoryId).toEqual(99);
    });

    describe("Test text elements", () => {

        it("Test latest personaReference value reuse", () => {
            // Arrange
            const graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization, shapesFactory, null, null, null);
            const testUserTask = ShapeModelMock.instance().UserTaskMock();

            const node = new UserTask(testUserTask, rootScope, null, shapesFactory);
            node.render(graph, 80, 80, false);
            node.renderLabels();
            node.personaReference = {
                id: 1,
                projectId: 1,
                name: "new persona",
                typePrefix: "PRO",
                baseItemTypePredefined: ItemTypePredefined.Actor,
                projectName: "test project",
                link: null,
                version: null
            };

            // Act
            const node1 = new UserTask(testUserTask, rootScope, null, shapesFactory);

            //Assert
            expect(node1.personaReference).toEqual(node.personaReference);
        });
    });

    describe("when using default process", () => {
        let testModel;
        let processModel: IProcessViewModel;
        let graph: ProcessGraph;

        beforeEach(() => {

            const startModel = new ProcessShapeModel(11);
            startModel.propertyValues = shapesFactory.createPropertyValuesForSystemTaskShape();
            startModel.propertyValues["clientType"].value = ProcessShapeType.Start;
            startModel.propertyValues["x"].value = 0;
            const preconditionModel = new ProcessShapeModel(22);
            preconditionModel.personaReference = {
                id: 1,
                projectId: 1,
                name: "test persona",
                typePrefix: "PRO",
                baseItemTypePredefined: ItemTypePredefined.Actor,
                projectName: "test project",
                link: null,
                version: null
            };
            preconditionModel.propertyValues = shapesFactory.createPropertyValuesForSystemTaskShape();
            preconditionModel.propertyValues["clientType"].value = ProcessShapeType.PreconditionSystemTask;
            preconditionModel.propertyValues["x"].value = 1;
            const userTask = shapesFactory.createModelUserTaskShape(2, 1, 33, 2, 0);
            const systemTask = shapesFactory.createModelSystemTaskShape(2, 1, 44, 3, 0);
            const endModel = new ProcessShapeModel(55);
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

            graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

            graph.render(false, null);
        });

        it("return system task when attempting to retrieve system task for user task", () => {
            // Arrange
            const node: IUserTask = <IUserTask>graph.getNodeById("33");

            // Act
            const systemTask = node.getNextSystemTasks(graph)[0];

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

            const testModel = createSystemDecisionForAddBranchTestModel();
            const processModel = new ProcessViewModel(testModel, communicationManager);
            processGraph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, msgService, null, null);

        });
        afterEach(() => {
            processGraph = null;
        });
        it("delete message simple user task", () => {

            //Arrange

            const simpleUtId = 40;
            processGraph.render(false, null);

            const ut4Node: IDiagramNode = processGraph.getNodeById(simpleUtId.toString());

            //Act
            const actualMessage = ut4Node.getDeleteDialogParameters().message;

            //Assert
            expect(actualMessage).toBe(userTaskMessage);
        });
        it("delete message user task with system decision and conditions", () => {

            //Arrange

            const simpleUtId = 20;
            processGraph.render(false, null);

            const ut1Node: IDiagramNode = processGraph.getNodeById(simpleUtId.toString());

            //Act
            const actualMessage = ut1Node.getDeleteDialogParameters().message;

            //Assert
            expect(actualMessage).toBe(userTaskAndSystemDecisionMessage);
        });

    });

    describe("StatefulSubArtifact changes", () => {
        let viewModel: IProcessViewModel,
            statefulArtifact: StatefulProcessArtifact,
            node: IUserTask,
            graph: ProcessGraph,
            statefulSubArtifact: StatefulProcessSubArtifact;

        const newPersonaReference = {
            id: 2,
            projectId: 1,
            name: "added persona",
            typePrefix: "PRO",
            baseItemTypePredefined: ItemTypePredefined.Actor,
            projectName: "test project",
            link: null,
            version: null
        };
        beforeEach(() => {
            // arrange
            const processModel = new ProcessModel();
            const mock = ShapeModelMock.instance().SystemTaskMock();
            const artifact: Models.IArtifact = ArtifactServiceMock.createArtifact(1);
            artifact.predefinedType = ItemTypePredefined.Process;
            processModel.shapes.push(mock);

            statefulArtifact = <StatefulProcessArtifact>statefulArtifactFactory.createStatefulArtifact(artifact);
            statefulArtifactFactory.populateStatefulProcessWithProcessModel(statefulArtifact, processModel);
            statefulSubArtifact = <StatefulProcessSubArtifact>statefulArtifact.subArtifactCollection.get(mock.id);

            node = new UserTask(<IUserTaskShape>statefulArtifact.shapes[0], rootScope, null, shapesFactory);

            viewModel = new ProcessViewModel(statefulArtifact, communicationManager);
            viewModel.userTaskPersonaReferenceList = [node.personaReference];
            viewModel.systemTaskPersonaReferenceList = [];

            graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization, shapesFactory, null, null, null);
        });

        it("when modifying personaReference - personaReference matches and personaReference list updated", () => {

            // arrange
            spyOn(statefulArtifact, "refresh")();
            spyOn(statefulArtifact, "lock")();

            // act
            node.render(graph, 80, 120, false);
            node.renderLabels();

            node.personaReference = newPersonaReference;

            // assert
            expect(statefulSubArtifact.specialProperties.get(PropertyTypePredefined.PersonaReference).value).toBe(node.personaReference.id);
            expect(viewModel.userTaskPersonaReferenceList.length).toBe(2);
        });

        it("when modifying personaReference - attempt lock is called", () => {

            // arrange
            spyOn(statefulArtifact, "refresh")();
            const lockSpy = spyOn(statefulArtifact, "lock");

            // act
            node.render(graph, 80, 120, false);
            node.renderLabels();

            node.personaReference = newPersonaReference;

            // assert
            expect(lockSpy).toHaveBeenCalled();
        });

        it("when modifying personaReference - artifact state is dirty", () => {

            // arrange
            spyOn(statefulArtifact, "refresh")();
            spyOn(statefulArtifact, "lock");

            // act
            node.render(graph, 80, 120, false);
            node.renderLabels();

            node.personaReference = newPersonaReference;

            // assert
            expect(statefulArtifact.artifactState.dirty).toBeTruthy();
        });
    });
});
