import {ILocalizationService, IMessageService} from "../../../../../../core/";
import {IProcessGraph, ILayout} from "./models/";
import {INotifyModelChanged, IConditionContext} from "./models/";
import {ICondition, IScopeContext, IStopTraversalCondition} from "./models/";
import {ISelectionListener, INextIdsProvider} from "./models/";
import {IOverlayHandler, IShapeInformation} from "./models/";
import {IDiagramNode, IDiagramNodeElement} from "./models/";
import {IProcessShape, IProcessLink} from "./models/";
import {SourcesAndDestinations, ProcessShapeType} from "./models/";
import {NodeType, NodeChange} from "./models/";
import {IProcessService} from "../../../../services/process/process.svc";
import {IProcessViewModel} from "../../viewmodel/process-viewmodel";
import {BpMxGraphModel} from "./bp-mxgraph-model";
import {ShapesFactory} from "./shapes/shapes-factory";
import {Layout} from "./layout";
import {ConnectorStyles} from "./shapes/connector-styles";
import {NodeShapes} from "./shapes/node-shapes";
import {DiagramNode, DiagramLink, SystemDecision} from "./shapes/";
import {ShapeInformation} from "./shapes/shape-information";
import {NodeLabelEditor} from "./node-label-editor";
import {ProcessDeleteHelper} from "./process-delete-helper";
import {ProcessAddHelper} from "./process-add-helper";
import {IDialogSettings, IDialogService} from "../../../../../../shared";

export class ProcessGraph implements IProcessGraph {
    public layout: ILayout;
    public startNode: IDiagramNode;
    public endNode: IDiagramNode;
    public nodeLabelEditor: NodeLabelEditor;
    //#TODO fix up references later 
    //public dragDropHandler: IDragDropHandler;
    private mxgraph: MxGraph;    
    private isIe11: boolean;
    private selectionListeners: Array<ISelectionListener> = [];
    private unsubscribeToolbarEvents = [];
    private executionEnvironmentDetector: any;
    private transitionTimeOut: number = 400;
    private bottomBorderWidt: number = 6;
    private highlightedEdgeStates: any[] = [];
    private deleteShapeHandler: string;

    public globalScope: IScopeContext;

    public static get MinConditions(): number {
        return 2;
    }

    public static get MaxConditions(): number {
        return 10;
    }

    public get isUserSystemProcess(): boolean {
        return this.viewModel.isUserToSystemProcess;
    }

    constructor(
        public rootScope: any,
        private scope: any,
        private htmlElement: HTMLElement,
        private processService: IProcessService,
        // #TODO fix up references later 
        //private artifactVersionControlService: Shell.IArtifactVersionControlService,
        public viewModel: IProcessViewModel, 
        private dialogService: IDialogService,
        private localization: ILocalizationService,
        public messageService: IMessageService = null,
        private $log: ng.ILogService = null,
        private shapesFactory: ShapesFactory = null) {

        // Creates the graph inside the given container
         
        // This is temporary code. It will be replaced with 
        // a class that wraps this global functionality.   
        let w: any = window; 
        this.executionEnvironmentDetector = new w.executionEnvironmentDetector();
         
        this.mxgraph = new mxGraph(this.htmlElement, new BpMxGraphModel());

        if (!shapesFactory) {
            this.shapesFactory = new ShapesFactory(this.rootScope);
        }
        
        this.layout = new Layout(this, viewModel, rootScope, this.shapesFactory, this.messageService, this.$log);        
        // this.viewModel.licenseType = processModelService && processModelService.licenseType;
        this.init();
    }

    private init() {
        this.setIsIe11();

        this.initializeGraphContainer();

        // #TODO: interaction with the toolbar will be different in Nova
        // this.subscribeToToolbarEvents();

        window.addEventListener("buttonUpdated", this.buttonUpdated, true);

        // non movable
        this.mxgraph.setCellsMovable(false);

        ConnectorStyles.createStyles();
        NodeShapes.register(this.mxgraph);

        this.addMouseEventListener(this.mxgraph);

        // Enables tooltips in the graph
        //this.graph.setTooltips(true);

        //Selection logic
        this.applyDefaultStyles();
        this.applyReadOnlyStyles();
        this.initSelection();
         
        if (!this.viewModel.isReadonly) {
            // #TODO: fix up these references later 
            // this.dragDropHandler = new DragDropHandler(this);
             this.nodeLabelEditor = new NodeLabelEditor(this.htmlElement);
        }

        this.disableEdgeSelection();

        // add a selection listener to highlight node edges
        this.addSelectionListener((elements) => {
            this.highlightNodeEdges(elements);
        });

        this.initializeGlobalScope();
  
    }
    
    public render(useAutolayout, selectedNodeId) {
        try {
            // uses layout object to draw a new diagram for process model
            this.layout.render(useAutolayout, selectedNodeId);
            if (this.nodeLabelEditor != null) {
                this.nodeLabelEditor.init();
            }

            this.deleteShapeHandler = 
               this.viewModel.communicationManager.toolbarCommunicationManager.registerClickDeleteObserver(this.deleteShape);
            
        } catch (e) {
            this.logError(e);
            if (this.messageService) {
                this.messageService.addError(e.message);
            }
        }
    }

