/* tslint:disable max-file-line-count */
import "script!mxClient";
import * as angular from "angular";
import "angular-mocks";
import {ExecutionEnvironmentDetectorMock} from "../../../../../../commonModule/services/executionEnvironmentDetector.mock";
import {ProcessGraph} from "./process-graph";
import {ShapesFactory} from "./shapes/shapes-factory";
import * as Enums from "../../../../models/enums";
import * as ProcessModels from "../../../../models/process-models";
import {IProcessViewModel, ProcessViewModel} from "../../viewmodel/process-viewmodel";
import {IProcessGraphModel, ProcessGraphModel} from "../../viewmodel/process-graph-model";
import {IDiagramNode, IProcessGraph, ProcessLinkModel} from "./models/";
import {UserTask, UserDecision, Condition} from "./shapes/";
import {NodeChange, NodeType} from "./models/";
import {ProcessValidator} from "./process-graph-validator";
import {ProcessDeleteHelper} from "./process-delete-helper";
import {ICommunicationManager, CommunicationManager} from "../../../../../bp-process";
import {LocalizationServiceMock} from "../../../../../../commonModule/localization/localization.service.mock";
import {DialogService} from "../../../../../../shared/widgets/bp-dialog";
import {ModalServiceMock} from "../../../../../../shell/login/mocks.spec";
import {IStatefulArtifactFactory} from "../../../../../../managers/artifact-manager/";
import {StatefulArtifactFactoryMock} from "../../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {FileUploadServiceMock} from "../../../../../../commonModule/fileUpload/fileUpload.service.mock";
import {MessageServiceMock} from "../../../../../../main/components/messages/message.mock";
import {Message, MessageType} from "../../../../../../main/components/messages/message";
import {IMessageService} from "../../../../../../main/components/messages/message.svc";
import {ProcessAddHelper} from "./process-add-helper";
import * as TestModels from "../../../../models/test-model-factory";

