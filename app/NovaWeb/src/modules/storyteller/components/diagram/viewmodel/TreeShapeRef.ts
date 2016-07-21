module Storyteller {
    export class TreeShapeRef {
        public index: number;
        public flow: IProcessFlow;
        public prevShapeIds: number[] = [];
        public nextShapeIds: number[] = [];
    }
}
