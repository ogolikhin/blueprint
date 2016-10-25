import * as angular from "angular";
import { ProcessGraph } from "../process-graph";
import { IProcessViewModel, ProcessViewModel } from "../../../viewmodel/process-viewmodel";
import { ShapesFactory } from "./shapes-factory";
import {
    ProcessModel,
    ProcessShapeModel,
    IPropertyValueInformation,
    IHashMapOfPropertyValues
} from "../../../../../models/process-models";
import { ProcessType } from "../../../../../models/enums";
import { DiagramNode, SystemTask } from "./";
import { PropertyTypePredefined } from "../../../../../../../main/models/enums";
import { NodeChange, IDiagramNode, ISystemTaskShape } from "../models/";
import { ICommunicationManager, CommunicationManager } from "../../../../../../bp-process";
import { LocalizationServiceMock } from "../../../../../../../core/localization/localization.mock";
import { DialogService } from "../../../../../../../shared/widgets/bp-dialog";
import { ModalServiceMock } from "../../../../../../../shell/login/mocks.spec";
import { IStatefulArtifact } from "../../../../../../../managers/artifact-manager/";
import { StatefulArtifactFactoryMock, IStatefulArtifactFactoryMock } from "../../../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import { StatefulProcessSubArtifact } from "../../../../../process-subartifact";
import { ShapeModelMock } from "./shape-model.mock";
import {StatefulProcessArtifact} from "../../../../../process-artifact";
import {Models} from "../../../../../../../main/models/";
import {ArtifactServiceMock} from "../../../../../../../managers/artifact-manager/artifact/artifact.svc.mock";

