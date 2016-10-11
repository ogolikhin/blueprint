import {IMessageService} from "../../../../../../core/";
import {IProcessShape, IProcessLink} from "../../../../models/process-models";
import {IProcessLinkModel, ProcessLinkModel} from "../../../../models/process-models";
import {NewUserTaskInfo, SourcesAndDestinations, EdgeGeo} from "../../../../models/process-models";
import {ProcessType, ProcessShapeType} from "../../../../models/enums";
import {NodeType, NodeChange} from "./models/";
import {GRAPH_LEFT, GRAPH_TOP, GRAPH_COLUMN_WIDTH, GRAPH_ROW_HEIGHT} from "./models/";
import {IProcessGraph, IDiagramNode} from "./models/";
import {IDiagramNodeElement} from "./models/";
import {ILayout, IGraphLayoutPreprocessor, IScopeContext} from "./models/";
import {IProcessViewModel} from "../../viewmodel/process-viewmodel";
import {GraphLayoutPreprocessor} from "./graph-layout-preprocessor";
import {ShapesFactory} from "./shapes/shapes-factory";
import {ProcessCellRenderer} from "./process-cell-renderer";
import {ProcessValidator} from "./process-graph-validator";
import {NodeFactory} from "./shapes/node-factory";
import {NodeFactorySettings} from "./shapes/node-factory-settings";
import {SystemTask} from "./shapes/system-task";
import {SystemDecision} from "./shapes/system-decision";
import {MergingPoint} from "./shapes/merging-point";
import {DiagramLink} from "./shapes/diagram-link";
import {Connector} from "./shapes/connector";
import {ProcessAddHelper} from "./process-add-helper";
import {ProcessDeleteHelper} from "./process-delete-helper";

export let tempShapeId: number = 0;

export class Layout implements ILayout {
    private mxgraph: MxGraph = null;
    private tempId = 0;
    private preprocessor: IGraphLayoutPreprocessor = null;
    private edgesGeo: EdgeGeo[] = [];
    private DRAG_PREVIEW_TO_EDGE_DISTANCE = 50;

    constructor(private processGraph: IProcessGraph,
                public viewModel: IProcessViewModel,
                private rootScope: any,
                private shapesFactoryService: ShapesFactory,
                private messageService: IMessageService,
                private $log: ng.ILogService) {

        // cache reference to mxGraph
        this.mxgraph = processGraph.getMxGraph();

        this.mxgraph.cellRenderer = new ProcessCellRenderer();

        this.tempId = 0;

    }

