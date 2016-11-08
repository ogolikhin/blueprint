import {BaseModalDialogController, IModalScope} from "../base-modal-dialog-controller";
import {DecisionEditorModel} from "./decision-editor-model";
import {IProcessLink} from "../../../models/process-models";
import {ProcessGraph} from "../../diagram/presentation/graph/process-graph";
import {ProcessDeleteHelper} from "../../diagram/presentation/graph/process-delete-helper";
import {Condition} from "../../diagram/presentation/graph/shapes";
import {NodeType, IDiagramNode, IDiagramLink, ICondition} from "../../diagram/presentation/graph/models";
import {ProcessEvents} from "../../diagram/process-diagram-communication";
import {ILocalizationService} from "../../../../../core/localization/localizationService";

export class DecisionEditorController extends BaseModalDialogController<DecisionEditorModel> implements ng.IComponentController {
    private CONDITION_MAX_LENGTH = 40;

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

    public isReadonly: boolean = false;

    public static $inject = [
        "$rootScope",
        "$scope",
        "$timeout",
        "$anchorScroll",
        "$location",
        "localization"
    ];

    constructor($rootScope: ng.IRootScopeService,
                $scope: IModalScope,
                private $timeout: ng.ITimeoutService,
                private $anchorScroll: ng.IAnchorScrollService,
                private $location: ng.ILocationService,
                private localization: ILocalizationService,
                $uibModalInstance?: ng.ui.bootstrap.IModalServiceInstance,
                dialogModel?: DecisionEditorModel) {
        super($rootScope, $scope, $uibModalInstance, dialogModel);

        this.isReadonly = this.dialogModel.isReadonly || this.dialogModel.isHistoricalVersion;
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

    public get canApplyChanges(): boolean {
        return !this.isReadonly && this.isLabelAvailable() && !this.areMergeNodesEmpty();
    }

    public saveData() {
        this.populateDecisionChanges();
        this.addNewBranchesToGraph();
        this.removeDeletedBranchesFromGraph();
    }

    public get canAddCondition(): boolean {
        return !this.isReadonly && !this.hasMaxConditions;
    }

    public addCondition() {
        if (!this.canAddCondition) {
            return;
        }

        const conditionNumber = this.dialogModel.conditions.length + 1;
        const processLink: IProcessLink = <IProcessLink>{
            sourceId: this.dialogModel.originalDecision.model.id,
            destinationId: null,
            orderindex: null,
            label: `${this.localization.get("ST_Decision_Modal_New_System_Task_Edge_Label")} ${conditionNumber}`
        };

        const validMergeNodes = this.dialogModel.graph.getValidMergeNodes(processLink);
        const newCondition: ICondition = Condition.create(processLink, null, validMergeNodes);

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
        return !this.hasMinConditions && !this.isFirstBranch(condition);
    }

    public canDeleteCondition(condition: ICondition): boolean {
        return !this.isReadonly && this.isDeleteConditionVisible(condition);
    }

    public deleteCondition(condition: ICondition) {
        if (!this.canDeleteCondition(condition)) {
            return;
        }

        const itemToDeleteIndex = this.dialogModel.conditions.indexOf(condition);

        if (itemToDeleteIndex > -1) {
            this.dialogModel.conditions.splice(itemToDeleteIndex, 1);

            if (condition.destinationId != null) {
                this.deletedConditions.push(condition);
            }
        }

        this.refreshView();
    }

    private getFirstNonMergingPointShapeId(link: IDiagramLink) {
        let targetId: number;

        if (link.targetNode.getNodeType() === NodeType.MergingPoint) {
            const outgoingLinks = link.targetNode.getOutgoingLinks(this.dialogModel.graph.getMxGraphModel());
            targetId = outgoingLinks[0].model.destinationId;
        } else {
            targetId = link.targetNode.model.id;
        }

        return targetId;
    }

    private updateExistingEdge(link: IDiagramLink) {
        const targetId: number = this.getFirstNonMergingPointShapeId(link);
        const conditionsToUpdate: ICondition[] = this.dialogModel.conditions.filter((condition: ICondition) => condition.destinationId === targetId);

        if (conditionsToUpdate.length === 0) {
            return false;
        }

        const conditionToUpdate: ICondition = conditionsToUpdate[0];
        const isMergeNodeUpdate = this.dialogModel.graph.updateMergeNode(this.dialogModel.originalDecision.model.id, conditionToUpdate);

        if (conditionToUpdate != null) {
            link.label = conditionToUpdate.label;
        }

        return isMergeNodeUpdate;
    }

    private populateDecisionChanges() {
        this.dialogModel.originalDecision.setLabelWithRedrawUi(this.dialogModel.label);

        let isMergeNodeUpdate: boolean = false;
        // update edges
        const outgoingLinks: IDiagramLink[] = this.dialogModel.originalDecision.getOutgoingLinks(this.dialogModel.graph.getMxGraphModel());

        for (let outgoingLink of outgoingLinks) {
            if (this.updateExistingEdge(outgoingLink)) {
                isMergeNodeUpdate = true;
            }
        }

        if (isMergeNodeUpdate) {
            this.dialogModel.graph.viewModel.communicationManager.processDiagramCommunication.modelUpdate(this.dialogModel.originalDecision.model.id);
            this.dialogModel.graph.viewModel.communicationManager.processDiagramCommunication.action(ProcessEvents.ArtifactUpdate);
        }
    }

    private removeDeletedBranchesFromGraph() {
        if (this.deletedConditions != null && this.deletedConditions.length > 0) {
            const targetIds: number[] = this.deletedConditions.map((condition: ICondition) => condition.destinationId);
            const decisionId = this.dialogModel.originalDecision.model.id;

            ProcessDeleteHelper.deleteDecisionBranches(decisionId, targetIds, this.dialogModel.graph);
        }
    }

    private addNewBranchesToGraph() {
        const newConditions = this.dialogModel.conditions.filter((condition: ICondition) => condition.destinationId == null);

        if (newConditions.length > 0) {
            (<ProcessGraph>this.dialogModel.graph).addDecisionBranches(this.dialogModel.originalDecision.model.id, newConditions);
        }
    }

    public isLabelAvailable(): boolean {
        return this.dialogModel.label != null && this.dialogModel.label !== "";
    }

    public getMergeNodeLabel(condition: ICondition): string {
        return condition.mergeNode ? condition.mergeNode.label : this.defaultMergeNodeLabel;
    }

    private areMergeNodesEmpty(): boolean {
        for (let i = 0; i < this.dialogModel.conditions.length; i++) {
            const condition = this.dialogModel.conditions[i];

            if (!condition.mergeNode && !this.isFirstBranch(condition)) {
                return true;
            }
        }

        return false;
    }

    public isFirstBranch(condition: ICondition): boolean {
        // Assumption: the conditions are always sorted by order index.
        return this.dialogModel.conditions[0] === condition;
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

    // This is a workaround to force re-rendering of the dialog
    public refreshView() {
        const element: HTMLElement = document.getElementsByClassName("modal-dialog")[0].parentElement;

        if (!element) {
            return;
        }

        const node = document.createTextNode(" ");
        element.appendChild(node);

        this.$timeout(
            () => {
                node.parentNode.removeChild(node);
            },
            20,
            false
        );
    }
}
