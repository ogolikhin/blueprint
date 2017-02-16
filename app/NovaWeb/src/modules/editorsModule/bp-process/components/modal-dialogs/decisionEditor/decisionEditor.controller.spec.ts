/* tslint:disable max-file-line-count */
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
import {
    IDecision,
    IDiagramLink,
    IDiagramNode,
    IProcessGraph,
    IProcessLinkModel,
    IUserTask,
    NodeType,
    ProcessShapeType
} from "../../diagram/presentation/graph/models";
import {ProcessEvents} from "../../diagram/process-diagram-communication";
import {ProcessDeleteHelper} from "../../diagram/presentation/graph/process-delete-helper";
import {ILocalizationService} from "../../../../../commonModule/localization/localization.service";
import {ProcessAddHelper} from "../../diagram/presentation/graph/process-add-helper";
import {IMessageService} from "../../../../../main/components/messages/message.svc";
import {MessageServiceMock} from "../../../../../main/components/messages/message.mock";
import {ICondition} from "./condition.model";

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
    let messageService: IMessageService;
    let noop = () => {/*noop*/};

    beforeEach(angular.mock.module("decisionEditor", ($provide: ng.auto.IProvideService) => {
        $provide.service("$uibModalInstance", ModalServiceInstanceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
    }));

    beforeEach(inject((
        _$rootScope_: ng.IRootScopeService,
        _$timeout_: ng.ITimeoutService,
        _$anchorScroll_: ng.IAnchorScrollService,
        _$location_: ng.ILocationService,
        _$q_: ng.IQService,
        _localization_: ILocalizationService,
        _$uibModalInstance_: ng.ui.bootstrap.IModalServiceInstance,
        _messageService_: IMessageService
    ) => {
        $rootScope = _$rootScope_;
        $rootScope["config"] = {
            labels: {
                "ST_Delete_CannotDelete_UD_AtleastTwoConditions": "ST_Delete_CannotDelete_UD_AtleastTwoConditions"
            }
        };
        $timeout = _$timeout_;
        $anchorScroll = _$anchorScroll_;
        $location = _$location_;
        $q = _$q_;
        localization = _localization_;
        $uibModalInstance = _$uibModalInstance_;
        messageService = _messageService_;

        model = new DecisionEditorModel();
        model.graph = createMockGraph();
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
            expect(lastAddedCondition.mergeNodeId).toBeDefined();
            expect(lastAddedCondition.mergeNodeId).toBe(mergingUserTask.model.id);
        });
    });

    describe("isDeleteConditionVisible", () => {
        it("is false for all conditions if minimum conditions reached", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions);
            spyOn(model.graph, "isFirstFlow").and.returnValue(false);

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
            spyOn(model.graph, "isFirstFlow").and.returnValue(true);

            // act
            const isDeleteConditionVisible = controller.isDeleteConditionVisible(primaryCondition);

            // assert
            expect(isDeleteConditionVisible).toBe(false);
        });

        it("is true for all conditions but primary if minimum conditions not reached", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);
            spyOn(model.graph, "isFirstFlow").and.returnValue(false);

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
            spyOn(model.graph, "isFirstFlow").and.returnValue(false);

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
            spyOn(model.graph, "isFirstFlow").and.returnValue(false);

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
            spyOn(model.graph, "isFirstFlow").and.returnValue(false);

            // act
            controller.deleteCondition(condition);

            // assert
            expect(model.conditions.length).toBe(ProcessGraph.MinConditions + 1);
        });

        it("correctly updates conditions", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);
            const refreshSpy = spyOn(controller, "refreshView").and.callFake(noop);
            spyOn(model.graph, "isFirstFlow").and.returnValue(false);

            // act
            controller.deleteCondition(model.conditions[model.conditions.length - 1]);

            // assert
            expect(model.conditions.length).toBe(ProcessGraph.MinConditions);
            expect(refreshSpy).toHaveBeenCalled();
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
            model.conditions[0].mergeNodeId = userTaskNode.model.id;
            model.conditions[0].mergeNodeLabel = userTaskNode.label;
            model.conditions[1].mergeNodeId = createDiagramNode(NodeType.ProcessEnd).model.id;

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
            const orderindex = condition.orderIndex;
            const prevIndex = index - 1;
            const prevCondition = model.conditions[prevIndex];
            const prevOrderindex = prevCondition.orderIndex;

            // act
            controller.moveUp(condition);

            // assert
            expect(model.conditions[index]).toBe(prevCondition);
            expect(prevCondition.orderIndex).toBe(orderindex);
            expect(model.conditions[prevIndex]).toBe(condition);
            expect(condition.orderIndex).toBe(prevOrderindex);
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
            const orderindex = condition.orderIndex;
            const nextIndex = index + 1;
            const nextCondition = model.conditions[nextIndex];
            const nextOrderindex = nextCondition.orderIndex;

            // act
            controller.moveDown(condition);

            // assert
            expect(model.conditions[index]).toBe(nextCondition);
            expect(nextCondition.orderIndex).toBe(orderindex);
            expect(model.conditions[nextIndex]).toBe(condition);
            expect(condition.orderIndex).toBe(nextOrderindex);
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
            spyOn(model.graph, "isFirstFlow").and.returnValue(false);

            // act
            model.label = "UD1";
            model.conditions[1].mergeNodeId = null;

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
            model.conditions[0].mergeNodeId = endNode.model.id;
            model.conditions[1].mergeNodeId = endNode.model.id;

            // assert
            expect(controller.canApplyChanges).toBe(true);
        });
    });

    describe("applyChanges", () => {
        let decision: IDecision;

        beforeEach(() => {
            decision = <IDecision>createDiagramNode(NodeType.UserDecision);
            decision.label = "UD1";
            decision.setLabelWithRedrawUi = noop;
            model.originalDecision = decision;
            model.label = model.originalDecision.label;
        });

        it("calls setLabel on decision shape", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions);
            const newLabel = "Decision Label";
            const setLabelSpy = spyOn(model.originalDecision, "setLabelWithRedrawUi");

            // act
            model.label = newLabel;
            controller.applyChanges();

            // assert
            expect(setLabelSpy).toHaveBeenCalledWith(newLabel);
        });

        it("doesn't apply changes if no conditions changed", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MaxConditions);
            const applyChangeSpies = _.map(model.conditions, condition => spyOn(condition, "applyChanges"));

            // act
            controller.applyChanges();

            // assert
            _.every(applyChangeSpies, spy => expect(spy).not.toHaveBeenCalled());
        });

        it("doesn't raise update model event if no conditions changed", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MaxConditions);
            const modelUpdatedSpy = spyOn(model.graph.viewModel.communicationManager.processDiagramCommunication, "modelUpdate");

            // act
            controller.applyChanges();

            // assert
            expect(modelUpdatedSpy).not.toHaveBeenCalled();
        });

        it("applies changes if condition label has changed", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MaxConditions);
            const conditionToChange = model.conditions[1];
            const applyChangesSpy = spyOn(conditionToChange, "applyChanges");

            spyOn(conditionToChange, "isLabelChanged").and.returnValue(true);

            // act
            controller.applyChanges();

            // assert
            expect(applyChangesSpy).toHaveBeenCalled();
        });

        it("raises update model event if condition changed", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MaxConditions);
            const conditionToChange = model.conditions[1];
            spyOn(conditionToChange, "isLabelChanged").and.returnValue(true);
            const modelUpdatedSpy = spyOn(model.graph.viewModel.communicationManager.processDiagramCommunication, "modelUpdate");

            // act
            controller.applyChanges();

            // assert
            expect(modelUpdatedSpy).toHaveBeenCalled();
        });

        it("applies changes if condition order index has changed", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MaxConditions);
            const conditionToChange = model.conditions[1];
            const applyChangesSpy = spyOn(conditionToChange, "applyChanges");

            spyOn(conditionToChange, "isOrderIndexChanged").and.returnValue(true);

            // act
            controller.applyChanges();

            // assert
            expect(applyChangesSpy).toHaveBeenCalled();
        });

        it("applies changes if condition merge node has changed", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MaxConditions);
            const conditionToChange = model.conditions[1];
            const applyChangesSpy = spyOn(conditionToChange, "applyChanges");

            spyOn(conditionToChange, "isMergeNodeChanged").and.returnValue(true);

            // act
            controller.applyChanges();

            // assert
            expect(applyChangesSpy).toHaveBeenCalled();
        });

        it("calls applyChanges on added condition if condition can be added", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions);
            spyOn(model.graph, "getValidMergeNodes").and.returnValue([]);
            spyOn(ProcessAddHelper, "canAddDecisionConditions").and.returnValue(true);
            spyOn(controller, "refreshView").and.callFake(noop);
            controller.addCondition();
            const applyChangesCondition = spyOn(_.last(model.conditions), "applyChanges");

            // act
            controller.applyChanges();

            // assert
            expect(applyChangesCondition).toHaveBeenCalled();
        });

        it("raises update model event if condition is added", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions);
            spyOn(model.graph, "getValidMergeNodes").and.returnValue([]);
            spyOn(ProcessAddHelper, "canAddDecisionConditions").and.returnValue(true);
            spyOn(controller, "refreshView").and.callFake(noop);
            controller.addCondition();
            const modelUpdatedSpy = spyOn(model.graph.viewModel.communicationManager.processDiagramCommunication, "modelUpdate");

            // act
            controller.applyChanges();

            // assert
            expect(modelUpdatedSpy).toHaveBeenCalled();
        });

        it("doesn't call applyChanges on added condition if condition cannot be added", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MinConditions);
            spyOn(model.graph, "getValidMergeNodes").and.returnValue([]);
            spyOn(ProcessAddHelper, "canAddDecisionConditions").and.returnValue(false);
            spyOn(controller, "refreshView").and.callFake(noop);
            controller.addCondition();
            const applyChangesCondition = spyOn(_.last(model.conditions), "applyChanges");

            // act
            controller.applyChanges();

            // assert
            expect(applyChangesCondition).not.toHaveBeenCalled();
        });

        it("calls applyChanges on deleted condition if condition can be deleted", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MaxConditions);
            spyOn(model.graph, "getValidMergeNodes").and.returnValue([]);
            spyOn(ProcessDeleteHelper, "canDeleteDecisionConditions").and.returnValue(true);
            spyOn(controller, "refreshView").and.callFake(noop);
            const conditionToDelete = model.conditions[1];
            const applyChangesCondition = spyOn(conditionToDelete, "applyChanges");

            // act
            controller.deleteCondition(conditionToDelete);
            controller.applyChanges();

            // assert
            expect(applyChangesCondition).toHaveBeenCalled();
        });

        it("raises update model event if condition is deleted", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MaxConditions);
            spyOn(model.graph, "getValidMergeNodes").and.returnValue([]);
            spyOn(ProcessDeleteHelper, "canDeleteDecisionConditions").and.returnValue(true);
            spyOn(controller, "refreshView").and.callFake(noop);
            const conditionToDelete = model.conditions[1];
            controller.deleteCondition(conditionToDelete);
            const modelUpdatedSpy = spyOn(model.graph.viewModel.communicationManager.processDiagramCommunication, "modelUpdate");

            // act
            controller.applyChanges();

            // assert
            expect(modelUpdatedSpy).toHaveBeenCalled();
        });

        it("doesn't call applyChanges on deleted condition if condition cannot be deleted", () => {
            // arrange
            model.conditions = createConditions(ProcessGraph.MaxConditions);
            spyOn(model.graph, "getValidMergeNodes").and.returnValue([]);
            spyOn(ProcessDeleteHelper, "canDeleteDecisionConditions").and.returnValue(false);
            spyOn(controller, "refreshView").and.callFake(noop);
            const conditionToDelete = model.conditions[1];
            const applyChangesCondition = spyOn(conditionToDelete, "applyChanges");

            // act
            controller.deleteCondition(conditionToDelete);
            controller.applyChanges();

            // assert
            expect(applyChangesCondition).not.toHaveBeenCalled();
        });

        it("re-orders links if order index for conditions has changed", () => {
            // arrange
            const links = createOrderIndexTestLinks();
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);
            model.graph.viewModel.links = links;
            spyOn(model.originalDecision, "setLabelWithRedrawUi").and.callFake(noop);
            spyOn(model.graph, "getMxGraphModel").and.returnValue(null);
            spyOn(model.graph, "isFirstFlow").and.returnValue(false);
            spyOn(model.graph, "getBranchStartingLink").and.returnValue({});
            spyOn(model.conditions[1], "isOrderIndexChanged").and.returnValue(true);

            const expectedLinks = _.orderBy(links, link => link.orderindex);

            // act
            controller.applyChanges();

            // assert
            expect(model.graph.viewModel.links[0]).toBe(expectedLinks[0]);
            expect(model.graph.viewModel.links[1]).toBe(expectedLinks[1]);
            expect(model.graph.viewModel.links[2]).toBe(expectedLinks[2]);
        });

        it("doesn't re-order links if order index for conditions has not changed", () => {
            // arrange
            const links = createOrderIndexTestLinks();
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);
            model.graph.viewModel.links = links;
            spyOn(model.originalDecision, "setLabelWithRedrawUi").and.callFake(noop);
            spyOn(model.graph, "getMxGraphModel").and.returnValue(null);
            spyOn(model.graph, "isFirstFlow").and.returnValue(false);
            spyOn(model.graph, "getBranchStartingLink").and.returnValue({});

            const expectedLinks = links;

            // act
            controller.applyChanges();

            // assert
            expect(model.graph.viewModel.links[0]).toBe(expectedLinks[0]);
            expect(model.graph.viewModel.links[1]).toBe(expectedLinks[1]);
            expect(model.graph.viewModel.links[2]).toBe(expectedLinks[2]);
        });

        function createOrderIndexTestLinks(): IProcessLinkModel[] {
            return [
                {
                    sourceId: model.originalDecision.model.id,
                    destinationId: 2,
                    orderindex: 0,
                    label: "",
                    parentId: null,
                    sourceNode: null,
                    destinationNode: null
                },
                {
                    sourceId: model.originalDecision.model.id,
                    destinationId: 4,
                    orderindex: 2,
                    label: "",
                    parentId: null,
                    sourceNode: null,
                    destinationNode: null
                },
                {
                    sourceId: model.originalDecision.model.id,
                    destinationId: 3,
                    orderindex: 1,
                    label: "",
                    parentId: null,
                    sourceNode: null,
                    destinationNode: null
                }
            ];
        }
    });

    function createConditions(howMany: number): ICondition[] {
        const conditions: ICondition[] = [];

        for (let i: number = 0; i < howMany; i++) {
            const condition = <ICondition>{
                orderIndex: i,
                label: `Condition ${i + 1}`,
                mergeNodeId: null,
                validMergeNodes: [],
                applyChanges: (graph) => true,
                isCreated: false,
                isDeleted: false,
                isLabelChanged: false,
                isOrderIndexChanged: false,
                isMergeNodeChanged: false
            };

            conditions.push(condition);
        }

        return conditions;
    }

    function createMockGraph(): IProcessGraph {
        return <IProcessGraph>{
            viewModel: {
                getShapeById: (id) => null,
                getShapeTypeById: (id) => ProcessShapeType.None,
                getNextShapeIds: (id) => [],
                communicationManager: {
                    processDiagramCommunication: {
                        modelUpdate: (id) => { return; },
                        action: (evts) => { return; }
                    }
                }
            },
            rootScope: $rootScope,
            messageService: messageService,
            getValidMergeNodes: (id) => { return; },
            getMxGraphModel: null,
            isFirstFlow: null,
            getBranchStartingLink: null,
            addDecisionBranch: (decisionId, label, mergeNodeId) => true,
            isInMainFlow: (id) => false,
            isInNestedFlow: (id) => false
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
});