    public render(useAutolayout: boolean, selectedNodeId: number) {
        const graphModel: MxGraphModel = this.mxgraph.getModel();
        let i, j;
        let link: IProcessLinkModel;
        const linksMap = [];
        const processValidator = new ProcessValidator();

        this.viewModel.updateTreeAndFlows();

        if (useAutolayout) {
            this.preprocessor = new GraphLayoutPreprocessor(this.viewModel);
            this.preprocessor.setCoordinates();
        }

        let validationErrors: string[] = [];
        if (!processValidator.isValid(this.viewModel, this.rootScope, validationErrors)) {
            if (this.messageService) {
                for (let error of validationErrors) {
                    this.messageService.addError(error);
                }
            }

        }

        // collect links with same destinations
        for (i in this.viewModel.links) {
            link = this.viewModel.links[i];
            if (linksMap[link.destinationId.toString()] == null) {
                linksMap[link.destinationId.toString()] = [];
            }

            link.parentId = this.viewModel.id;
            linksMap[link.destinationId.toString()].push(link);
        }

        const nodeFactorySettings = new NodeFactorySettings();
        nodeFactorySettings.isCommentsButtonEnabled = !this.viewModel.isHistorical && !this.viewModel.isSMB;
        nodeFactorySettings.isRelationshipButtonEnabled = !this.viewModel.isHistorical && !this.viewModel.isSMB;
        nodeFactorySettings.isDetailsButtonEnabled = this.viewModel.isSpa;
        nodeFactorySettings.isLinkButtonEnabled = this.viewModel.isSpa && !this.viewModel.isHistorical;
        nodeFactorySettings.isMockupButtonEnabled = this.viewModel.isSpa;
        nodeFactorySettings.isPreviewButtonEnabled = this.viewModel.isSpa && !this.viewModel.isHistorical;

        graphModel.beginUpdate();

        try {
            for (i in this.viewModel.shapes) {
                    const shape: IProcessShape = this.viewModel.shapes[i];
                    const node: IDiagramNode = NodeFactory.createNode(shape, this.rootScope, this.shapesFactoryService, nodeFactorySettings);

                    if (node != null) {
                        node.render(this.processGraph, this.getXbyColumn(node.column), this.getYbyRow(node.row),
                            this.viewModel.isShapeJustCreated(shape.id));

                        // Hide system tasks for process types different than 'Business Process'
                        if (this.viewModel.propertyValues["clientType"].value !== ProcessType.UserToSystemProcess) {
                            if (node.getNodeType() === NodeType.SystemTask) {
                                (<SystemTask>node).setCellVisible(this.mxgraph, false);
                            }
                            if (node.getNodeType() === NodeType.SystemDecision) {
                                (<SystemDecision>node).hideMenu(this.mxgraph);
                            }
                        }
                    }
            }

            for (i in linksMap) {
                const linkArray: Array<IProcessLinkModel> = linksMap[i];
                if (linkArray.length === 1) {
                    this.addConnector(graphModel, linkArray[0]);
                }
                else {
                    const destinationNode: IDiagramNode = graphModel.getCell(linkArray[0].destinationId.toString());

                    // Add merging point for links with the same destination
                    const mergingPointShape = this.shapesFactoryService.createModelMergeNodeShape(this.viewModel.id,
                        this.viewModel.projectId, --tempShapeId, destinationNode.column - 1, destinationNode.row);

                    const mergingPoint = new MergingPoint(mergingPointShape);
                    mergingPoint.render(this.processGraph, this.getXbyColumn(mergingPoint.column), this.getYbyRow(mergingPoint.row), false);

                    // Add connectors to the merging point
                    for (j in linkArray) {
                        this.addConnector(graphModel, linkArray[j], linkArray[j].sourceId, mergingPointShape.id);
                    }

                    // Add connector from merging point to the node that had multiple connections
                    link = new ProcessLinkModel();
                    link.destinationId = linkArray[0].destinationId;
                    link.sourceId = mergingPointShape.id;
                    link.orderindex = mergingPoint.model.propertyValues["y"].value;
                    link.label = "";
                    this.addConnector(graphModel, link);
                }
            }

        } catch (e) {
            this.logError(e);
        }
        finally {

            graphModel.endUpdate();

            this.viewModel.communicationManager.modalDialogManager.setGraph(this.getGraph);

            for (i in this.viewModel.shapes) {
                const thisShape: IProcessShape = this.viewModel.shapes[i];
                const thisNode = this.processGraph.getNodeById(thisShape.id.toString());
                thisNode.renderLabels();
            }

            let edges: DiagramLink[] = this.mxgraph.getChildEdges(this.mxgraph.getDefaultParent());
            for (let thisEdge of edges) {
                thisEdge.renderLabel();
            }

            if (useAutolayout) {
                if (selectedNodeId != null) {
                    this.postRender(selectedNodeId);
                }
            }

            for (let edgeGeo of this.edgesGeo) {
                if (edgeGeo) {
                    edgeGeo.state = this.getEdgeCellState(edgeGeo.edge);
                }
            }

            // Set vertices z-order on top in case some of them are overlaped by edges
            this.mxgraph.orderCells(false, this.mxgraph.getChildVertices(this.mxgraph.getDefaultParent()));

            this.viewModel.resetJustCreatedShapeIds();

            setTimeout(() => this.processGraph.updateAfterRender(), 100);

        }

        if (selectedNodeId) {
            this.scrollShapeToView(selectedNodeId.toString());
        }
    }

    public getGraph = () => {
        return this.processGraph;
    }

    public getTempShapeId(): number {
        return tempShapeId;
    }

    public setTempShapeId(id: number) {
        tempShapeId = id;
    }

    public scrollShapeToView(shapeId: string) {
        const node = this.getNodeById(shapeId);
        if (node) {
            (<any>this.mxgraph).scrollCellToVisible(node, true);
        }
    }

