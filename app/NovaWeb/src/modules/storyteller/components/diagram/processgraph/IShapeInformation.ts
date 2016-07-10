module Storyteller {
    export interface IShapeInformation {
        id: number;
        parentConditions: IConditionContext[];
        innerParentCondition(): IConditionContext;
    }
}