    public redraw(action: any): void {
        if (action == null) {
            return;
        }

        let model = this.mxgraph.getModel();

        model.beginUpdate();
        try {
            action();
        } catch (e) {
            this.logError(e);
        }
        finally {
            model.endUpdate();
        }
    }
    
    public getMxGraph(): MxGraph {
        return this.mxgraph;
    }
 
    public getMxGraphModel() {
        return this.mxgraph.getModel();
    }

    public getHtmlElement() {
        return this.htmlElement;
    }

    private getDecisionConditionInsertMethod(decisionId: number): (decisionId: number, layout: ILayout,
        shapesFactoryService: ShapesFactory, label?: string, conditionDestinationId?: number) => number {
        let shapeType = this.viewModel.getShapeTypeById(decisionId);

        switch (shapeType) {
            case ProcessShapeType.SystemDecision:
                return ProcessAddHelper.insertSystemDecisionCondition;
            case ProcessShapeType.UserDecision:
                return ProcessAddHelper.insertUserDecisionCondition;
            default:
                throw new Error(`Expected a decision type but found ${shapeType}`);
        }
    }

    private canAddDecisionConditions(decisionId: number, conditions: ICondition[]): boolean {
        let canAdd: boolean = true;
        let errorMessage: string;

        let shapeType = this.viewModel.getShapeTypeById(decisionId);

        if (!conditions || conditions.length <= 0) {
            canAdd = false;
        } else if (this.hasMaxConditions(decisionId)) {
            canAdd = false;
            errorMessage = this.rootScope.config.labels["ST_Add_CannotAdd_MaximumConditionsReached"];
        } else if (shapeType === ProcessShapeType.SystemDecision &&
            this.viewModel.isWithinShapeLimit(1) === false) {
            canAdd = false;
        } else if (shapeType === ProcessShapeType.UserDecision &&
            this.viewModel.isWithinShapeLimit(2) === false) {
            canAdd = false;
        }
        if (!canAdd && errorMessage && this.messageService) {
            this.messageService.addError(errorMessage);
        }

        return canAdd;
    }

    public addDecisionBranches(decisionId: number, newConditions: ICondition[]) {
        if (!this.canAddDecisionConditions(decisionId, newConditions)) {
            return;
        }

        let insertMethod = this.getDecisionConditionInsertMethod(decisionId);
        let id: number;

        for (let i: number = 0; i < newConditions.length; i++) {
            id = insertMethod(decisionId, this.layout, this.shapesFactory, newConditions[i].label, newConditions[i].mergeNode.model.id);
        }

        this.notifyUpdateInModel(NodeChange.Update, id);
    }

    private buttonUpdated = (event) => {
        var cellId = event.detail.id;
        var overlayHandler: IOverlayHandler = this.mxgraph.getModel().getCell(cellId);
        if (overlayHandler != null) {
            overlayHandler.updateOverlay(this.mxgraph);
        }
    };

    /*private subscribeToToolbarEvents() {
        // subscribe to toolbar commands using the event bus 

        // Note:the event bus is implemented as a decorator to the 
        // rootscope and is accessible through local scopes

        if (this.scope.subscribe) {

            if (this.unsubscribeToolbarEvents.length > 0) {
                // remove previous event listeners 
                this.removeToolbarEventListeners();
            }

            this.unsubscribeToolbarEvents.push(
                this.scope.subscribe("Toolbar:Delete", (event, target: IDiagramNode) => {
                    if (target) {

                        if (this.viewModel.status && this.viewModel.status.isReadOnly) {
                            var message = new Message(MessageType.Error, this.rootScope["config"].labels["ST_View_OpenedInReadonly_Message"]);
                            this.messageService.addMessage(message);
                            return;
                        }

                        if (target.getNodeType() === NodeType.UserTask) {
                            this.deleteUserTask(target.model.id, (nodeChange, id) => this.notifyUpdateInModel(nodeChange, id));
                        } else if (target.getNodeType() === NodeType.UserDecision || target.getNodeType() === NodeType.SystemDecision) {
                            this.deleteDecision(target.model.id, (nodeChange, id) => this.notifyUpdateInModel(nodeChange, id));
                        }
                    }
                })
            );
            this.unsubscribeToolbarEvents.push(
                this.scope.subscribe("Toolbar:SaveProcess", (event, target) => {
                    this.saveProcess();
                })
            );
            this.unsubscribeToolbarEvents.push(
                this.scope.subscribe("Toolbar:PublishProcess", (event, target) => {
                    this.publishProcess();
                })
            );
            this.unsubscribeToolbarEvents.push(
                this.scope.subscribe("Toolbar:DiscardChanges", (event, target) => {
                    this.discardChanges();
                })
            );
        }
    }*/

    private removeToolbarEventListeners() {

        if (this.unsubscribeToolbarEvents.length > 0) {
            for (var i = 0; i < this.unsubscribeToolbarEvents.length; i++) {
                this.unsubscribeToolbarEvents[i]();
                this.unsubscribeToolbarEvents[i] = null;
            }
        }
        this.unsubscribeToolbarEvents = [];
    }

