import {IDiagramNode, IProcessGraph, IProcessLink} from "../../diagram/presentation/graph/models";
import {ProcessDeleteHelper} from "../../diagram/presentation/graph/process-delete-helper";

export interface ICondition {
    decisionId: number;
    label: string;
    orderIndex: number;
    firstNodeId: number;
    mergeNodeId: number;
    mergeNodeLabel: string;
    validMergeNodes: IDiagramNode[];
    isChanged: boolean;
    isCreated: boolean;
    isDeleted: boolean;
    isLabelChanged: boolean;
    isOrderIndexChanged: boolean;
    isMergeNodeChanged: boolean;

    delete(): void;
    applyChanges(graph: IProcessGraph): void;
}

export class Condition implements ICondition {
    public label: string;
    public orderIndex: number;
    public mergeNodeId: number;
    private _isCreated: boolean;
    private _isDeleted: boolean;

    constructor(
        private originalLink: IProcessLink,
        private branchEndLink: IProcessLink,
        private branchDestinationLink: IProcessLink,
        public validMergeNodes: IDiagramNode[]
    ) {
        this.label = originalLink.label;
        this.orderIndex = originalLink.orderindex;
        this._isCreated = !originalLink.destinationId;
        this._isDeleted = false;

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

    public get isChanged(): boolean {
        return this.isLabelChanged
            || this.isOrderIndexChanged
            || this.isMergeNodeChanged;
    }

    public get isCreated(): boolean {
        return this._isCreated;
    }

    public get isDeleted(): boolean {
        return this._isDeleted;
    }

    public get isLabelChanged(): boolean {
        return this.originalLink
            && !_.isEqual(this.originalLink.label, this.label);
    }

    public get isOrderIndexChanged(): boolean {
        return this.originalLink
            && !_.isEqual(this.originalLink.orderindex, this.orderIndex);
    }

    public get isMergeNodeChanged(): boolean {
        return this.branchDestinationLink
            && this.mergeNodeId
            && !_.isEqual(this.branchDestinationLink.destinationId, this.mergeNodeId);
    }

    public get mergeNodeLabel(): string {
        const mergeNode = _.find(this.validMergeNodes, node => node.model.id === this.mergeNodeId);
        return mergeNode ? mergeNode.label : undefined;
    }

    public delete(): void {
        this._isDeleted = true;
    }

    private applyDelete(graph: IProcessGraph): boolean {
        if (this.isCreated) {
            return true;
        }

        return ProcessDeleteHelper.deleteDecisionBranch(this.originalLink, graph);
    }

    private applyCreate(graph: IProcessGraph): boolean {
        return graph.addDecisionBranch(this.decisionId, this.label, this.mergeNodeId);
    }

    private updateLabel(graph: IProcessGraph): boolean {
        if (!this.isLabelChanged) {
            return false;
        }

        this.originalLink.label = this.label;
        return true;
    }

    private updateMergeNode(graph: IProcessGraph): boolean {
        if (!this.isMergeNodeChanged) {
            return false;
        }

        this.branchEndLink.destinationId = this.mergeNodeId;
        this.branchDestinationLink.destinationId = this.mergeNodeId;
        return true;
    }

    private updateOrderIndex(graph: IProcessGraph): boolean {
        if (!this.originalLink || !this.branchDestinationLink) {
            return false;
        }

        if (!this.isOrderIndexChanged) {
            return false;
        }

        this.originalLink.orderindex = this.orderIndex;
        this.branchDestinationLink.orderindex = this.orderIndex;
        return true;
    }

    public applyChanges(graph: IProcessGraph): void {
        if (this.isDeleted) {
            this.applyDelete(graph);
            return;
        }

        if (this.isCreated) {
            this.applyCreate(graph);
            return;
        }

        if (this.isChanged) {
            this.updateLabel(graph);
            this.updateMergeNode(graph);
            this.updateOrderIndex(graph);
        }
    }
}
