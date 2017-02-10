import {IProcessShape, IProcessLink, IProcessLinkModel} from "./";
import {IArtifactProperty} from "./";
import {Direction, NodeType, NodeChange, ElementType} from "./";
import {IDialogParams} from "../../../../messages/message-dialog";
import {IProcessViewModel} from "../../../viewmodel/process-viewmodel";
import {SourcesAndDestinations, IUserStory, IArtifactReference} from "../../../../../models/process-models";
import {IProcessDiagramCommunication} from "../../../process-diagram-communication";
import {IBridgesHandler}  from "../bridges-handler";
import {IMessageService} from "../../../../../../../main/components/messages/message.svc";

export interface IDeletable {
    canDelete(): boolean;
}

export interface ILabel {
    render(): void;
    text: string;
    setVisible(value: boolean);
    onDispose(): void;
}
export interface IMouseEventHandler {
    onMouseEnter(sender, evt);
    onMouseLeave(sender, evt);
    onMouseDown(sender, evt);
    onMouseUp(sender, evt);
}

export interface IOverlayHandler {
    updateOverlay(graph: MxGraph);
}

export interface ISelectionListener {
    (elements: IDiagramNode[]): void;
}

export interface INotifyModelChanged {
    (nodeChange: NodeChange, selectedId: number): void;
}

export interface ILinkFilter {
    (value: IProcessLink, index: number, array: IProcessLink[]): boolean;
}

export interface ICondition extends IProcessLink {
    mergeNode: IDiagramNode;
    validMergeNodes: IDiagramNode[];
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

export interface IMenuContainer {
    showMenu(mxGraph: MxGraph);
}

export interface IUserStoryProvider {
    canGenerateUserStory(): boolean;
}

export interface IProcessGraph {
    rootScope: any;
    messageService: IMessageService;
    processDiagramCommunication: IProcessDiagramCommunication;
    viewModel: IProcessViewModel;
    layout: ILayout;
    startNode: IDiagramNode;
    endNode: IDiagramNode;
    isUserSystemProcess: boolean;
    globalScope: IScopeContext;
    defaultNextIdsProvider: INextIdsProvider;
    notifyUpdateInModel: INotifyModelChanged;
    systemTaskErrorPresented?: number;
    getMxGraph(): MxGraph;
    getMxGraphModel(): MxGraphModel;
    getHtmlElement(): HTMLElement;
    getDefaultParent(): MxCell;
    render(useAutolayout: boolean, selectedNodeId: number): void;
    updateMergeNode(decisionId: number, condition: ICondition): boolean;
    getDecisionBranchDestLinkForIndex(decisionId: number, orderIndex: number): IProcessLink;
    updateSourcesWithDestinations(shapeId: number, newDestinationId: number): ISourcesAndDestinations;
    getBranchScope(initialBranchLink: IProcessLink, nextIdsProvider: INextIdsProvider): IScopeContext;
    getLink(sourceId: number, destinationId: number): IProcessLink;
    getScope(id: number): IScopeContext;
    getShapeById(id: number): IProcessShape;
    getMergeNode(decisionId: number, orderIndex: number): IDiagramNode;
    getValidMergeNodes(condition: IProcessLink): IDiagramNode[];
    getNodeById(id: string): IDiagramNode;
    getNextLinks(id: number): IProcessLink[];
    addLink(link: IDiagramLink, parent, index?: number, source?: MxCell, target?: MxCell): MxCell;
    updateAfterRender(): void;
    redraw(action: any): void;
    updateSizeChanges(width?: number, height?: number): void;
    setSystemTasksVisible(value: boolean): void;
    clearSelection(): void;
    onUserStoriesGenerated(userStories: IUserStory[]): void;
    onValidation(invalidShapes: number[]): void;
    copySelectedShapes(): void;
    insertSelectedShapes(edge: MxCell): void;
    getSelectedNodes(): IDiagramNode[];
    getHighlightedCopyNodes(): IDiagramNode[];
    getCopyNodes(): IDiagramNode[];
    highlightNodeEdges(nodes: IDiagramNode[]): void;
    clearCopyGroupHighlight(): void;
    highlightCopyGroups(nodes: IDiagramNode[]): void;
    clearHighlightEdges(): void;
    highlightBridges(): void;
    destroy(): void;
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
    handleUserTaskDragDrop(userTaskShapeId: number, edge: MxCell);
    isValidForDrop(userTaskShapeId: number, edge: MxCell): boolean;
    getConditionDestination(decisionId: number): IProcessShape;
    createAutoInsertTaskMessage();
    getColumnByX(x: number): number;
    getRowByY(y: number): number;
    viewModel: IProcessViewModel;
    getSourcesAndDestinations(edge: MxCell): SourcesAndDestinations;
    updateLink(sourceId: number, oldDestinationId: number, newDestinationId: number);
    getDefaultBranchLabel(decisionId: number, nodeType: NodeType): string;
    getTempShapeId(): number;
    setTempShapeId(id: number);
    bridgesHandler: IBridgesHandler;
}

export interface ISourcesAndDestinations {
    sourceIds: number[];
    destinationIds: number[];
}

export interface IGraphLayoutPreprocessor {
    setCoordinates();
}

export interface IDiagramLink extends IDiagramElement, IMenuContainer {
    model: IProcessLinkModel;
    renderLabel();
    initializeLabel(graph: IProcessGraph, sourceNode: IDiagramNode, targetNode: IDiagramNode);
    label: string;
    sourceNode: IDiagramNode;
    targetNode: IDiagramNode;
    showMenu(mxGraph: MxGraph);
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
    canCopy: boolean;
    isValid?: boolean;