    private initializeGraphContainer() {

        mxEvent.disableContextMenu(this.htmlElement);

        // This enables scrolling for the container of mxGraph
        if (this.viewModel.isSpa) {
            window.addEventListener("resize", this.resizeWrapper, true);
            this.htmlElement.style.overflow = "auto";
            if (this.isIe11) {
                // fixed bug https://trello.com/c/lDdwGxZC
                this.htmlElement.style.msOverflowStyle = "scrollbar"; // "-ms-autohiding-scrollbar";
            }
        } else {
            this.htmlElement.style.overflowX = "auto";
            this.htmlElement.style.overflowY = "none";
        }
    }

    private getPosition(element) {
        var xPosition = 0;
        var yPosition = 0;

        while (element) {
            xPosition += (element.offsetLeft);
            yPosition += (element.offsetTop);
            element = element.offsetParent;
        }
        return { x: xPosition, y: yPosition };
    }

    private getMinHeight(): string {
        var shift = this.getPosition(this.htmlElement).y;
        var height = window.innerHeight - shift - this.bottomBorderWidt;
        return `${height}px`;
    }

    private getMinWidth(): string {
        let width = this.htmlElement.parentElement.offsetWidth;
        return `${width}px`;
    }

    private resizeWrapper = (event) => {
        this.updateSizeChanges();
    };

    private setContainerSize(width: number, height: number = 0) {
        var minHeight = height === 0 ? this.getMinHeight() : `${height - this.bottomBorderWidt}px`;
        var minWidth = width === 0 ? this.getMinWidth() : `${width}px`;
        if (width === 0) {
            this.htmlElement.style.transition = "";
        } else {
            this.htmlElement.style.transition = `width ${this.transitionTimeOut}ms`;
            setTimeout(this.fireUIEvent, this.transitionTimeOut, "resize");
        }

        this.htmlElement.style.height = minHeight;
        this.htmlElement.style.width = minWidth;
    }

    public updateSizeChanges(width: number = 0, height: number = 0) {
        this.setContainerSize(width, height);

        //// This prevents some weird issue with the graph growing as we drag off the container edge.
        var svgElement = angular.element(this.htmlElement.children[0])[0];
        var containerElement: any = angular.element(this.htmlElement)[0];
        svgElement.style.height = containerElement.style.height;
        svgElement.style.maxHeight = containerElement.style.height;
        if (this.isIe11) {
            svgElement.style.width = containerElement.style.width;
            svgElement.style.maxWidth = containerElement.style.width;
        } else {
            svgElement.style.width = containerElement.style.minWidth;
            svgElement.style.maxWidth = containerElement.style.minWidth;
        }
    }

    public updateAfterRender() {
        if (this.viewModel.isSpa) {
            this.updateSizeChanges(); //this.fireUIEvent("resize");
        }
    }

    private fireUIEvent(eventName: string) {
        var evt: UIEvent = document.createEvent("UIEvents");
        evt.initEvent(eventName, true, true);
        window.dispatchEvent(evt);
    }

    private applyReadOnlyStyles() {
        //init readonly mode
        this.mxgraph.edgeLabelsMovable = false;
        this.mxgraph.isCellDisconnectable = () => false;
        this.mxgraph.isTerminalPointMovable = () => false;
        this.mxgraph.isCellMovable = () => false;
        this.mxgraph.isCellResizable = () => false;
        this.mxgraph.isCellEditable = () => false;
        this.mxgraph.isCellDeletable = () => false;
        //this.graph.isCellsLocked = () => true;
    }

    private applyDefaultStyles() {
        //Selection styles
        mxConstants.DEFAULT_VALID_COLOR = "#d4d5da";
        mxConstants.VERTEX_SELECTION_COLOR = "#32CFFF";
        mxConstants.VERTEX_SELECTION_STROKEWIDTH = 2;
        mxConstants.VERTEX_SELECTION_DASHED = false;
        mxConstants.DROP_TARGET_COLOR = "#53BBED";
        mxConstants.EDGE_SELECTION_COLOR = "#53BBED";
        mxConstants.EDGE_SELECTION_STROKEWIDTH = 2;
        mxConstants.EDGE_SELECTION_DASHED = false;
        mxConstants.LOCKED_HANDLE_FILLCOLOR = "#22AA06";
        mxConstants.CURSOR_BEND_HANDLE = "default";
        mxConstants.CURSOR_TERMINAL_HANDLE = "default";
    }

   private disableEdgeSelection() {
       mxGraph.prototype.isCellSelectable = (cell) => {
           var selectable: boolean = false;
           if (cell) {
               if (cell.isEdge()) {
                   selectable = false;
               } else if (cell.getNodeType &&
                   cell.getNodeType() === NodeType.SystemTask &&
                   !this.viewModel.isUserToSystemProcess) {

                   selectable = false;

               } else {
                   selectable = true;
               }
           }
           return selectable;
       };
    }

    public addSelectionListener(listener: ISelectionListener) {
        if (listener != null) {
            this.selectionListeners.push(listener);
        }
    }

