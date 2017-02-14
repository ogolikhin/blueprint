require("script!mxClient");
import * as angular from "angular";
import "angular-mocks";
import "./";
import {DecisionEditorModel} from "./decisionEditor.model";
import {DecisionEditorController} from "./decisionEditor.controller";
import {ModalServiceInstanceMock} from "../../../../../shell/login/mocks.spec";
import {LocalizationServiceMock} from "../../../../../commonModule/localization/localization.service.mock";
import {IModalScope} from "../base-modal-dialog-controller";
import {ProcessGraph} from "../../diagram/presentation/graph/process-graph";
import {ICondition, IDecision, IDiagramLink, IDiagramNode, IProcessGraph, IUserTask, NodeType} from "../../diagram/presentation/graph/models";
import {ProcessEvents} from "../../diagram/process-diagram-communication";
import {ProcessDeleteHelper} from "../../diagram/presentation/graph/process-delete-helper";
import {ILocalizationService} from "../../../../../commonModule/localization/localization.service";

describe("DecisionEditorController", () => {
    let $rootScope: ng.IRootScopeService;
    let $scope: IModalScope;
    let $timeout: ng.ITimeoutService;
    let $anchorScroll: ng.IAnchorScrollService;
    let $location: ng.ILocationService;
    let $q: ng.IQService;
    let localization: ILocalizationService;
    let $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance;
    let model: DecisionEditorModel;
    let controller: DecisionEditorController;
    let noop = () => {/*noop*/};

    beforeEach(angular.mock.module("decisionEditor", ($provide: ng.auto.IProvideService) => {
        $provide.service("$uibModalInstance", ModalServiceInstanceMock);
        $provide.service("localization", LocalizationServiceMock);
    }));

    beforeEach(inject((
        _$rootScope_: ng.IRootScopeService,
        _$timeout_: ng.ITimeoutService,
        _$anchorScroll_: ng.IAnchorScrollService,
        _$location_: ng.ILocationService,
        _$q_: ng.IQService,
        _localization_: ILocalizationService,
        _$uibModalInstance_: ng.ui.bootstrap.IModalServiceInstance
    ) => {
        $rootScope = _$rootScope_;
        $timeout = _$timeout_;
        $anchorScroll = _$anchorScroll_;
        $location = _$location_;
        $q = _$q_;
        localization = _localization_;
        $uibModalInstance = _$uibModalInstance_;

        model = new DecisionEditorModel();
        $scope = <IModalScope>$rootScope.$new();
        controller = new DecisionEditorController(
            $rootScope, $scope, $timeout, $anchorScroll,
            $location, $q, localization, $uibModalInstance, model
        );
    }));

    describe("defaultMergeNodeLabel", () => {
        it("calls localization service with correct string", () => {
            // arrange
            model.isReadonly = true;
            model.isHistoricalVersion = false;
            const localizationSpy = spyOn(localization, "get");

            // act
            const label = controller.defaultMergeNodeLabel;

            // assert
            expect(localizationSpy).toHaveBeenCalledWith("ST_Decision_Modal_Next_Task_Label");
        });
    });

    describe("isReadonly", () => {
        it("is true if dialog model is read-only", () => {
            // arrange
            model.isReadonly = true;
            model.isHistoricalVersion = false;

            // act
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll,
                $location, $q, localization, $uibModalInstance, model
            );

            // assert
            expect(controller.isReadonly).toBe(true);
        });

        it("is true if dialog model is historical", () => {
            // arrange
            model.isReadonly = false;
            model.isHistoricalVersion = true;

            // act
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll,
                $location, $q, localization, $uibModalInstance, model
            );

            // assert
            expect(controller.isReadonly).toBe(true);
        });

        it("is false if dialog model is neither historical nor read-only", () => {
            // arrange
            model.isReadonly = false;
            model.isHistoricalVersion = false;

            // act
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll,
                $location, $q, localization, $uibModalInstance, model
            );

            // assert
            expect(controller.isReadonly).toBe(false);
        });
    });

    describe("hasMaxConditions", () => {
        it("is true when reached maximum conditions", () => {
            // arrange
            // act
            model.conditions = createConditions(ProcessGraph.MaxConditions);

            // assert
            expect(controller.hasMaxConditions).toBe(true);
        });

        it("is false when not reached maximum conditions", () => {
            // arrange
            // act
            model.conditions = createConditions(ProcessGraph.MaxConditions - 1);

            // assert
            expect(controller.hasMaxConditions).toBe(false);
        });
    });

    describe("hasMinConditions", () => {
        it("is true when reached minimum conditions", () => {
            // arrange
            // act
            model.conditions = createConditions(ProcessGraph.MinConditions);

            // assert
            expect(controller.hasMinConditions).toBe(true);
        });

        it("is false when not reached minimum conditions", () => {
            // arrange
            // act
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);

            // assert
            expect(controller.hasMinConditions).toBe(false);
        });
    });

    describe("canAddCondition", () => {
        it("is false when dialog is read-only", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MaxConditions - 1);

            // act
            model.isReadonly = true;

            // assert
            expect(controller.canAddCondition).toBe(false);
        });

        it("is false when reached maximum conditions", () => {
            // arrange
            // act
            model.conditions = createConditions(ProcessGraph.MaxConditions);

            // assert
            expect(controller.canAddCondition).toBe(false);
        });

        it("is true when not reached maximum conditions", () => {
            // arrange
            // act
            model.conditions = createConditions(ProcessGraph.MaxConditions - 1);

            // assert
            expect(controller.canAddCondition).toBe(true);
        });
    });

    describe("addCondition", () => {
        let decision: IDecision, model: DecisionEditorModel, $scope: IModalScope, controller: DecisionEditorController;

        beforeEach(() => {
            // arrange
            decision = <IDecision>{
                model: {id: 1}
            };
            model = new DecisionEditorModel();
            model.originalDecision = decision;
            model.graph = createMockGraph();
            model.conditions = createConditions(ProcessGraph.MinConditions);

            $scope = <IModalScope>$rootScope.$new();
            controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll,
                $location, $q, localization, $uibModalInstance, model
            );
        });

        it("doesn't update conditions if read-only", () => {
            // arrange
            model.isReadonly = true;
            spyOn(model.graph, "getValidMergeNodes").and.returnValue([]);

            // act
            controller.addCondition();

            // assert
            expect(model.conditions.length).toBe(ProcessGraph.MinConditions);
        });

        it("correcly updates conditions", () => {
            // arrange
            spyOn(model.graph, "getValidMergeNodes").and.returnValue([]);
            const refreshSpy = spyOn(controller, "refreshView").and.callFake(noop);
            const scrollSpy = spyOn(controller, "scrollToBottomOfConditionList").and.callFake(noop);

            // act
            controller.addCondition();

            // assert
            expect(model.conditions.length).toBe(ProcessGraph.MinConditions + 1);
            expect(refreshSpy).toHaveBeenCalled();
            expect(scrollSpy).toHaveBeenCalled();
        });

        it("defaults the merge node to be the one passed in the dialogModel", () => {
            // arrange
            const mergingUserTask = <IUserTask>{
                model: {id: 5}
            };
            spyOn(model.graph, "getValidMergeNodes").and.returnValue([mergingUserTask]);
            const refreshSpy = spyOn(controller, "refreshView").and.callFake(noop);
            const scrollSpy = spyOn(controller, "scrollToBottomOfConditionList").and.callFake(noop);
            model.defaultDestinationId = mergingUserTask.model.id;

            // act
            controller.addCondition();

            // assert
            const lastAddedCondition = model.conditions[model.conditions.length - 1];
            expect(lastAddedCondition.mergeNode).toBeDefined();
            expect(lastAddedCondition.mergeNode.model.id).toBe(mergingUserTask.model.id);
        });
    });

    describe("isDeleteConditionVisible", () => {
        it("is false for all conditions if minimum conditions reached", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions);

            // act
            const isDeleteConditionVisible = _.some(
                model.conditions,
                condition => controller.isDeleteConditionVisible(condition)
            );

            // assert
            expect(isDeleteConditionVisible).toBe(false);
        });

        it("is false for the primary condition if minimum conditions not reached", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);
            const primaryCondition = model.conditions[0];

            // act
            const isDeleteConditionVisible = controller.isDeleteConditionVisible(primaryCondition);

            // assert
            expect(isDeleteConditionVisible).toBe(false);
        });

        it("is true for all conditions but primary if minimum conditions not reached", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);

            // act
            const isDeleteConditionVisible = _.every(
                model.conditions.slice(1),
                condition => controller.isDeleteConditionVisible(condition)
            );

            // assert
            expect(isDeleteConditionVisible).toBe(true);
        });
    });

    describe("canDeleteCondition", () => {
        it("is false when read-only", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);
            model.isReadonly = true;

            // act
            const canDeleteCondition = _.some(
                model.conditions,
                condition => controller.canDeleteCondition(condition)
            );

            // assert
            expect(canDeleteCondition).toBe(false);
        });

        it("is false for all conditions if minimum conditions reached", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions);

            // act
            const canDeleteCondition = _.some(
                model.conditions,
                condition => controller.canDeleteCondition(condition)
            );

            // assert
            expect(canDeleteCondition).toBe(false);
        });

        it("is true for all conditions but primary if minimum conditions not reached", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);

            // act
            const canDeleteCondition = _.every(
                model.conditions.slice(1),
                condition => controller.canDeleteCondition(condition)
            );

            // assert
            expect(canDeleteCondition).toBe(true);
        });
    });

    describe("deleteCondition", () => {
        it("doesn't update conditions if read-only", () => {
            // arrange
            model.isReadonly = true;
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);

            // act
            controller.deleteCondition(model.conditions[model.conditions.length - 1]);

            // assert
            expect(model.conditions.length).toBe(ProcessGraph.MinConditions + 1);
        });

        it("doesn't update conditions if deleted condition is not part of the collection", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);
            spyOn(controller, "refreshView").and.callFake(noop);
            const condition = createConditions(1)[0];

            // act
            controller.deleteCondition(condition);

            // assert
            expect(model.conditions.length).toBe(ProcessGraph.MinConditions + 1);
        });

        it("correctly updates conditions", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);
            const refreshSpy = spyOn(controller, "refreshView").and.callFake(noop);

            // act
            controller.deleteCondition(model.conditions[model.conditions.length - 1]);

            // assert
            expect(model.conditions.length).toBe(ProcessGraph.MinConditions);
            expect(refreshSpy).toHaveBeenCalled();
        });
    });

    describe("isFirstBranch", () => {
        it("is true for primary condition", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions);
            const primaryCondition = model.conditions[0];

            // act
            const isFirstCondition = controller.isFirstBranch(primaryCondition);

            // assert
            expect(isFirstCondition).toBe(true);
        });

        it("is false for subsequent conditions", () => {
            model.conditions = createConditions(ProcessGraph.MinConditions);

            // act
            const isFirstCondition = _.some(
                model.conditions.slice(1),
                condition => controller.isFirstBranch(condition)
            );

            // assert
            expect(isFirstCondition).toBe(false);
        });
    });

    describe("getNodeIcon", () => {
        it("return user task icon for user task", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);
            const node = createDiagramNode(NodeType.UserTask);

            // act
            const icon = controller.getNodeIcon(node);

            // assert
            expect(icon).toBe(controller.userTaskIcon);
        });

        it("returns decision icon for user decision", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);
            const node = createDiagramNode(NodeType.UserDecision);

            // act
            const icon = controller.getNodeIcon(node);

            // assert
            expect(icon).toBe(controller.decisionIcon);
        });

        it("return end icon for end", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);
            const node = createDiagramNode(NodeType.ProcessEnd);

            // act
            const icon = controller.getNodeIcon(node);

            // assert
            expect(icon).toBe(controller.endIcon);
        });

        it("returns error icon for system task", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);
            const node = createDiagramNode(NodeType.SystemTask);

            // act
            const icon = controller.getNodeIcon(node);

            // assert
            expect(icon).toBe(controller.errorIcon);
        });
    });

    describe("getMergeNodeLabel", () => {
        it("returns default condition label if condition doesn't have merge node defined", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions);

            // act
            const label = controller.getMergeNodeLabel(model.conditions[0]);

            // assert
            expect(label).toBe(controller.defaultMergeNodeLabel);
        });

        it("returns condition merge node label if condition merge node exists", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions);
            const userTaskNode = createDiagramNode(NodeType.UserTask);
            userTaskNode.label = "Test";
            model.conditions[0].mergeNode = userTaskNode;
            model.conditions[1].mergeNode = createDiagramNode(NodeType.ProcessEnd);

            // act
            const label = controller.getMergeNodeLabel(model.conditions[0]);

            // assert
            expect(label).toBe(userTaskNode.label);
        });
    });

    describe("canReorder", () => {
        it("returns false for primary branch", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MaxConditions);

            // act
            const canReorder = controller.canReorder(model.conditions[0]);

            // assert
            expect(canReorder).toBe(false);
        });

        it("returns true for secondary branches", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MaxConditions);

            // act
            const canReorder = _.every(
                model.conditions.slice(1),
                condition => controller.canReorder(condition)
            );

            // assert
            expect(canReorder).toBe(true);
        });
    });

    describe("canMoveUp", () => {
        beforeEach(() => {
            model.conditions = createConditions(ProcessGraph.MaxConditions - 1);
        });

        it("is false when no conditions exist", () => {
            // arrange
            model.conditions = [];

            // act
            const canMoveUp = _.some(
                model.conditions,
                (condition) => controller.canMoveUp(condition)
            );

            // assert
            expect(canMoveUp).toBe(false);
        });

        it("is false when read-only", () => {
            // arrange
            model.isReadonly = true;

            // act
            const canMoveUp = _.some(
                model.conditions,
                (condition) => controller.canMoveUp(condition)
            );

            // assert
            expect(canMoveUp).toBe(false);
        });

        it("is false for the primary condition", () => {
            // arrange
            const primaryCondition = model.conditions[0];

            // act
            const canMoveUp = controller.canMoveUp(primaryCondition);

            // assert
            expect(canMoveUp).toBe(false);
        });

        it("is false for the first secondary condition", () => {
            // arrange
            const firstSecondaryCondition = model.conditions[1];

            // act
            const canMoveUp = controller.canMoveUp(firstSecondaryCondition);

            // assert
            expect(canMoveUp).toBe(false);
        });

        it("is true for remaining secondary conditions", () => {
            // arrange
            const remainingSecondaryConditions = model.conditions.slice(2);

            // act
            const canMoveUp = _.every(
                remainingSecondaryConditions,
                (condition) => controller.canMoveUp(condition)
            );

            // assert
            expect(canMoveUp).toBe(true);
        });
    });

    describe("moveUp", () => {
        it("doesn't change conditions if it cannot move condition up", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MaxConditions - 1);
            spyOn(controller, "canMoveUp").and.returnValue(false);
            const index = ProcessGraph.MaxConditions - 1;
            const condition = model.conditions[index];

            // act
            controller.moveUp(condition);

            // assert
            expect(model.conditions[index]).toBe(condition);
        });

        it("swaps with previous condition if it can move condition up", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MaxConditions);
            const index = model.conditions.length - 1;
            const condition = model.conditions[index];
            const orderindex = condition.orderindex;
            const prevIndex = index - 1;
            const prevCondition = model.conditions[prevIndex];
            const prevOrderindex = prevCondition.orderindex;

            // act
            controller.moveUp(condition);

            // assert
            expect(model.conditions[index]).toBe(prevCondition);
            expect(prevCondition.orderindex).toBe(orderindex);
            expect(model.conditions[prevIndex]).toBe(condition);
            expect(condition.orderindex).toBe(prevOrderindex);
        });
    });

    describe("canMoveDown", () => {
        beforeEach(() => {
            model.conditions = createConditions(ProcessGraph.MaxConditions - 1);
        });

        it("is false when no conditions exist", () => {
            // arrange
            model.conditions = [];

            // act
            const canMoveUp = _.some(
                model.conditions,
                (condition) => controller.canMoveUp(condition)
            );

            // assert
            expect(canMoveUp).toBe(false);
        });

        it("is false when read-only", () => {
            // arrange
            model.isReadonly = true;

            // act
            const canMoveDown = _.some(
                model.conditions,
                condition => controller.canMoveDown(condition)
            );

            // assert
            expect(canMoveDown).toBe(false);
        });

        it("is false for primary condition", () => {
            // arrange
            const primaryCondition = model.conditions[0];

            // act
            const canMoveDown = controller.canMoveDown(primaryCondition);

            // assert
            expect(canMoveDown).toBe(false);
        });

        it("is false for last condition", () => {
            // arrange
            const lastCondition = model.conditions[model.conditions.length - 1];

            // act
            const canMoveDown = controller.canMoveDown(lastCondition);

            // assert
            expect(canMoveDown).toBe(false);
        });

        it("is true for remaining conditions", () => {
            // arrange
            const remainingConditions = model.conditions.slice(1, model.conditions.length - 1);

            // act
            const canMoveDown = _.every(
                remainingConditions,
                condition => controller.canMoveDown(condition)
            );

            // assert
            expect(canMoveDown).toBe(true);
        });
    });

    describe("moveDown", () => {
        it("doesn't change conditions if it cannot move condition down", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MaxConditions - 1);
            spyOn(controller, "canMoveDown").and.returnValue(false);
            const index = 1;
            const condition = model.conditions[index];

            // act
            controller.moveDown(condition);

            // assert
            expect(model.conditions[index]).toBe(condition);
        });

        it("swaps with next condition if it can move condition down", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MaxConditions);
            const index = 1;
            const condition = model.conditions[index];
            const orderindex = condition.orderindex;
            const nextIndex = index + 1;
            const nextCondition = model.conditions[nextIndex];
            const nextOrderindex = nextCondition.orderindex;

            // act
            controller.moveDown(condition);

            // assert
            expect(model.conditions[index]).toBe(nextCondition);
            expect(nextCondition.orderindex).toBe(orderindex);
            expect(model.conditions[nextIndex]).toBe(condition);
            expect(condition.orderindex).toBe(nextOrderindex);
        });
    });

    describe("canApplyChanges", () => {
        it("is false if is read-only", () => {
            // arrange
            // act
            model.conditions = createConditions(ProcessGraph.MinConditions);
            model.isReadonly = true;

            // assert
            expect(controller.canApplyChanges).toBe(false);
        });

        it("is false if is label is empty", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions);

            // act
            model.label = null;

            // assert
            expect(controller.canApplyChanges).toBe(false);
        });

        it("is false if is at least one condition doesn't have merge node specified", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions);

            // act
            model.label = "UD1";
            model.conditions[1].mergeNode = null;

            // assert
            expect(controller.canApplyChanges).toBe(false);
        });

        it("is true if not read-only, has label, and all conditions have merge node specified", () => {
            // arrange
            const endNode = createDiagramNode(NodeType.ProcessEnd);
            model.conditions = createConditions(ProcessGraph.MinConditions);

            // act
            model.isReadonly = false;
            model.isHistoricalVersion = false;
            model.label = "UD1";
            model.conditions[0].mergeNode = endNode;
            model.conditions[1].mergeNode = endNode;

            // assert
            expect(controller.canApplyChanges).toBe(true);
        });
    });

    describe("saveData", () => {
        it("updates label", () => {
            // arrange
            model.label = "UD1";
            model.conditions = createConditions(ProcessGraph.MinConditions);
            const decision = <IDecision>createDiagramNode(NodeType.UserDecision);
            decision.setLabelWithRedrawUi = noop;
            decision.getOutgoingLinks = () => [];
            model.originalDecision = decision;
            model.graph = createMockGraph();
            const newValue = "Decision Label";
            const setLabelSpy = spyOn(model.originalDecision, "setLabelWithRedrawUi").and.callThrough();
            spyOn(model.graph, "getMxGraphModel").and.returnValue(null);

            // act
            model.label = newValue;
            controller.saveData();
            $rootScope.$digest();

            // assert
            expect(setLabelSpy).toHaveBeenCalledWith(newValue);
        });

        it("updates merge node", () => {
            // arrange
            model.label = "UD1";
            model.conditions = createConditions(ProcessGraph.MinConditions);
            model.conditions[0].mergeNode = createDiagramNode(NodeType.ProcessEnd);
            model.conditions[1].mergeNode = createDiagramNode(NodeType.UserTask);
            const decision = <IDecision>createDiagramNode(NodeType.UserDecision);
            decision.setLabelWithRedrawUi = noop;
            decision.getOutgoingLinks = () => [<IDiagramLink>{}, <IDiagramLink>{}];
            model.originalDecision = decision;
            model.graph = createMockGraph();

            spyOn(model.originalDecision, "setLabelWithRedrawUi").and.callThrough();
            spyOn(model.graph, "getMxGraphModel").and.returnValue(null);
            spyOn(controller, "updateExistingEdge").and.returnValue(true);

            const modelUpdateSpy = spyOn(model.graph.viewModel.communicationManager.processDiagramCommunication, "modelUpdate");
            const actionSpy = spyOn(model.graph.viewModel.communicationManager.processDiagramCommunication, "action");

            // act
            controller.saveData();
            $rootScope.$digest();

            // assert
            expect(modelUpdateSpy).toHaveBeenCalled();
            expect(actionSpy).toHaveBeenCalledWith(ProcessEvents.ArtifactUpdate);
        });

        it("updates deleted conditions", () => {
            // arrange
            const endNode = createDiagramNode(NodeType.ProcessEnd);
            model.label = "UD1";
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);
            model.conditions[0].mergeNode = endNode;
            model.conditions[1].mergeNode = endNode;
            model.conditions[2].mergeNode = endNode;
            const decision = <IDecision>createDiagramNode(NodeType.UserDecision);
            decision.setLabelWithRedrawUi = noop;
            decision.getOutgoingLinks = () => [];
            model.originalDecision = decision;
            model.graph = createMockGraph();

            spyOn(model.originalDecision, "setLabelWithRedrawUi").and.callThrough();
            spyOn(model.graph, "getMxGraphModel").and.returnValue(null);
            spyOn(controller, "refreshView").and.callFake(noop);
            const deleteSpy = spyOn(ProcessDeleteHelper, "deleteDecisionBranches").and.callFake(noop);

            // act
            controller.deleteCondition(model.conditions[model.conditions.length - 1]);
            controller.saveData();
            $rootScope.$digest();

            // assert
            expect(deleteSpy).toHaveBeenCalled();
        });

        it("updates added conditions", () => {
            // arrange
            const endNode = createDiagramNode(NodeType.ProcessEnd);
            model.label = "UD1";
            model.conditions = createConditions(ProcessGraph.MinConditions);
            model.conditions[0].mergeNode = endNode;
            model.conditions[1].mergeNode = endNode;
            const decision = <IDecision>createDiagramNode(NodeType.UserDecision);
            decision.setLabelWithRedrawUi = noop;
            decision.getOutgoingLinks = () => [];
            model.originalDecision = decision;
            model.graph = createMockGraph();
            model.graph["addDecisionBranches"] = noop;

            spyOn(model.originalDecision, "setLabelWithRedrawUi").and.callThrough();
            spyOn(model.graph, "getMxGraphModel").and.returnValue(null);
            spyOn(controller, "refreshView").and.callFake(noop);
            spyOn(model.graph, "getValidMergeNodes").and.returnValue([]);
            spyOn(controller, "scrollToBottomOfConditionList").and.callFake(noop);
            const addSpy = spyOn(model.graph, "addDecisionBranches").and.callFake(noop);

            // act
            controller.addCondition();
            const condition = model.conditions[model.conditions.length - 1];
            condition.mergeNode = endNode;
            controller.saveData();
            $rootScope.$digest();

            // assert
            expect(addSpy).toHaveBeenCalled();
        });
    });
});

function createConditions(howMany: number): ICondition[] {
    const conditions: ICondition[] = [];

    for (let i: number = 0; i < howMany; i++) {
        const condition = <ICondition>{
            sourceId: 0,
            destinationId: i,
            orderindex: i,
            label: `Condition ${i + 1}`,
            mergeNode: null,
            validMergeNodes: []
        };
        conditions.push(condition);
    }

    return conditions;
}

function createMockGraph(): IProcessGraph {
    return <IProcessGraph>{
        viewModel: {
            communicationManager: {
                processDiagramCommunication: {
                    modelUpdate: null,
                    action: null
                }
            }
        },
        getValidMergeNodes: null,
        getMxGraphModel: null
    };
}

function createDiagramNode(nodeType: NodeType): IDiagramNode {
    return <IDiagramNode>{
        model: {id: 1},
        direction: null,
        action: null,
        label: null,
        row: null,
        column: null,
        newShapeColor: null,
        getNodeType: () => nodeType
    };
}
