import * as angular from "angular";
import { ShapesFactory } from "./shapes-factory";
import { ProcessGraph } from "../process-graph";
import { ArtifactReferenceLinkMock, ShapeModelMock } from "./shape-model.mock";
import { ProcessModel, ProcessShapeModel, ProcessLinkModel, PropertyTypePredefined } from "../../../../../models/process-models";
import { ProcessShapeType, ProcessType } from "../../../../../models/enums";
import { ProcessViewModel, IProcessViewModel } from "../../../viewmodel/process-viewmodel";
import { SystemTask, DiagramNodeElement } from "./";
import { NodeChange, NodeType, ElementType } from "../models/";
import { ISystemTask, ISystemTaskShape } from "../models/";
import { ICommunicationManager, CommunicationManager } from "../../../../../../bp-process";
import { LocalizationServiceMock } from "../../../../../../../core/localization/localization.mock";
import { DialogService } from "../../../../../../../shared/widgets/bp-dialog";
import { ModalServiceMock } from "../../../../../../../shell/login/mocks.spec";
import { IStatefulArtifact } from "../../../../../../../managers/artifact-manager/";
import { StatefulArtifactFactoryMock, IStatefulArtifactFactoryMock } from "../../../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import { StatefulProcessSubArtifact } from "../../../../../process-subartifact";
import { StatefulProcessArtifact } from "../../../../../process-artifact";
import { Models } from "../../../../../../../main/models/";
import { ArtifactServiceMock } from "../../../../../../../managers/artifact-manager/artifact/artifact.svc.mock";

