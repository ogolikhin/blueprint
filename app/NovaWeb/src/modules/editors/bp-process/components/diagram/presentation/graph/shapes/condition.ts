import {IProcessLink} from "../../../../../models/process-models";
import {ICondition, IDiagramNode} from "../models/";


export class Condition implements ICondition {
    constructor(public sourceId: number,
                public destinationId: number,
                public orderindex: number,
                public label: string,
                public mergeNode: IDiagramNode,
                public validMergeNodes: IDiagramNode[]) {
    }

    public static create(link: IProcessLink, mergeNode: IDiagramNode, validMergeNodes: IDiagramNode[]): ICondition {
        return new Condition(link.sourceId, link.destinationId, link.orderindex, link.label, mergeNode, validMergeNodes);

    }
}