describe("ProcessGraph", () => {
    let shapesFactory: ShapesFactory;
    let localScope, rootScope, timeout, wrapper, container, statefulArtifactFactory: IStatefulArtifactFactory;
    let communicationManager: ICommunicationManager,
        dialogService: DialogService,
        messageService: IMessageService,
        localization: LocalizationServiceMock;

    const _window: any = window;
    _window.executionEnvironmentDetector = ExecutionEnvironmentDetectorMock;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("dialogService", DialogService);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("fileUploadService", FileUploadServiceMock);
        $provide.service("shapesFactory", ShapesFactory);
        $provide.service("messageService", MessageServiceMock);
    }));

    beforeEach(inject((_$window_: ng.IWindowService,
                       $rootScope: ng.IRootScopeService,
                       $timeout: ng.ITimeoutService,
                       _communicationManager_: ICommunicationManager,
                       _dialogService_: DialogService,
                       _localization_: LocalizationServiceMock,
                       _statefulArtifactFactory_: IStatefulArtifactFactory,
                       _shapesFactory_: ShapesFactory,
                       _messageService_: IMessageService) => {
        rootScope = $rootScope;
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
            "ST_Auto_Insert_Task": "The task and its associated shapes have been moved. Another task has been created at the old location.",
            "ST_Eighty_Percent_of_Shape_Limit_Reached":
                "The Process now has {0} of the maximum {1} shapes. Please consider refactoring it to move more detailed tasks to included Processes.",
            "ST_Shape_Limit_Exceeded":
             "The shape cannot be added. The Process will exceed the maximum {0} shapes. Please refactor it and move more detailed tasks to included Processes."
        };
        $rootScope["config"].settings = {
            StorytellerShapeLimit: "100",
            StorytellerIsSMB: "false"
        };

        localScope = {graphContainer: container, graphWrapper: wrapper, isSpa: false};
        shapesFactory = new ShapesFactory(rootScope, _statefulArtifactFactory_);
    }));

    describe("isUserSystemProcess", () => {
        it("returns false for no process", () => {
            // Arrange
            // Act
            const graph = createGraph(TestModels.createDefaultProcessModel());

            // Assert
            expect(graph.isUserSystemProcess).toBe(false);
        });

        it("returns false for business process", () => {
            // Arrange
            const process = TestModels.createDefaultProcessModel();
            process.propertyValues["clientType"].value = Enums.ProcessType.BusinessProcess;

            // Act
            const graph = createGraph(process);

            // Assert
            expect(graph.isUserSystemProcess).toBe(false);
        });

        it("returns false for system-to-system process", () => {
            // Arrange
            const process = TestModels.createDefaultProcessModel();
            process.propertyValues["clientType"].value = Enums.ProcessType.SystemToSystemProcess;

            // Act
            const graph = createGraph(process);

            // Assert
            expect(graph.isUserSystemProcess).toBe(false);
        });

        it("returns true for user-to-system process", () => {
            // Arrange
            const process = TestModels.createDefaultProcessModel();
            process.propertyValues["clientType"].value = Enums.ProcessType.UserToSystemProcess;

            // Act
            const graph = createGraph(process);

            // Assert
            expect(graph.isUserSystemProcess).toBe(true);
        });
    });

    describe("render", () => {
        it("adds error message when error is raised", () => {
            // Arrange
            const graph = createGraph(TestModels.createDefaultProcessModel());
            const message = "Test message";
            const renderSpy = spyOn(graph.layout, "render").and.throwError(message);
            const addErrorSpy = spyOn(messageService, "addError");

            // Act
            graph.render(true, null);

            // Assert
            expect(renderSpy).toHaveBeenCalled();
            expect(addErrorSpy).toHaveBeenCalled();
        });
    });

    describe("redraw", () => {
        it("does nothing when provided no action", () => {
            // Arrange
            const graph = createGraph(TestModels.createDefaultProcessModel());
            const getModelSpy = spyOn(graph.getMxGraph(), "getModel").and.callThrough();

            // Act
            graph.redraw(null);

            // Assert
            expect(getModelSpy).not.toHaveBeenCalled();
        });

        it("executes and redraws when provided an action", () => {
            // Arrange
            const test = {
                action: function () {
                    /* no op */
                }
            };
            const graph = createGraph(TestModels.createDefaultProcessModel());
            const model = graph.getMxGraphModel();
            const getModelSpy = spyOn(graph.getMxGraph(), "getModel").and.callThrough();
            const beginUpdateSpy = spyOn(model, "beginUpdate").and.callThrough();
            const actionSpy = spyOn(test, "action").and.callThrough();
            const endUpdateSpy = spyOn(model, "endUpdate").and.callThrough();

            // Act
            graph.redraw(test.action);

            // Assert
            expect(getModelSpy).toHaveBeenCalled();
            expect(beginUpdateSpy).toHaveBeenCalled();
            expect(actionSpy).toHaveBeenCalled();
            expect(endUpdateSpy).toHaveBeenCalled();
        });
    });

    describe("delete user task", () => {
        let process: ProcessModels.IProcess;
        let clientModel: IProcessGraphModel;
        let viewModel: IProcessViewModel;
        let graph: IProcessGraph;

        describe("that doesn't exist in process", () => {

            beforeEach(() => {
                process = TestModels.createDefaultProcessModel();
                clientModel = new ProcessGraphModel(process);
                viewModel = new ProcessViewModel(clientModel, communicationManager);
                graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization, shapesFactory, null, null, null);

                //bypass testing remove stateful shapes logic here
                spyOn(viewModel, "removeStatefulShape").and.returnValue(null);
            });

            afterEach(() => {
                process = null;
                clientModel = null;
                viewModel = null;
                graph = null;
            });

            it("fails", () => {
                // Arrange
                const userTaskId = 999;

                // Act
                const result = ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);

                // Assert
                expect(result).toBe(false);
            });

            it("doesn't modify shapes or links", () => {
                // Arrange
                const userTaskId = 999;
                const shapeLengthBeforeDelete = process.shapes.length;
                const linkLengthBeforeDelete = process.links.length;

                // Act
                ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);

                // Assert
                expect(process.shapes.length).toEqual(shapeLengthBeforeDelete); // 7
                expect(process.links.length).toEqual(linkLengthBeforeDelete);   // 6
            });
        });

        describe("that is the last user task in default process", () => {

            beforeEach(() => {
                process = TestModels.createDefaultProcessModel();
                clientModel = new ProcessGraphModel(process);
                viewModel = new ProcessViewModel(clientModel, communicationManager);
                graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization, shapesFactory, null, null, null);

                //bypass testing remove stateful shapes logic here
                spyOn(viewModel, "removeStatefulShape").and.returnValue(null);
            });

            afterEach(() => {
                process = null;
                clientModel = null;
                viewModel = null;
                graph = null;
            });

            it("fails", () => {
                //Arrange
                const userTaskId = 20;

                //Act
                const result = ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);

                //Assert
                expect(result).toBe(false);
            });

            it("doesn't modify shapes or links", () => {
                //Arrange
                const shapeLengthBeforeDelete = process.shapes.length;
                const linkLengthBeforeDelete = process.links.length;
                const userTaskId = 20;

                //Act
                ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);

                //Assert
                expect(process.shapes.length).toEqual(shapeLengthBeforeDelete); //5
                expect(process.links.length).toEqual(linkLengthBeforeDelete);   //4
            });
        });

        // Start -> Pre -> UT1 -> ST1 -> UT2 -> ST2 -> End
        it("deletes user task in a process with multiple user tasks and without any decisions", () => {
            // Arrange
            process = TestModels.createTwoUserTaskModel();
            clientModel = new ProcessGraphModel(process);
            viewModel = new ProcessViewModel(clientModel, communicationManager);
            graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization, shapesFactory, null, null, null);

            //bypass testing remove stateful shapes logic here
            spyOn(viewModel, "removeStatefulShape").and.returnValue(null);

            // Act
            const result = ProcessDeleteHelper.deleteUserTask(35, null, graph);

            // Assert
            expect(result).toBe(true);
            expect(process.shapes.length).toEqual(5);
            expect(hasShapes(process, 35, 40)).toBe(false);
        });

        describe("from user decision with two conditions", () => {
            it("fails for user task in a second condition", () => {
                // Arrange
                process = TestModels.createUserDecisionWithTwoBranchesModel();
                graph = createGraph(process);
                const userTaskId = 7;

                // Act
                const result = ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);

                // Assert
                expect(result).toBe(false);
            });

            it("adds error message when cannot delete", () => {
                // Arrange
                process = TestModels.createUserDecisionWithTwoBranchesModel();
                graph = createGraph(process);
                const userTaskId = 7;
                const spy = spyOn(messageService, "addError").and.callThrough();

                // Act
                const result = ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);

                // Assert
                expect(result).toBe(false);
                expect(spy).toHaveBeenCalledWith(rootScope.config.labels["ST_Delete_CannotDelete_UD_AtleastTwoConditions"]);
            });
        });

        describe("in user decision with three branches", () => {

            beforeEach(() => {
                process = TestModels.createUserDecisionWithThreeConditionsModel();
                // Start -> Pre -> UD -> UT1 -> ST1 -> End
                //                       UT2 -> ST2 -> End
                //                       UT3 -> ST3 -> End
                // UT1 = 40
                // UT2 = 60
                // UT3 = 80
                clientModel = new ProcessGraphModel(process);
                viewModel = new ProcessViewModel(clientModel, communicationManager);
                graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization, shapesFactory, null, null, null);

                //bypass testing remove stateful shapes logic here
                spyOn(viewModel, "removeStatefulShape").and.returnValue(null);
            });

            afterEach(() => {
                process = null;
                graph = null;
            });

            it("allows deleting the only user task in the first branch", () => {
                // Arrange
                const userTaskId = 40;

                // Act
                const result = ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);

                // Assert
                expect(result).toBe(true);
            });

            it("allows deleting the only user task in second condition", () => {
                // Arrange
                const userTaskId = 60;

                // Act
                const result = ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);

                // Assert
                expect(result).toBe(true);
            });

            it("deletes the whole second condition when deleting the only user task on that branch", () => {
                // Arrange
                const userTaskId = 60;
                const systemTaskId = 70;
                const userDecisionId = 30;
                const conditionDestinationCountBefore = viewModel.decisionBranchDestinationLinks.length;

                // Act
                ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);
                const conditionDestinationCountAfter = viewModel.decisionBranchDestinationLinks.length;

                // Assert
                expect(process.shapes.length).toBe(8);
                expect(hasShapes(process, userTaskId, systemTaskId)).toBe(false);
                expect(hasLinksFor(process, userTaskId, systemTaskId)).toBe(false);
                expect(process.links.filter((link) => link.sourceId === userDecisionId).length).toBe(2);
                expect(conditionDestinationCountBefore).toBe(2);
                expect(conditionDestinationCountAfter).toBe(1);
            });

            it("allows deleting the only user task in third condition", () => {
                // Arrange
                const userTaskId = 80;

                // Act
                const result = ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);

                // Assert
                expect(result).toBe(true);
            });

            it("deletes the whole third condition when deleting the only user task in that branch", () => {
                // Arrange
                const userTaskId = 80;
                const systemTaskId = 90;
                const userDecisionId = 30;
                const conditionDestinationCountBefore = viewModel.decisionBranchDestinationLinks.length;

                // Act
                ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);
                const conditionDestinationCountAfter = viewModel.decisionBranchDestinationLinks.length;

                // Assert
                expect(process.shapes.length).toBe(8);
                expect(hasShapes(process, userTaskId, systemTaskId)).toBe(false);
                expect(hasLinksFor(process, userTaskId, systemTaskId)).toBe(false);
                expect(process.links.filter((link) => link.sourceId === userDecisionId).length).toBe(2);
                expect(conditionDestinationCountBefore).toBe(2);
                expect(conditionDestinationCountAfter).toBe(1);
            });
        });

        describe("in user decision with three branches with two user tasks on each branch", () => {

            beforeEach(() => {
                process = TestModels.createUserDecisionWithThreeConditionsAndTwoUserTasksModel();
                clientModel = new ProcessGraphModel(process);
                viewModel = new ProcessViewModel(clientModel, communicationManager);
                graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization, shapesFactory, null, null, null);

                //bypass testing remove stateful shapes logic here
                spyOn(viewModel, "removeStatefulShape").and.returnValue(null);
            });

            afterEach(() => {
                process = null;
                graph = null;
            });

            it("allows deleting the first user task int the first condition", () => {
                // Arrange
                const userTaskId = 40;

                // Act
                const result = ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);

                // Assert
                expect(result).toBe(true);
            });

            it("deletes the first user task in the first condition", () => {
                // Arrange
                const userTaskId = 40;
                const systemTaskId = 50;
                const nextUserTaskId = 60;
                const userDecisionId = 30;

                // Act
                ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);

                // Assert
                expect(process.shapes.length).toBe(14);
                expect(hasShapes(process, userTaskId, systemTaskId)).toBe(false);
                expect(hasLinksFor(process, userTaskId, systemTaskId)).toBe(false);
                expect(hasLink(process, userDecisionId, nextUserTaskId)).toBe(true);
            });

            it("allows deleting the second user task in the first condition", () => {
                // Arrange
                const userTaskId = 60;

                // Act
                const result = ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);

                // Assert
                expect(result).toBe(true);
            });

            it("deletes the second user task in the first condition", () => {
                // Arrange
                const userTaskId = 60;
                const systemTaskId = 70;
                const previousSystemTaskId = 50;
                const endId = 160;

                // Act
                ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);

                // Assert
                expect(process.shapes.length).toBe(14);
                expect(hasShapes(process, userTaskId, systemTaskId)).toBe(false);
                expect(hasLinksFor(process, userTaskId, systemTaskId)).toBe(false);
                expect(hasLink(process, previousSystemTaskId, endId)).toBe(true);
            });

            it("allows deleting the first user task in a second condition", () => {
                // Arrange
                const userTaskId = 80;

                // Act
                const result = ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);

                // Assert
                expect(result).toBe(true);
            });

            it("deletes the first user task in the second condition", () => {
                // Arrange
                const userTaskId = 80;
                const systemTaskId = 90;
                const nextUserTaskId = 100;
                const userDecisionId = 30;

                // Act
                ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);

                // Assert
                expect(process.shapes.length).toBe(14);
                expect(hasShapes(process, userTaskId, systemTaskId)).toBe(false);
                expect(hasLinksFor(process, userTaskId, systemTaskId)).toBe(false);
                expect(hasLink(process, userDecisionId, nextUserTaskId)).toBe(true);
            });

            it("allows deleting the second user task in a second condition", () => {
                // Arrange
                const userTaskId = 100;

                // Act
                const result = ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);

                // Assert
                expect(result).toBe(true);
            });

            it("deletes the second user task in the second condition", () => {
                // Arrange
                const userTaskId = 100;
                const systemTaskId = 110;
                const previousSystemTaskId = 90;
                const endId = 160;

                // Act
                ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);

                // Assert
                expect(process.shapes.length).toBe(14);
                expect(hasShapes(process, userTaskId, systemTaskId)).toBe(false);
                expect(hasLinksFor(process, userTaskId, systemTaskId)).toBe(false);
                expect(hasLink(process, previousSystemTaskId, endId)).toBe(true);
            });
        });

        it("fails for user task in a nested user decision condition", () => {
            // Start -> Pre -> UD1 -> UT1 -> ST1 -> UD3 -> UT5 -> ST5 -> End
            //                                             UT6 -> ST6 -> End
            //                        UT2 -> ST2 -> UD2 -> UT3 -> ST3 -> UD3
            //                                          -> UT4 -> ST4 -> UD3
            // Attempt to delete UT3

            // Arrange
            process = TestModels.createUserDecisionInSecondConditionModel();
            graph = createGraph(process);
            const userTaskId = 90;
            const shapesNumBefore = process.shapes.length;
            const spy = spyOn(ProcessDeleteHelper, "deleteUserTaskInternal");

            // Act
            const result = ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);

            // Assert
            expect(result).toBe(false);
            expect(spy).not.toHaveBeenCalled();
            expect(process.shapes.length).toBe(shapesNumBefore);
        });

        it("reconnects links joined into the scope", () => {
            // Arrange
            process = TestModels.createMergingSystemDecisionsModel();
            graph = createGraph(process);
            (<ProcessGraph>graph).initializeGlobalScope();

            // Act
            const result = ProcessDeleteHelper.deleteUserTask(70, null, graph);

            // Assert
            expect(result).toBe(true);
            expect(hasLinksFor(process, 70, 80, 90, 100, 110, 120)).toBe(false);
            expect(hasLink(process, 60, 140)).toBe(true);
            expect(hasLink(process, 130, 140)).toBe(true);
        });

        it("reconnects links for infinite loop", () => {
            // Arrange
            process = TestModels.createMergingSystemDecisionsWithInfiniteLoopModel();
            graph = createGraph(process);
            (<ProcessGraph>graph).initializeGlobalScope();

            // Act
            const result = ProcessDeleteHelper.deleteUserTask(70, null, graph);

            // Assert
            expect(result).toBe(true);
            expect(hasLinksFor(process, 70, 80, 90, 100, 110, 120)).toBe(false);
            expect(hasLink(process, 60, 140)).toBe(true);
            expect(hasLink(process, 130, 40)).toBe(true);
        });

        it("fails for user task that is the only user task in process but has other user tasks in its system decisions", () => {
            //Arrange
            const testModel = TestModels.createSimpleProcessModelWithSystemDecision();
            const processModel = new ProcessViewModel(testModel, communicationManager);
            graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            const shapeLengthBeforeDelete = processModel.shapes.length;
            const linkLengthBeforeDelete = processModel.links.length;

            const userTaskShape = shapesFactory.createModelUserTaskShape(2, 1, 20, 2, 0);
            const userTaskShapeDiagramNode = new UserTask(userTaskShape, rootScope, null, shapesFactory);

            //Act
            const result = ProcessDeleteHelper.deleteUserTask(userTaskShapeDiagramNode.model.id, null, graph);

            //Assert
            expect(processModel.shapes.length).toEqual(shapeLengthBeforeDelete); //5
            expect(processModel.links.length).toEqual(linkLengthBeforeDelete);  //4
            expect(result).toBe(false);
        });

        it("simple case, correct number of shapes/links", () => {

            /* Before deletion:
             start-> ST1 -> UT -> ST2 -> UT1 -> ST3 -> END

             After deletion:
             start -> ST1 -> UT1 -> ST3 -> END
             */

            //Arrange
            const userTaskShape = shapesFactory.createModelUserTaskShape(2, 1, 20, 2, 0);
            const testModel = TestModels.createDeleteUserTaskSimpleModel(userTaskShape);
            const processModel = new ProcessViewModel(testModel, communicationManager);

            //bypass testing remove stateful shapes logic here
            spyOn(processModel, "removeStatefulShape").and.returnValue(null);

            const userTaskShapeDiagramNode = new UserDecision(userTaskShape, rootScope);

            graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

            //Act
            const result = ProcessDeleteHelper.deleteUserTask(userTaskShapeDiagramNode.model.id, null, graph);

            //Assert
            expect(processModel.shapes.length).toEqual(5);
            expect(processModel.links.length).toEqual(4);
            expect(result).toBe(true);
        });

        it("deletes system decision after it (with all shapes in branch), correct number of shapes/links", () => {

            /* Before deletion:
             start -> ST1 -> UT1 -> ST2 -> UT2 -> SD -> ST3 -> END
             -> ST3 -> END

             After deletion:
             start -> ST1 -> UT1 -> ST2 -> END
             */

            //Arrange
            const userTaskShape = shapesFactory.createModelUserTaskShape(2, 1, 280, 4, 0);
            const testModel = TestModels.createUserTaskFollowedBySystemDecision(userTaskShape);
            const processModel = new ProcessViewModel(testModel, communicationManager);

            //bypass testing remove stateful shapes logic here
            spyOn(processModel, "removeStatefulShape").and.returnValue(null);

            const userTaskShapeDiagramNode = new UserDecision(userTaskShape, rootScope);

            graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

            //Act
            const result = ProcessDeleteHelper.deleteUserTask(userTaskShapeDiagramNode.model.id, null, graph);

            //Assert
            expect(processModel.shapes.length).toEqual(5);
            expect(processModel.links.length).toEqual(4);
            expect(result).toBe(true);
        });

        it("deletes system decision after it (with all shapes in branch), correct link source and destination", () => {

            /* Before deletion:
             start -> ST1 -> UT1 -> ST2 -> UT2 -> SD -> ST3 -> END
             -> ST3 -> END

             After deletion:
             start -> ST1 -> UT1 -> ST2 -> END
             */

            //Arrange
            const userTaskShape = shapesFactory.createModelUserTaskShape(2, 1, 280, 4, 0);
            const testModel = TestModels.createUserTaskFollowedBySystemDecision(userTaskShape);
            const processModel = new ProcessViewModel(testModel, communicationManager);

            //bypass testing remove stateful shapes logic here
            spyOn(processModel, "removeStatefulShape").and.returnValue(null);

            const userTaskShapeDiagramNode = new UserDecision(userTaskShape, rootScope);

            graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

            const shapeIdBeforeUserTask = processModel.links.filter(a => a.destinationId === userTaskShape.id)[0].sourceId;

            const destinationId = processModel.decisionBranchDestinationLinks[0].destinationId;

            //Act
            const result = ProcessDeleteHelper.deleteUserTask(userTaskShapeDiagramNode.model.id, null, graph);

            const linksContainingDecision = processModel.links.filter(a => a.sourceId === userTaskShapeDiagramNode.id ||
            a.destinationId === userTaskShapeDiagramNode.id).length;
            const updatedLink = processModel.links.filter(a => a.sourceId === shapeIdBeforeUserTask);

            // Assert
            expect(linksContainingDecision).toEqual(0);
            expect(updatedLink.length).toEqual(1);
            expect(updatedLink[0].sourceId).toEqual(shapeIdBeforeUserTask);
            expect(updatedLink[0].destinationId).toEqual(destinationId);
            expect(processModel.decisionBranchDestinationLinks.length).toBe(0);
            expect(result).toBe(true);
        });

        it("executes action after delete if one is provided", () => {
            // Arrange
            process = TestModels.createTwoUserTaskModel();
            graph = createGraph(process);
            const userTaskId = 35;
            const test = {action: null};
            const spy = spyOn(test, "action");

            // Act
            const result = ProcessDeleteHelper.deleteUserTask(userTaskId, test.action, graph);

            // Assert
            expect(result).toBe(true);
            expect(spy).toHaveBeenCalledWith(NodeChange.Remove, userTaskId);
        });

        it("infinite loop, delete task, success", () => {
            const testModel = TestModels.createUserDecisionInfiniteLoopModel();
            // Start -> Pre -> UD -> UT1 -> ST1 -> End
            //                       UT2 -> ST2 -> UT3 -> ST3 -> UT5
            //                       UT4 -> ST4 -> UT5 -> ST5 -> UT3
            const ST2 = 70;
            const UT3 = 80;
            const UT5 = 120;
            const ST5 = 130;
            const END = 140;
            const processModel = new ProcessViewModel(testModel, communicationManager);
            graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

            //bypass testing remove stateful shapes logic here
            spyOn(processModel, "removeStatefulShape").and.returnValue(null);

            // Act
            const result = ProcessDeleteHelper.deleteUserTask(UT3, null, graph);

            // Assert
            processModel.updateTree();
            const errorMessages: string[] = [];
            const validator = new ProcessValidator();
            validator.isValid(processModel, rootScope, errorMessages);

            expect(result).toBe(true);
            expect(errorMessages.length).toBe(0);
            expect(processModel.getNextShapeIds(ST2).length).toBe(1);
            expect(processModel.getNextShapeIds(ST2)[0]).toBe(UT5);
            expect(processModel.getNextShapeIds(ST5).length).toBe(1);
            expect(processModel.getNextShapeIds(ST5)[0]).toBe(END);
        });
        //Bug 1086
        it("infinite loops, different decisions, delete task in loop, success", () => {
            //Arrange
            const testModel = TestModels.createInfiniteLoopFromDifferentDecisions();
            const processModel = new ProcessViewModel(testModel, communicationManager);
            graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

            //bypass testing remove stateful shapes logic here
            spyOn(processModel, "removeStatefulShape").and.returnValue(null);

            const utToDelete = 210;

            const incomingSystemTaskSameCondition = 200;
            const incomingSystemTaskDifferentCondition = 110;

            //Act
            ProcessDeleteHelper.deleteUserTask(utToDelete, null, graph);
            (<ProcessGraph>graph).initializeGlobalScope();

            //Assert
            expect(processModel.getNextShapeIds(incomingSystemTaskSameCondition)[0]).toBe(100);
            expect(processModel.getNextShapeIds(incomingSystemTaskDifferentCondition)[0]).toBe(140);
        });
    });

    describe("delete decision", () => {
        it("fails if user decision does not exist in the process", () => {
            // Before deletion:
            //   Start -> Pre -> UT1 -> ST1 -> UD -> UT2 -> ST2 -> End
            //                                    -> UT3 -> ST3 -> End

            //Arrange
            const decisionShape = shapesFactory.createModelUserDecisionShape(2, 1, 30, 0, 0);
            // otherDecisionShape does not exist in the graph shapes.
            const otherDecisionShape = shapesFactory.createModelUserDecisionShape(2, 1, 999, 0, 0);
            const testModel = TestModels.createUserDecisionTestModel(decisionShape);
            const processModel = new ProcessViewModel(testModel, communicationManager);

            //bypass testing remove stateful shapes logic here
            spyOn(processModel, "removeStatefulShape").and.returnValue(null);

            const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            const shapeLengthBeforeDelete = processModel.shapes.length;
            const linkLengthBeforeDelete = processModel.links.length;

            //Act
            ProcessDeleteHelper.deleteDecision(otherDecisionShape.id, null, graph, shapesFactory);

            //Assert
            expect(processModel.shapes.length).toEqual(shapeLengthBeforeDelete);
            expect(processModel.shapes.length).toEqual(10);
            expect(processModel.links.length).toEqual(linkLengthBeforeDelete);
            expect(processModel.links.length).toEqual(10);
        });

        it("deletes shapes and links for user decision", () => {
            // Before deletion:
            //  Start -> Pre -> UT1 -> ST1 -> UD -> UT2 -> ST2 -> End
            //                                   -> UT3 -> ST3 -> End

            // After deletion:
            //  Start -> Pre -> UT1 -> ST1 -> UT2 -> ST2 -> End

            // User Decision = 35, End = 30

            //Arrange
            const decisionShape = shapesFactory.createModelUserDecisionShape(2, 1, 30, 0, 0);
            const testModel = TestModels.createUserDecisionTestModel(decisionShape);
            const processModel = new ProcessViewModel(testModel, communicationManager);

            //bypass testing remove stateful shapes logic here
            spyOn(processModel, "removeStatefulShape").and.returnValue(null);

            const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            const shapeLengthBeforeDelete = processModel.shapes.length;
            const linkLengthBeforeDelete = processModel.links.length;

            //Act
            ProcessDeleteHelper.deleteDecision(decisionShape.id, null, graph, shapesFactory);

            //Assert
            expect(processModel.shapes.length).not.toEqual(shapeLengthBeforeDelete);
            expect(processModel.shapes.length).toEqual(7);
            expect(processModel.links.length).not.toEqual(linkLengthBeforeDelete);
            expect(processModel.links.length).toEqual(6);
            expect(processModel.decisionBranchDestinationLinks.length).toBe(0);
        });

        it("has correct links for user decision after deletion", () => {
            // Before deletion:
            //  Start -> Pre -> UT1 -> ST1 -> UD -> UT2 -> ST2 -> End
            //                                   -> UT3 -> ST3 -> End

            // After deletion:
            //  Start -> Pre -> UT1 -> ST1 -> UT2 -> ST2 -> End

            // User Decision = 35, End = 30

            //Arrange
            const decisionShape = shapesFactory.createModelUserDecisionShape(2, 1, 30, 0, 0);
            const testModel = TestModels.createUserDecisionTestModel(decisionShape);
            const processModel = new ProcessViewModel(testModel, communicationManager);

            //bypass testing remove stateful shapes logic here
            spyOn(processModel, "removeStatefulShape").and.returnValue(null);

            const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            const shapeIdBeforeDecision = processModel.links.filter(a => a.destinationId === decisionShape.id)[0].sourceId;
            const shapeIdToConnectAfterDecision = processModel.links
                .filter(a => a.sourceId === decisionShape.id)
                .reduce((a, b) => a.orderindex < b.orderindex ? a : b).destinationId;

            //Act
            ProcessDeleteHelper.deleteDecision(decisionShape.id, null, graph, shapesFactory);

            //Assert
            const linksContainingDecision = processModel.links.filter(a => a.sourceId === decisionShape.id || a.destinationId === decisionShape.id).length;
            const updatedLink = processModel.links.filter(a => a.sourceId === shapeIdBeforeDecision);

            expect(linksContainingDecision).toEqual(0);
            expect(updatedLink.length).toEqual(1);
            expect(updatedLink[0].destinationId).toEqual(shapeIdToConnectAfterDecision);
            expect(processModel.decisionBranchDestinationLinks.length).toBe(0);
        });

        it("fails if system decision not in the process", () => {
            // Before deletion:
            //  Start -> Pre -> UT1 -> SD ->  ST1 -> End
            //                            ->  ST2 -> End

            //Arrange
            const decisionShape = shapesFactory.createSystemDecisionShapeModel(2, 1, 25, 0, 0);
            // otherDecisionShape does not exist in the graph shapes.
            const otherDecisionShape = shapesFactory.createModelUserDecisionShape(2, 1, 999, 0, 0);
            const testModel = TestModels.createSystemDecisionTestModel(decisionShape);
            const processModel = new ProcessViewModel(testModel, communicationManager);

            //bypass testing remove stateful shapes logic here
            spyOn(processModel, "removeStatefulShape").and.returnValue(null);

            const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            const shapeLengthBeforeDelete = processModel.shapes.length;
            const linkLengthBeforeDelete = processModel.links.length;

            //Act
            ProcessDeleteHelper.deleteDecision(otherDecisionShape.id, null, graph, shapesFactory);

            //Assert

            expect(processModel.shapes.length).toEqual(shapeLengthBeforeDelete);
            expect(processModel.shapes.length).toEqual(7);
            expect(processModel.links.length).toEqual(linkLengthBeforeDelete);
            expect(processModel.links.length).toEqual(7);
        });

        it("has correct shapes for system decision after deletion", () => {
            // Before deletion:
            //  Start -> Pre -> UT1 -> SD ->  ST1 -> End
            //                            ->  ST2 -> End

            // After deletion:
            //  Start -> Pre -> UT1 -> ST1 -> End

            //Arrange
            const decisionShape = shapesFactory.createSystemDecisionShapeModel(2, 1, 25, 0, 0);
            const testModel = TestModels.createSystemDecisionTestModel(decisionShape);
            const processModel = new ProcessViewModel(testModel, communicationManager);

            //bypass testing remove stateful shapes logic here
            spyOn(processModel, "removeStatefulShape").and.returnValue(null);

            const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            const shapeLengthBeforeDelete = processModel.shapes.length;
            const linkLengthBeforeDelete = processModel.links.length;

            //Act
            ProcessDeleteHelper.deleteDecision(decisionShape.id, null, graph, shapesFactory);

            //Assert
            expect(processModel.shapes.length).not.toEqual(shapeLengthBeforeDelete);
            expect(processModel.shapes.length).toEqual(5);
            expect(processModel.links.length).not.toEqual(linkLengthBeforeDelete);
            expect(processModel.links.length).toEqual(4);
            expect(processModel.decisionBranchDestinationLinks.length).toBe(0);
        });

        it("has correct links for system decision after deletion", () => {
            // Before deletion:
            //  Start -> Pre -> UT1 -> SD ->  ST1 -> End
            //                            ->  ST2 -> End

            // After deletion:
            //  Start -> Pre -> UT1 -> ST1 -> End

            //Arrange
            const decisionShape = shapesFactory.createSystemDecisionShapeModel(2, 1, 25, 0, 0);
            const testModel = TestModels.createSystemDecisionTestModel(decisionShape);
            const processModel = new ProcessViewModel(testModel, communicationManager);

            //bypass testing remove stateful shapes logic here
            spyOn(processModel, "removeStatefulShape").and.returnValue(null);

            const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            const shapeIdBeforeDecision = processModel.links.filter(a => a.destinationId === decisionShape.id)[0].sourceId;
            const shapeIdToConnectAfterDecision = processModel.links
                .filter(a => a.sourceId === decisionShape.id)
                .reduce((a, b) => a.orderindex < b.orderindex ? a : b).destinationId;

            //Act
            ProcessDeleteHelper.deleteDecision(decisionShape.id, null, graph, shapesFactory);

            //Assert
            const linksContainingDecision = processModel.links.filter(a => a.sourceId === decisionShape.id || a.destinationId === decisionShape.id).length;
            const updatedLink = processModel.links.filter(a => a.sourceId === shapeIdBeforeDecision);

            expect(linksContainingDecision).toEqual(0);
            expect(updatedLink.length).toEqual(1);
            expect(updatedLink[0].destinationId).toEqual(shapeIdToConnectAfterDecision);
            expect(processModel.decisionBranchDestinationLinks.length).toBe(0);
        });

        it("succeeds if decision has no-op and is the last decision in the process and creates new user task and system task", () => {
            // Arrange
            const process = TestModels.createUserDecisionWithoutUserTaskInFirstConditionModel();
            const graph = createGraph(process);
            const decisionId = 40;
            const test = {
                action: function () {
                    /* no op */
                }
            };
            const spy = spyOn(test, "action").and.callThrough();
            spyOn(messageService, "addMessage").and.callThrough();

            // Act
            const result = ProcessDeleteHelper.deleteDecision(decisionId, test.action, graph, shapesFactory);

            // Assert
            expect(result).toBe(true);
            expect(hasTypes(process, Enums.ProcessShapeType.UserTask, Enums.ProcessShapeType.SystemTask));
            expect(spy).toHaveBeenCalled();

            //TODO: Add back when message service is implemented in layout.ts
            //expect(addMessageSpy).toHaveBeenCalled();
        });
    });

    describe("decision conditions", () => {
        it("has minimum number less than or equal to maximum number", () => {
            expect(ProcessGraph.MinConditions <= ProcessGraph.MaxConditions).toBe(true);
        });

        describe("add", () => {
            it("fails for non-decision id", () => {
                // Arrange
                const graph = createGraph(TestModels.createUserDecisionWithTwoBranchesModel());
                const startId = 2;
                const endId = 9;
                let error = null;
                const spy = spyOn(graph, "notifyUpdateInModel").and.callThrough();

                // Act
                try {
                    graph.addDecisionBranch(startId, "", endId);
                } catch (exception) {
                    error = exception;
                }

                // Assert
                expect(error).not.toBeNull();
                expect(error.message).toBe("Expected a decision type but found " + <number>Enums.ProcessShapeType.Start);
                expect(spy).not.toHaveBeenCalled();
            });

            it("fails if null for label is provided", () => {
                // Arrange
                const graph = createGraph(TestModels.createUserDecisionWithTwoBranchesModel());
                const decisionId = 4;
                const endId = 9;
                const spy = spyOn(graph, "notifyUpdateInModel").and.callThrough();

                // Act
                graph.addDecisionBranch(decisionId, null, endId);

                // Assert
                expect(spy).not.toHaveBeenCalled();
            });

            it("fails if null for merge node id is provided", () => {
                // Arrange
                const graph = createGraph(TestModels.createUserDecisionWithTwoBranchesModel());
                const decisionId = 4;
                const spy = spyOn(graph, "notifyUpdateInModel").and.callThrough();

                // Act
                graph.addDecisionBranch(decisionId, "Condition 1", null);

                // Assert
                expect(spy).not.toHaveBeenCalled();
            });

            describe("to user decision", () => {
                it("succeeds if condition is provided", () => {
                    // Arrange
                    const graph = createGraph(TestModels.createUserDecisionWithTwoBranchesModel());
                    const decisionId = 4;
                    const endId = 9;
                    let error = null;
                    const spy = spyOn(ProcessAddHelper, "insertUserDecisionCondition").and.callThrough();

                    // Act
                    try {
                        graph.addDecisionBranch(decisionId, "Condition 1", endId);
                    } catch (exception) {
                        error = exception;
                    }

                    // Assert
                    expect(error).toBeNull();
                    expect(spy).toHaveBeenCalled();
                });

                it("succeeds if no user task exist in first condition", () => {
                    // Arrange
                    const process = TestModels.createUserDecisionWithoutUserTaskInFirstConditionModel();
                    const graph = createGraph(process);
                    const decisionId = 40;
                    const endId = 70;
                    const spy = spyOn(ProcessAddHelper, "insertUserDecisionCondition").and.callThrough();

                    // Act
                    graph.addDecisionBranch(decisionId, "Condition 1", endId);

                    // Assert
                    expect(spy).toHaveBeenCalled();
                    expect(process.links.filter(link => link.sourceId === decisionId).length).toBe(3);
                });

                it("fails if maximum number of conditions reached", () => {
                    // Arrange
                    const graph = createGraph(TestModels.createUserDecisionWithMaximumConditionsModel());
                    const decisionId = 40;
                    const endId = 250;
                    const spy = spyOn(ProcessAddHelper, "insertUserDecisionCondition").and.callThrough();
                    const addErrorSpy = spyOn(messageService, "addError").and.callThrough();

                    // Act
                    graph.addDecisionBranch(decisionId, "Condition 1", endId);

                    // Assert
                    expect(spy).not.toHaveBeenCalled();
                    expect(addErrorSpy).toHaveBeenCalledWith(rootScope.config.labels["ST_Add_CannotAdd_MaximumConditionsReached"]);
                });

                it("succeed if conditions being added are at the limit of number of shapes", () => {
                    // Arrange
                    const graph = createGraph(TestModels.createUserDecisionWithFourShapesLessThanMaximumShapesModel());
                    const decisionId = 50;
                    const endId = 160;
                    const spy = spyOn(ProcessAddHelper, "insertUserDecisionCondition").and.callThrough();
                    const messageText =
                        "The Process now has 14 of the maximum 14 shapes. Please consider refactoring it to move more detailed tasks to included Processes.";
                    const message = new Message(MessageType.Warning, messageText);
                    const addMessageSpy = spyOn(messageService, "addMessage").and.callThrough();

                    graph.viewModel.shapeLimit = 14;

                    // Act
                    graph.addDecisionBranch(decisionId, "Condition 1", endId);
                    graph.addDecisionBranch(decisionId, "Condition 2", endId);
                    graph.viewModel.shapeLimit = 100;

                    // Assert
                    expect(spy).toHaveBeenCalled();
                    expect(addMessageSpy).toHaveBeenCalledWith(message);
                });

                it("fails if conditions being added are more than the limit of number of shapes", () => {
                    // Arrange
                    const graph = createGraph(TestModels.createUserDecisionWithFourShapesLessThanMaximumShapesModel());
                    const decisionId = 50;
                    const endId = 160;
                    const messageText =
"The shape cannot be added. The Process will exceed the maximum 14 shapes. Please refactor it and move more detailed tasks to included Processes.";
                    const message = new Message(MessageType.Error, messageText);
                    graph.addDecisionBranch(decisionId, "Condition 1", endId);
                    graph.addDecisionBranch(decisionId, "Condition 2", endId);
                    const spy = spyOn(ProcessAddHelper, "insertUserDecisionCondition").and.callThrough();

                    const addMessageSpy = spyOn(messageService, "addMessage").and.callThrough();

                    graph.viewModel.shapeLimit = 14;

                    // Act
                    graph.addDecisionBranch(decisionId, "Condition 3", endId);
                    graph.viewModel.shapeLimit = 100;

                    // Assert
                    expect(spy).not.toHaveBeenCalled();
                    expect(addMessageSpy).toHaveBeenCalledWith(message);
                });
            });

            describe("to system decision", () => {
                it("succeeds if condition is provided", () => {
                    // Arrange
                    const graph = createGraph(TestModels.createSystemDecisionWithTwoBranchesModel());
                    const decisionId = 5;
                    const endId = 8;
                    let error = null;
                    const spy = spyOn(ProcessAddHelper, "insertSystemDecisionCondition").and.callThrough();

                    // Act
                    try {
                        graph.addDecisionBranch(decisionId, "Condition 1", endId);
                    } catch (exception) {
                        error = exception;
                    }

                    // Assert
                    expect(error).toBeNull();
                    expect(spy).toHaveBeenCalled();
                });

                it("fails if maximum number of conditions reached", () => {
                    // Arrange
                    const graph = createGraph(TestModels.createSystemDecisionWithMaximumConditionsModel());
                    const decisionId = 50;
                    const endId = 160;
                    const spy = spyOn(ProcessAddHelper, "insertSystemDecisionCondition").and.callThrough();
                    const addErrorSpy = spyOn(messageService, "addError").and.callThrough();

                    // Act
                    graph.addDecisionBranch(decisionId, "Condition 1", endId);

                    // Assert
                    expect(spy).not.toHaveBeenCalled();
                    expect(addErrorSpy).toHaveBeenCalledWith(rootScope.config.labels["ST_Add_CannotAdd_MaximumConditionsReached"]);
                });
            });
        });

        describe("delete", () => {
            let testModel: ProcessModels.IProcess;
            let processModel: IProcessViewModel;
            let graph: ProcessGraph;

            it("fails when no target ids provided", () => {
                // Arrange
                graph = createGraph(TestModels.createUserDecisionWithTwoBranchesModel());
                const decisionId = 4;
                const spy = spyOn(ProcessDeleteHelper, "deleteShapesAndLinksByIds");
                const link = {sourceId: decisionId, destinationId: undefined, orderindex: 0, label: ""};

                // Act
                ProcessDeleteHelper.deleteDecisionBranch(link, graph);

                // Assert
                expect(spy).not.toHaveBeenCalled();
            });

            it("fails when no decision condition destinations ids exist", () => {
                // Arrange
                const process = TestModels.createUserDecisionWithThreeConditionsModel();
                graph = createGraph(process);
                const decisionId = 30;
                const userDecisionId = 60;
                const spy = spyOn(ProcessDeleteHelper, "deleteShapesAndLinksByIds");
                process.decisionBranchDestinationLinks = [];
                const link = {sourceId: decisionId, destinationId: userDecisionId, orderindex: 0, label: ""};

                // Act
                ProcessDeleteHelper.deleteDecisionBranch(link, graph);

                // Assert
                expect(spy).not.toHaveBeenCalled();
            });

            it("adds error message when cannot delete conditions", () => {
                // Arrange
                graph = createGraph(TestModels.createUserDecisionWithTwoBranchesModel());
                const decisionId = 4;
                const userTaskId = 7;
                const spy = spyOn(ProcessDeleteHelper, "deleteShapesAndLinksByIds");
                const addErrorSpy = spyOn(messageService, "addError");
                const link = {sourceId: decisionId, destinationId: userTaskId, orderindex: 0, label: ""};

                // Act
                ProcessDeleteHelper.deleteDecisionBranch(link, graph);

                // Assert
                expect(spy).not.toHaveBeenCalled();
                expect(addErrorSpy).toHaveBeenCalledWith(rootScope.config.labels["ST_Delete_CannotDelete_UD_AtleastTwoConditions"]);
            });

            describe("in user decision with two branches", () => {
                beforeEach(() => {
                    testModel = TestModels.createUserDecisionWithTwoBranchesModel();
                    processModel = new ProcessViewModel(testModel, communicationManager);
                    graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService,
                        localization, shapesFactory, messageService, null, null);
                    graph.render(true, null);
                });

                afterEach(() => {
                    processModel = null;
                    testModel = null;
                    graph = null;
                });

                it("disallows to delete first branch", () => {
                    // Arrange
                    const userDecisionId = 4;
                    const branchUserTaskId = 5;
                    const conditionDestinationCountBefore = processModel.decisionBranchDestinationLinks.length;
                    const link = {sourceId: userDecisionId, destinationId: branchUserTaskId, orderindex: 0, label: ""};

                    // Act
                    const result = ProcessDeleteHelper.deleteDecisionBranch(link, graph);
                    const conditionDestinationCountAfter = processModel.decisionBranchDestinationLinks.length;

                    // Assert
                    expect(result).toBe(false);
                    expect(conditionDestinationCountAfter).toBe(conditionDestinationCountBefore);
                });

                it("disallows to delete second branch", () => {
                    // Arrange
                    const userDecisionId = 4;
                    const conditionDestinationCountBefore = processModel.decisionBranchDestinationLinks.length;
                    const link = {sourceId: userDecisionId, destinationId: 7, orderindex: 0, label: ""};

                    // Act
                    const result = ProcessDeleteHelper.deleteDecisionBranch(link, graph);
                    const conditionDestinationCountAfter = processModel.decisionBranchDestinationLinks.length;

                    // Assert
                    expect(result).toBe(false);
                    expect(conditionDestinationCountAfter).toBe(conditionDestinationCountBefore);
                });
            });

            describe("in user decision with multiple branches", () => {
                beforeEach(() => {
                    /*
                     start -> pre -> ud -> ut1 -> st1 -> end
                     ->ut2 -> st2 -> end
                     ->ut3 -> st3 -> end
                     ->ut4 -> st4 -> end

                     */
                    testModel = TestModels.createUserDecisionWithMultipleBranchesModel();
                    processModel = new ProcessViewModel(testModel, communicationManager);

                    //bypass testing remove stateful shapes logic here
                    spyOn(processModel, "removeStatefulShape").and.returnValue(null);

                    graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
                    graph.render(true, null);
                });

                afterEach(() => {
                    processModel = null;
                    testModel = null;
                    graph = null;
                });

                it("deletes a single branch successfully", () => {
                    // Arrange
                    const userDecisionId = 4;
                    const userTaskId = 7;
                    const systemTaskId = 8;
                    const link = {sourceId: userDecisionId, destinationId: userTaskId, orderindex: 1, label: ""};

                    // Act
                    ProcessDeleteHelper.deleteDecisionBranch(link, graph);

                    // Assert
                    expect(processModel.shapes.length).toEqual(10);
                    expect(processModel.shapes.filter((s) =>
                    s.id === userTaskId ||
                    s.id === systemTaskId).length).toEqual(0);
                    expect(processModel.links.filter((l) =>
                    l.sourceId === userTaskId ||
                    l.destinationId === userTaskId ||
                    l.sourceId === systemTaskId ||
                    l.destinationId === systemTaskId).length).toEqual(0);
                });

                it("deletes multiple branches successfully", () => {
                    // Arrange
                    const userDecisionId = 4;
                    const userTask1Id = 9;
                    const systemTask1Id = 10;
                    const userTask2Id = 7;
                    const systemTask2Id = 8;
                    const conditionDestinationCountBefore = processModel.decisionBranchDestinationLinks.length;
                    const link1 = {sourceId: userDecisionId, destinationId: userTask1Id, orderindex: 2, label: ""};
                    const link2 = {sourceId: userDecisionId, destinationId: userTask2Id, orderindex: 1, label: ""};

                    // Act
                    ProcessDeleteHelper.deleteDecisionBranch(link1, graph);
                    ProcessDeleteHelper.deleteDecisionBranch(link2, graph);
                    const conditionDestinationCountAfter = processModel.decisionBranchDestinationLinks.length;

                    // Assert
                    expect(processModel.shapes.length).toEqual(8);
                    expect(processModel.shapes.filter((s) =>
                    s.id === userTask1Id ||
                    s.id === systemTask1Id ||
                    s.id === userTask2Id ||
                    s.id === systemTask2Id).length).toEqual(0);
                    expect(processModel.links.filter((l) =>
                    l.sourceId === userTask1Id ||
                    l.destinationId === userTask1Id ||
                    l.sourceId === systemTask1Id ||
                    l.destinationId === systemTask1Id ||
                    l.sourceId === userTask2Id ||
                    l.destinationId === userTask2Id ||
                    l.sourceId === systemTask2Id ||
                    l.destinationId === systemTask2Id).length).toEqual(0);
                    expect(conditionDestinationCountBefore).toBe(3);
                    expect(conditionDestinationCountAfter).toBe(1);
                });
            });

            describe("in multiple user decisions with multiple branches", () => {
                beforeEach(() => {
                    testModel = TestModels.createMultipleUserDecisionsWithMultipleBranchesModel();
                    processModel = new ProcessViewModel(testModel, communicationManager);
                    graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
                    graph.render(true, null);
                });

                afterEach(() => {
                    processModel = null;
                    testModel = null;
                    graph = null;
                });

                it("does not delete user task between two user decisions", () => {
                    // Arrange
                    const userTaskId = 5;

                    // Act
                    const result = ProcessDeleteHelper.deleteUserTask(userTaskId, null, graph);

                    // Assert
                    expect(processModel.shapes.length).toEqual(13);
                    expect(result).toEqual(false);
                });
            });

            describe("in system decision with two branches", () => {
                beforeEach(() => {
                    testModel = TestModels.createSystemDecisionWithTwoBranchesModel();
                    processModel = new ProcessViewModel(testModel, communicationManager);
                    processModel.communicationManager = communicationManager;
                    graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization,
                        shapesFactory, messageService, null, null);
                    graph.render(true, null);
                });

                afterEach(() => {
                    processModel = null;
                    testModel = null;
                    graph = null;
                });

                it("disallows to delete first branch", () => {
                    // Arrange
                    const systemDecisionId = 5;
                    const userTaskId = 6;
                    const link = {sourceId: systemDecisionId, destinationId: userTaskId, orderindex: 0, label: ""};

                    // Act
                    const result = ProcessDeleteHelper.deleteDecisionBranch(link, graph);

                    // Assert
                    expect(result).toEqual(false);
                });

                it("disallows to delete second branch", () => {
                    // Arrange
                    const systemDecisionId = 5;
                    const userTaskId = 7;
                    const link = {sourceId: systemDecisionId, destinationId: userTaskId, orderindex: 0, label: ""};

                    // Act
                    const result = ProcessDeleteHelper.deleteDecisionBranch(link, graph);

                    // Assert
                    expect(result).toEqual(false);
                });
            });

            describe("in system decision with multiple branches", () => {
                beforeEach(() => {
                    testModel = TestModels.createSystemDecisionWithMultipleBranchesModel();
                    processModel = new ProcessViewModel(testModel, communicationManager);

                    //bypass testing remove stateful shapes logic here
                    spyOn(processModel, "removeStatefulShape").and.returnValue(null);

                    graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
                    graph.render(true, null);
                });

                afterEach(() => {
                    testModel = null;
                    processModel = null;
                    graph = null;
                });

                it("deletes a single branch successfully", () => {
                    // Arrange
                    const systemDecisionId = 5;
                    const systemTaskId = 7;
                    const link = {sourceId: systemDecisionId, destinationId: systemTaskId, orderindex: 1, label: ""};

                    // Act
                    ProcessDeleteHelper.deleteDecisionBranch(link, graph);

                    // Assert
                    expect(processModel.shapes.length).toEqual(7);
                    expect(processModel.shapes.filter((s) =>
                    s.id === systemTaskId).length).toEqual(0);
                    expect(processModel.links.filter((l) =>
                    l.sourceId === systemTaskId ||
                    l.destinationId === systemTaskId).length).toEqual(0);
                });

                it("deletes multiple branches successfully", () => {
                    // Arrange
                    const systemDecisionId = 5;
                    const systemTask1Id = 8;
                    const systemTask2Id = 7;
                    const link1 = {sourceId: systemDecisionId, destinationId: systemTask1Id, orderindex: 2, label: ""};
                    const link2 = {sourceId: systemDecisionId, destinationId: systemTask2Id, orderindex: 1, label: ""};

                    // Act
                    ProcessDeleteHelper.deleteDecisionBranch(link1, graph);
                    ProcessDeleteHelper.deleteDecisionBranch(link2, graph);

                    // Assert
                    expect(processModel.shapes.length).toEqual(6);
                    expect(processModel.shapes.filter((s) =>
                    s.id === systemTask1Id ||
                    s.id === systemTask2Id).length).toEqual(0);
                    expect(processModel.links.filter((l) =>
                    l.sourceId === systemTask1Id ||
                    l.destinationId === systemTask1Id ||
                    l.sourceId === systemTask2Id ||
                    l.destinationId === systemTask2Id).length).toEqual(0);
                });
            });
        });
    });

    describe("decision branch destinations", () => {
        let testModel: ProcessModels.IProcess;
        let processModel: IProcessViewModel;
        let graph: ProcessGraph;
        beforeEach(() => {
            /*
             start -> pre - ud -> ut1 -> st1 ->                  ut5 -> st5 ->  end
             -> ut2 -> st2 -> ut6 -> st6 ->
             -> ut3 -> st3 ->
             -> ut4 -> st4 ->
             */
            testModel = TestModels.createUserDecisionWithMultipleBranchesModel_V2();
            processModel = new ProcessViewModel(testModel, communicationManager);
            graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            graph.render(null, null);
        });

        describe("getValidMergeNodes", () => {
            it("simple test", () => {
                // Arrange
                const ud_ut3_link: ProcessModels.IProcessLink = processModel.links.filter(a => a.sourceId === 4 && a.destinationId === 9)[0];

                // Act
                const scopeNodes = graph.getValidMergeNodes(ud_ut3_link);

                // Assert
                expect(scopeNodes.length).toEqual(4);
                expect(scopeNodes.filter(a => a.model.name === "ud").length).toBe(1);
                expect(scopeNodes.filter(a => a.model.name === "ut6").length).toBe(1);
                expect(scopeNodes.filter(a => a.model.name === "ut5").length).toBe(1);
                expect(scopeNodes.filter(a => a.model.name === "end").length).toBe(1);
            });

            it("does not include items in own branch", () => {
                // Arrange
                const ud_ut2_link: ProcessModels.IProcessLink = processModel.links.filter(a => a.sourceId === 4 && a.destinationId === 7)[0];

                // Act
                const scopeNodes = graph.getValidMergeNodes(ud_ut2_link);

                // Assert
                expect(scopeNodes.length).toEqual(3);
                expect(scopeNodes.filter(a => a.model.name === "ut6").length).toBe(0);
                expect(scopeNodes.filter(a => a.model.name === "ut2").length).toBe(0);
            });

            it("returns end in case of user decision with no-op in first condition", () => {
                // Arrange
                const process = TestModels.createUserDecisionWithoutUserTaskInFirstConditionModel();
                graph = createGraph(process);
                graph.render(true, null);

                // Act
                const result = graph.getValidMergeNodes(process.links[3]);

                // Assert
                expect(result.length).toBe(2);
                expect(result.filter(node => node.getNodeType() === NodeType.ProcessEnd).length).toBeGreaterThan(0);
            });
        });
    });

    describe("highlightCopyGroups", () => {
        it("doesn't highlight anything when start shape is selected", () => {
            // arrange
            const graph = createGraph(TestModels.createDefaultProcessModel());
            graph.render(true, null);
            const start = graph.getNodeById("10");
            const spy = spyOn(graph, "highlightNode");

            // act
            graph.highlightCopyGroups([start]);

            // assert
            expect(spy).not.toHaveBeenCalled();
        });

        it("doesn't highlight anything when pre-condition is selected", () => {
            // arrange
            const graph = createGraph(TestModels.createDefaultProcessModel());
            graph.render(true, null);
            const precondition = graph.getNodeById("15");
            const spy = spyOn(graph, "highlightNode");

            // act
            graph.highlightCopyGroups([precondition]);

            // assert
            expect(spy).not.toHaveBeenCalled();
        });

        it("doesn't highlight anything when system task is selected", () => {
            // arrange
            const graph = createGraph(TestModels.createDefaultProcessModel());
            graph.render(true, null);
            const systemTask = graph.getNodeById("25");
            const spy = spyOn(graph, "highlightNode");

            // act
            graph.highlightCopyGroups([systemTask]);

            // assert
            expect(spy).not.toHaveBeenCalled();
        });

        it("doesn't highlight anything when end shape is selected", () => {
            // arrange
            const graph = createGraph(TestModels.createDefaultProcessModel());
            graph.render(true, null);
            const end = graph.getNodeById("30");
            const spy = spyOn(graph, "highlightNode");

            // act
            graph.highlightCopyGroups([end]);

            // assert
            expect(spy).not.toHaveBeenCalled();
        });

        it("highlights system task when user task is selected", () => {
            // arrange
            const graph = createGraph(TestModels.createDefaultProcessModel());
            graph.render(true, null);
            const userTask = graph.getNodeById("20");
            const systemTask = graph.getNodeById("25");
            const spy = spyOn(graph, "highlightNode");

            // act
            graph.highlightCopyGroups([userTask]);

            // assert
            expect(spy).toHaveBeenCalledWith(systemTask);
        });

        it("doesn't highlight anything when user decision is selected", () => {
            // arrange
            const decisionShape = shapesFactory.createModelUserDecisionShape(2, 1, 999, 0, 0);
            const process = TestModels.createUserDecisionTestModel(decisionShape);
            const graph = createGraph(process);
            graph.render(true, null);
            const userDecision = graph.getNodeById("999");
            const spy = spyOn(graph, "highlightNode");

            // act
            graph.highlightCopyGroups([userDecision]);

            // assert
            expect(spy).not.toHaveBeenCalled();
        });

        it("doesn't highlight anything when system decision is selected", () => {
            // arrange
            const decisionShape = shapesFactory.createModelSystemDecisionShape(2, 1, 999, 0, 0);
            const process = TestModels.createSystemDecisionTestModel(decisionShape);
            const graph = createGraph(process);
            graph.render(true, null);
            const systemDecision = graph.getNodeById("999");
            const spy = spyOn(graph, "highlightNode");

            // act
            graph.highlightCopyGroups([systemDecision]);

            // assert
            expect(spy).not.toHaveBeenCalled();
        });

        it("doesn't highlight user decision when single first user task is selected", () => {
            // arrange
            const decisionShape = shapesFactory.createModelUserDecisionShape(2, 1, 999, 0, 0);
            const process = TestModels.createUserDecisionTestModel(decisionShape);
            const graph = createGraph(process);
            graph.render(true, null);
            const userTask = graph.getNodeById("35");
            const userDecision = graph.getNodeById("999");
            const spy = spyOn(graph, "highlightNode");

            // act
            graph.highlightCopyGroups([userTask]);

            // assert
            expect(spy).not.toHaveBeenCalledWith(userDecision);
        });

        it("highlights user decision when multiple first user tasks are selected", () => {
            // arrange
            const decisionShape = shapesFactory.createModelUserDecisionShape(2, 1, 999, 0, 0);
            const process = TestModels.createUserDecisionTestModel(decisionShape);
            const graph = createGraph(process);
            graph.render(true, null);
            const userTask = graph.getNodeById("35");
            const userTask2 = graph.getNodeById("45");
            const userDecision = graph.getNodeById("999");
            const spy = spyOn(graph, "highlightNode");

            // act
            graph.highlightCopyGroups([userTask, userTask2]);

            // assert
            expect(spy).toHaveBeenCalledWith(userDecision);
        });

        it("highlights nodes in the user task scope when user task is selected", () => {
            // arrange
            const process = TestModels.createSimpleProcessModelWithSystemDecision();
            const graph = createGraph(process);
            graph.render(true, null);
            const userTask = graph.getNodeById("20");
            const systemDecision = graph.getNodeById("25");
            const systemDecisionSpy = spyOn(systemDecision, "highlight");
            const systemTask1 = graph.getNodeById("26");
            const systemTask1Spy = spyOn(systemTask1, "highlight");
            const systemTask2 = graph.getNodeById("27");
            const systemTask2Spy = spyOn(systemTask2, "highlight");
            const userTask2 = graph.getNodeById("28");
            const userTask2Spy = spyOn(userTask2, "highlight");
            const systemTask3 = graph.getNodeById("29");
            const systemTask3Spy = spyOn(systemTask3, "highlight");

            // act
            graph.highlightCopyGroups([userTask]);

            // assert
            expect(systemDecisionSpy).toHaveBeenCalled();
            expect(systemTask1Spy).toHaveBeenCalled();
            expect(systemTask2Spy).toHaveBeenCalled();
            expect(userTask2Spy).toHaveBeenCalled();
            expect(systemTask3Spy).toHaveBeenCalled();
        });
    });

    describe("getBranchStartingLink", () => {

        it("returns null when link does not exist in graph", () => {
            const process = TestModels.createDefaultProcessModel();

            const graph = createGraph(process);
            const newLink = new ProcessLinkModel(null, 0, 0, 0);
            const startLink = graph.getBranchStartingLink(newLink);

            expect(startLink).toBeNull();
        });

        it("returns null if link is not a starting branch link", () => {
            const process = TestModels.createNestedSystemDecisionsWithLoopModel();
            const graph = createGraph(process);
            const startId = graph.viewModel.getStartShapeId();
            const nextLinks = graph.viewModel.getSortedNextLinks(startId);
            const firstLink = nextLinks[0];

            const endLink = graph.getBranchStartingLink(firstLink);
            expect(endLink).toBeNull();
        });

        it("returns same link if it finds it in the model", () => {
            const process = TestModels.createNestedSystemDecisionsWithLoopModel();
            const graph = createGraph(process);
            const decisionId = 4;
            const nextLinks = graph.viewModel.getSortedNextLinks(decisionId);
            const firstLink = nextLinks[1];

            const startLink = graph.getBranchStartingLink(firstLink);
            expect(startLink.sourceId).toBe(firstLink.sourceId);
            expect(startLink.destinationId).toBe(firstLink.destinationId);
            expect(startLink.orderindex).toBe(firstLink.orderindex);
            expect(startLink.label).toBe(firstLink.label);
        });

        it("returns parent decision link if it is first branch of nested decision", () => {
            const process = TestModels.createNestedSystemDecisionsWithLoopModel();
            const graph = createGraph(process);
            const decisionId = 4;
            const parentNextLinks = graph.viewModel.getSortedNextLinks(decisionId);
            const parentLink = parentNextLinks[1];
            const nestedDecisionId = 8;
            const nextLinks = graph.viewModel.getSortedNextLinks(nestedDecisionId);
            const firstLink = nextLinks[0];

            const startLink = graph.getBranchStartingLink(firstLink);
            expect(startLink.sourceId).toBe(parentLink.sourceId);
            expect(startLink.destinationId).toBe(parentLink.destinationId);
            expect(startLink.orderindex).toBe(parentLink.orderindex);
        });
    });

    describe("getBranchEndingLink", () => {

        it("returns null when link does not exist in graph", () => {
            const process = TestModels.createNestedSystemDecisionsWithLoopModel();
            const graph = createGraph(process);
            const newLink = new ProcessLinkModel(null, 0, 0, 0);
            const endLink = graph.getBranchEndingLink(newLink);
            expect(endLink).toBeNull();
        });

        it("returns null if its first link of decision on main branch", () => {
            const process = TestModels.createNestedSystemDecisionsWithLoopModel();
            const graph = createGraph(process);
            const decisionId = 4;
            const nextLinks = graph.viewModel.getSortedNextLinks(decisionId);
            const firstLink = nextLinks[0];

            const endLink = graph.getBranchEndingLink(firstLink);
            expect(endLink).toBeNull();
        });

        it("returns null if link is not a starting branch link", () => {
            const process = TestModels.createNestedSystemDecisionsWithLoopModel();
            const graph = createGraph(process);
            const startId = graph.viewModel.getStartShapeId();
            const nextLinks = graph.viewModel.getSortedNextLinks(startId);
            const firstLink = nextLinks[0];

            const endLink = graph.getBranchEndingLink(firstLink);
            expect(endLink).toBeNull();
        });

        it("returns parent branch's end link for the first branch of nested decision", () => {
            const process = TestModels.createNestedSystemDecisionsWithLoopModel();
            const graph = createGraph(process);
            const lastShapeInBranchId = 9;
            const branchDestinationId = 14;
            const decisionId = 8;
            const nextLinks = graph.viewModel.getSortedNextLinks(decisionId);
            const nextLink = nextLinks[0];

            const endLink = graph.getBranchEndingLink(nextLink);
            expect(endLink.sourceId).toBe(lastShapeInBranchId);
            expect(endLink.destinationId).toBe(branchDestinationId);
        });

        it("returns branch's end link for a branch in the decision", () => {
            const process = TestModels.createNestedSystemDecisionsWithLoopModel();
            const graph = createGraph(process);
            const decisionId = 4;
            const lastShapeInBranchId = 9;
            const branchDestinationId = 14;
            const parentNextLinks = graph.viewModel.getSortedNextLinks(decisionId);
            const parentLink = parentNextLinks[1];

            const endLink = graph.getBranchEndingLink(parentLink);
            expect(endLink.sourceId).toBe(lastShapeInBranchId);
            expect(endLink.destinationId).toBe(branchDestinationId);
        });
    });

    function hasShapes(process: ProcessModels.IProcess, ...id: number[]): boolean {
        return process.shapes.filter((shape) => id.indexOf(shape.id) > -1).length > 0;
    }

    function hasLink(process: ProcessModels.IProcess, sourceId: number, destinationId: number): boolean {
        return process.links.filter((link) => link.sourceId === sourceId && link.destinationId === destinationId).length > 0;
    }

    function hasLinksFor(process: ProcessModels.IProcess, ...id: number[]): boolean {
        return process.links.filter((link) => id.indexOf(link.sourceId) > -1 || id.indexOf(link.destinationId) > -1).length > 0;
    }

    function hasTypes(process: ProcessModels.IProcess, ...type: Enums.ProcessShapeType[]): boolean {
        return process.shapes.filter(shape => type.indexOf(getType(shape)) > -1).length > 0;
    }

    function getType(shape: ProcessModels.IProcessShape): Enums.ProcessShapeType {
        return shape.propertyValues["clientType"].value;
    }

    function createGraph(process: ProcessModels.IProcess): ProcessGraph {
        const clientModel = new ProcessGraphModel(process);
        const viewModel = new ProcessViewModel(clientModel, communicationManager, rootScope, localScope, messageService);

        //bypass testing stateful shapes logic here
        spyOn(viewModel, "removeStatefulShape").and.returnValue(null);
        spyOn(viewModel, "addToSubArtifactCollection").and.returnValue(null);
        return new ProcessGraph(
            rootScope, localScope, container, viewModel, dialogService, localization,
            shapesFactory, messageService, null, statefulArtifactFactory
        );
    }
});