    getId(): string;
    setId(value: string);
    getNodeType(): NodeType;
    getConnectableElement(): IDiagramNodeElement;
    // returns array of incoming diagram links ordered by asc. order index
    getIncomingLinks(graphModel: MxGraphModel): IDiagramLink[];
    // returns array of outgoing diagram links ordered by asc. order index
    getOutgoingLinks(graphModel: MxGraphModel): IDiagramLink[];
    // returns array of connected sources
    getSources(graphModel: MxGraphModel): IDiagramNode[];
    // return array of connected targets
    getTargets(graphModel: MxGraphModel): IDiagramNode[];
    render(graph: IProcessGraph, x: number, y: number, justCreated: boolean): MxCell;
    renderLabels(): void;
    // gets immediate successor nodes
    getNextNodes(): IDiagramNode[];
    // gets immediate precursor nodes
    getPreviousNodes(): IDiagramNode[];
    getDeleteDialogParameters(): IDialogParams;
    getLabelCell(): MxCell;
    highlight(mxGraph: MxGraph, color?: string): void;
    clearHighlight(mxGraph: MxGraph): void;
    setEditMode(): void;
}

export interface IDiagramElement extends MxCell {
    getElementType(): ElementType;
    isHtmlElement(): boolean;
    getX(): number;
    getY(): number;
    getHeight(): number;
    getWidth(): number;
    getCenter(): MxPoint;
}

export interface IDiagramNodeElement extends IDiagramElement {
    getNode(): IDiagramNode;
}

export interface ITask extends IDiagramNode {
    description: string;
    associatedArtifact: IArtifactReference;
    personaReference: IArtifactReference;
}

export interface IUserStoryProperties {
    nfr: IArtifactProperty;
    businessRules: IArtifactProperty;
}

export interface ISystemTask extends ITask {
    associatedImageUrl: string;
    imageId: string;
}

export interface IUserTask extends ITask {
    objective: string;
    userStoryId: number;
    getNextSystemTasks(graph: IProcessGraph): ISystemTask[];
}

export interface IDecision extends IDiagramNode, IMenuContainer {
    setLabelWithRedrawUi(value: string);
}

