import {ProcessAddHelper} from "../../diagram/presentation/graph/process-add-helper";
import {IDecision} from "../../diagram/presentation/graph/models/process-graph-interfaces";
import {BaseModalDialogController, IModalScope} from "../base-modal-dialog-controller";
import {DecisionEditorModel} from "./decisionEditor.model";
import {IProcessLink} from "../../../models/process-models";
import {ProcessGraph} from "../../diagram/presentation/graph/process-graph";
import {ProcessDeleteHelper} from "../../diagram/presentation/graph/process-delete-helper";
import {IDiagramLink, IDiagramNode, IProcessGraph, NodeType} from "../../diagram/presentation/graph/models";
import {ProcessEvents} from "../../diagram/process-diagram-communication";
import {ILocalizationService} from "../../../../../commonModule/localization/localization.service";
import {ICondition, Condition} from "./condition.model";

export class DecisionEditorController extends BaseModalDialogController<DecisionEditorModel> implements ng.IComponentController {
    private CONDITION_MAX_LENGTH = 512;
    private LABEL_MAX_LENGTH = 140;

    private deletedConditions: ICondition[] = [];

    public get userTaskIcon(): string {
        return "fonticon fonticon-bp-actor";
    }

    public get decisionIcon(): string {
        return "fonticon fonticon-decision-diamond";
    }

    public get endIcon(): string {
        return "fonticon fonticon-storyteller-end";
    }

    public get errorIcon(): string {
        return "fonticon fonticon-error";
    }

    public static $inject = [
        "$rootScope",
        "$scope",
        "$timeout",
        "$anchorScroll",
        "$location",
        "$q",
        "localization"
    ];

    constructor(
        $rootScope: ng.IRootScopeService,
        $scope: IModalScope,
        $timeout: ng.ITimeoutService,
        private $anchorScroll: ng.IAnchorScrollService,
        private $location: ng.ILocationService,
        private $q: ng.IQService,
        private localization: ILocalizationService,
        $uibModalInstance?: ng.ui.bootstrap.IModalServiceInstance,
        dialogModel?: DecisionEditorModel
    ) {
        super($rootScope, $scope, $timeout, $uibModalInstance, dialogModel);
    }

    public get defaultMergeNodeLabel(): string {
        return this.localization.get("ST_Decision_Modal_Next_Task_Label");
    }

    public get hasMaxConditions(): boolean {
        return this.dialogModel.conditions.length >= ProcessGraph.MaxConditions;
    }

    public get hasMinConditions(): boolean {
        return this.dialogModel.conditions.length <= ProcessGraph.MinConditions;
    }

    public get isReadonly(): boolean {
        return !this.dialogModel || this.dialogModel.isReadonly || this.dialogModel.isHistoricalVersion;
    }

    public get canApplyChanges(): boolean {
        return !this.isReadonly && this.isLabelAvailable() && !this.areMergeNodesEmpty();
    }

    private get createdConditions(): ICondition[] {
        return _.filter(
            this.dialogModel.conditions,
            condition => condition.isCreated
        );
    }

    private get changedConditions(): ICondition[] {
        return _.filter(
            this.dialogModel.conditions,
            condition => condition.isChanged
        );
    }

    public applyChanges(): ng.IPromise<void> {
        const decisionId: number = this.dialogModel.subArtifactId;
        const graph: IProcessGraph = this.dialogModel.graph;
        const hasOrderIndexChanges = this.changedConditions.some(condition => condition.isOrderIndexChanged);
        const firstNodeIds = this.deletedConditions.filter(condition => condition.firstNodeId).map(condition => condition.firstNodeId);
        let updated: boolean = false;

        this.dialogModel.originalDecision.setLabelWithRedrawUi(this.dialogModel.label);

        if (this.changedConditions.length > 0) {
            _.each(this.changedConditions, condition => condition.applyChanges(graph));
            updated = true;
        }

        if (this.createdConditions.length > 0 &&
            ProcessAddHelper.canAddDecisionConditions(decisionId, this.createdConditions.length, graph)) {
            _.each(this.createdConditions, condition => condition.applyChanges(graph));
            updated = true;
        }

        if (this.deletedConditions.length > 0 &&
            ProcessDeleteHelper.canDeleteDecisionConditions(decisionId, firstNodeIds, graph)) {
            _.each(this.deletedConditions, condition => condition.applyChanges(graph));
            updated = true;
        }

        if (updated) {
            // For process, links are always assumed to be ordered by ascending order index
            if (hasOrderIndexChanges) {
                graph.viewModel.links = _.orderBy(graph.viewModel.links, link => link.orderindex);
            }

            graph.viewModel.communicationManager.processDiagramCommunication.modelUpdate(decisionId);
            graph.viewModel.communicationManager.processDiagramCommunication.action(ProcessEvents.ArtifactUpdate);
        }

        return this.$q.resolve();
    }

    public get canAddCondition(): boolean {
        return !this.isReadonly && !this.hasMaxConditions;
    }

