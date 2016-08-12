﻿import {IProcessLink} from "../../../../../models/processModels";
import {ICondition, IDiagramNode} from "../process-graph-interfaces";


export class Condition implements ICondition {
    constructor(
        public sourceId: number,
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