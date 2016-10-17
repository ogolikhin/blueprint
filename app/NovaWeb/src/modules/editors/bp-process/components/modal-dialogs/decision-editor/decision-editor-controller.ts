import {BaseModalDialogController, IModalScope} from "../base-modal-dialog-controller";
import {SubArtifactDecisionDialogModel} from "../models/sub-artifact-decision-dialog-model";
import {IModalProcessViewModel} from "../models/modal-process-view-model";
import {IArtifactReference, IProcessLink} from "../../../models/process-models";
import {ProcessGraph} from "../../diagram/presentation/graph/process-graph";
import {ProcessDeleteHelper} from "../../diagram/presentation/graph/process-delete-helper";
import {Condition} from "../../diagram/presentation/graph/shapes";
import {NodeType, NodeChange, IDiagramNode, IDiagramLink, ICondition, ISystemTaskShape} from "../../diagram/presentation/graph/models";
import {IProcessService} from "../../../services/process.svc";

export class DecisionEditorController extends BaseModalDialogController<SubArtifactDecisionDialogModel> implements ng.IComponentController {
    private CONDITION_MAX_LENGTH = 40;
    private TASK_LABEL_MAX_LENGTH = 32;

    private userTaskIcon: string = "fonticon fonticon-bp-actor";
    private decisionIcon: string = "fonticon fonticon-decision-diamond";
    private endIcon: string = "fonticon fonticon-storyteller-end";
    private errorIcon: string = "fonticon fonticon-error";

    private modalProcessViewModel: IModalProcessViewModel;

    private deletedConditions: ICondition[] = [];
    private isReadonly: boolean = false;

    public static $inject = [
        "$rootScope",
        "$scope",
        "$timeout",
        "$anchorScroll",
        "$location"
    ];

    constructor(
        $rootScope: ng.IRootScopeService,
        $scope: IModalScope,
        private $timeout: ng.ITimeoutService,
        private $anchorScroll: ng.IAnchorScrollService,
        private $location: ng.ILocationService
    ) {
        super($rootScope, $scope);

        this.modalProcessViewModel = <IModalProcessViewModel>this.resolve["modalProcessViewModel"];

        this.isReadonly = false;
        this.setNextNode();
    }

    public get defaultNextLabel(): string {
        return (<any>this.$rootScope).config.labels["ST_Decision_Modal_Next_Task_Label"];
    }

    public get hasMaxConditions(): boolean {
        return this.dialogModel.conditions.length >= ProcessGraph.MaxConditions;
    }

    public get hasMinConditions(): boolean {
        return this.dialogModel.conditions.length <= ProcessGraph.MinConditions;
    }

    public setNextNode() {
        this.dialogModel.nextNode = this.modalProcessViewModel.getNextNode(<ISystemTaskShape>this.dialogModel.clonedDecision.model);
    }

    private sortById(p1: IArtifactReference, p2: IArtifactReference) {
        return p1.id - p2.id;
    }

    private filterByDisplayLabel(process: IArtifactReference, viewValue: string): boolean {
        //exlude current process
        if (process.id === this.modalProcessViewModel.processViewModel.id) {
            return false;
        }

        //show all if viewValue is null/'underfined' or empty string
        if (!viewValue) {
            return true;
        }

        if ((`${process.typePrefix}${process.id}: ${process.name}`).toLowerCase().indexOf(viewValue.toLowerCase()) > -1) {
            return true;
        }
        
        return false;
    }

    public saveData() {
        this.populateDecisionChanges();
        this.addNewBranchesToGraph();
        this.removeDeletedBranchesFromGraph();
    }

    public addCondition() {
        const conditionNumber = this.dialogModel.conditions.length + 1;
        const processLink: IProcessLink = <IProcessLink>{
            sourceId: this.dialogModel.clonedDecision.model.id,
            destinationId: null,
            orderindex: null,
            label: (<any>this.$rootScope).config.labels["ST_Decision_Modal_New_System_Task_Edge_Label"] + conditionNumber
        };

        const validMergeNodes = this.dialogModel.graph.getValidMergeNodes(processLink);
        const newCondition: ICondition = Condition.create(processLink, null, validMergeNodes);
        
        this.dialogModel.conditions.push(newCondition);
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

    public deleteCondition(item: ICondition) {
        const itemToDeleteIndex = this.dialogModel.conditions.indexOf(item);

        if (itemToDeleteIndex > -1) {
            this.dialogModel.conditions.splice(itemToDeleteIndex, 1);

            if (item.destinationId != null) {
                this.deletedConditions.push(item);
            }
        }
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
        const isMergeNodeUpdate = this.updateMergeNode(conditionToUpdate);

        if (conditionToUpdate != null) {
            link.label =  conditionToUpdate.label;
        }

        return isMergeNodeUpdate;
    }

    private updateMergeNode(condition: ICondition) {
        return this.dialogModel.graph.updateMergeNode(this.dialogModel.originalDecision.model.id, condition);
    }

    private populateDecisionChanges() {
        this.dialogModel.originalDecision.setLabelWithRedrawUi(this.dialogModel.clonedDecision.label);

        let isMergeNodeUpdate: boolean = false;
        // update edges
        const outgoingLinks: IDiagramLink[] = this.dialogModel.originalDecision.getOutgoingLinks(this.dialogModel.graph.getMxGraphModel());
        
        for (let outgoingLink of outgoingLinks) {
            if (this.updateExistingEdge(outgoingLink)) {
                isMergeNodeUpdate = true ;
            }
        }

        if (isMergeNodeUpdate) {
            this.dialogModel.graph.notifyUpdateInModel(NodeChange.Update, this.dialogModel.clonedDecision.model.id);
        }
    }

    private removeDeletedBranchesFromGraph() {
        if (this.deletedConditions != null && this.deletedConditions.length > 0) {
            let targetIds: number[] = this.deletedConditions.map((condition: ICondition) => condition.destinationId);
            let decisionId = this.dialogModel.originalDecision.model.id;
            
            ProcessDeleteHelper.deleteDecisionBranches(decisionId, targetIds, this.dialogModel.graph);
        }
    }

    private addNewBranchesToGraph() {
        const newConditions = this.dialogModel.conditions.filter((condition: ICondition) => condition.destinationId == null);

        if (newConditions.length > 0) {
            let decisionId = this.dialogModel.originalDecision.model.id;
            (<ProcessGraph>this.dialogModel.graph).addDecisionBranches(decisionId, newConditions);
        }
    }

    public isLabelAvailable(): boolean {
        return this.dialogModel.clonedDecision.label != null && this.dialogModel.clonedDecision.label !== "";
    }
    
    public areMergeNodesEmpty(): boolean {
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
}