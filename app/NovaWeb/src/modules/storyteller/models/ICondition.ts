module Storyteller {

    // @todo: see if this can be avoided with an updated graph model from NW
    export interface ICondition extends IProcessLink {
        mergeNode: IDiagramNode;
        validMergeNodes: IDiagramNode[];
    }
}
