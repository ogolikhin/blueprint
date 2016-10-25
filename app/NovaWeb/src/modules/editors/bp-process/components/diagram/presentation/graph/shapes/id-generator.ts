import {ProcessShapeType} from "../../../../../models/enums";

export interface IIdGenerator {
    getId(processShapeType: ProcessShapeType): number;
}

export class IdGenerator implements IIdGenerator {
    private tempId: number = 0;

    constructor(private userTaskNewNodeCounter: number = 0,
                private systemTaskNewNodeCounter: number = 0,
                private userDecisionNewNodeCounter: number = 0,
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

    public reset() {
        this.systemDecisionNewNodeCounter = 0;
        this.systemTaskNewNodeCounter = 0;
        this.userDecisionNewNodeCounter = 0;
        this.userTaskNewNodeCounter = 0;        
    }
}
