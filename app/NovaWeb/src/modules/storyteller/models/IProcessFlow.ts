module Storyteller {
    export interface IProcessFlow {
        parentFlow: IProcessFlow;
        orderIndex: number,
        startShapeId: number,
        endShapeId: number,
        shapes: Shell.IHashMap<IProcessShape>;
    }
}
