module Storyteller {

    export interface IIdGeneratorService {
        getId(processShapeType: ProcessShapeType): number;
    }

    export class IdGeneratorService implements IIdGeneratorService {
        private tempId: number = 0;        
        
        constructor(private userTaskNewNodeCounter: number = 0,
            private systemTaskNewNodeCounter: number = 0,
            private userDecisionNewNodeCounter : number = 0,
            private systemDecisionNewNodeCounter: number = 0) {
        }

        public getId(processShapeType: ProcessShapeType): number {
            if (processShapeType === ProcessShapeType.UserTask) {
                return ++this.userTaskNewNodeCounter;
            }
            if (processShapeType === ProcessShapeType.SystemTask) {
                return ++this.systemTaskNewNodeCounter;
            }
            if (processShapeType === ProcessShapeType.UserDecision) {
                return ++this.userDecisionNewNodeCounter;
            }
            if (processShapeType === ProcessShapeType.SystemDecision) {
                return ++this.systemDecisionNewNodeCounter;
            }
            return ++this.tempId;

        }
    }
}