import {IScopeContext, IShapeInformation} from "./models/process-graph-interfaces";

export interface IScopeHelper {
    isInNestedFlow(id: number): boolean;
    isInMainFlow(id: number): boolean;
}
export class ScopeHelper implements IScopeHelper {
    private scope: IScopeContext;

    constructor(_scope: IScopeContext) {
        this.scope = _scope;
    }

    public isInNestedFlow(id: number): boolean {
        const shapeContext = this.scope.visitedIds[id];
        return !!shapeContext ? this.hasParentConditions(shapeContext) : false;
    }

    public isInMainFlow(id: number): boolean {
        const shapeContext = this.scope.visitedIds[id];
        return  !!shapeContext ? !this.hasParentConditions(shapeContext) : false;
    }

    private hasParentConditions(shapeContext: IShapeInformation): boolean {
        return shapeContext.parentConditions.length !== 0;
    }
}