describe("SystemTask", () => {
    let statefulArtifactFactory: IStatefulArtifactFactoryMock;
    const LABEL_EDIT_MAXLENGTH = 35;
    const LABEL_VIEW_MAXLENGTH = 35;
    const PERSONA_EDIT_MAXLENGTH = 40;
    const PERSONA_VIEW_MAXLENGTH = 12;
    let shapesFactory: ShapesFactory;
    let localScope, rootScope: ng.IRootScopeService, wrapper, container;
    let communicationManager: ICommunicationManager,
        dialogService: DialogService,
        localization: LocalizationServiceMock;

    const testArtifactReferenceLink2 = new ArtifactReferenceLinkMock(2);

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("dialogService", DialogService);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
    }));

    beforeEach(inject((_$window_: ng.IWindowService,
        $rootScope: ng.IRootScopeService,
        _communicationManager_: ICommunicationManager,
        _dialogService_: DialogService,
        _localization_: LocalizationServiceMock,
        _statefulArtifactFactory_: IStatefulArtifactFactoryMock) => {

        rootScope = $rootScope;
        communicationManager = _communicationManager_;
        dialogService = _dialogService_;
        localization = _localization_;
        statefulArtifactFactory = _statefulArtifactFactory_;

        wrapper = document.createElement("DIV");
        container = document.createElement("DIV");
        wrapper.appendChild(container);
        document.body.appendChild(wrapper);

        $rootScope["config"] = {};
        $rootScope["config"].labels = {};
        shapesFactory = new ShapesFactory($rootScope, statefulArtifactFactory);
        localScope = { graphContainer: container, graphWrapper: wrapper, isSpa: false };
    }));


    it("Test SystemTask class", () => {
        // Arrange
        const processModel = new ProcessModel();
        const viewModel = new ProcessViewModel(processModel, communicationManager);
        viewModel.isReadonly = false;

        // Act
        const graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization);

        const node = new SystemTask(ShapeModelMock.instance().SystemTaskMock(), rootScope, "", null, shapesFactory);
        node.render(graph, 80, 120, false);
        node.renderLabels();

        node.label = "test label";
        node.persona = "test persona";
        node.description = "test description";
        node.associatedImageUrl = "test.jpg";
        node.imageId = "2";
        node.associatedArtifact = testArtifactReferenceLink2;

        //Assert
        expect(graph.getNodeById("30").getNodeType()).toEqual(NodeType.SystemTask);
        expect(node.label).toEqual("test label");
        expect(node.persona).toEqual("test persona");
        expect(node.description).toEqual("test description");
        expect(node.associatedImageUrl).toEqual("test.jpg");
        expect(node.imageId).toEqual("2");
        expect(node.associatedArtifact).toEqual(testArtifactReferenceLink2);
    });

    describe("Test text elements", () => {

        it("Test formatElementText - label overflow", () => {
            // Arrange
            const testSystemTask = ShapeModelMock.instance().SystemTaskMock();

            const node = new SystemTask(testSystemTask, rootScope, "", null, shapesFactory);
            const textInput = "0123456789,0123456789,0123456789,0123456789";
            const expectedText = textInput.substr(0, LABEL_VIEW_MAXLENGTH) + " ...";
            // Act
            const actualText = node.formatElementText(node, textInput);

            //Assert
            expect(actualText).toEqual(expectedText);
        });

        it("Test formatElementText - persona overflow", () => {
            // Arrange 
            const testSystemTask = ShapeModelMock.instance().SystemTaskMock();

            const node = new SystemTask(testSystemTask, rootScope, "", null, shapesFactory);
            const editNode = new DiagramNodeElement("H1", ElementType.SystemTaskHeader, "", new mxGeometry(), "");

            const textInput = "01234567890123456789";
            const expectedText = textInput.substr(0, PERSONA_VIEW_MAXLENGTH) + " ...";
            // Act
            const actualText = node.formatElementText(editNode, textInput);

            //Assert
            expect(actualText).toEqual(expectedText);
        });

        it("Test getElementTextLength - label", () => {
            // Arrange
            const testSystemTask = ShapeModelMock.instance().SystemTaskMock();

            const node = new SystemTask(testSystemTask, rootScope, "", null, shapesFactory);


            // Act
            const textLength = node.getElementTextLength(node);

            //Assert
            expect(textLength).toEqual(LABEL_EDIT_MAXLENGTH);
        });

        it("Test getElementTextLength - persona", () => {
            // Arrange
            const testSystemTask = ShapeModelMock.instance().SystemTaskMock();

            const node = new SystemTask(testSystemTask, rootScope, "", null, shapesFactory);
            const editNode = new DiagramNodeElement("H1", ElementType.SystemTaskHeader, "", new mxGeometry(), "");


            // Act
            const textLength = node.getElementTextLength(editNode);

            //Assert
            expect(textLength).toEqual(PERSONA_EDIT_MAXLENGTH);
        });
        it("Test setElementText - label", () => {
            // Arrange
            const testSystemTask = ShapeModelMock.instance().SystemTaskMock();

            const node = new SystemTask(testSystemTask, rootScope, "", null, shapesFactory);

            const testLabelText = "test label";


            // Act
            node.setElementText(node, testLabelText);

            //Assert
            expect(node.label).toEqual(testLabelText);
        });

        it("Test setElementText - persona", () => {
            // Arrange
            const testSystemTask = ShapeModelMock.instance().SystemTaskMock();

            const node = new SystemTask(testSystemTask, rootScope, "", null, shapesFactory);
            const editNode = new DiagramNodeElement("H1", ElementType.SystemTaskHeader, "", new mxGeometry(), "");

            const testLabelText = "test label";

            // Act
            node.setElementText(editNode, testLabelText);

            //Assert
            expect(node.persona).toEqual(testLabelText);
        });

        it("Test latest persona value reuse", () => {
            // Arrange
            const testSystemTask = ShapeModelMock.instance().SystemTaskMock();

            const node = new SystemTask(testSystemTask, rootScope, "", null, shapesFactory);
            node.persona = "12345";


            // Act
            const node1 = new SystemTask(testSystemTask, rootScope, "", null, shapesFactory);

            //Assert
            expect(node1.persona).toEqual(node.persona);
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

            graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization);

            graph.render(false, null);
        });

        it("return null when attempting to retrieve user task for pre-condition", () => {
            // Arrange
            const node: ISystemTask = <ISystemTask>graph.getNodeById("22");

            // Act
            const userTask = node.getUserTask(graph);

            //Assert
            expect(userTask).toBeNull();
        });

        it("return user task when attempting to retrieve user task for system task", () => {
            // Arrange
            const node: ISystemTask = <ISystemTask>graph.getNodeById("44");

            // Act
            const userTask = node.getUserTask(graph);

            //Assert
            expect(userTask).not.toBeNull();
            expect(userTask.model).toEqual(processModel.shapes[2]);
        });
    });

    describe("StatefulSubArtifact changes", () => {
        let viewModel: IProcessViewModel,
            statefulArtifact: StatefulProcessArtifact,
            node: ISystemTask,
            graph: ProcessGraph,
            statefulSubArtifact: StatefulProcessSubArtifact;
        beforeEach(() => {
            // arrange
            const processModel = new ProcessModel();
            const mock = ShapeModelMock.instance().SystemTaskMock();
            const artifact: Models.IArtifact = ArtifactServiceMock.createArtifact(1);
            artifact.predefinedType = Models.ItemTypePredefined.Process;
            processModel.shapes.push(mock);

            statefulArtifact = <StatefulProcessArtifact>statefulArtifactFactory.createStatefulArtifact(artifact);
            statefulArtifactFactory.populateStatefulProcessWithPorcessModel(statefulArtifact, processModel);
            statefulSubArtifact = <StatefulProcessSubArtifact>statefulArtifact.subArtifactCollection.get(mock.id);
            const peronsaPropertyValue = {
                propertyTypeId: 0,
                propertyTypeVersionId: null,
                propertyTypePredefined: PropertyTypePredefined.Persona,
                isReuseReadOnly: false,
                value: ""
            };
            statefulSubArtifact.specialProperties.initialize([peronsaPropertyValue]);

            node = new SystemTask(<ISystemTaskShape>statefulArtifact.shapes[0], rootScope, "", null, shapesFactory);

            viewModel = new ProcessViewModel(statefulArtifact, communicationManager);

            graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization);
        });

        it("when modifying persona - persona matches", () => {

            // arrange             
            spyOn(statefulArtifact, "refresh")();
            spyOn(statefulArtifact, "lock")();

            // act
            node.render(graph, 80, 120, false);
            node.renderLabels();

            node.persona = "test persona";

            // assert
            expect(statefulSubArtifact.specialProperties.get(PropertyTypePredefined.Persona).value).toBe(node.persona);
        });

        it("when modifying persona - attempt lock is called", () => {

            // arrange             
            spyOn(statefulArtifact, "refresh")();
            const lockSpy = spyOn(statefulArtifact, "lock");

            // act
            node.render(graph, 80, 120, false);
            node.renderLabels();

            node.persona = "test persona";

            // assert
            expect(lockSpy).toHaveBeenCalled();
        });

       it("when modifying persona - artifact state is dirty", () => {

            // arrange             
            spyOn(statefulArtifact, "refresh")();
            const lockSpy = spyOn(statefulArtifact, "lock");

            // act
            node.render(graph, 80, 120, false);
            node.renderLabels();

            node.persona = "test persona";

            // assert
            expect(statefulArtifact.artifactState.dirty).toBeTruthy();
        });
    });
});
