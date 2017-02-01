import {ProcessShapeType} from "../../../../../models/enums";

export interface IIdGenerator {
    getId(processShapeType: ProcessShapeType): number;
}

export class IdGenerator implements IIdGenerator {
    private tempId: number = 0;

    constructor(private userTaskNewNodeCounter: number = 1,
                private systemTaskNewNodeCounter: number = 1,
                private userDecisionNewNodeCounter: number = 0,
                private systemDecisionNewNodeCounter: number = 0,
                private userPersonaNewNodeCounter: number = -1,
                private systemPersonaNewNodeCounter: number = -2) {
    }

    public getId(processShapeType: ProcessShapeType): number {
        switch (processShapeType) {
            case ProcessShapeType.UserTask:
                return ++this.userTaskNewNodeCounter;

            case ProcessShapeType.SystemTask:
                return ++this.systemTaskNewNodeCounter;

            case ProcessShapeType.UserDecision:
                return ++this.userDecisionNewNodeCounter;

            case ProcessShapeType.SystemDecision:
                return ++this.systemDecisionNewNodeCounter;

            default:
                return ++this.tempId;
        }
    }

    public getUserPeronaId(): number {
        return this.userPersonaNewNodeCounter;
    }

    public getSystemPeronaId(): number {
        return this.systemPersonaNewNodeCounter;
    }

    public reset() {
        this.systemDecisionNewNodeCounter = 0;
        this.systemTaskNewNodeCounter = 1;
        this.systemPersonaNewNodeCounter = 0;
        this.userDecisionNewNodeCounter = 0;
        this.userTaskNewNodeCounter = 1;
        this.userPersonaNewNodeCounter = 0;
    }
}
