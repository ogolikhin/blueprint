import * as angular from "angular";
import "angular-mocks";
require("script!mxClient");
import "../../..";
import {DecisionEditorModel} from "./decision-editor-model";
import {DecisionEditorController} from "./decision-editor-controller";
import {ModalServiceInstanceMock} from "../../../../../shell/login/mocks.spec";
import {ILocalizationService} from "../../../../../core/localization";
import {LocalizationServiceMock} from "../../../../../core/localization/localization.mock";
import {IModalScope} from "../base-modal-dialog-controller";
import {ProcessGraph} from "../../diagram/presentation/graph/process-graph";
import {IProcessGraph, IDiagramNode, IDiagramLink, NodeType, ICondition, IDecision} from "../../diagram/presentation/graph/models";
import {ProcessEvents} from "../../diagram/process-diagram-communication";
import {ProcessDeleteHelper} from "../../diagram/presentation/graph/process-delete-helper";

describe("DecisionEditorController", () => {
    let $rootScope: ng.IRootScopeService;
    let $timeout: ng.ITimeoutService;
    let $anchorScroll: ng.IAnchorScrollService;
    let $location: ng.ILocationService;
    let localization: ILocalizationService;
    let $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance;
    let noop = () => {/*noop*/};

    beforeEach(angular.mock.module("bp.editors.process", ($provide: ng.auto.IProvideService) => {
        $provide.service("$uibModalInstance", ModalServiceInstanceMock);
        $provide.service("localization", LocalizationServiceMock);
    }));

    beforeEach(inject((
        _$rootScope_: ng.IRootScopeService,
        _$timeout_: ng.ITimeoutService,
        _$anchorScroll_: ng.IAnchorScrollService,
        _$location_: ng.ILocationService,
        _localization_: ILocalizationService,
        _$uibModalInstance_: ng.ui.bootstrap.IModalServiceInstance
    ) => {
        $rootScope = _$rootScope_;
        $timeout = _$timeout_;
        $anchorScroll = _$anchorScroll_;
        $location = _$location_;
        localization = _localization_;
        $uibModalInstance = _$uibModalInstance_;
    }));

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
            model: { id: 1 }, 
            direction: null, 
            action: null, 
            label: null, 
            row: null, 
            column: null, 
            newShapeColor: null, 
            getNodeType: () => nodeType
        };
    }

    describe("defaultMergeNodeLabel", () => {
        it("calls localization service with correct string", () => {
            // arrange
            const model = new DecisionEditorModel();
            model.isReadonly = true;
            model.isHistoricalVersion = false;

            const $scope = <IModalScope>$rootScope.$new();
            const localizationSpy = spyOn(localization, "get");
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            // act
            const label = controller.defaultMergeNodeLabel;

            // assert
            expect(localizationSpy).toHaveBeenCalledWith("ST_Decision_Modal_Next_Task_Label");
        });
    });

    describe("isReadonly", () => {
        it("is true if dialog model is read-only", () => {
            // arrange
            const model = new DecisionEditorModel();
            model.isReadonly = true;
            model.isHistoricalVersion = false;

            const $scope = <IModalScope>$rootScope.$new();

            // act
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            // assert
            expect(controller.isReadonly).toBe(true);
        });

        it("is true if dialog model is historical", () => {
            // arrange
            const model = new DecisionEditorModel();
            model.isReadonly = false;
            model.isHistoricalVersion = true;

            const $scope = <IModalScope>$rootScope.$new();

            // act
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            // assert
            expect(controller.isReadonly).toBe(true);
        });

        it("is false if dialog model is neither historical nor read-only", () => {
            // arrange
            const model = new DecisionEditorModel();
            model.isReadonly = false;
            model.isHistoricalVersion = false;

            const $scope = <IModalScope>$rootScope.$new();

            // act
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            // assert
            expect(controller.isReadonly).toBe(false);
        });
    });

    describe("hasMaxConditions", () => {
        it("is true when reached maximum conditions", () => {
            // arrange
            const model = new DecisionEditorModel();
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            // act
            model.conditions = createConditions(ProcessGraph.MaxConditions);

            // assert
            expect(controller.hasMaxConditions).toBe(true);
        });

        it("is false when not reached maximum conditions", () => {
            // arrange
            const model = new DecisionEditorModel();
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            // act
            model.conditions = createConditions(ProcessGraph.MaxConditions - 1);

            // assert
            expect(controller.hasMaxConditions).toBe(false);
        });
    });

    describe("hasMinConditions", () => {
        it("is true when reached minimum conditions", () => {
            // arrange
            const model = new DecisionEditorModel();
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            // act
            model.conditions = createConditions(ProcessGraph.MinConditions);

            // assert
            expect(controller.hasMinConditions).toBe(true);
        });

        it("is false when not reached minimum conditions", () => {
            // arrange
            const model = new DecisionEditorModel();
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            // act
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);

            // assert
            expect(controller.hasMinConditions).toBe(false);
        });
    });

    describe("canAddCondition", () => {
        it("is false when dialog is read-only", () => {
            // arrange
            const model = new DecisionEditorModel();
            model.conditions = createConditions(ProcessGraph.MaxConditions - 1);
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            // act
            controller.isReadonly = true;

            // assert
            expect(controller.canAddCondition).toBe(false);
        });

        it("is false when reached maximum conditions", () => {
            // arrange
            const model = new DecisionEditorModel();
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            // act
            model.conditions = createConditions(ProcessGraph.MaxConditions);

            // assert
            expect(controller.canAddCondition).toBe(false);
        });

        it("is true when not reached maximum conditions", () => {
            // arrange
            const model = new DecisionEditorModel();
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            // act
            model.conditions = createConditions(ProcessGraph.MaxConditions - 1);

            // assert
            expect(controller.canAddCondition).toBe(true);
        });
    });

    describe("addCondition", () => {
        it("doesn't update conditions if read-only", () => {
            // arrange
            const $scope = <IModalScope>$rootScope.$new();
            const decision = <IDecision>{
                model: { id: 1 }
            };
            const model = new DecisionEditorModel();
            model.originalDecision = decision;
            model.conditions = createConditions(ProcessGraph.MinConditions);

            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );
            controller.isReadonly = true;

            // act
            controller.addCondition();

            // assert
            expect(model.conditions.length).toBe(ProcessGraph.MinConditions);
        });

        it("correcly updates conditions", () => {
            // arrange
            const decision = <IDecision>{
                model: { id: 1 }
            };
            const model = new DecisionEditorModel();
            model.originalDecision = decision;
            model.graph = createMockGraph(); 
            model.conditions = createConditions(ProcessGraph.MinConditions);

            spyOn(model.graph, "getValidMergeNodes").and.returnValue([]);

            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            const refreshSpy = spyOn(controller, "refreshView").and.callFake(noop);
            const scrollSpy = spyOn(controller, "scrollToBottomOfConditionList").and.callFake(noop);

            // act
            controller.addCondition();

            // assert
            expect(model.conditions.length).toBe(ProcessGraph.MinConditions + 1);
            expect(refreshSpy).toHaveBeenCalled();
            expect(scrollSpy).toHaveBeenCalled();
        });
    });

    describe("isDeleteConditionVisible", () => {
        it("is false for all conditions if minimum conditions reached", () => {
            // arrange
            const model = new DecisionEditorModel();
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            // act
            model.conditions = createConditions(ProcessGraph.MinConditions);

            // assert
            for (const condition of model.conditions) {
                expect(controller.isDeleteConditionVisible(condition)).toBe(false);
            }
        });

        it("is true for all conditions but first if minimum conditions not reached", () => {
            // arrange
            const model = new DecisionEditorModel();
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            // act
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);

            // assert
            for (const condition of model.conditions) {
                if (condition.orderindex === 0) {
                    expect(controller.isDeleteConditionVisible(condition)).toBe(false);
                    continue;
                }

                expect(controller.isDeleteConditionVisible(condition)).toBe(true);
            }
        });
    });

    describe("canDeleteCondition", () => {
        it("is false when read-only", () => {
            // arrange
            const model = new DecisionEditorModel();
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );
            controller.isReadonly = true;

            // act
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);

            // assert
            expect(controller.canDeleteCondition).toBe(false);
        });

        it("is true when not read-only", () => {
            // arrange
            const model = new DecisionEditorModel();
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            // act
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);

            // arrange
            expect(controller.canDeleteCondition).toBe(true);
        });
    });

    describe("deleteCondition", () => {
        it("doesn't update conditions if read-only", () => {
            // arrange
            const model = new DecisionEditorModel();
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);

            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );
            controller.isReadonly = true;

            // act
            controller.deleteCondition(model.conditions[model.conditions.length - 1]);

            // assert
            expect(model.conditions.length).toBe(ProcessGraph.MinConditions + 1);
        });

        it("doesn't update conditions if deleted condition is not part of the collection", () => {
            // arrange
            const model = new DecisionEditorModel();
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);

            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            spyOn(controller, "refreshView").and.callFake(noop);
            
            const condition = createConditions(1)[0];

            // act
            controller.deleteCondition(condition);

            // assert
            expect(model.conditions.length).toBe(ProcessGraph.MinConditions + 1);
        });

        it("correctly updates conditions", () => {
            // arrange
            const model = new DecisionEditorModel();
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);

            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            const refreshSpy = spyOn(controller, "refreshView").and.callFake(noop);

            // act
            controller.deleteCondition(model.conditions[model.conditions.length - 1]);

            // assert
            expect(model.conditions.length).toBe(ProcessGraph.MinConditions);
            expect(refreshSpy).toHaveBeenCalled();
        });
    });

    describe("isFirstBranch", () => {
        it("is true for first condition", () => {
            // arrange
            const model = new DecisionEditorModel();
            model.conditions = createConditions(ProcessGraph.MinConditions);
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            // act
            const isFirstCondition = controller.isFirstBranch(model.conditions[0]);

            // assert
            expect(isFirstCondition).toBe(true);
        });

        it("is false for subsequent conditions", () => {
            const model = new DecisionEditorModel();
            model.conditions = createConditions(ProcessGraph.MinConditions);
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            // act
            const isFirstCondition = controller.isFirstBranch(model.conditions[0]);

            // assert
            for (let i: number = 1; i < model.conditions.length; i++) {
                expect(controller.isFirstBranch(model.conditions[i])).toBe(false);
            }
        });
    });

    describe("getNodeIcon", () => {
        it("return user task icon for user task", () => {
            // arrange
            const model = new DecisionEditorModel();
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );
            const node = createDiagramNode(NodeType.UserTask);

            // act
            const icon = controller.getNodeIcon(node);

            // assert
            expect(icon).toBe(controller.userTaskIcon);
        });

        it("returns decision icon for user decision", () => {
            // arrange
            const model = new DecisionEditorModel();
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );
            const node = createDiagramNode(NodeType.UserDecision);

            // act
            const icon = controller.getNodeIcon(node);

            // assert
            expect(icon).toBe(controller.decisionIcon);
        });

        it("return end icon for end", () => {
            // arrange
            const model = new DecisionEditorModel();
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );
            const node = createDiagramNode(NodeType.ProcessEnd);

            // act
            const icon = controller.getNodeIcon(node);

            // assert
            expect(icon).toBe(controller.endIcon);
        });

        it("returns error icon for system task", () => {
            // arrange
            const model = new DecisionEditorModel();
            model.conditions = createConditions(ProcessGraph.MinConditions + 1);
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );
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
            const model = new DecisionEditorModel();
            model.conditions = createConditions(ProcessGraph.MinConditions);
            const userTaskNode = createDiagramNode(NodeType.UserTask);
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );
            
            // act
            const label = controller.getMergeNodeLabel(model.conditions[0]);

            // assert
            expect(label).toBe(controller.defaultMergeNodeLabel);
        });

        it("returns condition merge node label if condition merge node exists", () => {
            // arrange
            const model = new DecisionEditorModel();
            model.conditions = createConditions(ProcessGraph.MinConditions);
            const userTaskNode = createDiagramNode(NodeType.UserTask);
            userTaskNode.label = "Test";
            model.conditions[0].mergeNode = userTaskNode;
            model.conditions[1].mergeNode = createDiagramNode(NodeType.ProcessEnd);
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );
            
            // act
            const label = controller.getMergeNodeLabel(model.conditions[0]);

            // assert
            expect(label).toBe(userTaskNode.label);
        });
    });

    describe("canApplyChanges", () => {
        it("is false if is read-only", () => {
            // arrange
            const model = new DecisionEditorModel();
            model.conditions = createConditions(ProcessGraph.MinConditions);
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            // act
            controller.isReadonly = true;

            // assert
            expect(controller.canApplyChanges).toBe(false);
        });

        it("is false if is label is empty", () => {
            // arrange
            const model = new DecisionEditorModel();
            model.conditions = createConditions(ProcessGraph.MinConditions);
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            // act
            model.label = null;

            // assert
            expect(controller.canApplyChanges).toBe(false);
        });

        it("is false if is at least one condition doesn't have merge node specified", () => {
            // arrange
            const model = new DecisionEditorModel();
            model.conditions = createConditions(ProcessGraph.MinConditions);
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            // act
            model.label = "UD1";
            model.conditions[1].mergeNode = null;

            // assert
            expect(controller.canApplyChanges).toBe(false);
        });

        it("is true if not read-only, has label, and all conditions have merge node specified", () => {
            // arrange
            const endNode = createDiagramNode(NodeType.ProcessEnd);
            const model = new DecisionEditorModel();
            model.conditions = createConditions(ProcessGraph.MinConditions);
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

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
            const model = new DecisionEditorModel();
            model.label = "UD1";
            model.conditions = createConditions(ProcessGraph.MinConditions);
            const decision = <IDecision>createDiagramNode(NodeType.UserDecision);
            decision.setLabelWithRedrawUi = noop;
            decision.getOutgoingLinks = () => [];
            model.originalDecision = decision;
            model.graph = createMockGraph();
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );
            const newValue = "Decision Label";
            const setLabelSpy = spyOn(model.originalDecision, "setLabelWithRedrawUi").and.callThrough();
            spyOn(model.graph, "getMxGraphModel").and.returnValue(null);

            // act
            model.label = newValue;
            controller.saveData();

            // assert
            expect(setLabelSpy).toHaveBeenCalledWith(newValue);
        });

        it("updates merge node", () => {
            // arrange
            const model = new DecisionEditorModel();
            model.label = "UD1";
            model.conditions = createConditions(ProcessGraph.MinConditions);
            model.conditions[0].mergeNode = createDiagramNode(NodeType.ProcessEnd);
            model.conditions[1].mergeNode = createDiagramNode(NodeType.UserTask);
            const decision = <IDecision>createDiagramNode(NodeType.UserDecision);
            decision.setLabelWithRedrawUi = noop;
            decision.getOutgoingLinks = () => [<IDiagramLink>{}, <IDiagramLink>{}];
            model.originalDecision = decision;
            model.graph = createMockGraph();
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            spyOn(model.originalDecision, "setLabelWithRedrawUi").and.callThrough();
            spyOn(model.graph, "getMxGraphModel").and.returnValue(null);
            spyOn(controller, "updateExistingEdge").and.returnValue(true);
            
            const modelUpdateSpy = spyOn(model.graph.viewModel.communicationManager.processDiagramCommunication, "modelUpdate");
            const actionSpy = spyOn(model.graph.viewModel.communicationManager.processDiagramCommunication, "action");

            // act
            controller.saveData();

            // assert
            expect(modelUpdateSpy).toHaveBeenCalled();
            expect(actionSpy).toHaveBeenCalledWith(ProcessEvents.ArtifactUpdate);
        });

        it("updates deleted conditions", () => {
            // arrange
            const endNode = createDiagramNode(NodeType.ProcessEnd);
            const model = new DecisionEditorModel();
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
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

            spyOn(model.originalDecision, "setLabelWithRedrawUi").and.callThrough();
            spyOn(model.graph, "getMxGraphModel").and.returnValue(null);
            spyOn(controller, "refreshView").and.callFake(noop);
            const deleteSpy = spyOn(ProcessDeleteHelper, "deleteDecisionBranches").and.callFake(noop);
            
            // act
            controller.deleteCondition(model.conditions[model.conditions.length - 1]);
            controller.saveData();

            // assert
            expect(deleteSpy).toHaveBeenCalled();
        });

        it("updates added conditions", () => {
            // arrange
            const endNode = createDiagramNode(NodeType.ProcessEnd);
            const model = new DecisionEditorModel();
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
            const $scope = <IModalScope>$rootScope.$new();
            const controller = new DecisionEditorController(
                $rootScope, $scope, $timeout, $anchorScroll, 
                $location, localization, $uibModalInstance, model
            );

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

            // assert
            expect(addSpy).toHaveBeenCalled();
        });
    });
});