    public destroy() {

        this.selectionListeners = null;

        if (this.viewModel.isSpa) {
            window.removeEventListener("resize", this.resizeWrapper, true);
        }
        window.removeEventListener("buttonUpdated", this.buttonUpdated);

        this.removeToolbarEventListeners();        

        // remove graph
        this.mxgraph.getModel().clear();
        this.mxgraph.destroy();

        while (this.htmlElement.hasChildNodes()) {
            this.htmlElement.removeChild(this.htmlElement.firstChild);
        }

        // #TODO: fix up these references later 
        // Dispose handlers
        //if (this.dragDropHandler != null) {
        //    this.dragDropHandler.dispose();
        //}

        if (this.nodeLabelEditor != null) {
            this.nodeLabelEditor.dispose();
        }

        this.viewModel.communicationManager.toolbarCommunicationManager.removeClickDeleteObserver(this.deleteShapeHandler);
        
    }

    private addMouseEventListener(graph: MxGraph) {
        graph.addMouseListener(
            {
                currentState: null,
                previousStyle: null,
                mouseDown: function (sender, me) {
                    let cell = graph.getCellAt(me.graphX, me.graphY);
                    var tmp = sender.view.getState(cell);
                    this.currentState = tmp;
                    if (this.currentState != null) {
                        this.onMouseLeave(sender, me.getEvent(), this.currentState);
                        this.onMouseDown(sender, me.getEvent(), this.currentState);
                    }
                },
                mouseMove: function (sender, me) {
                    if (this.currentState != null && me.getState() === this.currentState) {
                        return;
                    }

                    var tmp = sender.view.getState(me.getCell());

                    // Ignores everything but vertices
                    if (sender.isMouseDown || (tmp != null && !sender.getModel().isVertex(tmp.cell))) {
                        tmp = null;
                    }

                    if (tmp !== this.currentState) {
                        if (this.currentState != null) {
                            this.onMouseLeave(sender, me.getEvent(), this.currentState);
                        }

                        this.currentState = tmp;

                        if (this.currentState != null) {
                            this.onMouseEnter(sender, me.getEvent(), this.currentState);
                        }
                    }
                },
                mouseUp: function (sender, me) { },
                onMouseEnter: function (sender, evt, state) {
                    if (state != null) {
                        if (state.cell != null && state.cell.onMouseEnter != null) {
                            state.cell.onMouseEnter(sender, evt);
                        }

                        if (state.shape != null) {
                            state.shape.apply(state);
                            state.shape.redraw();
                        }
                    }
                },
                onMouseLeave: function (sender, evt, state) {
                    if (state != null) {
                        if (state.cell != null && state.cell.onMouseLeave != null) {
                            state.cell.onMouseLeave(sender, evt);
                        }

                        if (state.shape != null) {
                            state.shape.apply(state);
                            state.shape.redraw();
                        }
                    }
                },
                onMouseDown: function (sender, evt, state) {
                    if (state != null) {
                        if (state.cell != null && state.cell.onMouseDown != null) {
                            state.cell.onMouseDown(sender, evt);
                        }

                        if (state.shape != null) {
                            state.shape.apply(state);
                            state.shape.redraw();
                        }
                    }
                }
            }
        );
    }


    public addCellOverlay(parentCell, overlay) {
        return this.mxgraph.addCellOverlay(parentCell, overlay);
    }

    public getDefaultParent() {
        return this.mxgraph.getDefaultParent();
    }

    public addCell<T extends IProcessShape>(node: DiagramNode<T>, parent) {
        return this.mxgraph.addCell(node, parent);
    }

    public addLink(link: DiagramLink, parent, index?: number, source?: MxCell, target?: MxCell) {
        return this.mxgraph.addCell(link, parent, index, source, target);
    }

    public insertVertex(parent, id, value, x, y, width, height, style, relative?) {
        return this.mxgraph.insertVertex(parent, id, value, x, y, width, height, style, relative);
    }

    public getCellOverlays(cell: MxCell) {
        return this.mxgraph.getCellOverlays(cell);
    }

    public removeCellOverlays(cell: MxCell) {
        this.mxgraph.removeCellOverlays(cell);
    }

    public insertEdge(parent, id, value, source, target, style: string) {
        return this.mxgraph.insertEdge(parent, id, value, source, target, style);
    }

    public getXbyColumn(col: number): number {
        return this.layout.getXbyColumn(col);
    }

    public getYbyRow(row: number): number {
        return this.layout.getYbyRow(row);
    }

    public getColumnByX(x: number): number {
        return this.layout.getColumnByX(x);
    }

    public getRowByY(y: number): number {
        return this.layout.getRowByY(y);
    }

    public getNodeById(id: string): IDiagramNode {
        return this.getMxGraphModel().getCell(id);
    }

    public getNodeAt(x: number, y: number): IDiagramNode {
        let cells = this.mxgraph.getChildVertices(this.mxgraph.getDefaultParent());
        let nodes: IDiagramNode[] = cells.filter(cell => cell.getNodeType);

        for (let node of nodes) {
            if (node.getX() === x && node.getY() === y) {
                return node;
            }
        }

        return null;
    }

    public updateGraphNodes(filter: (MxCell) => boolean, update: (MxCell) => void) {
        var cells = this.mxgraph.getChildVertices(this.mxgraph.getDefaultParent());
        for (let cell of cells) {
            if (filter(cell)) {
                update(cell);
            }
        }
    }