    public getDropEdgeState(mouseCoordinates: MxPoint): any {
        const view: MxGraphView = this.mxgraph.getView();
        let edge: MxCell = null;
        let state = null;
        for (let edgeGeo of this.edgesGeo) {
            if (edgeGeo) {
                for (let i = 1; i < edgeGeo.state.absolutePoints.length; i++) {

                    const p1 = edgeGeo.state.absolutePoints[i - 1];
                    const p2 = edgeGeo.state.absolutePoints[i];

                    if (p1.y === p2.y) {
                        // horizontal
                        const  fromX = Math.min(p1.x, p2.x);
                        const toX = Math.max(p1.x, p2.x);
                        if ((mouseCoordinates.x > fromX) && (mouseCoordinates.x < toX) &&
                            Math.abs(mouseCoordinates.y - p1.y) < this.DRAG_PREVIEW_TO_EDGE_DISTANCE) {
                            edge = edgeGeo.edge;
                            break;
                        }
                    } else {
                        // vertical
                        const  fromY = Math.min(p1.y, p2.y);
                        const  toY = Math.max(p1.y, p2.y);
                        if ((mouseCoordinates.y > fromY) && (mouseCoordinates.y < toY) &&
                            Math.abs(mouseCoordinates.x - p1.x) < this.DRAG_PREVIEW_TO_EDGE_DISTANCE) {
                            edge = edgeGeo.edge;
                            break;
                        }
                    }
                }
            }
        }
        if (edge) {
            state = view.getState(edge);
        }
        return state;
    }

    public getXbyColumn(col: number): number {
        return col * GRAPH_COLUMN_WIDTH + GRAPH_LEFT;
    }

    public getYbyRow(row: number): number {
        return row * GRAPH_ROW_HEIGHT + GRAPH_TOP;
    }

    public getNodeById(id: string): IDiagramNode {
        return this.processGraph.getNodeById(id);
    }

    public updateBranchDestinationId(oldDestinationId: number, newDestinationId: number) {
        this.viewModel.getDecisionBranchDestinationLinks((link: IProcessLink) => {
            return link.destinationId === oldDestinationId;
        }).forEach((link: IProcessLink) => {
            link.destinationId = newDestinationId;
        });
    }

    public updateProcessChangedState(id: number, change: NodeChange = NodeChange.Add, redraw: boolean = false) {
        const eventArguments = {
            processId: this.viewModel.id,
            nodeChanges: [
                {
                    nodeId: id,
                    change: change,
                    redraw: redraw
                }
            ]
        };
        const evt = document.createEvent("CustomEvent");
        evt.initCustomEvent("graphUpdated", true, true, eventArguments);
        window.dispatchEvent(evt);
    }


    public getConditionDestination(decisionId: number): IProcessShape {
        if (this.viewModel.isDecision(decisionId)) {
            let conditionShapeIds: number[] = this.viewModel.getNextShapeIds(decisionId);
            let destinationId = conditionShapeIds[0];
            return this.getDecisionConditionDestination(destinationId);
        }

        return null;
    }

    // #UNUSED
    //private isConditionDestinationInScope(decisionId: number, orderindex: number, scopeIds: number[]) {
    //    return scopeIds.indexOf(this.graph.getDecisionBranchDestLinkForIndex(decisionId, orderindex).destinationId) > -1;
    //}

    private getNextSystemTaskOnMainCondition(id: number): number {
        let nextIds = this.viewModel.getNextShapeIds(id);
        if (nextIds && nextIds.length > 0) {
            let nextMainConditionId = nextIds[0];
            if (this.viewModel.getShapeTypeById(nextMainConditionId) === ProcessShapeType.SystemTask) {
                return nextMainConditionId;
            }
            return this.getNextSystemTaskOnMainCondition(nextMainConditionId);
        }
        return undefined;
    }

    private needToReplaceUserTask(userTaskId: number, previousIds: number[], nextId: number): boolean {
        return ProcessDeleteHelper.isLastUserTaskInCondition(userTaskId, previousIds, nextId, this.processGraph) ||
            ProcessDeleteHelper.isUserTaskBetweenTwoUserDecisions(userTaskId, previousIds, nextId, this.processGraph);
    }

