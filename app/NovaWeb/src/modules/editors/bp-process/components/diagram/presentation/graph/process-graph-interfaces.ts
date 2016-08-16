﻿import {IProcessShape, IProcessLink, IProcessLinkModel, IArtifactProperty} from "../../../../models/processModels";
import {ItemIndicatorFlags} from "../../../../models/enums";
import {Direction, NodeType, NodeChange, ElementType} from "./process-graph-constants";
import {IDialogParams} from "../../../messages/message-dialog";
import {IProcessViewModel} from "../../viewmodel/process-viewmodel";
import {ModalDialogType} from "../../../dialogs/modal-dialog-constants";
import {IconRackHelper} from "./shapes/icon-rack-helper";

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

export interface IIconRackListener {
    (element: IProcessShape): void;
}

export interface IIconRackSelectionListener {
    (elements: Array<IProcessShape>): void;
}

export interface ISelectionListener {
    (elements: Array<IDiagramNode>): void;
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
    hideMenu(mxGraph: MxGraph);
    showMenu(mxGraph: MxGraph);
}

export interface IUserStoryProvider {
    canGenerateUserStory(): boolean;
}

export interface IProcessGraph {
    viewModel: IProcessViewModel;
    layout: ILayout;
    startNode: IDiagramNode;
    endNode: IDiagramNode;
    iconRackHelper: IconRackHelper;
    getMxGraph(): MxGraph;
    getMxGraphModel(): MxGraphModel;
    getHtmlElement(): HTMLElement;
    getDefaultParent();
    render(useAutolayout: boolean, selectedNodeId: number);
    deleteUserTask(userTaskId: number, postDeleteFunction?: INotifyModelChanged);
    deleteDecision(decisionId: number, postDeleteFunction?: INotifyModelChanged);
    addDecisionBranches(decisionId: number, newConditions: ICondition[]);
    deleteDecisionBranches(decisionId: number, targetIds: number[]);
    updateMergeNode(decisionId: number, condition: ICondition): boolean;
    getDecisionBranchDestLinkForIndex(decisionId: number, orderIndex: number): IProcessLink;
    isUserTaskBetweenTwoUserDecisions(userTaskId: number, previousIds: number[], nextShapeId: number): boolean;
    isLastUserTaskInCondition(userTaskId: number, previousShapeIds: number[], nextShapeId: number): boolean;
    updateSourcesWithDestinations(shapeId: number, newDestinationId: number): ISourcesAndDestinations;
    getScope(id: number): IScopeContext;
    notifyUpdateInModel: INotifyModelChanged;
    getShapeById(id: number): IProcessShape;
    getValidMergeNodes(condition: IProcessLink): IDiagramNode[];
    getNodeById(id: string): IDiagramNode;
    getNextLinks(id: number): IProcessLink[];
    addLink(link: IDiagramLink, parent, index?: number, source?: MxCell, target?: MxCell);
    addSelectionListener(listener: ISelectionListener);
    updateAfterRender();
    redraw(action: any);
    saveProcess();
    publishProcess();
    updateSizeChanges(width?: number);
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
    getConditionDestination(decisionId: number): IProcessShape;
    createAutoInsertTaskMessage();
    getColumnByX(x: number): number;
    getRowByY(y: number): number;
    hideInsertNodePopupMenu();
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
    hideMenu(mxGraph: MxGraph);
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

export interface ITask extends IDiagramNode {
    persona: string;
    description: string;
    associatedArtifact: any;
    activateButton(itemFlag: ItemIndicatorFlags): void;
}

export interface IUserStoryProperties {
    nfr: IArtifactProperty;
    businessRules: IArtifactProperty;
}

export interface ISystemTask extends ITask {
    associatedImageUrl: string;
    imageId: string;
    getUserTask(graph: IProcessGraph): IUserTask;
}

export interface IUserTask extends ITask {
    objective: string;
    userStoryProperties: IUserStoryProperties;
    getNextSystemTasks(graph: IProcessGraph): ISystemTask[];
}

export interface IDecision extends IDiagramNode, IMenuContainer {
    getMergeNode(graph: IProcessGraph, orderIndex: number): IProcessShape;
    setLabelWithRedrawUi(value: string);
}

export interface IUserTaskChildElement extends IDiagramNode {
    getUserTask(graph: IProcessGraph): IUserTask;
}

export interface ISystemDecision {
    setLabelWithRedrawUi(value: string);
    updateCellLabel(value: string);
    showMenu(graph: IProcessGraph);
    hideMenu(graph: IProcessGraph);
    renderLabels();
    render(graph: IProcessGraph, x: number, y: number, justCreated: boolean): IDiagramNode;
    getElementTextLength(cell: MxCell): number;
    formatElementText(cell: MxCell, text: string): string;
    setElementText(cell: MxCell, text: string);
    getFirstSystemTask(graph: IProcessGraph): ISystemTask;
    getSystemNodes(graph: IProcessGraph): IDiagramNode[];
    openDialog(dialogType: ModalDialogType);
    getDeleteDialogParameters(): IDialogParams;
    canDelete(): boolean;
    getMergeNode(graph: IProcessGraph, orderIndex: number): IProcessShape;
}

export interface IUserTaskChildElement extends IDiagramNode {
    getUserTask(graph: IProcessGraph): IUserTask;
}