    private deleteShape = () => {
        let selectedNodes = this.getSelectedNodes();
        if (selectedNodes.length > 0) {
            let selectedNode: IDiagramNode = selectedNodes[0];
            let dialogParameters = selectedNode.getDeleteDialogParameters();
        
            this.dialogService.open(<IDialogSettings>{
                okButton: this.localization.get("App_Button_Ok"),
                template: require("../../../../../../shared/widgets/bp-dialog/bp-dialog.html"),
                header: this.localization.get("App_DialogTitle_Alert"),
                message: dialogParameters.message
            }).then((confirm: boolean) => {
                if (confirm) {
                    if (selectedNode.getNodeType() === NodeType.UserTask) {
                        ProcessDeleteHelper.deleteUserTask(selectedNode.model.id, (nodeChange, id) => this.notifyUpdateInModel(nodeChange, id), this);
                    } else if (selectedNode.getNodeType() === NodeType.UserDecision || selectedNode.getNodeType() === NodeType.SystemDecision) {
                        ProcessDeleteHelper.deleteDecision(selectedNode.model.id, 
                        (nodeChange, id) => this.notifyUpdateInModel(nodeChange, id), this, this.shapesFactory);
                    }
                };
            });
        }
    }

    private hasMaxConditions(decisionId: number): boolean {
        return this.viewModel.getNextShapeIds(decisionId).length >= ProcessGraph.MaxConditions;
    }

    public updateSourcesWithDestinations(shapeId: number, newDestinationId: number): SourcesAndDestinations {
        let sources = this.viewModel.getPrevShapeIds(shapeId);
        if (sources.length > 1) {
            this.updateBranchDestinationId(shapeId, newDestinationId);
        }
        let originalShapeSourcesAndDestinations: SourcesAndDestinations = { sourceIds: [], destinationIds: [] };
        for (let sourceId of sources) {
            let linkIndex = this.viewModel.getLinkIndex(sourceId, shapeId);
            let link = this.viewModel.links[linkIndex];
            if (link.destinationId === shapeId) {
                originalShapeSourcesAndDestinations.sourceIds.push(sourceId);
                let sourceCondition = this.globalScope.visitedIds[link.sourceId].innerParentCondition();
                let destinationCondition = this.globalScope.visitedIds[newDestinationId].innerParentCondition();
                let currentShapeCondition = this.globalScope.visitedIds[shapeId].innerParentCondition();

                // if the new destination id belongs to the same condition as the source id, but current shape is not in same condition
                // then need to change destination id to be the default destination of the decision.
                if (sourceCondition && destinationCondition && currentShapeCondition &&
                    (sourceCondition === destinationCondition && currentShapeCondition !== destinationCondition)) {

                    let backupDestId = this.layout.getConditionDestination(destinationCondition.decisionId).id;
                    this.viewModel.updateDecisionDestinationId(sourceCondition.decisionId, sourceCondition.orderindex, backupDestId);
                    link.destinationId = backupDestId;
                    originalShapeSourcesAndDestinations.destinationIds.push(backupDestId);
                } else {
                    link.destinationId = newDestinationId;
                    originalShapeSourcesAndDestinations.destinationIds.push(newDestinationId);
                }
            }
        }
        return originalShapeSourcesAndDestinations;
    }

    private updateProcessChangedState(id: number, change: NodeChange = NodeChange.Add, redraw: boolean = false) {
        this.layout.updateProcessChangedState(id, change, redraw);
    }

    private updateBranchDestinationId(oldDestinationId: number, newDestinationId: number) {
        this.layout.updateBranchDestinationId(oldDestinationId, newDestinationId);
    }

    public notifyUpdateInModel: INotifyModelChanged = (nodeChange: NodeChange, selectedId: number) => {
        this.viewModel.communicationManager.processDiagramCommunication.modelUpdate(selectedId);
        this.updateProcessChangedState(selectedId, nodeChange);
    }

    public saveProcess() {
        // #TODO: implement this function later 

        //this.processService.save().then((result: IProcess) => {
        //    this.rootScope.$broadcast("processSaved", result);
        //}).catch(() => {

        //});
    }

    public publishProcess() {
          
        // business logic to publish the current process
        
        // #TODO: implement this function later 

        //this.artifactVersionControlService.publish(this.viewModel,
        //    this.viewModel.isChanged).then((result: boolean) => {
        //        this.viewModel.isChanged = false;
        //        this.viewModel.resetLock();

        //        this.viewModel.status.hasEverBeenPublished = true;
        //        this.viewModel.isChanged = false;
        //    });
    }

    public discardChanges() {
         
        // business logic to discard changes
         
        // #TODO: implement this function later 

        //this.artifactVersionControlService.discardArtifactChanges([
        //    {
        //        artifactId: this.viewModel.id,
        //        status: this.viewModel.status
        //    }]).then(() => {
        //        this.rootScope.$broadcast("processChangesDiscarded");
        //    });
    }