    public addCondition() {
        if (!this.canAddCondition) {
            return;
        }

        const decisionId = this.dialogModel.originalDecision.model.id;
        const newOrderIndex = _.last(this.dialogModel.conditions).orderIndex + 1;
        const processLink = <IProcessLink>{
            sourceId: decisionId,
            destinationId: null,
            orderindex: newOrderIndex,
            label: `${this.dialogModel.conditionHeader}${newOrderIndex + 1}`
        };

        const validMergeNodes = this.dialogModel.graph.getValidMergeNodes(processLink);
        const newCondition: ICondition = new Condition(processLink, null, null, validMergeNodes);
        newCondition.mergeNodeId = this.dialogModel.defaultDestinationId;

        this.dialogModel.conditions.push(newCondition);
        this.refreshView();
        this.scrollToBottomOfConditionList();
    }

    private scrollToBottomOfConditionList() {
        const self = this;
        this.$timeout(function () {
            const oldHash = self.$location.hash();
            self.$location.hash("decision-modal-list-bottom");
            self.$anchorScroll();
            self.$location.hash(oldHash);
        });
    }

    public isDeleteConditionVisible(condition: ICondition): boolean {
        return !this.hasMinConditions && !this.isFirstCondition(condition);
    }

    public canDeleteCondition(condition: ICondition): boolean {
        return !this.isReadonly && this.isDeleteConditionVisible(condition);
    }

    public deleteCondition(condition: ICondition) {
        if (!this.canDeleteCondition(condition)) {
            return;
        }

        condition.delete();
        this.deletedConditions.push(condition);

        const index = this.dialogModel.conditions.indexOf(condition);
        if (index !== -1) {
            this.dialogModel.conditions.splice(index, 1);
        }

        this.refreshView();
    }

    public canReorder(condition: ICondition): boolean {
        return this.canMoveUp(condition) || this.canMoveDown(condition);
    }

    public canMoveUp(condition: ICondition): boolean {
        return !this.isReadonly
            && this.dialogModel.conditions
            && this.dialogModel.conditions.indexOf(condition) > 1;
    }

    public moveUp(condition: ICondition): void {
        if (!this.canMoveUp(condition)) {
            return;
        }

        const index = this.dialogModel.conditions.indexOf(condition);
        const previousCondition = this.dialogModel.conditions[index - 1];
        this.dialogModel.conditions.splice(index - 1, 2, condition, previousCondition);
        this.swapConditions(previousCondition, condition);
    }

    public canMoveDown(condition: ICondition): boolean {
        if (this.isReadonly) {
            return false;
        }

        const index = this.dialogModel.conditions.indexOf(condition);

        return index > 0 && index < this.dialogModel.conditions.length - 1;
    }

    public moveDown(condition: ICondition): void {
        if (!this.canMoveDown(condition)) {
            return;
        }

        const index = this.dialogModel.conditions.indexOf(condition);
        const nextCondition = this.dialogModel.conditions[index + 1];
        this.dialogModel.conditions.splice(index, 2, nextCondition, condition);
        this.swapConditions(condition, nextCondition);
    }

    private swapConditions(condition1: ICondition, condition2: ICondition): void {
        const orderIndex = condition1.orderIndex;
        condition1.orderIndex = condition2.orderIndex;
        condition2.orderIndex = orderIndex;
    }

    public isLabelAvailable(): boolean {
        return this.dialogModel.label != null && this.dialogModel.label !== "";
    }

    public getMergeNodeLabel(condition: ICondition): string {
        return condition.mergeNodeLabel || this.defaultMergeNodeLabel;
    }

    private areMergeNodesEmpty(): boolean {
        for (let i = 0; i < this.dialogModel.conditions.length; i++) {
            const condition = this.dialogModel.conditions[i];

            if (!condition.mergeNodeId && !this.isFirstConditionOnMainFlow(condition)) {
                return true;
            }
        }

        return false;
    }

    public isFirstCondition(condition: ICondition): boolean {
        return this.dialogModel.conditions.indexOf(condition) === 0;
    }

    public isFirstConditionOnMainFlow(condition: ICondition): boolean {
        return this.isFirstCondition(condition)
            && this.dialogModel.graph.isInMainFlow(condition.decisionId);
    }

    public getNodeIcon(node: IDiagramNode): string {
        switch (node.getNodeType()) {
            case NodeType.UserTask:
                return this.userTaskIcon;

            case NodeType.UserDecision:
                return this.decisionIcon;

            case NodeType.ProcessEnd:
                return this.endIcon;

            default:
                return this.errorIcon;
        }
    }

    public isUserDecision(): boolean {
        return this.dialogModel.originalDecision.getNodeType() === NodeType.UserDecision;
    }

    public get deleteConditionLabel(): string {
        return `${this.localization.get("App_Button_Delete")} ${this.dialogModel.conditionHeader}`;
    }

    public get addConditionLabel(): string {
        return `${this.localization.get("App_Button_Add")} ${this.dialogModel.conditionHeader}`;
    }
}
