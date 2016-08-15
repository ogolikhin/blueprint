import {ShapesFactory} from "./shapes-factory";
import {ProcessGraph} from "../process-graph";
import {ArtifactReferenceLinkMock, ShapeModelMock} from "./shape-model.mock";
import {ProcessServiceMock} from "../../../../../services/process/process.svc.mock";
import {IProcessService} from "../../../../../services/process/process.svc";
import {ProcessModel, ProcessShapeModel, ProcessLinkModel} from "../../../../../models/processModels";
import {ProcessShapeType, ProcessType} from "../../../../../models/enums";
import {ProcessViewModel, IProcessViewModel} from "../../../viewmodel/process-viewmodel";
import {SystemTask, DiagramNodeElement} from "./";
import {NodeChange, NodeType, ElementType} from "../process-graph-constants";
import {ISystemTask} from "../process-graph-interfaces";

describe("SystemTask", () => {

    var LABEL_EDIT_MAXLENGTH = 35;
    var LABEL_VIEW_MAXLENGTH = 35;
    var PERSONA_EDIT_MAXLENGTH = 40;
    var PERSONA_VIEW_MAXLENGTH = 12;
    var shapesFactory: ShapesFactory;
    var graph: ProcessGraph;
    var localScope, rootScope, processModelService, artifactVersionControlService, wrapper, container;

    var testArtifactReferenceLink2 = new ArtifactReferenceLinkMock(2);

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("processModelService", ProcessServiceMock);
    }));

    beforeEach(inject((_$window_: ng.IWindowService, $rootScope: ng.IRootScopeService, processModelService: IProcessService) => {
        rootScope = $rootScope;
        processModelService = processModelService;
        wrapper = document.createElement('DIV');
        container = document.createElement('DIV');
        wrapper.appendChild(container);
        document.body.appendChild(wrapper);

        $rootScope["config"] = {};
        $rootScope["config"].labels = {};
        shapesFactory = new ShapesFactory($rootScope);
        localScope = { graphContainer: container, graphWrapper: wrapper, isSpa: false };
    }));


    it("Test SystemTask class", () => {
        // Arrange
        let processModel = new ProcessModel();
        let viewModel = new ProcessViewModel(processModel);
        viewModel.isReadonly = false;

        // Act
        var graph = new ProcessGraph(rootScope, localScope, container, processModelService, viewModel);

        var node = new SystemTask(ShapeModelMock.instance().SystemTaskMock(), rootScope, "", null, shapesFactory);
        node.render(graph, 80, 120, false);
        node.renderLabels();

        spyOn(SystemTask.prototype, "notify").and.callThrough();

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
        expect(SystemTask.prototype.notify).toHaveBeenCalledWith(NodeChange.Update);
    });

    describe("Test text elements", () => {

        it("Test formatElementText - label overflow", () => {
            // Arrange
            var testSystemTask = ShapeModelMock.instance().SystemTaskMock();

            var node = new SystemTask(testSystemTask, rootScope, "", null, shapesFactory);
            var textInput = "0123456789,0123456789,0123456789,0123456789";
            var expectedText = textInput.substr(0, LABEL_VIEW_MAXLENGTH) + " ...";
            // Act
            var actualText = node.formatElementText(node, textInput);

            //Assert
            expect(actualText).toEqual(expectedText);
        });

        it("Test formatElementText - persona overflow", () => {
            // Arrange 
            var testSystemTask = ShapeModelMock.instance().SystemTaskMock();

            var node = new SystemTask(testSystemTask, rootScope, "", null, shapesFactory);
            var editNode = new DiagramNodeElement("H1", ElementType.SystemTaskHeader, "", new mxGeometry(), "");

            var textInput = "01234567890123456789";
            var expectedText = textInput.substr(0, PERSONA_VIEW_MAXLENGTH) + " ...";
            // Act
            var actualText = node.formatElementText(editNode, textInput);

            //Assert
            expect(actualText).toEqual(expectedText);
        });

        it("Test getElementTextLength - label", () => {
            // Arrange
            var testSystemTask = ShapeModelMock.instance().SystemTaskMock();

            var node = new SystemTask(testSystemTask, rootScope, "", null, shapesFactory);


            // Act
            var textLength = node.getElementTextLength(node);

            //Assert
            expect(textLength).toEqual(LABEL_EDIT_MAXLENGTH);
        });

        it("Test getElementTextLength - persona", () => {
            // Arrange
            var testSystemTask = ShapeModelMock.instance().SystemTaskMock();

            var node = new SystemTask(testSystemTask, rootScope, "", null, shapesFactory);
            var editNode = new DiagramNodeElement("H1", ElementType.SystemTaskHeader, "", new mxGeometry(), "");


            // Act
            var textLength = node.getElementTextLength(editNode);

            //Assert
            expect(textLength).toEqual(PERSONA_EDIT_MAXLENGTH);
        });
        it("Test setElementText - label", () => {
            // Arrange
            var testSystemTask = ShapeModelMock.instance().SystemTaskMock();

            var node = new SystemTask(testSystemTask, rootScope, "", null, shapesFactory);

            var testLabelText = "test label";


            // Act
            node.setElementText(node, testLabelText);

            //Assert
            expect(node.label).toEqual(testLabelText);
        });

        it("Test setElementText - persona", () => {
            // Arrange
            var testSystemTask = ShapeModelMock.instance().SystemTaskMock();

            var node = new SystemTask(testSystemTask, rootScope, "", null, shapesFactory);
            var editNode = new DiagramNodeElement("H1", ElementType.SystemTaskHeader, "", new mxGeometry(), "");

            var testLabelText = "test label";

            // Act
            node.setElementText(editNode, testLabelText);

            //Assert
            expect(node.persona).toEqual(testLabelText);
        });

        it("Test latest persona value reuse", () => {
            // Arrange
            var testSystemTask = ShapeModelMock.instance().SystemTaskMock();

            var node = new SystemTask(testSystemTask, rootScope, "", null, shapesFactory);
            node.persona = "12345";


            // Act
            var node1 = new SystemTask(testSystemTask, rootScope, "", null, shapesFactory);

            //Assert
            expect(node1.persona).toEqual(node.persona);
        });

    });


    describe("when using default process", () => {

        var testModel;
        var processModel: IProcessViewModel;

        beforeEach(() => {

            var startModel = new ProcessShapeModel(11);
            startModel.propertyValues = shapesFactory.createPropertyValuesForSystemTaskShape();
            startModel.propertyValues["clientType"].value = ProcessShapeType.Start;
            startModel.propertyValues["x"].value = 0;
            var preconditionModel = new ProcessShapeModel(22);
            preconditionModel.propertyValues = shapesFactory.createPropertyValuesForSystemTaskShape();
            preconditionModel.propertyValues["clientType"].value = ProcessShapeType.PreconditionSystemTask;
            preconditionModel.propertyValues["x"].value = 1;
            var userTask = shapesFactory.createModelUserTaskShape(2, 1, 33, 2, 0);
            var systemTask = shapesFactory.createModelSystemTaskShape(2, 1, 44, 3, 0);
            var endModel = new ProcessShapeModel(55);
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
            processModel = new ProcessViewModel(testModel);

            graph = new ProcessGraph(rootScope, localScope, container, processModelService, processModel);

            graph.render(false, null);
        });

        it("return null when attempting to retrieve user task for pre-condition", () => {
            // Arrange
            var node: ISystemTask = <ISystemTask>graph.getNodeById("22");

            // Act
            var userTask = node.getUserTask(graph);

            //Assert
            expect(userTask).toBeNull();
        });

        it("return user task when attempting to retrieve user task for system task", () => {
            // Arrange
            var node: ISystemTask = <ISystemTask>graph.getNodeById("44");

            // Act
            var userTask = node.getUserTask(graph);

            //Assert
            expect(userTask).not.toBeNull();
            expect(userTask.model).toEqual(processModel.shapes[2]);
        });
    });

});