    public setSystemTasksVisible(value: boolean) {
        var cells = this.mxgraph.getChildVertices(this.mxgraph.getDefaultParent());
        var edges = this.mxgraph.getChildEdges(this.mxgraph.getDefaultParent());
        var graphModel: MxGraphModel = this.mxgraph.getModel();

        this.logInfo("Enter setSystemTasksVisible, value = " + value);

        this.layout.hidePopupMenu();

        graphModel.beginUpdate();

        try {
            for (var j: number = 0; j < edges.length; j++) {
                var edge: MxCell = edges[j];
                if (edge && edge.target) {
                    var sourceNode = (<IDiagramNodeElement>edge.source).getNode();
                    var targetNode = (<IDiagramNodeElement>edge.target).getNode();
                    if (sourceNode
                        && sourceNode.getNodeType() !== NodeType.ProcessStart
                        && targetNode
                        && this.viewModel.isReadonly === false
                        && (targetNode.getNodeType() === NodeType.SystemTask || targetNode.getNodeType() === NodeType.SystemDecision)) {
                        if (value) {
                            (<DiagramLink>edge).showMenu(this.mxgraph);
                        } else {
                            (<DiagramLink>edge).hideMenu(this.mxgraph);
                        }
                    }
                }
            }
            for (var i: number = 0; i < cells.length; i++) {
                var cell = cells[i];
                if (cell.getNodeType) {
                    this.logInfo("cell.getNodeType() = " + cell.getNodeType());
                    if (cell.getNodeType() === NodeType.SystemTask) {
                        this.logInfo("Call cell.setCellVisible, value = " + value);
                        cell.setCellVisible(this.mxgraph, value);
                    }
                    if (cell.getNodeType() === NodeType.SystemDecision &&
                        !this.viewModel.isReadonly) {
                        if (value) {
                            (<SystemDecision>cell).showMenu(this.mxgraph);
                        } else {
                            (<SystemDecision>cell).hideMenu(this.mxgraph);
                        }
                    }
                }
            }
        } catch (e) {
            this.logError("setSystemTasksVisible, error = " + e.message);
        }
        finally {
            graphModel.endUpdate();
        }
    }

    private setIsIe11() {
        var myBrowser = this.executionEnvironmentDetector.getBrowserInfo();
        let ver = parseInt(myBrowser.version, 10);
        this.isIe11 = (myBrowser.msie && (ver === 11));
    }

    private initSelection() {
        //let that = this;
        this.mxgraph.getSelectionModel().setSingleSelection(true);
        this.mxgraph.getSelectionModel().addListener(mxEvent.CHANGE, (sender, evt) => {
            let elements = this.getSelectedNodes();
            let deletable = elements.length > 0;
            if (deletable) {
                let element: IDiagramNode = elements[0];
                deletable = element.getNodeType() === NodeType.UserDecision || 
                            element.getNodeType() === NodeType.SystemDecision ||
                            element.getNodeType() === NodeType.UserTask;
            } 

            this.viewModel.communicationManager.toolbarCommunicationManager.enableDelete(deletable);

            if (!!this.selectionListeners) {
                this.selectionListeners.forEach((listener: ISelectionListener) => {
                    listener(elements);
                });
            }
        });
    }

    private getSelectedNodes(): Array<IDiagramNode> {
        var elements = <Array<IDiagramNode>>this.mxgraph.getSelectionCells();
        elements = elements.filter(e => e instanceof DiagramNode);
        return elements;
    }

    private findConditionStart(context: IScopeContext, nextId: number): IConditionContext {
        let link: IProcessLink = this.getLink(context.id, nextId);
        let mappingLink: IProcessLink = this.getDecisionBranchDestLinkForIndex(context.id, link.orderindex);
        if (mappingLink) {
            context.mergeIds.push(mappingLink.destinationId);
            return {
                decisionId: context.id,
                orderindex: mappingLink.orderindex,
                endId: null,
                targetId: null,
                shapeIdsInCondition: []
            };
        }
        return null;
    }

    public getBranchScope(initialBranchLink: IProcessLink, nextIdsProvider: INextIdsProvider): IScopeContext {
        let context: IScopeContext = {
            id: initialBranchLink.destinationId,
            previousId: null,
            visitedIds: {},
            mergeIds: [],
            mappings: [],
            currentMappings: []
        };

        if (initialBranchLink.orderindex < 1) {
            context.visitedIds[context.id] = new ShapeInformation(context.id, []);
            return context;
        }

        let conditionDestinationLink = this.getDecisionBranchDestLinkForIndex(initialBranchLink.sourceId, initialBranchLink.orderindex);
        let branchEndIds = [conditionDestinationLink.destinationId];
        context.mergeIds = branchEndIds;
        let mapping: IConditionContext = {
            decisionId: initialBranchLink.sourceId,
            orderindex: initialBranchLink.orderindex,
            endId: null,
            targetId: null,
            shapeIdsInCondition: []
        };
        context.mappings = [mapping];
        this.getScopeInternal(context, this.defaultStopCondition, nextIdsProvider);
        // all visited ids are in the 1 mapping for getBranchScope
        mapping.shapeIdsInCondition = context.visitedIds;
        return context;
    }

    public getLink(sourceId: number, destinationId: number): IProcessLink {
        let index: number = this.viewModel.getLinkIndex(sourceId, destinationId);
        if (index && index > -1) {
            return this.viewModel.links[index];
        }

        return null;
    }

    public initializeGlobalScope() {
        if (this.viewModel) {
            let startId = this.viewModel.getStartShapeId();
            if (!startId) {
                return;
            }
            let context: IScopeContext = {
                id: startId,
                previousId: null,
                visitedIds: [],
                mappings: [],
                mergeIds: [],
                currentMappings: []
            };
            this.getScopeInternal(context, this.defaultStopCondition, this.defaultNextIdsProvider);
            this.globalScope = context;

        }
    }

