import * as angular from "angular";
import {ProcessGraph} from "../process-graph";
import {IProcessViewModel, ProcessViewModel} from "../../../viewmodel/process-viewmodel";
import {ShapesFactory} from "./shapes-factory";
import {ProcessModel, ProcessShapeModel, IPropertyValueInformation, IHashMapOfPropertyValues} from "../../../../../models/process-models";
import {ProcessType} from "../../../../../models/enums";
import {DiagramNode} from "./";
import {PropertyTypePredefined} from "../../../../../../../main/models/enums";
import {NodeChange} from "../models/";
import {ICommunicationManager, CommunicationManager} from "../../../../../../bp-process"; 
import {LocalizationServiceMock} from "../../../../../../../core/localization/localization.mock";
import {DialogService} from "../../../../../../../shared/widgets/bp-dialog";
import { ModalServiceMock } from "../../../../../../../shell/login/mocks.spec";
import { IStatefulArtifactFactory } from "../../../../../../../managers/artifact-manager/";
import { StatefulArtifactFactoryMock } from "../../../../../../../managers/artifact-manager/artifact/artifact.factory.mock";

describe("DiagramNode", () => {

    describe("when getting nodes for graph", () => {
        let graph: ProcessGraph;
        let shapesFactory;
        let rootScope: ng.IRootScopeService; 
        let communicationManager: ICommunicationManager,
            dialogService: DialogService,
            localization: LocalizationServiceMock;
        
        beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
            $provide.service("communicationManager", CommunicationManager);
            $provide.service("$uibModal", ModalServiceMock);
            $provide.service("dialogService", DialogService);
            $provide.service("localization", LocalizationServiceMock);
            $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        }));

        beforeEach(inject((
            $rootScope: ng.IRootScopeService, 
            _communicationManager_: ICommunicationManager,
            _dialogService_: DialogService,
            _localization_: LocalizationServiceMock,
            statefulArtifactFactory: IStatefulArtifactFactory) => {

            communicationManager = _communicationManager_;
            dialogService = _dialogService_;
            localization = _localization_;
            rootScope = $rootScope;

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
            shapesFactory = new ShapesFactory(rootScope, statefulArtifactFactory);
        }));


        afterEach(() => {
            graph = null;
        });

        describe("for user task -> system task model", () => {
            var testModel;
            var processModel: IProcessViewModel;

            beforeEach(() => {
                var userTaskModel = shapesFactory.createModelUserTaskShape(1, 1, 77, 0, 0);
                var systemTaskModel = shapesFactory.createModelSystemTaskShape(1, 1, 88, 1, 0);
                var link = { sourceId: 77, destinationId: 88, orderindex: 1, label: "" };

                testModel = new ProcessModel();
                testModel.propertyValues = {};
                testModel.propertyValues["clientType"] = shapesFactory.createClientTypeValueForProcess(ProcessType.UserToSystemProcess);
                testModel.shapes = [];
                testModel.shapes.push(userTaskModel);
                testModel.shapes.push(systemTaskModel);
                testModel.links = [];
                testModel.links.push(link);
                processModel = new ProcessViewModel(testModel, communicationManager);

                var wrapper = document.createElement("DIV");
                var container = document.createElement("DIV");
                wrapper.appendChild(container);
                document.body.appendChild(wrapper);

                graph = new ProcessGraph(rootScope, { graphContainer: container, graphWrapper: wrapper }, 
                                         container, processModel, dialogService, localization);
                graph.render(false, null);
            });

            afterEach(() => {
                processModel = null;
                graph = null;
            });

            it("returns empty list for user task source nodes", () => {
                // Arrange
                var node = graph.getNodeById("77");

                // Act
                var actual = node.getSources(graph.getMxGraphModel());

                // Assert
                expect(actual.length).toEqual(0);
            });
            it("returns empty list for system task target nodes", () => {
                // Arrange
                var node = graph.getNodeById("88");

                // Act
                var actual = node.getTargets(graph.getMxGraphModel());

                // Assert
                expect(actual.length).toEqual(0);
            });
            it("returns user task for system task source nodes", () => {
                // Arrange
                var node = graph.getNodeById("88");

                // Act
                var actual = node.getSources(graph.getMxGraphModel());

                // Assert
                expect(actual.length).toEqual(1);
                expect(actual[0].model).toEqual(processModel.shapes[0]);
            });
            it("returns system task for user task target nodes", () => {
                // Arrange
                var node = graph.getNodeById("77");

                // Act
                var actual = node.getTargets(graph.getMxGraphModel());

                // Assert
                expect(actual.length).toEqual(1);
                expect(actual[0].model).toEqual(processModel.shapes[1]);
            });
            it("returns empty list for user task previous nodes", () => {
                // Arrange
                var node = graph.getNodeById("77");

                // Act
                var actual = node.getPreviousNodes();

                // Assert
                expect(actual.length).toEqual(0);
            });
            it("returns empty list for system task next nodes", () => {
                // Arrange
                var node = graph.getNodeById("88");

                // Act
                var actual = node.getNextNodes();

                // Assert
                expect(actual.length).toEqual(0);
            });
            it("returns user task for system task previous nodes", () => {
                // Arrange
                var node = graph.getNodeById("88");

                // Act
                var actual = node.getPreviousNodes();

                // Assert
                expect(actual.length).toEqual(1);
                expect(actual[0].model).toEqual(processModel.shapes[0]);
            });
            it("returns system task for user task next nodes", () => {
                // Arrange
                var node = graph.getNodeById("77");

                // Act
                var actual = node.getNextNodes();

                // Assert
                expect(actual.length).toEqual(1);
                expect(actual[0].model).toEqual(processModel.shapes[1]);
            });

            it("throws exception when rendered", () => {
                // Arrange
                var model = new ProcessShapeModel();
                var diagramNode = new DiagramNode(model);
                var exception = null;

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
                var model = new ProcessShapeModel();
                var diagramNode = new DiagramNode(model);

                // Act
                var actual = diagramNode.getCenter();

                // Assert
                expect(actual).toEqual(new mxPoint(0, 0));
            });

            it("modifies model's name when name is changed", () => {
                // Arrange
                var oldValue = "Default";
                var newValue = "New Name";
                var model = new ProcessShapeModel();
                model.name = oldValue;
                var label = "label";
                var propertyValue: IPropertyValueInformation = {
                    propertyName: label,
                    typePredefined: PropertyTypePredefined.Label,
                    typeId: 5,
                    value: oldValue
                };
                var propertyValues: IHashMapOfPropertyValues = {};
                propertyValues[label] = propertyValue;
                model.propertyValues = propertyValues;
                var diagramNode = new DiagramNode(model);
                var notifySpy = spyOn(diagramNode, "notify");
                // Act
                diagramNode.action = newValue;

                // Assert
                expect(model.propertyValues[label].value).toEqual(newValue);
            });

            it("notifies of change when name is changed", () => {
                // Arrange
                var oldValue = "Default";
                var newValue = "New Name";
                var model = new ProcessShapeModel();
                model.name = oldValue;
                var diagramNode = new DiagramNode(model);
                var notifySpy = spyOn(diagramNode, "notify");

                // Act
                diagramNode.label = newValue;

                // Assert
                expect(notifySpy).toHaveBeenCalled();
            });

            it("modifies model's 'x' when column is updated", () => {
                // Arrange
                var oldValue = 0;
                var newValue = 1;
                var model = new ProcessShapeModel();
                model.propertyValues = {};

                model.propertyValues[shapesFactory.X.key] = shapesFactory.createXValue(oldValue);
                var diagramNode = new DiagramNode(model);

                // Act
                diagramNode.column = newValue;

                // Assert
                expect(model.propertyValues[shapesFactory.X.key].value).toEqual(newValue);
            });

            it("modifies model's 'y' when row is updated", () => {
                // Arrange
                var oldValue = 0;
                var newValue = 1;
                var model = new ProcessShapeModel();
                model.propertyValues = {};
                model.propertyValues[shapesFactory.Y.key] = shapesFactory.createXValue(oldValue);
                var diagramNode = new DiagramNode(model);

                // Act
                diagramNode.row = newValue;

                // Assert
                expect(model.propertyValues[shapesFactory.Y.key].value).toEqual(newValue);
            });
        });

        describe("for user decision -> user task models", () => {
            var testModel;
            var processModel: IProcessViewModel;

            beforeEach(() => {
                var systemTaskModel = shapesFactory.createModelUserTaskShape(1, 1, 5, 0, 0);
                var userDecisionModel = shapesFactory.createModelUserDecisionShape(1, 1, 10, 1, 0);
                var userTaskModel1 = shapesFactory.createModelUserTaskShape(1, 1, 20, 2, 0);
                var userTaskModel2 = shapesFactory.createModelUserTaskShape(1, 1, 30, 2, 1);
                var userTaskModel3 = shapesFactory.createModelUserTaskShape(1, 1, 40, 2, 2);
                var link0 = { sourceId: 5, destinationId: 10, orderindex: 0, label: "" };
                var link1 = { sourceId: 10, destinationId: 20, orderindex: 0, label: "" };
                var link2 = { sourceId: 10, destinationId: 30, orderindex: 10, label: "" };
                var link3 = { sourceId: 10, destinationId: 40, orderindex: 20, label: "" };

                testModel = new ProcessModel();
                testModel.propertyValues = {};
                testModel.propertyValues["clientType"] = shapesFactory.createClientTypeValueForProcess(ProcessType.UserToSystemProcess);
                testModel.shapes = [systemTaskModel, userDecisionModel, userTaskModel1, userTaskModel2, userTaskModel3];
                testModel.links = [link3, link2, link1, link0];
                processModel = new ProcessViewModel(testModel, communicationManager);

                var wrapper = document.createElement("DIV");
                var container = document.createElement("DIV");
                wrapper.appendChild(container);
                document.body.appendChild(wrapper);

                graph = new ProcessGraph(rootScope, { graphContainer: container, graphWrapper: wrapper }, 
                                         container, processModel, dialogService, localization);
                graph.render(false, null);
            });

            afterEach(() => {
                processModel = null;
                graph = null;
            });

            it("returns empty list for user task sources", () => {
                // Arrange
                var node = graph.getNodeById("5");

                // Act
                var actual = node.getSources(graph.getMxGraphModel());

                // Assert
                expect(actual.length).toEqual(0);
            });

            it("returns user decision for user task target", () => {
                // Arrange
                var node = graph.getNodeById("5");

                // Act
                var actual = node.getTargets(graph.getMxGraphModel());

                // Assert
                expect(actual.length).toEqual(1);
                expect(actual[0].model.id).toEqual(10);
            });

            it("returns user task for user decision sources", () => {
                // Arrange
                var node = graph.getNodeById("10");

                // Act
                var actual = node.getSources(graph.getMxGraphModel());

                // Assert
                expect(actual.length).toEqual(1);
                expect(actual[0].model.id).toEqual(5);
            });

            it("returns user tasks for user decision targets in order of ascending link order index", () => {
                // Arrange
                var node = graph.getNodeById("10");

                // Act
                var actual = node.getTargets(graph.getMxGraphModel());

                // Assert
                expect(actual.length).toEqual(3);
                expect(actual[0].model.id).toEqual(20);
                expect(actual[1].model.id).toEqual(30);
                expect(actual[2].model.id).toEqual(40);
            });
        });
    });    
});