    private replaceUserTask(userTaskId: number, previousIds: number[], nextId: number): NewUserTaskInfo {
        // add user task and system task shapes
        const newUserTaskId = ProcessAddHelper.insertUserTaskInternal(this, this.shapesFactoryService);
        const newSystemId = ProcessAddHelper.insertSystemTaskInternal(this, this.shapesFactoryService);

        if (previousIds.length > 1) {
            this.updateBranchDestinationId(userTaskId, newUserTaskId);
        }

        // update links
        for (let id of previousIds) {
            this.updateLink(id, userTaskId, newUserTaskId);
        }

        ProcessAddHelper.addLinkInfo(newUserTaskId, newSystemId, this);
        ProcessAddHelper.addLinkInfo(newSystemId, nextId, this);

        return {
            userTaskId: newUserTaskId,
            systemTaskId: newSystemId
        };
    }

    public createAutoInsertTaskMessage() {
        // #TODO: use Nova message service to display message
        //var message = new Shell.Message(Shell.MessageType.Info, this.rootScope["config"].labels["ST_Auto_Insert_Task"]);
        //this.messageService.addMessage(message);
    }

    public handleUserTaskDragDrop(userTaskShapeId: number, edge: MxCell) {
        let oldAfterShapeId: number = this.viewModel.getFirstNonSystemShapeId(userTaskShapeId);
        let oldBeforeShapeIds = this.viewModel.getPrevShapeIds(userTaskShapeId);
        let systemTaskShapeId: number = this.getNextSystemTaskOnMainCondition(userTaskShapeId);

        let newUserTask: NewUserTaskInfo = null;
        let newSourcesAndDestinations = this.getSourcesAndDestinations(edge);
        let originalSourcesWithNewDestinations: SourcesAndDestinations = null;
        // First remove the shape from the graph and update any condition destinations that were point to the user task,
        // to be pointing to shape after it that's not getting moved
        if (this.needToReplaceUserTask(userTaskShapeId, oldBeforeShapeIds, oldAfterShapeId)) {
            // update old location links to point to new user task
            newUserTask = this.replaceUserTask(userTaskShapeId, oldBeforeShapeIds, oldAfterShapeId);
            this.createAutoInsertTaskMessage();
        } else {
            // update old location links
            originalSourcesWithNewDestinations = this.processGraph.updateSourcesWithDestinations(userTaskShapeId, oldAfterShapeId);
        }

        // If the destination of edge is the task you're dragging, point edge to the shape after the move.
        this.updateDestinationsIfDestinationIsDraggedShape(newSourcesAndDestinations, originalSourcesWithNewDestinations,
            userTaskShapeId, oldAfterShapeId, newUserTask);

        // if edge source ids contains system task id of the user task you're dragging,
        // need to add the before system task id of selected dragging user task as part of list of source ids.
        this.addAdditionaSourcesAndDestinations(newSourcesAndDestinations, systemTaskShapeId, oldAfterShapeId, oldBeforeShapeIds, newUserTask);

        // update source links of the edge to point to the dropped user task, ignoring system task id associated with dragged user task
        for (let i = 0; i < newSourcesAndDestinations.sourceIds.length; i++) {
            let sourceId = newSourcesAndDestinations.sourceIds[i];
            if (sourceId !== systemTaskShapeId) {
                this.updateLink(sourceId, newSourcesAndDestinations.destinationIds[i], userTaskShapeId);
            }
        }

        // update system task's link to point to destination of the edge
        for (let i = 0; i < newSourcesAndDestinations.destinationIds.length; i++) {
            if (newSourcesAndDestinations.destinationIds[i] !== userTaskShapeId) {
                this.changeLink(systemTaskShapeId, newSourcesAndDestinations.destinationIds[i]);
            }
        }

        // this case happens when dragging to after mergepoint. update all branch destination end points

        for (let i = 0; i < newSourcesAndDestinations.destinationIds.length; i++) {
            if (newSourcesAndDestinations.sourceIds.length > 1) {
                this.updateSubtreeBranchesDestinationId(newSourcesAndDestinations.destinationIds[i], userTaskShapeId);
            }
        }


        this.updateProcessChangedState(userTaskShapeId, NodeChange.Update);
        this.viewModel.communicationManager.processDiagramCommunication.modelUpdate(userTaskShapeId);
    }

