module Storyteller {
    export interface IDecision extends IDiagramNode, IMenuContainer {
        getMergeNode(graph: ProcessGraph, orderIndex: number): IProcessShape;
        setLabelWithRedrawUi(value: string);
    }
}