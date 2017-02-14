import {IDecision} from "../../diagram/presentation/graph/models/process-graph-interfaces";
import {BaseModalDialogController, IModalScope} from "../base-modal-dialog-controller";
import {DecisionEditorModel} from "./decisionEditor.model";
import {IProcessLink} from "../../../models/process-models";
import {ProcessGraph} from "../../diagram/presentation/graph/process-graph";
import {ProcessDeleteHelper} from "../../diagram/presentation/graph/process-delete-helper";
import {Condition} from "../../diagram/presentation/graph/shapes";
import {NodeType, IDiagramNode, IDiagramLink, ICondition} from "../../diagram/presentation/graph/models";
import {ProcessEvents} from "../../diagram/process-diagram-communication";
import {ILocalizationService} from "../../../../../commonModule/localization/localization.service";

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
        private $timeout: ng.ITimeoutService,
        private $anchorScroll: ng.IAnchorScrollService,
        private $location: ng.ILocationService,
        private $q: ng.IQService,
        private localization: ILocalizationService,
        $uibModalInstance?: ng.ui.bootstrap.IModalServiceInstance,
        dialogModel?: DecisionEditorModel
    ) {
        super($rootScope, $scope, $uibModalInstance, dialogModel);
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

    public saveData(): ng.IPromise<void> {
        this.populateDecisionChanges();
        this.addNewBranchesToGraph();
        this.removeDeletedBranchesFromGraph();
        return this.$q.resolve();
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
            label: `${this.dialogModel.conditionLabel} ${conditionNumber}`
        };

        const validMergeNodes = this.dialogModel.graph.getValidMergeNodes(processLink);
        const defaultMergeNode = _.find(validMergeNodes, node => node.model.id === this.dialogModel.defaultDestinationId);
        const newCondition: ICondition = Condition.create(processLink, defaultMergeNode, validMergeNodes);

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
        return !this.hasMinConditions && !this.dialogModel.graph.isFirstFlow(condition);
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

    private getFirstNonMergingPointShapeId(link: IProcessLink) {
        let targetId: number;
        const linkDestinationNode = this.dialogModel.graph.getNodeById(link.destinationId.toString());
        if (linkDestinationNode.getNodeType() === NodeType.MergingPoint) {
            const outgoingLinks = linkDestinationNode.getOutgoingLinks(this.dialogModel.graph.getMxGraphModel());
            targetId = outgoingLinks[0].model.destinationId;
        } else {
            targetId = linkDestinationNode.model.id;
        }

        return targetId;
    }

    private updateConditionLabels(condition: ICondition): boolean {
        if (condition != null) {
            let diagramLink: IDiagramLink;

            const decisionNode = this.dialogModel.graph.getNodeById(condition.sourceId.toString());
            diagramLink = _.find(decisionNode.getOutgoingLinks(this.dialogModel.graph.getMxGraphModel()),
                (link: IDiagramLink) => link.model.orderindex === condition.orderindex);

            const result = !_.isEqual(diagramLink.label, condition.label);
            if (result) {
                diagramLink.label = condition.label;
            }
            return result;
        }
        return false;
    }

    private populateDecisionChanges() {
        this.dialogModel.originalDecision.setLabelWithRedrawUi(this.dialogModel.label);

        let isModelUpdate: boolean = false;
        // update edges
        const decisionsToUpdate: number[] = [];
        _.each(this.dialogModel.conditions, (condition: ICondition, index: number) => {
            if (!condition.mergeNode) {
                return;
            }
            let link: IProcessLink = this.dialogModel.graph.getBranchStartingLink(condition);

            if (link) {
                const didUpdateEdge = this.dialogModel.graph.updateMergeNode(link.sourceId, link, condition.mergeNode.model.id);
                const didUpdateLabel = this.updateConditionLabels(condition);

                if (!isModelUpdate) {
                    isModelUpdate = didUpdateLabel || didUpdateEdge;
                }
            }
        });

        if (isModelUpdate) {
            // Calls model update to redraw components of the decision to show changes.
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
    }

    public canMoveDown(condition: ICondition): boolean {
        if (this.isReadonly) {
            return false;
        }

        if (!this.dialogModel.conditions || this.dialogModel.conditions.length === 0) {
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

            if (!condition.mergeNode && !this.isFirstConditionOnMainFlow(condition)) {
                return true;
            }
        }

        return false;
    }

    public isFirstConditionOnMainFlow(condition: ICondition): boolean {
        return  this.dialogModel.graph.isFirstFlow(condition)
                &&  this.dialogModel.graph.isInMainFlow(condition.sourceId);
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

    // This is a workaround to force re-rendering of the dialog
    public refreshView() {
        const element: HTMLElement = document.getElementsByClassName("modal-dialog").item(0).parentElement;

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

    public get deleteConditionLabel(): string {
        return `${this.localization.get("App_Button_Delete")} ${this.dialogModel.conditionLabel}`;
    }

    public get addConditionLabel(): string {
        return `${this.localization.get("App_Button_Add")} ${this.dialogModel.conditionLabel}`;
    }
}