    private addAdditionaSourcesAndDestinations(newSourcesAndDestinations: SourcesAndDestinations,
                                               systemTaskShapeId: number,
                                               oldAfterShapeId: number,
                                               oldBeforeShapeIds: number[],
                                               newUserTask: NewUserTaskInfo) {
        if (newSourcesAndDestinations.sourceIds.filter(id => systemTaskShapeId === id).length > 0) {
            if (newUserTask) {
                newSourcesAndDestinations.sourceIds.push(newUserTask.systemTaskId);
                newSourcesAndDestinations.destinationIds.push(oldAfterShapeId);
            } else {
                let newIds = oldBeforeShapeIds.filter(a => newSourcesAndDestinations.sourceIds.indexOf(a) < 0);
                for (let i = 0; i < newIds.length; i++) {
                    newSourcesAndDestinations.sourceIds.push(newIds[i]);
                    newSourcesAndDestinations.destinationIds.push(oldAfterShapeId);
                }

            }
        }
    }

    private updateDestinationsIfDestinationIsDraggedShape(newSourcesAndDestinations: SourcesAndDestinations,
                                                          originalSourcesWithNewDestinations: SourcesAndDestinations,
                                                          userTaskShapeId: number,
                                                          oldAfterShapeId,
                                                          newUserTask: NewUserTaskInfo = null) {
        for (let i = 0; i < newSourcesAndDestinations.destinationIds.length; i++) {
            if (newSourcesAndDestinations.destinationIds[i] === userTaskShapeId) {
                if (newUserTask) {
                    newSourcesAndDestinations.destinationIds[i] = newUserTask.userTaskId;
                } else {
                    if (originalSourcesWithNewDestinations) {
                        let index = originalSourcesWithNewDestinations.sourceIds.indexOf(newSourcesAndDestinations.sourceIds[i]);
                        if (index > -1) {
                            newSourcesAndDestinations.destinationIds[i] = originalSourcesWithNewDestinations.destinationIds[index];
                        }
                    } else {
                        newSourcesAndDestinations.destinationIds[i] = oldAfterShapeId;
                    }
                }
            }
        }
    }

    public isValidForDrop(userTaskShapeId: number, edge: MxCell): boolean {
        if (!(edge instanceof DiagramLink)) {
            return false;
        }

        let diagramLink = <DiagramLink>edge;
        let dropSourceNode: IDiagramNode = diagramLink.sourceNode;
        let dropTargetNode: IDiagramNode = diagramLink.targetNode;

        // find all shapes involved in the drag
        let scopeContext: IScopeContext = this.processGraph.getScope(userTaskShapeId);
        let shapeIds: number[] = Object.keys(scopeContext.visitedIds).map(a => Number(a));

        // don't allow dropping on any links coming into or coming out of the scope
        if (shapeIds.filter(id => id === dropSourceNode.model.id || id === dropTargetNode.model.id).length > 0) {
            return false;
        }

        // basic validation
        let nodeType: NodeType = dropSourceNode.getNodeType();

        return nodeType === NodeType.SystemTask ||
            nodeType === NodeType.UserDecision ||
            nodeType === NodeType.MergingPoint;
    }

    public getColumnByX(x: number): number {
        return (x - GRAPH_LEFT) / GRAPH_COLUMN_WIDTH;
    }

    public getRowByY(y: number): number {
        return (y - GRAPH_TOP) / GRAPH_ROW_HEIGHT;
    }

    private getEdgeCellState(edge: MxCell): MxCellState {
        const view: MxGraphView = this.mxgraph.getView();
        const state = view.getState(edge);
        return state;
    }

    private hasOverlay(sourceNode: IDiagramNode, targetNode: IDiagramNode): boolean {
        if (this.viewModel.propertyValues["clientType"].value !== ProcessType.UserToSystemProcess) {
            if ((sourceNode.getNodeType() === NodeType.UserTask && targetNode.getNodeType() === NodeType.SystemTask) ||
                (sourceNode.getNodeType() === NodeType.SystemDecision) ||
                (targetNode.getNodeType() === NodeType.SystemDecision)) {
                return false;
            }
        }
        return true;
    }

    private postRender(id: number) {
        this.selectNode(this.getNodeById(id.toString()));
    }

    private addConnector(graphModel, link: IProcessLinkModel, sourceId: number = link.sourceId, destinationId: number = link.destinationId) {
        let sourceNode: IDiagramNode = graphModel.getCell(sourceId.toString());
        let targetNode: IDiagramNode = graphModel.getCell(destinationId.toString());

        if (sourceNode !== null && targetNode !== null) {
            let edgeGeo = new EdgeGeo();
            edgeGeo.edge = Connector.render(this.processGraph, link, sourceNode, targetNode, this.hasOverlay(sourceNode, targetNode), link.label, null);
            this.edgesGeo.push(edgeGeo);
        }

        return null;
    }

