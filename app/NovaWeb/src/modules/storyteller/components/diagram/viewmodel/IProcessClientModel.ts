module Storyteller {
    export interface IProcessClientModel {
        
        // IProcess wrapper
      
        id: number;
        name: string;
        typePrefix: string;
        projectId: number;
        baseItemTypePredefined: ItemTypePredefined;
        shapes: IProcessShape[];
        links: IProcessLinkModel[];
        propertyValues: IHashMapOfPropertyValues;
        decisionBranchDestinationLinks: IProcessLink[];
        status: IItemStatus;

        updateTree();
        updateTreeAndFlows();
        getTree(): Shell.IHashMap<TreeShapeRef>;
        getLinkIndex(sourceId: number, destinationId: number): number;
        getNextOrderIndex(id: number): number;
        getShapeById(id: number): IProcessShape;
        getShapeTypeById(id: number): ProcessShapeType;
        getShapeType(shape: IProcessShape): ProcessShapeType;
        getNextShapeIds(id: number): number[];
        getPrevShapeIds(id: number): number[];
        getStartShapeId(): number;
        getPreconditionShapeId(): number;
        getEndShapeId(): number;
        hasMultiplePrevShapesById(id: number): boolean;
        getFirstNonSystemShapeId(id: number): number;
        getDecisionBranchDestinationLinks(isMatch: (link: IProcessLink) => boolean): IProcessLink[];
        getConnectedDecisionIds(destinationId: number): number[];
        getBranchDestinationIds(decisionId: number): number[];
        getBranchDestinationId(decisionId: number, firstShapeInConditionId: number): number;
        isInSameFlow(id: number, otherId: number): boolean;
        isInChildFlow(id: number, otherId: number): boolean;

        updateDecisionDestinationId(decisionId: number, orderIndex: number, newDestinationId: number);

        isDecision(id: number): boolean;

        destroy();
    }
}