    private initializeShapeInformation(context: IScopeContext): IShapeInformation {
        let currentShapeInfo = new ShapeInformation(context.id, []);
        for (let index = 0; index < context.currentMappings.length; index++) {
            let mapping = context.currentMappings[index];
            currentShapeInfo.parentConditions.push(mapping);
        };
        return currentShapeInfo;
    }
    private getScopeInternal(
        context: IScopeContext,
        stopCondition: IStopTraversalCondition,
        getNextIds: INextIdsProvider
    ): void {
        if (stopCondition(context)) {
            return;
        }

        context.visitedIds[context.id] = this.initializeShapeInformation(context);

        let nextIds: number[] = getNextIds(context);
        if (nextIds.length > 0) {

            for (let i: number = 0; i < nextIds.length; i++) {
                let nextId: number = nextIds[i];

                let mapping = this.findConditionStart(context, nextId);

                let newContext: IScopeContext = {
                    id: nextId,
                    previousId: context.id,
                    visitedIds: [],
                    mergeIds: context.mergeIds,
                    mappings: context.mappings,
                    currentMappings: context.currentMappings
                };

                if (mapping) {
                    context.mappings.push(mapping);
                    context.currentMappings.push(mapping);

                    this.getScopeInternal(newContext, stopCondition, getNextIds);

                    mapping.shapeIdsInCondition = newContext.visitedIds;

                    context.currentMappings.pop();
                } else {
                    this.getScopeInternal(newContext, stopCondition, getNextIds);
                }

                for (let visitedIdKey in newContext.visitedIds) {
                    context.visitedIds[visitedIdKey] = newContext.visitedIds[visitedIdKey];
                }
            }
        }
    }

    private defaultStopCondition: IStopTraversalCondition = (context): boolean => {
        if (context.mergeIds.indexOf(context.id) > -1) {
            context.mergeIds.pop();
            context.mappings[context.mappings.length - 1].endId = context.previousId;
            context.mappings[context.mappings.length - 1].targetId = context.id;
            return true;
        }

        return false;
    }

    public defaultNextIdsProvider: INextIdsProvider = (context) => {
        return this.viewModel.getNextShapeIds(context.id).map(id => Number(id));
    }

    private defaultDecisionNextIdsProvider: INextIdsProvider = (context) => {
        let nextShapeIds = this.viewModel.getNextShapeIds(context.id);
        // Remove the main branch, as decisions do not include the main branch in the scope
        if (context.mappings.length === 0 && context.mergeIds.length === 0) {
            nextShapeIds.splice(0, 1);
        }
        return nextShapeIds.map(id => Number(id));
    }

    private defaultUserTaskStopCondition: IStopTraversalCondition = (context): boolean => {
        let isStop: boolean = this.defaultStopCondition(context);

        if (!isStop && context.mergeIds.length === 0 && this.viewModel.getShapeTypeById(context.id) === ProcessShapeType.SystemTask) {

            context.visitedIds[context.id] = this.initializeShapeInformation(context);

            isStop = true;
        }

        return isStop;
    }

    private defaultUserTaskNextIdsProvider: INextIdsProvider = (context) => {
        return this.defaultNextIdsProvider(context);
    }

    public getScope(id: number): IScopeContext {
        let type: ProcessShapeType = this.viewModel.getShapeTypeById(id);
        let context: IScopeContext = {
            id: id,
            previousId: null,
            visitedIds: [],
            mergeIds: [],
            mappings: [],
            currentMappings: []
        };

        if (this.viewModel.isDecision(id)) {
            this.getScopeInternal(context, this.defaultStopCondition, this.defaultDecisionNextIdsProvider);
        } else if (type === ProcessShapeType.UserTask) {
            this.getScopeInternal(context, this.defaultUserTaskStopCondition, this.defaultUserTaskNextIdsProvider);
        }

        return context;
    }

    // returns a list of links in sorted order index.
    public getNextLinks(sourceId: number): IProcessLink[] {
        return this.viewModel.links.filter(a => a.sourceId === sourceId).sort((a, b) => a.orderindex - b.orderindex);
    }

    // #UNUSED
    //private findNestedDecisions(link: IProcessLink, targetIds: number[]) {
    //    let decisionMergeLinks = this.viewModel.decisionBranchDestinationLinks.filter(b => b.sourceId === link.sourceId && b.orderindex === link.orderindex);
    //    if (decisionMergeLinks.length > 0) {
    //        decisionMergeLinks.forEach((mergeDestinationLink) => {
    //            // Append to targetIds, if the id is not already in the list.
    //            if (targetIds.indexOf(mergeDestinationLink.destinationId) < 0) {
    //                targetIds.push(mergeDestinationLink.destinationId);
    //            }
    //        });
    //    }
    //}

    private getDecisionBranchDestinationLinks(decisionId: number): IProcessLink[] {
        return this.viewModel.getDecisionBranchDestinationLinks((link) => link.sourceId === decisionId);
    }

    public getDecisionBranchDestLinkForIndex(decisionId: number, orderIndex: number): IProcessLink {
        var links = this.getDecisionBranchDestinationLinks(decisionId).filter(a => a.orderindex === orderIndex);
        if (links.length > 0) {
            return links[0];
        }
        return null;
    }

