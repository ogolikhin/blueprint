﻿import {ProcessGraph} from "./process-graph";
import {ShapesFactory} from "./shapes/shapes-factory";
import {ProcessServiceMock} from "../../../../services/process/process.svc.mock";
import {IProcessService} from "../../../../services/process/process.svc";
import * as Enums from "../../../../models/enums";
import * as ProcessModels from "../../../../models/processModels";
import {MessageServiceMock} from "../../../../../../core/messages/message.mock";
import {IMessageService} from "../../../../../../core/messages/message.svc";
import {IProcessViewModel, ProcessViewModel} from "../../viewmodel/process-viewmodel";
import {IProcessGraphModel, ProcessGraphModel} from "../../viewmodel/process-graph-model";
import {IProcessGraph, IDiagramNode, IDecision} from "./process-graph-interfaces";
import {UserTask, SystemTask, UserDecision, Condition} from "./shapes/";
import {NodeChange, NodeType} from "./process-graph-constants";
import {ProcessValidator} from "./process-graph-validator";


import * as TestModels from "../../../../models/test-model-factory";

describe("ProcessGraph", () => {
    let graph: ProcessGraph;
    let shapesFactory: ShapesFactory;
    let localScope, rootScope, timeout, processModelService, wrapper, container;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("processModelService", ProcessServiceMock);
    }));

    beforeEach(inject((_$window_: ng.IWindowService, $rootScope: ng.IRootScopeService, $timeout: ng.ITimeoutService, processModelService: IProcessService) => {
        rootScope = $rootScope;
        timeout = $timeout;
        processModelService = processModelService;
        wrapper = document.createElement('DIV');
        container = document.createElement('DIV');
        wrapper.appendChild(container);
        document.body.appendChild(wrapper);

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
        localScope = { graphContainer: container, graphWrapper: wrapper, isSpa: false };
        shapesFactory = new ShapesFactory(rootScope);
    }));

    describe("IsUserSystemProcess", () => {
        it("returns false for no process", () => {
            // Arrange
            // Act
            let graph = createGraph(TestModels.createDefaultProcessModel());

            // Assert
            expect(graph.IsUserSystemProcess).toBe(false);
        });

        it("returns false for business process", () => {
            // Arrange
            let process = TestModels.createDefaultProcessModel();
            process.propertyValues["clientType"].value = Enums.ProcessType.BusinessProcess;

            // Act
            let graph = createGraph(process);

            // Assert
            expect(graph.IsUserSystemProcess).toBe(false);
        });

        it("returns false for system-to-system process", () => {
            // Arrange
            let process = TestModels.createDefaultProcessModel();
            process.propertyValues["clientType"].value = Enums.ProcessType.SystemToSystemProcess;

            // Act
            let graph = createGraph(process);

            // Assert
            expect(graph.IsUserSystemProcess).toBe(false);
        });

        it("returns true for user-to-system process", () => {
            // Arrange
            let process = TestModels.createDefaultProcessModel();
            process.propertyValues["clientType"].value = Enums.ProcessType.UserToSystemProcess;

            // Act
            let graph = createGraph(process);

            // Assert
            expect(graph.IsUserSystemProcess).toBe(true);
        });
    });

    describe("render", () => {
        it("adds error message when error is raised", () => {
            // Arrange
            let mockMessageService = new MessageServiceMock();
            let graph = createGraph(TestModels.createDefaultProcessModel(), mockMessageService);
            let message = "Test message";
            let renderSpy = spyOn(graph.layout, "render").and.throwError(message);
            let addErrorSpy = spyOn(mockMessageService, "addError");

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
            let test = {
                action: null
            };
            let graph = createGraph(TestModels.createDefaultProcessModel());
            let getModelSpy = spyOn(graph.getMxGraph(), "getModel").and.callThrough();

            // Act
            graph.redraw(null);

            // Assert
            expect(getModelSpy).not.toHaveBeenCalled();
        });

        it("executes and redraws when provided an action", () => {
            // Arrange
            let test = {
                action: function () {
                }
            };
            let graph = createGraph(TestModels.createDefaultProcessModel());
            let model = graph.getMxGraphModel();
            let getModelSpy = spyOn(graph.getMxGraph(), "getModel").and.callThrough();
            let beginUpdateSpy = spyOn(model, "beginUpdate").and.callThrough();
            let actionSpy = spyOn(test, "action").and.callThrough();
            let endUpdateSpy = spyOn(model, "endUpdate").and.callThrough();

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
                viewModel = new ProcessViewModel(clientModel);
                graph = new ProcessGraph(rootScope, localScope, container, processModelService,  viewModel);
            });

            afterEach(() => {
                process = null;
                clientModel = null;
                viewModel = null;
                graph = null;
            });

            it("fails", () => {
                // Arrange
                let userTaskId = 999;

                // Act
                let result = graph.deleteUserTask(userTaskId);

                // Assert
                expect(result).toBe(false);
            });

            it("doesn't modify shapes or links", () => {
                // Arrange
                let userTaskId = 999;
                let shapeLengthBeforeDelete = process.shapes.length;
                let linkLengthBeforeDelete = process.links.length;

                // Act
                let result = graph.deleteUserTask(userTaskId);

                // Assert
                expect(process.shapes.length).toEqual(shapeLengthBeforeDelete); // 7
                expect(process.links.length).toEqual(linkLengthBeforeDelete);   // 6
            });
        });

        describe("that is the last user task in default process", () => {

            beforeEach(() => {
                process = TestModels.createDefaultProcessModel();
                clientModel = new ProcessGraphModel(process);
                viewModel = new ProcessViewModel(clientModel);
                graph = new ProcessGraph(rootScope, localScope, container, processModelService, viewModel);
            });

            afterEach(() => {
                process = null;
                clientModel = null;
                viewModel = null;
                graph = null;
            });

            it("fails", () => {
                //Arrange
                let userTaskId = 20;

                //Act
                let result = graph.deleteUserTask(userTaskId);

                //Assert
                expect(result).toBe(false);
            });

            it("doesn't modify shapes or links", () => {
                //Arrange
                let shapeLengthBeforeDelete = process.shapes.length;
                let linkLengthBeforeDelete = process.links.length;
                let userTaskId = 20;
                let userTaskShape = shapesFactory.createModelUserTaskShape(2, 1, userTaskId, 2, 0);

                //Act
                let result = graph.deleteUserTask(userTaskId);

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
            viewModel = new ProcessViewModel(clientModel);
            graph = new ProcessGraph(rootScope, localScope, container, processModelService,  viewModel);

            // Act
            let result = graph.deleteUserTask(35);

            // Assert
            expect(result).toBe(true);
            expect(process.shapes.length).toEqual(5);
            expect(hasShapes(process, 35, 40)).toBe(false);
        });

        describe("from user decision with two conditions", () => {
            it("fails for user task in a second condition", () => {
                // Arrange
                let process = TestModels.createUserDecisionWithTwoBranchesModel();
                let graph = createGraph(process);
                let userTaskId = 7;

                // Act
                let result = graph.deleteUserTask(userTaskId);

                // Assert
                expect(result).toBe(false);
            });

            it("adds error message when cannot delete", () => {
                // Arrange
                let messageService = new MessageServiceMock();
                let process = TestModels.createUserDecisionWithTwoBranchesModel();
                let graph = createGraph(process, messageService);
                let userTaskId = 7;
                let spy = spyOn(messageService, "addError").and.callThrough();

                // Act
                let result = graph.deleteUserTask(userTaskId);

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
                viewModel = new ProcessViewModel(clientModel);
                graph = new ProcessGraph(rootScope, localScope, container, processModelService,  viewModel);
            });

            afterEach(() => {
                process = null;
                graph = null;
            });

            it("allows deleting the only user task in the first branch", () => {
                // Arrange
                let userTaskId = 40;

                // Act
                let result = graph.deleteUserTask(userTaskId);

                // Assert
                expect(result).toBe(true);
            });

            it("allows deleting the only user task in second condition", () => {
                // Arrange
                let userTaskId = 60;

                // Act
                let result = graph.deleteUserTask(userTaskId);

                // Assert
                expect(result).toBe(true);
            });

            it("deletes the whole second condition when deleting the only user task on that branch", () => {
                // Arrange
                let userTaskId = 60;
                let systemTaskId = 70;
                let userDecisionId = 30;
                let conditionDestinationCountBefore = viewModel.decisionBranchDestinationLinks.length;

                // Act
                let result = graph.deleteUserTask(userTaskId);
                let conditionDestinationCountAfter = viewModel.decisionBranchDestinationLinks.length;

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
                let userTaskId = 80;

                // Act
                let result = graph.deleteUserTask(userTaskId);

                // Assert
                expect(result).toBe(true);
            });

            it("deletes the whole third condition when deleting the only user task in that branch", () => {
                // Arrange
                let userTaskId = 80;
                let systemTaskId = 90;
                let userDecisionId = 30;
                let conditionDestinationCountBefore = viewModel.decisionBranchDestinationLinks.length;

                // Act
                let result = graph.deleteUserTask(userTaskId);
                let conditionDestinationCountAfter = viewModel.decisionBranchDestinationLinks.length;

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
                viewModel = new ProcessViewModel(clientModel);
                graph = new ProcessGraph(rootScope, localScope, container, processModelService,  viewModel);
            });

            afterEach(() => {
                process = null;
                graph = null;
            });

            it("allows deleting the first user task int the first condition", () => {
                // Arrange
                let userTaskId = 40;

                // Act
                let result = graph.deleteUserTask(userTaskId);

                // Assert
                expect(result).toBe(true);
            });

            it("deletes the first user task in the first condition", () => {
                // Arrange
                let userTaskId = 40;
                let systemTaskId = 50;
                let nextUserTaskId = 60;
                let userDecisionId = 30;

                // Act
                let result = graph.deleteUserTask(userTaskId);

                // Assert
                expect(process.shapes.length).toBe(14);
                expect(hasShapes(process, userTaskId, systemTaskId)).toBe(false);
                expect(hasLinksFor(process, userTaskId, systemTaskId)).toBe(false);
                expect(hasLink(process, userDecisionId, nextUserTaskId)).toBe(true);
            });

            it("allows deleting the second user task in the first condition", () => {
                // Arrange
                let userTaskId = 60;

                // Act
                let result = graph.deleteUserTask(userTaskId);

                // Assert
                expect(result).toBe(true);
            });

            it("deletes the second user task in the first condition", () => {
                // Arrange
                let userTaskId = 60;
                let systemTaskId = 70;
                let previousSystemTaskId = 50;
                let endId = 160;

                // Act
                let result = graph.deleteUserTask(userTaskId);

                // Assert
                expect(process.shapes.length).toBe(14);
                expect(hasShapes(process, userTaskId, systemTaskId)).toBe(false);
                expect(hasLinksFor(process, userTaskId, systemTaskId)).toBe(false);
                expect(hasLink(process, previousSystemTaskId, endId)).toBe(true);
            });

            it("allows deleting the first user task in a second condition", () => {
                // Arrange
                let userTaskId = 80;

                // Act
                let result = graph.deleteUserTask(userTaskId);

                // Assert
                expect(result).toBe(true);
            });

            it("deletes the first user task in the second condition", () => {
                // Arrange
                let userTaskId = 80;
                let systemTaskId = 90;
                let nextUserTaskId = 100;
                let userDecisionId = 30;

                // Act
                let result = graph.deleteUserTask(userTaskId);

                // Assert
                expect(process.shapes.length).toBe(14);
                expect(hasShapes(process, userTaskId, systemTaskId)).toBe(false);
                expect(hasLinksFor(process, userTaskId, systemTaskId)).toBe(false);
                expect(hasLink(process, userDecisionId, nextUserTaskId)).toBe(true);
            });

            it("allows deleting the second user task in a second condition", () => {
                // Arrange
                let userTaskId = 100;

                // Act
                let result = graph.deleteUserTask(userTaskId);

                // Assert
                expect(result).toBe(true);
            });

            it("deletes the second user task in the second condition", () => {
                // Arrange
                let userTaskId = 100;
                let systemTaskId = 110;
                let previousSystemTaskId = 90;
                let endId = 160;

                // Act
                let result = graph.deleteUserTask(userTaskId);

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
            let process = TestModels.createUserDecisionInSecondConditionModel();
            let graph = createGraph(process);
            let userTaskId = 90;
            let shapesNumBefore = process.shapes.length;
            let spy = spyOn(graph, "deleteUserTaskInternal");

            // Act
            let result = graph.deleteUserTask(userTaskId);

            // Assert
            expect(result).toBe(false);
            expect(spy).not.toHaveBeenCalled();
            expect(process.shapes.length).toBe(shapesNumBefore);
        });

        it("reconnects links joined into the scope", () => {
            // Arrange
            let process = TestModels.createMergingSystemDecisionsModel();
            let graph = createGraph(process);
            graph.initializeGlobalScope();

            // Act
            let result = graph.deleteUserTask(70);

            // Assert
            expect(result).toBe(true);
            expect(hasLinksFor(process, 70, 80, 90, 100, 110, 120)).toBe(false);
            expect(hasLink(process, 60, 140)).toBe(true);
            expect(hasLink(process, 130, 140)).toBe(true);
        });

        it("reconnects links for infinite loop", () => {
            // Arrange
            let process = TestModels.createMergingSystemDecisionsWithInfiniteLoopModel();
            let graph = createGraph(process);
            graph.initializeGlobalScope();

            // Act
            let result = graph.deleteUserTask(70);

            // Assert
            expect(result).toBe(true);
            expect(hasLinksFor(process, 70, 80, 90, 100, 110, 120)).toBe(false);
            expect(hasLink(process, 60, 140)).toBe(true);
            expect(hasLink(process, 130, 40)).toBe(true);
        });

        it("fails for user task that is the only user task in process but has other user tasks in its system decisions", () => {
            //Arrange
            var testModel = TestModels.createSimpleProcessModelWithSystemDecision();
            var processModel = new ProcessViewModel(testModel);

            var graph = new ProcessGraph(rootScope, localScope, container, processModelService, processModel);
            var shapeLengthBeforeDelete = processModel.shapes.length;
            var linkLengthBeforeDelete = processModel.links.length;

            var userTaskShape = shapesFactory.createModelUserTaskShape(2, 1, 20, 2, 0);
            let shapesFactoryService = new ShapesFactory(rootScope);
            var userTaskShapeDiagramNode = new UserTask(userTaskShape, rootScope, null, shapesFactoryService);

            //Act
            let result = graph.deleteUserTask(userTaskShapeDiagramNode.model.id);

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
            var userTaskShape = shapesFactory.createModelUserTaskShape(2, 1, 20, 2, 0);
            var testModel = TestModels.createDeleteUserTaskSimpleModel(userTaskShape);
            var processModel = new ProcessViewModel(testModel);

            var userTaskShapeDiagramNode = new UserDecision(userTaskShape, rootScope);

            var graph = new ProcessGraph(rootScope, localScope, container, processModelService,  processModel);

            //Act
            let result = graph.deleteUserTask(userTaskShapeDiagramNode.model.id);

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
            var userTaskShape = shapesFactory.createModelUserTaskShape(2, 1, 280, 4, 0);
            var testModel = TestModels.createUserTaskFollowedBySystemDecision(userTaskShape);
            var processModel = new ProcessViewModel(testModel);

            var userTaskShapeDiagramNode = new UserDecision(userTaskShape, rootScope);

            var graph = new ProcessGraph(rootScope, localScope, container, processModelService,  processModel);

            //Act
            let result = graph.deleteUserTask(userTaskShapeDiagramNode.model.id);

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
            var userTaskShape = shapesFactory.createModelUserTaskShape(2, 1, 280, 4, 0);
            var testModel = TestModels.createUserTaskFollowedBySystemDecision(userTaskShape);
            var processModel = new ProcessViewModel(testModel);

            var userTaskShapeDiagramNode = new UserDecision(userTaskShape, rootScope);

            var graph = new ProcessGraph(rootScope, localScope, container, processModelService,  processModel);

            var shapeIdBeforeUserTask = processModel.links.filter(a => a.destinationId == userTaskShape.id)[0].sourceId;

            let destinationId = processModel.decisionBranchDestinationLinks[0].destinationId;

            //Act
            let result = graph.deleteUserTask(userTaskShapeDiagramNode.model.id);

            var linksContainingDecision = processModel.links.filter(a => a.sourceId == userTaskShapeDiagramNode.id || a.destinationId == userTaskShapeDiagramNode.id).length;
            var updatedLink = processModel.links.filter(a => a.sourceId == shapeIdBeforeUserTask);

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
            let process = TestModels.createTwoUserTaskModel();
            let graph = createGraph(process);
            let userTaskId = 35;
            let endId = 30;
            let test = { action: null };
            let spy = spyOn(test, "action");

            // Act
            let result = graph.deleteUserTask(userTaskId, test.action);

            // Assert
            expect(result).toBe(true);
            expect(spy).toHaveBeenCalledWith(NodeChange.Remove, userTaskId);
        });

        it("infinite loop, delete task, success", () => {
            let testModel = TestModels.createUserDecisionInfiniteLoopModel();
            // Start -> Pre -> UD -> UT1 -> ST1 -> End
            //                       UT2 -> ST2 -> UT3 -> ST3 -> UT5
            //                       UT4 -> ST4 -> UT5 -> ST5 -> UT3
            let ST2 = 70;
            let UT3 = 80;
            let UT5 = 120;
            let ST5 = 130;
            let END = 140;
            let processModel = new ProcessViewModel(testModel);
            let graph = new ProcessGraph(rootScope, localScope, container, processModelService, processModel);

            // Act
            let result = graph.deleteUserTask(UT3);

            // Assert
            processModel.updateTree();
            let errorMessages: string[] = [];
            let validator = new ProcessValidator();
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
            let testModel = TestModels.createInfiniteLoopFromDifferentDecisions();
            let processModel = new ProcessViewModel(testModel);
            let graph = new ProcessGraph(rootScope, localScope, container, processModelService,  processModel);
            let utToDelete = 210;

            let incomingSystemTaskSameCondition = 200;
            let incomingSystemTaskDifferentCondition = 110;

            //Act
            graph.deleteUserTask(utToDelete, null);
            graph.initializeGlobalScope();

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
            let decisionShape = shapesFactory.createModelUserDecisionShape(2, 1, 30, 0, 0);
            // otherDecisionShape does not exist in the graph shapes.
            let otherDecisionShape = shapesFactory.createModelUserDecisionShape(2, 1, 999, 0, 0);
            let testModel = TestModels.createUserDecisionTestModel(decisionShape);
            let processModel = new ProcessViewModel(testModel);

            let graph = new ProcessGraph(rootScope, localScope, container, processModelService, processModel);
            let shapeLengthBeforeDelete = processModel.shapes.length;
            let linkLengthBeforeDelete = processModel.links.length;

            //Act
            graph.deleteDecision(otherDecisionShape.id);

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
            let decisionShape = shapesFactory.createModelUserDecisionShape(2, 1, 30, 0, 0);
            let testModel = TestModels.createUserDecisionTestModel(decisionShape);
            let processModel = new ProcessViewModel(testModel);
            let graph = new ProcessGraph(rootScope, localScope, container, processModelService, processModel);
            let shapeLengthBeforeDelete = processModel.shapes.length;
            let linkLengthBeforeDelete = processModel.links.length;

            //Act
            graph.deleteDecision(decisionShape.id);

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
            let decisionShape = shapesFactory.createModelUserDecisionShape(2, 1, 30, 0, 0);
            let testModel = TestModels.createUserDecisionTestModel(decisionShape);
            let processModel = new ProcessViewModel(testModel);
            let graph = new ProcessGraph(rootScope, localScope, container, processModelService, processModel);
            let shapeIdBeforeDecision = processModel.links.filter(a => a.destinationId == decisionShape.id)[0].sourceId;
            let shapeIdToConnectAfterDecision = processModel.links.filter(a => a.sourceId == decisionShape.id).reduce((a, b) => a.orderindex < b.orderindex ? a : b).destinationId;

            //Act
            graph.deleteDecision(decisionShape.id);

            //Assert
            var linksContainingDecision = processModel.links.filter(a => a.sourceId == decisionShape.id || a.destinationId == decisionShape.id).length;
            var updatedLink = processModel.links.filter(a => a.sourceId == shapeIdBeforeDecision);

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
            let decisionShape = shapesFactory.createSystemDecisionShapeModel(2, 1, 25, 0, 0);
            // otherDecisionShape does not exist in the graph shapes.
            let otherDecisionShape = shapesFactory.createModelUserDecisionShape(2, 1, 999, 0, 0);
            let testModel = TestModels.createSystemDecisionTestModel(decisionShape);
            let processModel = new ProcessViewModel(testModel);
            let graph = new ProcessGraph(rootScope, localScope, container, processModelService, processModel);
            let shapeLengthBeforeDelete = processModel.shapes.length;
            let linkLengthBeforeDelete = processModel.links.length;

            //Act
            graph.deleteDecision(otherDecisionShape.id);

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
            let decisionShape = shapesFactory.createSystemDecisionShapeModel(2, 1, 25, 0, 0);
            let testModel = TestModels.createSystemDecisionTestModel(decisionShape);
            let processModel = new ProcessViewModel(testModel);
            let graph = new ProcessGraph(rootScope, localScope, container, processModelService, processModel);
            let shapeLengthBeforeDelete = processModel.shapes.length;
            let linkLengthBeforeDelete = processModel.links.length;

            //Act
            graph.deleteDecision(decisionShape.id);

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
            let decisionShape = shapesFactory.createSystemDecisionShapeModel(2, 1, 25, 0, 0);
            let testModel = TestModels.createSystemDecisionTestModel(decisionShape);
            let processModel = new ProcessViewModel(testModel);
            let graph = new ProcessGraph(rootScope, localScope, container, processModelService, processModel);
            let shapeIdBeforeDecision = processModel.links.filter(a => a.destinationId == decisionShape.id)[0].sourceId;
            let shapeIdToConnectAfterDecision = processModel.links.filter(a => a.sourceId == decisionShape.id).reduce((a, b) => a.orderindex < b.orderindex ? a : b).destinationId;

            //Act
            graph.deleteDecision(decisionShape.id);

            //Assert
            var linksContainingDecision = processModel.links.filter(a => a.sourceId == decisionShape.id || a.destinationId == decisionShape.id).length;
            var updatedLink = processModel.links.filter(a => a.sourceId == shapeIdBeforeDecision);

            expect(linksContainingDecision).toEqual(0);
            expect(updatedLink.length).toEqual(1);
            expect(updatedLink[0].destinationId).toEqual(shapeIdToConnectAfterDecision);
            expect(processModel.decisionBranchDestinationLinks.length).toBe(0);
        });

        it("succeeds if decision has no-op and is the last decision in the process and creates new user task and system task", () => {
            // Arrange
            let messageServiceMock = new MessageServiceMock();
            let process = TestModels.createUserDecisionWithoutUserTaskInFirstConditionModel();
            let graph = createGraph(process, messageServiceMock);
            let decisionId = 40;
            let test = {
                action: function () {
                }
            };
            let spy = spyOn(test, "action").and.callThrough();
            let addMessageSpy = spyOn(messageServiceMock, "addMessage").and.callThrough();

            // Act
            let result = graph.deleteDecision(decisionId, test.action);

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
                let graph = createGraph(TestModels.createUserDecisionWithTwoBranchesModel());
                let decisionId = 4;
                let startId = 2;
                let endId = 9;
                let mergeNode = <IDiagramNode>{
                    model: {
                        id: endId
                    }
                };
                let condition = new Condition(decisionId, 999, 0, "", mergeNode, []);
                let error = null;
                let spy = spyOn(graph, "notifyUpdateInModel").and.callThrough();

                // Act
                try {
                    graph.addDecisionBranches(startId, [condition]);
                } catch (exception) {
                    error = exception;
                }

                // Assert
                expect(error).not.toBeNull();
                expect(error.message).toBe("Expected a decision type but found " + <number>Enums.ProcessShapeType.Start);
                expect(spy).not.toHaveBeenCalled();
            });

            it("fails if null for conditions is provided", () => {
                // Arrange
                let graph = createGraph(TestModels.createUserDecisionWithTwoBranchesModel());
                let decisionId = 4;
                let spy = spyOn(graph, "notifyUpdateInModel").and.callThrough();

                // Act
                graph.addDecisionBranches(decisionId, null);

                // Assert
                expect(spy).not.toHaveBeenCalled();
            });

            it("fails if no conditions provided", () => {
                // Arrange
                let graph = createGraph(TestModels.createUserDecisionWithTwoBranchesModel());
                let decisionId = 4;
                let spy = spyOn(graph, "notifyUpdateInModel").and.callThrough();

                // Act
                graph.addDecisionBranches(decisionId, []);

                // Assert
                expect(spy).not.toHaveBeenCalled();
            });

            describe("to user decision", () => {
                it("succeeds if condition is provided", () => {
                    // Arrange
                    let graph = createGraph(TestModels.createUserDecisionWithTwoBranchesModel());
                    let decisionId = 4;
                    let endId = 9;
                    let mergeNode = <IDiagramNode>{
                        model: {
                            id: endId
                        }
                    };
                    let condition = new Condition(decisionId, 999, 0, "", mergeNode, []);
                    let error = null;
                    let spy = spyOn(graph, "notifyUpdateInModel").and.callThrough();

                    // Act
                    try {
                        graph.addDecisionBranches(decisionId, [condition]);
                    } catch (exception) {
                        error = exception;
                    }

                    // Assert
                    expect(error).toBeNull();
                    expect(spy).toHaveBeenCalled();
                });

                it("succeeds if no user task exist in first condition", () => {
                    // Arrange
                    let process = TestModels.createUserDecisionWithoutUserTaskInFirstConditionModel();
                    let graph = createGraph(process);
                    let decisionId = 40;
                    let endId = 70;
                    let mergeNode = <IDiagramNode>{
                        model: {
                            id: endId
                        }
                    };
                    let condition = new Condition(decisionId, 999, 0, "", mergeNode, []);
                    let spy = spyOn(graph, "notifyUpdateInModel").and.callThrough();

                    // Act
                    graph.addDecisionBranches(decisionId, [condition]);

                    // Assert
                    expect(spy).toHaveBeenCalled();
                    expect(process.links.filter(link => link.sourceId === decisionId).length).toBe(3);
                });

                it("fails if maximum number of conditions reached", () => {
                    // Arrange
                    let messageServiceMock = new MessageServiceMock();
                    let graph = createGraph(TestModels.createUserDecisionWithMaximumConditionsModel(), messageServiceMock);
                    let decisionId = 40;
                    let endId = 250;
                    let mergeNode = <IDiagramNode>{
                        model: {
                            id: endId
                        }
                    };
                    let condition = new Condition(decisionId, 999, 0, "", mergeNode, []);
                    let spy = spyOn(graph, "notifyUpdateInModel").and.callThrough();
                    let addErrorSpy = spyOn(messageServiceMock, "addError").and.callThrough();

                    // Act
                    graph.addDecisionBranches(decisionId, [condition]);

                    // Assert
                    expect(spy).not.toHaveBeenCalled();
                    expect(addErrorSpy).toHaveBeenCalledWith(rootScope.config.labels["ST_Add_CannotAdd_MaximumConditionsReached"]);
                });
            });

            describe("to system decision", () => {
                it("succeeds if condition is provided", () => {
                    // Arrange
                    let graph = createGraph(TestModels.createSystemDecisionWithTwoBranchesModel());
                    let decisionId = 5;
                    let endId = 8;
                    let mergeNode = <IDiagramNode>{
                        model: {
                            id: endId
                        }
                    };
                    let condition = new Condition(decisionId, 999, 0, "", mergeNode, []);
                    let error = null;
                    let spy = spyOn(graph, "notifyUpdateInModel").and.callThrough();

                    // Act
                    try {
                        graph.addDecisionBranches(decisionId, [condition]);
                    } catch (exception) {
                        error = exception;
                    }

                    // Assert
                    expect(error).toBeNull();
                    expect(spy).toHaveBeenCalled();
                });

                it("fails if maximum number of conditions reached", () => {
                    // Arrange
                    let messageServiceMock = new MessageServiceMock();
                    let graph = createGraph(TestModels.createSystemDecisionWithMaximumConditionsModel(), messageServiceMock);
                    let decisionId = 50;
                    let endId = 160;
                    let mergeNode = <IDiagramNode>{
                        model: {
                            id: endId
                        }
                    };
                    let condition = new Condition(decisionId, 999, 0, "", mergeNode, []);
                    let spy = spyOn(graph, "notifyUpdateInModel").and.callThrough();
                    let addErrorSpy = spyOn(messageServiceMock, "addError").and.callThrough();

                    // Act
                    graph.addDecisionBranches(decisionId, [condition]);

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
                let graph = createGraph(TestModels.createUserDecisionWithTwoBranchesModel());
                let decisionId = 4;
                let spy = spyOn(graph, "deleteShapesAndLinksByIds");

                // Act
                graph.deleteDecisionBranches(decisionId, []);

                // Assert
                expect(spy).not.toHaveBeenCalled();
            });

            it("fails when no decision condition destinations ids exist", () => {
                // Arrange
                let process = TestModels.createUserDecisionWithThreeConditionsModel();
                let graph = createGraph(process);
                let decisionId = 30;
                let userDecisionId = 60;
                let spy = spyOn(graph, "deleteShapesAndLinksByIds");
                process.decisionBranchDestinationLinks = [];

                // Act
                graph.deleteDecisionBranches(decisionId, [userDecisionId]);

                // Assert
                expect(spy).not.toHaveBeenCalled();
            });

            it("adds error message when cannot delete conditions", () => {
                // Arrange
                let messageService = new MessageServiceMock();
                let graph = createGraph(TestModels.createUserDecisionWithTwoBranchesModel(), messageService);
                let decisionId = 4;
                let userTaskId = 7;
                let spy = spyOn(graph, "deleteShapesAndLinksByIds");
                let addErrorSpy = spyOn(messageService, "addError");

                // Act
                graph.deleteDecisionBranches(decisionId, [userTaskId]);

                // Assert
                expect(spy).not.toHaveBeenCalled();
                expect(addErrorSpy).toHaveBeenCalledWith(rootScope.config.labels["ST_Delete_CannotDelete_UD_AtleastTwoConditions"]);
            });

            describe("in user decision with two branches", () => {
                beforeEach(() => {
                    testModel = TestModels.createUserDecisionWithTwoBranchesModel();
                    processModel = new ProcessViewModel(testModel);
                    graph = new ProcessGraph(rootScope, localScope, container, processModelService, processModel);
                    graph.render(true, null);
                });

                afterEach(() => {
                    processModel = null;
                    testModel = null;
                    graph = null;
                });

                it("disallows to delete first branch", () => {
                    // Arrange
                    let userDecisionId = 4;
                    let userDecision = <IDecision>graph.getNodeById(userDecisionId.toString());
                    let branchUserTaskId = 5;
                    let error = null;
                    let conditionDestinationCountBefore = processModel.decisionBranchDestinationLinks.length;

                    // Act
                    let result = graph.deleteDecisionBranches(userDecisionId, [branchUserTaskId]);
                    let conditionDestinationCountAfter = processModel.decisionBranchDestinationLinks.length;

                    // Assert
                    expect(result).toBe(false);
                    expect(conditionDestinationCountAfter).toBe(conditionDestinationCountBefore);
                });

                it("disallows to delete second branch", () => {
                    // Arrange
                    let userDecisionId = 4;
                    let userDecision = <IDecision>graph.getNodeById(userDecisionId.toString());
                    let branchUserTaskId = 7;
                    let conditionDestinationCountBefore = processModel.decisionBranchDestinationLinks.length;

                    // Act
                    let result = graph.deleteDecisionBranches(userDecisionId, [7]);
                    let conditionDestinationCountAfter = processModel.decisionBranchDestinationLinks.length;

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
                    processModel = new ProcessViewModel(testModel);
                    graph = new ProcessGraph(rootScope, localScope, container, processModelService, processModel);
                    graph.render(true, null);
                });

                afterEach(() => {
                    processModel = null;
                    testModel = null;
                    graph = null;
                });

                it("deletes a single branch successfully", () => {
                    // Arrange
                    let userDecisionId = 4;
                    let userDecision = <IDecision>graph.getNodeById(userDecisionId.toString());
                    let userTaskId = 7;
                    let systemTaskId = 8;

                    // Act
                    graph.deleteDecisionBranches(userDecisionId, [userTaskId]);

                    // Assert
                    expect(processModel.shapes.length).toEqual(10);
                    expect(processModel.shapes.filter((s) =>
                        s.id == userTaskId ||
                        s.id == systemTaskId).length).toEqual(0);
                    expect(processModel.links.filter((l) =>
                        l.sourceId == userTaskId ||
                        l.destinationId == userTaskId ||
                        l.sourceId == systemTaskId ||
                        l.destinationId == systemTaskId).length).toEqual(0);
                });

                it("deletes multiple branches successfully", () => {
                    // Arrange
                    let userDecisionId = 4;
                    let userDecision = <IDecision>graph.getNodeById(userDecisionId.toString());
                    let userTask1Id = 9;
                    let systemTask1Id = 10;
                    let userTask2Id = 7;
                    let systemTask2Id = 8;
                    let conditionDestinationCountBefore = processModel.decisionBranchDestinationLinks.length;

                    // Act
                    graph.deleteDecisionBranches(userDecisionId, [userTask1Id, userTask2Id]);
                    let conditionDestinationCountAfter = processModel.decisionBranchDestinationLinks.length;

                    // Assert
                    expect(processModel.shapes.length).toEqual(8);
                    expect(processModel.shapes.filter((s) =>
                        s.id == userTask1Id ||
                        s.id == systemTask1Id ||
                        s.id == userTask2Id ||
                        s.id == systemTask2Id).length).toEqual(0);
                    expect(processModel.links.filter((l) =>
                        l.sourceId == userTask1Id ||
                        l.destinationId == userTask1Id ||
                        l.sourceId == systemTask1Id ||
                        l.destinationId == systemTask1Id ||
                        l.sourceId == userTask2Id ||
                        l.destinationId == userTask2Id ||
                        l.sourceId == systemTask2Id ||
                        l.destinationId == systemTask2Id).length).toEqual(0);
                    expect(conditionDestinationCountBefore).toBe(3);
                    expect(conditionDestinationCountAfter).toBe(1);
                });
            });

            describe("in multiple user decisions with multiple branches", () => {
                beforeEach(() => {
                    testModel = TestModels.createMultipleUserDecisionsWithMultipleBranchesModel();
                    processModel = new ProcessViewModel(testModel);
                    graph = new ProcessGraph(rootScope, localScope, container, processModelService, processModel);
                    graph.render(true, null);
                });

                afterEach(() => {
                    processModel = null;
                    testModel = null;
                    graph = null;
                });

                it("does not delete user task between two user decisions", () => {
                    // Arrange
                    let userTaskId = 5;

                    // Act
                    var result = graph.deleteUserTask(userTaskId);

                    // Assert
                    expect(processModel.shapes.length).toEqual(13);
                    expect(result).toEqual(false);
                });
            });

            describe("in system decision with two branches", () => {
                beforeEach(() => {
                    testModel = TestModels.createSystemDecisionWithTwoBranchesModel();
                    processModel = new ProcessViewModel(testModel);
                    graph = new ProcessGraph(rootScope, localScope, container, processModelService,  processModel);
                    graph.render(true, null);
                });

                afterEach(() => {
                    processModel = null;
                    testModel = null;
                    graph = null;
                });

                it("disallows to delete first branch", () => {
                    // Arrange
                    let systemDecisionId = 5;
                    let systemDecision = <IDecision>graph.getNodeById(systemDecisionId.toString());
                    let userTaskId = 6;

                    // Act
                    let result = graph.deleteDecisionBranches(systemDecisionId, [userTaskId]);

                    // Assert
                    expect(result).toEqual(false);
                });

                it("disallows to delete second branch", () => {
                    // Arrange
                    let systemDecisionId = 5;
                    let systemDecision = <IDecision>graph.getNodeById(systemDecisionId.toString());
                    let userTaskId = 7;

                    // Act
                    let result = graph.deleteDecisionBranches(systemDecisionId, [userTaskId]);

                    // Assert
                    expect(result).toEqual(false);
                });
            });

            describe("in system decision with multiple branches", () => {
                beforeEach(() => {
                    testModel = TestModels.createSystemDecisionWithMultipleBranchesModel();
                    processModel = new ProcessViewModel(testModel);
                    graph = new ProcessGraph(rootScope, localScope, container, processModelService, processModel);
                    graph.render(true, null);
                });

                afterEach(() => {
                    testModel = null;
                    processModel = null;
                    graph = null;
                });

                it("deletes a single branch successfully", () => {
                    // Arrange
                    let systemDecisionId = 5;
                    let systemDecision = <IDecision>graph.getNodeById(systemDecisionId.toString());
                    let systemTaskId = 7;

                    // Act
                    graph.deleteDecisionBranches(systemDecisionId, [systemTaskId]);

                    // Assert
                    expect(processModel.shapes.length).toEqual(7);
                    expect(processModel.shapes.filter((s) =>
                        s.id == systemTaskId).length).toEqual(0);
                    expect(processModel.links.filter((l) =>
                        l.sourceId == systemTaskId ||
                        l.destinationId == systemTaskId).length).toEqual(0);
                });

                it("deletes multiple branches successfully", () => {
                    // Arrange
                    let systemDecisionId = 5;
                    let systemDecision = <IDecision>graph.getNodeById(systemDecisionId.toString());
                    let systemTask1Id = 8;
                    let systemTask2Id = 7;

                    // Act
                    graph.deleteDecisionBranches(systemDecisionId, [systemTask1Id, systemTask2Id]);

                    // Assert
                    expect(processModel.shapes.length).toEqual(6);
                    expect(processModel.shapes.filter((s) =>
                        s.id == systemTask1Id ||
                        s.id == systemTask2Id).length).toEqual(0);
                    expect(processModel.links.filter((l) =>
                        l.sourceId == systemTask1Id ||
                        l.destinationId == systemTask1Id ||
                        l.sourceId == systemTask2Id ||
                        l.destinationId == systemTask2Id).length).toEqual(0);
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
            processModel = new ProcessViewModel(testModel);
            graph = new ProcessGraph(rootScope, localScope, container, processModelService,  processModel);
            graph.render(null, null);
        });

        describe("getValidMergeNodes", () => {
            it("simple test", () => {
                // Arrange
                let ud_ut3_link: ProcessModels.IProcessLink = processModel.links.filter(a => a.sourceId === 4 && a.destinationId === 9)[0];

                // Act
                let scopeNodes = graph.getValidMergeNodes(ud_ut3_link);

                // Assert
                expect(scopeNodes.length).toEqual(4);
                expect(scopeNodes.filter(a => a.model.name === "ud").length).toBe(1);
                expect(scopeNodes.filter(a => a.model.name === "ut6").length).toBe(1);
                expect(scopeNodes.filter(a => a.model.name === "ut5").length).toBe(1);
                expect(scopeNodes.filter(a => a.model.name === "end").length).toBe(1);
            });

            it("does not include items in own branch", () => {
                // Arrange
                let ud_ut2_link: ProcessModels.IProcessLink = processModel.links.filter(a => a.sourceId === 4 && a.destinationId === 7)[0];

                // Act
                let scopeNodes = graph.getValidMergeNodes(ud_ut2_link);

                // Assert
                expect(scopeNodes.length).toEqual(3);
                expect(scopeNodes.filter(a => a.model.name === "ut6").length).toBe(0);
                expect(scopeNodes.filter(a => a.model.name === "ut2").length).toBe(0);
            });

            it("returns end in case of user decision with no-op in first condition", () => {
                // Arrange
                let process = TestModels.createUserDecisionWithoutUserTaskInFirstConditionModel();
                let graph = createGraph(process);
                graph.render(true, null);

                // Act
                let result = graph.getValidMergeNodes(process.links[3]);

                // Assert
                expect(result.length).toBe(2);
                expect(result.filter(node => node.getNodeType() === NodeType.ProcessEnd).length).toBeGreaterThan(0);
            });
        });

        it("updateMergeNode - merge null", () => {

            // Arrange
            let decisionId = 4;
            let ud_ut3_link: ProcessModels.IProcessLink = processModel.links.filter(a => a.sourceId === decisionId && a.destinationId === 9)[0];

            let condition = Condition.create(ud_ut3_link, null, null);

            // Act
            let isUpdated = graph.updateMergeNode(decisionId, condition);

            // Assert
            expect(isUpdated).toBeFalsy();
        });
        it("updateMergeNode - simple update", () => {
            // Arrange
            /*
            UDPATED->

                start -> pre - ud -> ut1 -> st1 ----------------> ut5 -> st5 ->  end
                                -> ut2 -> st2 -> ut6 -> st6 -> 
                                -> ut3 -> st3 ----------------------------->
                                -> ut4 -> st4-------------- ->
            */
            let decisionId = 4;
            let endId = 17;
            let ud_ut3_link: ProcessModels.IProcessLink = processModel.links.filter(a => a.sourceId === decisionId && a.destinationId === 9)[0];
            let mergeNode = <IDiagramNode>{
                model: {
                    id: endId
                }
            }
            let condition = Condition.create(ud_ut3_link, mergeNode, null);

            // Act
            let isUpdated = graph.updateMergeNode(decisionId, condition);

            // Assert
            let decisionScopesToEnd = processModel.decisionBranchDestinationLinks.filter(a => a.sourceId === decisionId && a.destinationId === endId);
            expect(isUpdated).toBeTruthy();
            expect(decisionScopesToEnd.length).toEqual(1);
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

    function createGraph(process: ProcessModels.IProcess, messageService?: IMessageService): ProcessGraph {
        let clientModel = new ProcessGraphModel(process);
        let viewModel = new ProcessViewModel(clientModel);
        return new ProcessGraph(rootScope, localScope, container, processModelService,  viewModel, messageService);
    }
});

