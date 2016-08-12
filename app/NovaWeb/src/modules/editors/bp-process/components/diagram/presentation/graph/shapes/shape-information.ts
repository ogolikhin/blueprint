import {IShapeInformation, IConditionContext} from "../process-graph-interfaces";

export class ShapeInformation implements IShapeInformation {
    constructor(public id, public parentConditions: IConditionContext[]) {

    }
    public innerParentCondition(): IConditionContext {
        if (this.parentConditions.length > 0) {
            return this.parentConditions[this.parentConditions.length - 1];
        }
        return null;
    }
}