    public getShapeById(id: number): IProcessShape {
        return this.viewModel.getShapeById(id);
    }

    public getValidMergeNodes(condition: IProcessLink): IDiagramNode[] {
        // find all nodes in current condition
        let scopeContext: IScopeContext = null;
        let lastShapeInBranch: IProcessShape;
        let shapesInBranch: number[] = [];

        if (condition.destinationId !== null) {
            let originalMergeNode = this.getDecisionBranchDestLinkForIndex(condition.sourceId, condition.orderindex);
            if (originalMergeNode) {
                scopeContext = this.getBranchScope(condition, this.defaultNextIdsProvider);

                lastShapeInBranch = this.viewModel.getShapeById(scopeContext.mappings[0].endId);
                shapesInBranch = Object.keys(scopeContext.visitedIds).map(a => Number(a));
            }
        } else {
            // for newly added conditions, the list of valid merge nodes should be after the decision point itself.
            lastShapeInBranch = this.viewModel.getShapeById(condition.sourceId);
        }
        let firstTasks: number[] = [];
        this.viewModel.links.forEach((a: IProcessLink) => {
            if (a.sourceId === condition.sourceId) {
                if (this.viewModel.getShapeTypeById(a.destinationId) === ProcessShapeType.UserTask) {
                    firstTasks.push(a.destinationId);
                }
            }
        });

        let invalidShapes = shapesInBranch.concat(firstTasks);
        let validShapeIds: IDiagramNode[] = [];
        for (let shape of this.viewModel.shapes) {
            // Filters out the shapes that aren't allowed to be merge nodes
            let clientType = shape.propertyValues[this.shapesFactory.ClientType.key].value;
            if (clientType === ProcessShapeType.Start ||
                clientType === ProcessShapeType.PreconditionSystemTask ||
                clientType === ProcessShapeType.SystemDecision ||
                clientType === ProcessShapeType.SystemTask) {
                continue;
            }

            if (invalidShapes.indexOf(shape.id) < 0) {
                validShapeIds.push(this.getNodeById(shape.id.toString()));
            }
        }
        return validShapeIds;
    }

    public updateMergeNode(decisionId: number, condition: ICondition): boolean {
        let originalEndNode: IProcessLink = this.getDecisionBranchDestLinkForIndex(decisionId, condition.orderindex);

        if (condition.mergeNode &&
            originalEndNode &&
            originalEndNode.destinationId !== condition.mergeNode.model.id) {
            let mainBranchOnly: INextIdsProvider = (context) => {
                let ids = this.viewModel.getNextShapeIds(context.id);
                return [Number(ids[0])];
            };
            var scope = this.getBranchScope(condition, mainBranchOnly);
            let lastShapeId = scope.mappings[0].endId;
            let origLastLinkInCondition = this.getLink(lastShapeId, originalEndNode.destinationId);
            // Updates end branch link to point to new destination id
            if (origLastLinkInCondition) {
                origLastLinkInCondition.destinationId = condition.mergeNode.model.id;
            }

            // Updates merge point for specific branch to be new destination id
            originalEndNode.destinationId = condition.mergeNode.model.id;

            return true;
        }
        return false;
    }

    private highlightNodeEdges(nodes: Array<IDiagramNode>) {
        this.clearHighlightEdges();
        if (nodes.length > 0) {
            let selectedNode: IDiagramNode = nodes[0];

            let highLightEdges = this.getHighlightScope(selectedNode, this.mxgraph.getModel());
            for (let edge of highLightEdges) {
                this.highlightEdge(edge);
            }
            this.mxgraph.orderCells(false, highLightEdges);
        }
    }

    private getHighlightScope(diagramNode: IDiagramNode, graphModel: MxGraphModel): MxCell[] {

        let connectableElement = diagramNode.getConnectableElement();
        let returnEdges: MxCell[] = [];
        for (let edge of graphModel.getOutgoingEdges(connectableElement)) {
            let targetDiagramNode = <IDiagramNode>edge.target;
            if (targetDiagramNode) {
                let actualTargetDiagramNode = targetDiagramNode.getNode();
                if (actualTargetDiagramNode.getNodeType() === NodeType.SystemDecision ||
                    actualTargetDiagramNode.getNodeType() === NodeType.SystemTask) {
                    returnEdges = returnEdges.concat(this.getHighlightScope(actualTargetDiagramNode, graphModel));
                }
                returnEdges.push(edge);
            }
        }
        return returnEdges;
    }

    private highlightEdge(edge: MxCell) {
        let state: any = this.mxgraph.getView().getState(edge);
        if (state.shape) {
            state.shape.stroke = mxConstants.EDGE_SELECTION_COLOR;
            state.shape.reconfigure();
            this.highlightedEdgeStates.push(state);
        }
    }

    private clearHighlightEdges() {
        for (let edge of this.highlightedEdgeStates) {
            if (edge.shape) {
                edge.shape.stroke = mxConstants.DEFAULT_VALID_COLOR;
                edge.shape.reconfigure();
            }
        }
        this.highlightedEdgeStates = [];
    }

    private logError(arg: any) {
        if (this.$log) {
            this.$log.error(arg);
        }
    }

    private logInfo(arg: any) {
        if (this.$log) {
            this.$log.info(arg);
        }
    }
}