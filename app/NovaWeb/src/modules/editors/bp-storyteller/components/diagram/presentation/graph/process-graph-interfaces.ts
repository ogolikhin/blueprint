
import {IProcessShape, IProcessLink} from "../../../../models/processModels";
import {Direction, NodeType, NodeChange, ElementType} from "./process-graph-constants";
import {IDialogParams} from "../../../message/message-dialog";

export interface IDeletable {
    canDelete(): boolean;
}

export interface IIconRackListener {
    (element: IProcessShape): void;
}

export interface ISelectionListener {
    (elements: Array<IDiagramNode>): void;
}

export interface INotifyModelChanged {
    (nodeChange: NodeChange, selectedId: number): void
}

export interface ILinkFilter {
    (value: IProcessLink, index: number, array: IProcessLink[]): boolean;
}

export interface IConditionContext {
    decisionId: number;
    orderindex: number;
    endId: number;
    targetId: number;
    shapeIdsInCondition: IShapesInScopeMap;
}

export interface IScopeContext {
    id: number;
    previousId: number;
    mergeIds: number[];
    visitedIds: IShapesInScopeMap;
    mappings: IConditionContext[];
    currentMappings: IConditionContext[];
}

export interface IStopTraversalCondition {
    (context: IScopeContext): boolean;
}

export interface INextIdsProvider {
    (context: IScopeContext): number[];
}

export interface IShapesInScopeMap {
    [id: number]: IShapeInformation;
}

export interface IShapeInformation {
    id: number;
    parentConditions: IConditionContext[];
    innerParentCondition(): IConditionContext;
}

export interface ICondition extends IProcessLink {
    mergeNode: IDiagramNode;
    validMergeNodes: IDiagramNode[];
}

export interface IMenuContainer {
    hideMenu(graph: IProcessGraph);
    showMenu(graph: IProcessGraph);
}

export interface IUserStoryProvider {
    canGenerateUserStory(): boolean;
}

export interface IProcessGraph {
    graph: MxGraph;
    layout: ILayout;

    deleteUserTask(userTaskId: number, postDeleteFunction?: INotifyModelChanged);
    deleteDecision(decisionId: number, postDeleteFunction?: INotifyModelChanged);
    addDecisionBranches(decisionId: number, newConditions: ICondition[]);
    deleteDecisionBranches(decisionId: number, targetIds: number[]);
    updateMergeNode(decisionId: number, condition: ICondition): boolean;
    notifyUpdateInModel: INotifyModelChanged;
    getValidMergeNodes(condition: IProcessLink): IDiagramNode[];
    getNodeById(id: string): IDiagramNode;
    getNextLinks(id: number): IProcessLink[];
    redraw(action: any);
    saveProcess();
    publishProcess();
    destroy();
}

export interface ILayout {
    render(useAutolayout: boolean, selectedNodeId: number);
    scrollShapeToView(shapeId: string);
    getDropEdgeState(mouseCoordinates: MxPoint): any;
    getXbyColumn(col: number): number;
    getYbyRow(row: number): number;
    getNodeById(id: string): IDiagramNode;
    updateBranchDestinationId(oldDestinationId: number, newDestinationId: number);
    updateProcessChangedState(id: number, change: NodeChange, redraw: boolean);
    insertTask(sourceIds: number[], destinationId: number): number;
    insertTaskWithUpdate(edge: MxCell);
    insertSystemDecision(connector: IDiagramLink);
    insertSystemDecisionCondition(decisionId: number, label?: string, conditionDestinationId?: number): number;
    insertSystemDecisionConditionWithUpdate(decisionId: number, label?: string, conditionDestinationId?: number);
    insertUserDecision(edge: MxCell);
    insertUserDecisionCondition(decisionId: number, label?: string, conditionDestinationId?: number): number;
    insertUserDecisionConditionWithUpdate(decisionId: number, label?: string, conditionDestinationId?: number);
    handleUserTaskDragDrop(userTaskShapeId: number, edge: MxCell);
    isValidForDrop(userTaskShapeId: number, edge: MxCell): boolean;
    getColumnByX(x: number): number;
    getRowByY(y: number): number;
    hideInsertNodePopupMenu();
}

export interface IDiagramLink extends IDiagramElement, IMenuContainer {
    renderLabel();
    initializeLabel(graph: IProcessGraph, sourceNode: IDiagramNode, targetNode: IDiagramNode);
    label: string;
    sourceNode: IDiagramNode;
    targetNode: IDiagramNode;
    hideMenu(graph: IProcessGraph);
    showMenu(graph: IProcessGraph);
    getParentId(): number;
}

export interface IDiagramNode extends IDiagramNodeElement, MxCell, IDeletable, IUserStoryProvider {
    model: IProcessShape;
    direction: Direction;
    action: string;
    label: string;
    row: number;
    column: number;
    newShapeColor: string;

    getId(): string;
    setId(value: string);
    getNodeType(): NodeType;
    getConnectableElement(): IDiagramNodeElement;
    // returns array of incoming diagram links ordered by asc. order index
    getIncomingLinks(graph: IProcessGraph): IDiagramLink[];
    // returns array of outgoing diagram links ordered by asc. order index
    getOutgoingLinks(graph: IProcessGraph): IDiagramLink[];
    // returns array of connected sources
    getSources(graph: IProcessGraph): IDiagramNode[];
    // return array of connected targets
    getTargets(graph: IProcessGraph): IDiagramNode[];
    render(graph: IProcessGraph, x: number, y: number, justCreated: boolean): MxCell;
    renderLabels(): void;
    addNode(graph: IProcessGraph): IDiagramNode;
    deleteNode(graph: IProcessGraph);
    // gets immediate successor nodes
    getNextNodes(): IDiagramNode[];
    // gets immediate precursor nodes
    getPreviousNodes(): IDiagramNode[];
    notify(change: NodeChange);

    getDeleteDialogParameters(): IDialogParams;

    getLabelCell(): MxCell;
}

export interface IDiagramElement extends MxCell {
    getElementType(): ElementType;
    isHtmlElement(): boolean;
    getX(): number;
    getY(): number;
    getHeight(): number;
    getWidth(): number;
    getCenter(): MxPoint;
    setElementText(cell: MxCell, text: string);
    formatElementText(cell: MxCell, text: string): string;
    getElementTextLength(cell: MxCell): number;
}

export interface IDiagramNodeElement extends IDiagramElement {
    getNode(): IDiagramNode;
}