    public  getSourcesAndDestinations(edge: MxCell): SourcesAndDestinations {
        let source = (<IDiagramNodeElement>edge.source).getNode();
        let sourceId = Number(source.getId());
        let sourceIds: number[] = [];
        sourceIds.push(sourceId);

        let destination = (<IDiagramNodeElement>edge.target).getNode();
        let destinationId: number = Number(destination.getId());
        if (destination.getNodeType() === NodeType.MergingPoint) {
            let nextNode = destination.getNextNodes()[0];
            destinationId = Number(nextNode.getId());
        }

        if (source.getNodeType() === NodeType.MergingPoint) {
            sourceIds = this.viewModel.getPrevShapeIds(destinationId);
        }

        let sourcesAndDestinations = new SourcesAndDestinations();
        sourcesAndDestinations.sourceIds = sourceIds;
        sourcesAndDestinations.destinationIds = [];

        for (let i = 0; i < sourceIds.length; i++) {
            sourcesAndDestinations.destinationIds.push(destinationId);
        }

        return sourcesAndDestinations;
    }

    private updateSubtreeBranchesDestinationId(oldDestinationId: number, newDestinationId: number) {

        this.viewModel.getDecisionBranchDestinationLinks((link: IProcessLink) => {
            return link.destinationId === oldDestinationId;
        }).forEach((link: IProcessLink) => {
            link.destinationId = newDestinationId;
        });
    }

    // #UNUSED
    //private getBranchDestinationIds(decisionId: number): number[] {
    //    if (this.viewmodel.decisionBranchDestinationLinks == null) {
    //        // select the end shape
    //        return [Number(this.viewmodel.getEndShapeId())];
    //    }

    //    return this.viewmodel.decisionBranchDestinationLinks
    //        .filter((link: IProcessLink) => link.sourceId === decisionId)
    //        .map((link: IProcessLink) => link.destinationId);
    //}

    private getDecisionConditionDestination(shapeId: number): IProcessShape {
        let nextShapeIds: number[] = this.viewModel.getNextShapeIds(shapeId);

        if (nextShapeIds && nextShapeIds.length > 0) {
            let nextShapeId: number = nextShapeIds[0];
            let shapeType: ProcessShapeType = this.viewModel.getShapeTypeById(shapeId);

            if (shapeType === ProcessShapeType.SystemTask) {
                return this.viewModel.getShapeById(nextShapeId);
            } else {
                return this.getDecisionConditionDestination(nextShapeId);
            }
        }

        return this.viewModel.getShapeById(shapeId);
    }

    private selectNode(node) {
        if (node) {
            const evt = {
                consume() {
                    //fixme: why is this empty? it should be undefined or have a return
                }
            };
            this.mxgraph.selectCellForEvent(node, evt);
        }
    }

    public getDefaultBranchLabel(decisionId: number): string {
        const nextLinks = this.viewModel.getNextShapeIds(decisionId);
        const branchIndex = nextLinks ? nextLinks.length + 1 : 1;
        return `${this.rootScope.config.labels["ST_Decision_Modal_New_System_Task_Edge_Label"]}${branchIndex}`;
    }

    public updateLink(sourceId: number, oldDestinationId: number, newDestinationId: number) {
        const index: number = this.viewModel.getLinkIndex(sourceId, oldDestinationId);
        if (index > -1) {
            this.viewModel.links[index].destinationId = newDestinationId;
        }
    }

    private changeLink(sourceId: number, newDestinationId: number) {
        for (let link of this.viewModel.links) {
            if (link.sourceId === sourceId) {
                link.destinationId = newDestinationId;
            }
        }
    }

    // #UNUSED
    //private deleteLink(sourceId: number, destinationId: number): void {
    //    var links = this.viewmodel.links.filter(link => {
    //        return !(link.sourceId === sourceId && link.destinationId === destinationId);
    //    });
    //    this.viewmodel.links = links;
    //}

    private logError(arg: any) {
        if (this.$log) {
            this.$log.error(arg);
        }
    }

    // #UNUSED
    //private logInfo(arg: any) {
    //    if (this.$log) {
    //        this.$log.info(arg);
    //    }
    //}
}