describe("DiagramNode", () => {
    let graph: ProcessGraph;
    let shapesFactory;
    let rootScope: ng.IRootScopeService;
    let communicationManager: ICommunicationManager,
        dialogService: DialogService,
        localization: LocalizationServiceMock;

    let statefulArtifactFactory: IStatefulArtifactFactoryMock;
    let wrapper: HTMLElement, container: HTMLElement;
    let localScope;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("dialogService", DialogService);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("shapesFactory", ShapesFactory);
    }));

    beforeEach(inject(($rootScope: ng.IRootScopeService,
        _communicationManager_: ICommunicationManager,
        _dialogService_: DialogService,
        _localization_: LocalizationServiceMock,
        _statefulArtifactFactory_: IStatefulArtifactFactoryMock,
        _shapesFactory_: ShapesFactory) => {

        communicationManager = _communicationManager_;
        dialogService = _dialogService_;
        localization = _localization_;
        rootScope = $rootScope;
        statefulArtifactFactory = _statefulArtifactFactory_;
        shapesFactory = _shapesFactory_;

        rootScope["config"] = {
            labels: {
                "ST_Persona_Label": "Persona",
                "ST_Colors_Label": "Color",
                "ST_Comments_Label": "Comments",
                "ST_New_User_Task_Label": "New User Task",
                "ST_New_User_Task_Persona": "User",
                "ST_New_User_Decision_Label": "New Decision",
                "ST_New_System_Task_Label": "New System Task",
                "ST_New_System_Task_Persona": "System"
            }
        };
        wrapper = document.createElement("DIV");
        container = document.createElement("DIV");
        wrapper.appendChild(container);
        document.body.appendChild(wrapper);
        localScope = {graphContainer: container, graphWrapper: wrapper, isSpa: false};
        shapesFactory = new ShapesFactory(rootScope, statefulArtifactFactory);
    }));


    afterEach(() => {
        graph = null;
    });
    describe("when getting nodes for graph", () => {

        describe("for user task -> system task model", () => {
            let testModel;
            let processModel: IProcessViewModel;

            beforeEach(() => {
                const userTaskModel = shapesFactory.createModelUserTaskShape(1, 1, 77, 0, 0);
                const systemTaskModel = shapesFactory.createModelSystemTaskShape(1, 1, 88, 1, 0);
                const link = { sourceId: 77, destinationId: 88, orderindex: 1, label: "" };

                testModel = new ProcessModel();
                testModel.propertyValues = {};
                testModel.propertyValues["clientType"] = shapesFactory.createClientTypeValueForProcess(ProcessType.UserToSystemProcess);
                testModel.shapes = [];
                testModel.shapes.push(userTaskModel);
                testModel.shapes.push(systemTaskModel);
                testModel.links = [];
                testModel.links.push(link);
                processModel = new ProcessViewModel(testModel, communicationManager);

                const wrapper = document.createElement("DIV");
                const container = document.createElement("DIV");
                wrapper.appendChild(container);
                document.body.appendChild(wrapper);

                graph = new ProcessGraph(rootScope, { graphContainer: container, graphWrapper: wrapper },
                    container, processModel, dialogService, localization, null, null, null, shapesFactory);
                graph.render(false, null);
            });

            afterEach(() => {
                processModel = null;
                graph = null;
            });

            it("returns empty list for user task source nodes", () => {
                // Arrange
                const node = graph.getNodeById("77");

                // Act
                const actual = node.getSources(graph.getMxGraphModel());

                // Assert
                expect(actual.length).toEqual(0);
            });
            it("returns empty list for system task target nodes", () => {
                // Arrange
                const node = graph.getNodeById("88");

                // Act
                const actual = node.getTargets(graph.getMxGraphModel());

                // Assert
                expect(actual.length).toEqual(0);
            });
            it("returns user task for system task source nodes", () => {
                // Arrange
                const node = graph.getNodeById("88");

                // Act
                const actual = node.getSources(graph.getMxGraphModel());

                // Assert
                expect(actual.length).toEqual(1);
                expect(actual[0].model).toEqual(processModel.shapes[0]);
            });
            it("returns system task for user task target nodes", () => {
                // Arrange
                const node = graph.getNodeById("77");

                // Act
                const actual = node.getTargets(graph.getMxGraphModel());

                // Assert
                expect(actual.length).toEqual(1);
                expect(actual[0].model).toEqual(processModel.shapes[1]);
            });
            it("returns empty list for user task previous nodes", () => {
                // Arrange
                const node = graph.getNodeById("77");

                // Act
                const actual = node.getPreviousNodes();

                // Assert
                expect(actual.length).toEqual(0);
            });
            it("returns empty list for system task next nodes", () => {
                // Arrange
                const node = graph.getNodeById("88");

                // Act
                const actual = node.getNextNodes();

                // Assert
                expect(actual.length).toEqual(0);
            });
            it("returns user task for system task previous nodes", () => {
                // Arrange
                const node = graph.getNodeById("88");

                // Act
                const actual = node.getPreviousNodes();

                // Assert
                expect(actual.length).toEqual(1);
                expect(actual[0].model).toEqual(processModel.shapes[0]);
            });
            it("returns system task for user task next nodes", () => {
                // Arrange
                const node = graph.getNodeById("77");

                // Act
                const actual = node.getNextNodes();

                // Assert
                expect(actual.length).toEqual(1);
                expect(actual[0].model).toEqual(processModel.shapes[1]);
            });

            it("throws exception when rendered", () => {
                // Arrange
                const model = new ProcessShapeModel();
                const diagramNode = new DiagramNode(model);
                let exception = null;

                // Act
                try {
                    diagramNode.render(null, 0, 0, false);
                } catch (error) {
                    exception = error;
                }

                // Assert
                expect(exception).not.toBeNull();
            });

            it("returns default point when getting the center without defined geometry", () => {
                // Arrange
                const model = new ProcessShapeModel();
                const diagramNode = new DiagramNode(model);

                // Act
                const actual = diagramNode.getCenter();

                // Assert
                expect(actual).toEqual(new mxPoint(0, 0));
            });

            it("modifies model's name when name is changed", () => {
                // Arrange
                const oldValue = "Default";
                const newValue = "New Name";
                const model = new ProcessShapeModel();
                model.name = oldValue;
                const label = "label";
                const propertyValue: IPropertyValueInformation = {
                    propertyName: label,
                    typePredefined: PropertyTypePredefined.Label,
                    typeId: 5,
                    value: oldValue
                };
                const propertyValues: IHashMapOfPropertyValues = {};
                propertyValues[label] = propertyValue;
                model.propertyValues = propertyValues;
                const diagramNode = new DiagramNode(model);
                // Act
                diagramNode.action = newValue;

                // Assert
                expect(model.propertyValues[label].value).toEqual(newValue);
            });

            it("modifies model's 'x' when column is updated", () => {
                // Arrange
                const oldValue = 0;
                const newValue = 1;
                const model = new ProcessShapeModel();
                model.propertyValues = {};

                model.propertyValues[shapesFactory.X.key] = shapesFactory.createXValue(oldValue);
                const diagramNode = new DiagramNode(model);

                // Act
                diagramNode.column = newValue;

                // Assert
                expect(model.propertyValues[shapesFactory.X.key].value).toEqual(newValue);
            });

            it("modifies model's 'y' when row is updated", () => {
                // Arrange
                const oldValue = 0;
                const newValue = 1;
                const model = new ProcessShapeModel();
                model.propertyValues = {};
                model.propertyValues[shapesFactory.Y.key] = shapesFactory.createXValue(oldValue);
                const diagramNode = new DiagramNode(model);

                // Act
                diagramNode.row = newValue;

                // Assert
                expect(model.propertyValues[shapesFactory.Y.key].value).toEqual(newValue);
            });
        });

        describe("for user decision -> user task models", () => {
            let testModel;
            let processModel: IProcessViewModel;

            beforeEach(() => {
                const systemTaskModel = shapesFactory.createModelUserTaskShape(1, 1, 5, 0, 0);
                const userDecisionModel = shapesFactory.createModelUserDecisionShape(1, 1, 10, 1, 0);
                const userTaskModel1 = shapesFactory.createModelUserTaskShape(1, 1, 20, 2, 0);
                const userTaskModel2 = shapesFactory.createModelUserTaskShape(1, 1, 30, 2, 1);
                const userTaskModel3 = shapesFactory.createModelUserTaskShape(1, 1, 40, 2, 2);
                const link0 = { sourceId: 5, destinationId: 10, orderindex: 0, label: "" };
                const link1 = { sourceId: 10, destinationId: 20, orderindex: 0, label: "" };
                const link2 = { sourceId: 10, destinationId: 30, orderindex: 10, label: "" };
                const link3 = { sourceId: 10, destinationId: 40, orderindex: 20, label: "" };

                testModel = new ProcessModel();
                testModel.propertyValues = {};
                testModel.propertyValues["clientType"] = shapesFactory.createClientTypeValueForProcess(ProcessType.UserToSystemProcess);
                testModel.shapes = [systemTaskModel, userDecisionModel, userTaskModel1, userTaskModel2, userTaskModel3];
                testModel.links = [link3, link2, link1, link0];
                processModel = new ProcessViewModel(testModel, communicationManager);

                const wrapper = document.createElement("DIV");
                const container = document.createElement("DIV");
                wrapper.appendChild(container);
                document.body.appendChild(wrapper);

                graph = new ProcessGraph(rootScope, { graphContainer: container, graphWrapper: wrapper },
                    container, processModel, dialogService, localization, null, null, null, shapesFactory);
                graph.render(false, null);
            });

            afterEach(() => {
                processModel = null;
                graph = null;
            });

            it("returns empty list for user task sources", () => {
                // Arrange
                const node = graph.getNodeById("5");

                // Act
                const actual = node.getSources(graph.getMxGraphModel());

                // Assert
                expect(actual.length).toEqual(0);
            });

            it("returns user decision for user task target", () => {
                // Arrange
                const node = graph.getNodeById("5");

                // Act
                const actual = node.getTargets(graph.getMxGraphModel());

                // Assert
                expect(actual.length).toEqual(1);
                expect(actual[0].model.id).toEqual(10);
            });

            it("returns user task for user decision sources", () => {
                // Arrange
                const node = graph.getNodeById("10");

                // Act
                const actual = node.getSources(graph.getMxGraphModel());

                // Assert
                expect(actual.length).toEqual(1);
                expect(actual[0].model.id).toEqual(5);
            });

            it("returns user tasks for user decision targets in order of ascending link order index", () => {
                // Arrange
                const node = graph.getNodeById("10");

                // Act
                const actual = node.getTargets(graph.getMxGraphModel());

                // Assert
                expect(actual.length).toEqual(3);
                expect(actual[0].model.id).toEqual(20);
                expect(actual[1].model.id).toEqual(30);
                expect(actual[2].model.id).toEqual(40);
            });
        });
    });

    describe("StatefulSubArtifact changes", () => {
        let viewModel: IProcessViewModel,
            statefulArtifact: StatefulProcessArtifact,
            node: IDiagramNode,
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

            node = new SystemTask(<ISystemTaskShape>statefulArtifact.shapes[0], rootScope, "", null, shapesFactory);

            viewModel = new ProcessViewModel(statefulArtifact, communicationManager);

            graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization, null, null, null, shapesFactory);
        });
        it("when modifying label - labels matches", () => {

            // arrange
            spyOn(statefulArtifact, "refresh")();
            spyOn(statefulArtifact, "lock")();

            // act
            node.render(graph, 80, 120, false);
            node.renderLabels();

            node.label = "test label";

            // assert
            expect(statefulSubArtifact.name).toBe(node.label);
        });

        it("when modifying label - attempt lock is called", () => {

            // arrange
            spyOn(statefulArtifact, "refresh")();
            const lockSpy = spyOn(statefulArtifact, "lock");

            // act
            node.render(graph, 80, 120, false);
            node.renderLabels();

            node.label = "test label";

            // assert
            expect(lockSpy).toHaveBeenCalled();
        });
        it("when modifying label - artifact state is dirty", () => {

            // arrange
            spyOn(statefulArtifact, "refresh")();
            spyOn(statefulArtifact, "lock");

            // act
            node.render(graph, 80, 120, false);
            node.renderLabels();

            node.label = "test label";

            // assert
            expect(statefulArtifact.artifactState.dirty).toBeTruthy();
        });
    });
});
