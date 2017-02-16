import {ProcessDeleteHelper} from "../process-delete-helper";
import {IDiagramLink, IProcessGraph} from "../models/process-graph-interfaces";
import {IProcessLink} from "../../../../../models/process-models";
import {IDiagramNode} from "../models/";

export interface ICondition {
    decisionId: number;
    label: string;
    orderIndex: number;
    firstNodeId: number;
    mergeNodeId: number;
    mergeNodeLabel: string;
    validMergeNodes: IDiagramNode[];
    isCreated: boolean;
    isDeleted: boolean;

    applyChanges(graph: IProcessGraph): boolean;
}

export class Condition implements ICondition {
    public label: string;
    public orderIndex: number;
    public mergeNodeId: number;
    public isDeleted: boolean;

    constructor(
        private originalLink: IProcessLink,
        private branchEndLink: IProcessLink,
        private branchDestinationLink: IProcessLink,
        public validMergeNodes: IDiagramNode[]
    ) {
        this.label = originalLink.label;
        this.orderIndex = originalLink.orderindex;

        if (branchDestinationLink) {
            this.mergeNodeId = branchDestinationLink.destinationId;
        }
    }

    public get decisionId(): number {
        return this.originalLink.sourceId;
    }

    public get firstNodeId(): number {
        return this.originalLink.destinationId;
    }

    public get isCreated(): boolean {
        return !this.originalLink.destinationId;
    }

    public get mergeNodeLabel(): string {
        const mergeNode = _.find(this.validMergeNodes, node => node.model.id === this.mergeNodeId);
        return mergeNode ? mergeNode.label : null;
    }

    private delete(graph: IProcessGraph): boolean {
        if (this.isCreated) {
            return true;
        }

        return ProcessDeleteHelper.deleteDecisionBranch(this.originalLink, graph);
    }

    private create(graph: IProcessGraph): boolean {
        return graph.addDecisionBranch(this.decisionId, this.label, this.mergeNodeId);
    }

    private updateLabel(graph: IProcessGraph): boolean {
        // const decisionNode = graph.getNodeById(this.decisionId.toString());

        // const diagramLink = _.find(
        //     decisionNode.getOutgoingLinks(graph.getMxGraphModel()),
        //     link => _.isEqual(link.model.orderindex, this.orderIndex)
        // );

        // if (!_.isEqual(diagramLink.label, this.label)) {
        //     diagramLink.label = this.label;
        //     return true;
        // }

        if (!_.isEqual(this.originalLink.label, this.label)) {
            this.originalLink.label = this.label;
            return true;
        }

        return false;
    }

    private updateMergeNode(graph: IProcessGraph): boolean {
        if (!this.mergeNodeId || !this.branchEndLink || !this.branchDestinationLink) {
            return false;
        }

        if (!_.isEqual(this.branchDestinationLink.destinationId, this.mergeNodeId)) {
            this.branchEndLink.destinationId = this.mergeNodeId;
            this.branchDestinationLink.destinationId = this.mergeNodeId;
            return true;
        }

        return false;
    }

    private updateOrderIndex(graph: IProcessGraph): boolean {
        if (!this.originalLink || !this.branchDestinationLink) {
            return false;
        }

        if (!_.isEqual(this.originalLink.orderindex, this.orderIndex)) {
            this.originalLink.orderindex = this.orderIndex;
            this.branchDestinationLink.orderindex = this.orderIndex;
            return true;
        }

        return false;
    }

    public applyChanges(graph: IProcessGraph): boolean {
        if (this.isDeleted) {
            return this.delete(graph);
        }

        if (this.isCreated) {
            return this.create(graph);
        }

        const isLabelUpdated = this.updateLabel(graph);
        const isMergeNodeUpdated = this.updateMergeNode(graph);
        const isOrderIndexUpdated = this.updateOrderIndex(graph);

        return isLabelUpdated || isMergeNodeUpdated || isOrderIndexUpdated;
    }
}
