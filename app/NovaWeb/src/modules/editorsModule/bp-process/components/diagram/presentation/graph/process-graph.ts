/* tslint:disable max-file-line-count */
import {ILoadingOverlayService} from "../../../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {IProcessGraph, ILayout, INotifyModelChanged, IConditionContext} from "./models/";
import {ICondition, IScopeContext, IStopTraversalCondition, IUserStory} from "./models/";
import {IUserTask, INextIdsProvider, IOverlayHandler, IShapeInformation} from "./models/";
import {IDiagramNode, IDiagramNodeElement, IProcessShape, IProcessLink} from "./models/";
import {SourcesAndDestinations, ProcessShapeType, NodeType, NodeChange} from "./models/";
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
import {ProcessCopyPasteHelper} from "./process-copy-paste-helper";
import {IDialogService} from "../../../../../../shared";
import {NodePopupMenu} from "./popup-menu/node-popup-menu";
import {ProcessGraphSelectionHelper} from "./process-graph-selection";
import {IStatefulArtifactFactory} from "../../../../../../managers/artifact-manager";
import {ProcessEvents, IProcessDiagramCommunication} from "../../process-diagram-communication";
import {IDragDropHandler, DragDropHandler} from "./drag-drop-handler";
import {ILocalizationService} from "../../../../../../commonModule/localization/localization.service";
import {IClipboardService} from "../../../../services/clipboard.svc";
import {IFileUploadService} from "../../../../../../commonModule/fileUpload/fileUpload.service";
import {IMessageService} from "../../../../../../main/components/messages/message.svc";
import {SystemTask} from "./shapes/system-task";
import {ItemTypePredefined} from "../../../../../../main/models/enums";
import {IArtifact} from "../../../../../../main/models/models";
import {ConfirmPublishController, IConfirmPublishDialogData} from "../../../../../../main/components/dialogs/bp-confirm-publish/bp-confirm-publish";
import {IDialogSettings} from "../../../../../../shared/widgets/bp-dialog/bp-dialog";

export class ProcessGraph implements IProcessGraph {
    public layout: ILayout;
    public startNode: IDiagramNode;
    public endNode: IDiagramNode;
    public nodeLabelEditor: NodeLabelEditor;
    public globalScope: IScopeContext;
    public dragDropHandler: IDragDropHandler;
    public systemTaskErrorPresented: number = 0;
    private mxgraph: MxGraph;
    private isIe11: boolean;
    private selectionHelper: ProcessGraphSelectionHelper = null;
    private executionEnvironmentDetector: any;
    private transitionTimeOut: number = 400;
    private highlightedEdgeStates: any[] = [];
    private highlightedCopyNodes: IDiagramNode[] = [];
    private deleteShapeHandler: string;
    private popupMenu: NodePopupMenu = null;
    private processCopyPasteHelper: ProcessCopyPasteHelper;
    private selectionChangedHandler: string = null;
    private minNoOfShapesAddedPerSystemDecision: number = 1;
    private minNoOfShapesAddedPerUserDecision: number = 2;
    private invalidShapes: number[] = [];

    public get processDiagramCommunication(): IProcessDiagramCommunication {
        return this.viewModel.communicationManager.processDiagramCommunication;
    }

    public static get MinConditions(): number {
        return 2;
    }

    public static get MaxConditions(): number {
        return 10;
    }

    public get isUserSystemProcess(): boolean {
        return this.viewModel.isUserToSystemProcess;
    }

    constructor(public rootScope: any,
                private scope: any,
                private htmlElement: HTMLElement,
                public viewModel: IProcessViewModel,
                private dialogService: IDialogService,
                private localization: ILocalizationService,
                private shapesFactory: ShapesFactory,
                public messageService: IMessageService = null,
                private $log: ng.ILogService = null,
                private statefulArtifactFactory: IStatefulArtifactFactory = null,
                private clipboard: IClipboardService = null,
                private fileUploadService: IFileUploadService = null,
                private $q: ng.IQService = null,
                private loadingOverlayService: ILoadingOverlayService = null) {
        // Creates the graph inside the given container
        // This is temporary code. It will be replaced with
        // a class that wraps this global functionality.
        //fixme: angular provides a service to access window
        let w: any = window;
        this.executionEnvironmentDetector = new w.executionEnvironmentDetector();
        this.mxgraph = new mxGraph(this.htmlElement, new BpMxGraphModel());
        this.layout = new Layout(this, viewModel, rootScope, this.shapesFactory, this.messageService, this.$log);
        this.init();
    }

    private init() {
        this.setIsIe11();
        this.initializeGraphContainer();
        //fixme: dont use event listenters where you can use ng-click and other such events
        window.addEventListener("buttonUpdated", this.buttonUpdated, true);
        // non movable
        this.mxgraph.setCellsMovable(false);
        this.mxgraph.isCellSelectable = this.isCellSelectable;
        this.mxgraph.foldingEnabled = false;
        ConnectorStyles.createStyles();
        NodeShapes.register(this.mxgraph);
        this.addMouseEventListener(this.mxgraph);
        //Selection logic
        this.createSelectionListeners();

        this.applyDefaultStyles();
        this.applyReadOnlyStyles();
        this.initializePopupMenu();

        if (!this.viewModel.isReadonly) {
            this.dragDropHandler = new DragDropHandler(this);
        }

        this.nodeLabelEditor = new NodeLabelEditor(this.htmlElement);
        this.initializeGlobalScope();
        this.processCopyPasteHelper = new ProcessCopyPasteHelper(
            this, this.clipboard, this.shapesFactory, this.messageService, this.$log, this.fileUploadService, this.$q, this.loadingOverlayService,
            this.localization);
    }

    private getElementByClassName (el: HTMLElement, cls: string): HTMLElement {
        while ((el = el.parentElement) && !el.classList.contains(cls)) {
            // nothing here
        }
        return el;
    }

    private isCellSelectable = (cell: MxCell) => {
        if (cell instanceof DiagramNode) {
            return cell.isVertex();
        }

        return false;
    }

    private createSelectionListeners() {
        this.selectionHelper = new ProcessGraphSelectionHelper(this);
        this.selectionHelper.initSelection();
    }

    public clearSelection(): void {
        this.mxgraph.clearSelection();
    }

    private initializePopupMenu() {
        // initialize a popup menu for the graph
        this.popupMenu = new NodePopupMenu(this.layout, this.shapesFactory, this.localization, this.clipboard, this.htmlElement, this.mxgraph,
            ProcessAddHelper.insertTaskWithUpdate, ProcessAddHelper.insertUserDecision, ProcessAddHelper.insertUserDecisionConditionWithUpdate,
            ProcessAddHelper.insertSystemDecision, ProcessAddHelper.insertSystemDecisionConditionWithUpdate, this.insertSelectedShapes);
    }

    public copySelectedShapes(): void {
        this.processCopyPasteHelper.copySelectedShapes();
    }

    public insertSelectedShapes = (edge: MxCell): void => {
        const sourcesAndDestinations = this.layout.getSourcesAndDestinations(edge);
        const sourceIds = sourcesAndDestinations.sourceIds;
        const destinationId = sourcesAndDestinations.destinationIds[0];
        this.processCopyPasteHelper.insertSelectedShapes(sourceIds, destinationId);
    };

    public render(useAutolayout, selectedNodeId): void {
        try {
            // uses layout object to draw a new diagram for process model
            this.layout.render(useAutolayout, selectedNodeId);
            if (this.nodeLabelEditor != null) {
                this.nodeLabelEditor.init();
            }

            this.deleteShapeHandler = this.viewModel.communicationManager
                .processDiagramCommunication.register(ProcessEvents.DeleteShape, this.deleteShape);
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
        } finally {
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

    private getDecisionConditionInsertMethod(decisionId: number): (
        decisionId: number, layout: ILayout, shapesFactoryService: ShapesFactory,
        label?: string, conditionDestinationId?: number) => number {
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
            this.viewModel.isWithinShapeLimit(conditions.length * this.minNoOfShapesAddedPerSystemDecision) === false) {
            canAdd = false;
        } else if (shapeType === ProcessShapeType.UserDecision &&
            this.viewModel.isWithinShapeLimit(conditions.length * this.minNoOfShapesAddedPerUserDecision) === false) {
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
        const cellId = event.detail.id;
        const overlayHandler: IOverlayHandler = this.mxgraph.getModel().getCell(cellId);

        if (overlayHandler != null) {
            overlayHandler.updateOverlay(this.mxgraph);
        }
    };

    private initializeGraphContainer() {
        mxEvent.disableContextMenu(this.htmlElement);

        // Remove  unneeded scrollbar
        const parentElem = this.getElementByClassName(this.htmlElement, "page-body-wrapper");
        if (!!parentElem) {
            parentElem.style.overflow = "hidden";
        }

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
        let xPosition = 0;
        let yPosition = 0;

        while (element) {
            xPosition += (element.offsetLeft);
            yPosition += (element.offsetTop);
            element = element.offsetParent;
        }

        return {x: xPosition, y: yPosition};
    }

    private getMinHeight(): string {
        const shift = this.getPosition(this.htmlElement).y;
        const height = window.innerHeight - shift;
        return `${height}px`;
    }

    private getMinWidth(): string {
        const width = this.htmlElement.parentElement.offsetWidth;
        return `${width}px`;
    }

    private resizeWrapper = (event) => {
        this.updateSizeChanges();
    };

    private setContainerSize(width: number, height: number = 0) {
        const minHeight = height === 0 ? this.getMinHeight() : `${height}px`;
        const minWidth = width === 0 ? this.getMinWidth() : `${width}px`;

        if (width === 0) {
            this.htmlElement.style.transition = "";
        } else {
            this.htmlElement.style.transition = `width ${this.transitionTimeOut}ms`;
            setTimeout(this.fireUIEvent, this.transitionTimeOut, "resize");
        }

        this.htmlElement.style.height = minHeight;
        this.htmlElement.style.width = minWidth;
    }

    public updateSizeChanges(width: number = 0, height: number = 0): void {
        this.setContainerSize(width, height);
        // This prevents some weird issue with the graph growing as we drag off the container edge.
        const svgElement = angular.element(this.htmlElement.children[0])[0];
        const containerElement: any = angular.element(this.htmlElement)[0];

        if (svgElement && containerElement) {
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
    }

    public updateAfterRender(): void {
        if (this.viewModel.isSpa) {
            this.updateSizeChanges();
        }
    }

    private fireUIEvent(eventName: string) {
        const evt: UIEvent = document.createEvent("UIEvents");
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

    private addMouseEventListener(graph: MxGraph) {
        graph.addMouseListener(
            {
                currentState: null,
                previousStyle: null,
                mouseDown: function (sender, me) {
                    let cell = graph.getCellAt(me.graphX, me.graphY);
                    this.currentState = sender.view.getState(cell);

                    if (this.currentState != null) {
                        this.onMouseLeave(sender, me.getEvent(), this.currentState);
                        this.onMouseDown(sender, me.getEvent(), this.currentState);
                    }
                },
                mouseMove: function (sender, me) {
                    if (this.currentState != null && me.getState() === this.currentState) {
                        return;
                    }

                    let cell = graph.getCellAt(me.graphX, me.graphY);
                    let tmp = sender.view.getState(cell);

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
                mouseUp: function (sender, me) {
                //fixme: this is empty and thus should be removed or return undefined
                },
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
                            // Set vertices z-order on top in case some of them are overlaped by edges
                            graph.orderCells(false, graph.getChildVertices(graph.getDefaultParent()));
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

    public getDefaultParent(): MxCell {
        return this.mxgraph.getDefaultParent();
    }

    public addCell<T extends IProcessShape>(node: DiagramNode<T>, parent) {
        return this.mxgraph.addCell(node, parent);
    }

    public addLink(link: DiagramLink, parent, index?: number, source?: MxCell, target?: MxCell): MxCell {
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

    public onValidation(invalidShapes: number[]): void {
        let systemTaskErrorPresented: number = 0;

        for (const shape of _.difference(invalidShapes, this.invalidShapes)) {
            const task = <IDiagramNode>this.getNodeById((shape).toString());
            if (task) {
                task.isValid = false;

                if (task instanceof SystemTask) {
                    systemTaskErrorPresented++;
                }
            }
        }

        for (const shape of _.difference(this.invalidShapes, invalidShapes)) {
            const task = <IDiagramNode>this.getNodeById((shape).toString());
            if (task) {
                task.isValid = true;

                if (task instanceof SystemTask) {
                    systemTaskErrorPresented--;
                }
            }
        }

        this.invalidShapes = invalidShapes;
        this.systemTaskErrorPresented = this.systemTaskErrorPresented + systemTaskErrorPresented;
    }

    public onUserStoriesGenerated(userStories: IUserStory[]): void {
        for (const userStory of userStories) {
            const userTask = <IUserTask>this.getNodeById(userStory.processTaskId.toString());

            if (userTask) {
                userTask.userStoryId = userStory.id;
            }
        }
    }

    private deleteShape = () => {
        const selectedNodes = this.getSelectedNodes();
        const artifactList = this.createArtifactFromDiagramNodes(selectedNodes);

        this.dialogService.open(<IDialogSettings>{
                okButton: this.localization.get("App_Button_Ok"),
                cancelButton: this.localization.get("App_Button_Cancel"),
                message: this.localization.get("ST_Bulk_Delete_Confirmation"),
                template: require("../../../../../../main/components/dialogs/bp-confirm-publish/bp-confirm-publish.html"),
                controller: ConfirmPublishController,
                css: "modal-alert nova-publish",
                header: this.localization.get("App_DialogTitle_Alert")
            },
            <IConfirmPublishDialogData>{
                artifactList: artifactList,
                projectList: null,
                selectedProject: null
            })
            .then(() => {
                if (selectedNodes.length > 1) {
                    const nodeIds = selectedNodes.map(node => node.model.id);
                    ProcessDeleteHelper.deleteUserTasks(nodeIds, this);
                } else {
                    if (selectedNodes[0].getNodeType() === NodeType.UserTask) {
                        ProcessDeleteHelper.deleteUserTask(selectedNodes[0].model.id, (nodeChange, id) => this.notifyUpdateInModel(nodeChange, id), this);
                    } else if (selectedNodes[0].getNodeType() === NodeType.UserDecision || selectedNodes[0].getNodeType() === NodeType.SystemDecision) {
                        ProcessDeleteHelper.deleteDecision(selectedNodes[0].model.id,
                            (nodeChange, id) => this.notifyUpdateInModel(nodeChange, id), this, this.shapesFactory);
                    }
                }
            });
    };

    private createArtifactFromDiagramNodes(nodeList: IDiagramNode[]): IArtifact[] {
        const artifactList: IArtifact[] = [];

        nodeList.forEach((node: IDiagramNode) => {
            const artifact = {
                id: Number(node.getId()),
                name: node.label,
                predefinedType: ItemTypePredefined.Process,
                itemTypeId: ItemTypePredefined.Process,
                projectId: null,
                prefix: "PRO"
            } as IArtifact;
            artifactList.push(artifact);
        });

        return artifactList;
    }

    private hasMaxConditions(decisionId: number): boolean {
        return this.viewModel.getNextShapeIds(decisionId).length >= ProcessGraph.MaxConditions;
    }

    public updateSourcesWithDestinations(shapeId: number, newDestinationId: number): SourcesAndDestinations {
        let sources = this.viewModel.getPrevShapeIds(shapeId);

        if (sources.length > 1) {
            this.updateBranchDestinationId(shapeId, newDestinationId);
        }

        let originalShapeSourcesAndDestinations: SourcesAndDestinations = {sourceIds: [], destinationIds: []};

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

    public setSystemTasksVisible(value: boolean): void {
        const cells = this.mxgraph.getChildVertices(this.mxgraph.getDefaultParent());
        const edges = this.mxgraph.getChildEdges(this.mxgraph.getDefaultParent());
        const graphModel: MxGraphModel = this.mxgraph.getModel();

        this.logInfo("Enter setSystemTasksVisible, value = " + value);
        this.popupMenu.hidePopupMenu();

        graphModel.beginUpdate();

        try {
            for (let j: number = 0; j < edges.length; j++) {
                const edge: MxCell = edges[j];

                if (edge && edge.target) {
                    const sourceNode = (<IDiagramNodeElement>edge.source).getNode();
                    const targetNode = (<IDiagramNodeElement>edge.target).getNode();
                    if (sourceNode
                        && sourceNode.getNodeType() !== NodeType.ProcessStart
                        && targetNode
                        && this.viewModel.isReadonly === false
                        && (targetNode.getNodeType() === NodeType.SystemTask || targetNode.getNodeType() === NodeType.SystemDecision)) {
                        if (value) {
                            (<DiagramLink>edge).showMenu(this.mxgraph);
                        }
                    }
                }
            }

            for (let i: number = 0; i < cells.length; i++) {
                const cell = cells[i];

                if (cell.getNodeType) {
                    this.logInfo("cell.getNodeType() = " + cell.getNodeType());

                    if (cell.getNodeType() === NodeType.SystemTask) {
                        this.logInfo("Call cell.setCellVisible, value = " + value);
                        cell.setCellVisible(this.mxgraph, value);

                        if (this.invalidShapes.indexOf(parseInt(cell.id, 10)) > -1) {

                            //this code show and hide system tasks alert when change view mode for process
                            if (!value) {
                                cell.isValid = true;
                            } else {
                                if (cell.isValid) {
                                    cell.isValid = false;
                                }
                            }
                        }
                    }

                    if (cell.getNodeType() === NodeType.SystemDecision && !this.viewModel.isReadonly) {
                        if (value) {
                            (<SystemDecision>cell).showMenu(this.mxgraph);
                        }
                    }
                }
            }
        } catch (e) {
            this.logError("setSystemTasksVisible, error = " + e.message);
        } finally {
            graphModel.endUpdate();
        }
    }

    private setIsIe11(): void {
        const myBrowser = this.executionEnvironmentDetector.getBrowserInfo();
        const ver = parseInt(myBrowser.version, 10);
        this.isIe11 = (myBrowser.msie && (ver === 11));
    }

    public getSelectedNodes(): IDiagramNode[] {
        let cells = this.mxgraph.getSelectionCells();
        if (cells) {
            return <IDiagramNode[]>cells.filter(cell => cell instanceof DiagramNode);
        }

        return [];
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
        }

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
        return this.viewModel.getSortedNextLinks(sourceId);
    }

    private getDecisionBranchDestinationLinks(decisionId: number): IProcessLink[] {
        return this.viewModel.getDecisionBranchDestinationLinks((link) => link.sourceId === decisionId);
    }

    public getDecisionBranchDestLinkForIndex(decisionId: number, orderIndex: number): IProcessLink {
        const links = this.getDecisionBranchDestinationLinks(decisionId).filter(a => a.orderindex === orderIndex);

        if (links.length > 0) {
            return links[0];
        }

        return null;
    }

    public getShapeById(id: number): IProcessShape {
        return this.viewModel.getShapeById(id);
    }

    public getMergeNode(decisionId: number, orderIndex: number): IDiagramNode {
        const branchLink = this.getDecisionBranchDestLinkForIndex(decisionId, orderIndex);
        const destinationId = branchLink.destinationId;
        return this.getNodeById(destinationId.toString());
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

    public updateMergeNode(decisionId: number, condition: IProcessLink, mergeNodeId: number): boolean {

        let originalEndNode: IProcessLink = this.getDecisionBranchDestLinkForIndex(decisionId, condition.orderindex);

        if (!!mergeNodeId &&
            originalEndNode &&
            originalEndNode.destinationId !== mergeNodeId) {
            let mainBranchOnly: INextIdsProvider = (context) => {
                let ids = this.viewModel.getNextShapeIds(context.id);
                return [Number(ids[0])];
            };
            const scope = this.getBranchScope(condition, mainBranchOnly);
            let lastShapeId = scope.mappings[0].endId;
            let origLastLinkInCondition = this.getLink(lastShapeId, originalEndNode.destinationId);

            // Updates end branch link to point to new destination id
            if (origLastLinkInCondition) {
                origLastLinkInCondition.destinationId = mergeNodeId;
            }

            // Updates merge point for specific branch to be new destination id
            originalEndNode.destinationId = mergeNodeId;

            return true;
        }

        return false;
    }

    public getHighlightedCopyNodes(): IDiagramNode[] {
        return this.highlightedCopyNodes;
    }

    public getCopyNodes(): IDiagramNode[] {
        const copyNodes: IDiagramNode[] = this.getSelectedNodes().slice(0);

        for (const node of this.highlightedCopyNodes) {
            if (copyNodes.indexOf(node) < 0) {
                copyNodes.push(node);
            }
        }

        return copyNodes;
    }

    public highlightCopyGroups = (nodes: IDiagramNode[]): void => {
        if (!nodes) {
            throw new Error("nodes are not defined");
        }

        const copyNodes: IDiagramNode[] = nodes.filter((node: IDiagramNode) => node.canCopy);

        if (copyNodes.length === 0) {
            return;
        }

        const nodesToHighlight: IDiagramNode[] = [];

        const userTasks = <IUserTask[]>nodes.filter((node: IDiagramNode) => node.getNodeType() === NodeType.UserTask);

        if (userTasks.length > 1) {
            const commonUserDecisions = this.processCopyPasteHelper.getCommonUserDecisions(userTasks);
            nodesToHighlight.push(...commonUserDecisions);
        }

        for (const selectedNode of nodes) {
            const familyNodes = this.processCopyPasteHelper.getCopyFamilyNodes(selectedNode);
            nodesToHighlight.push(...familyNodes);
        }

        this.mxgraph.getModel().beginUpdate();

        try {
            for (const node of nodesToHighlight) {
                this.highlightNode(node);
            }
        } finally {
            this.mxgraph.getModel().endUpdate();
        }
    };

    private highlightNode(node: IDiagramNode): void {
        node.highlight(this.mxgraph);
        this.highlightedCopyNodes.push(node);
    }

    public clearCopyGroupHighlight(): void {
        this.mxgraph.getModel().beginUpdate();

        try {
            for (let node of this.highlightedCopyNodes) {
                this.clearNodeHighlight(node);
            }
        } finally {
            this.mxgraph.getModel().endUpdate();
        }

        this.highlightedCopyNodes = [];
    }

    private clearNodeHighlight(node: IDiagramNode): void {
        node.clearHighlight(this.mxgraph);
    }

    public highlightNodeEdges = (nodes: IDiagramNode[]) => {
        _.each(nodes, (node) => {
            let highLightEdges = this.getHighlightScope(node, this.mxgraph.getModel());

            for (let edge of highLightEdges) {
                this.highlightEdge(edge);
            }

            this.mxgraph.orderCells(false, highLightEdges);
        });
    };

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

    public clearHighlightEdges() {
        for (let edge of this.highlightedEdgeStates) {
            if (edge.shape) {
                edge.shape.stroke = mxConstants.DEFAULT_VALID_COLOR;
                edge.shape.reconfigure();
            }
        }

        this.highlightedEdgeStates = [];
    }

    public highlightBridges() {
        this.layout.bridgesHandler.highlightBridges();
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

    public isFirstFlow(link: IProcessLink): boolean {
        return this.viewModel.isFirstFlow(link);
    }

    public isInNestedFlow(id: number): boolean {
        return this.viewModel.isInNestedFlow(id);
    }

    public isInMainFlow(id: number): boolean {
        return this.viewModel.isInMainFlow(id);
    }

    public getBranchStartingLink(link: IProcessLink): IProcessLink {

        if (this.viewModel.isFirstFlow(link) && !this.viewModel.isInMainFlow(link.sourceId)) {
            const shapeContext = this.globalScope.visitedIds[link.sourceId];
            const last = _.last(shapeContext.parentConditions);
            return this.getNextLinks(last.decisionId).filter(a => a.orderindex === last.orderindex)[0];

        }

        return this.getLink(link.sourceId, link.destinationId);
    }

    public destroy(): void {
        if (this.viewModel.isSpa) {
            window.removeEventListener("resize", this.resizeWrapper, true);
        }

        window.removeEventListener("buttonUpdated", this.buttonUpdated);

        // remove graph
        this.mxgraph.getModel().clear();
        this.mxgraph.destroy();

        while (this.htmlElement.hasChildNodes()) {
            this.htmlElement.removeChild(this.htmlElement.firstChild);
        }

        // Dispose handlers
        this.processDiagramCommunication.unregister(ProcessEvents.DeleteShape, this.deleteShapeHandler);

        if (this.dragDropHandler != null) {
            this.dragDropHandler.dispose();
        }

        if (this.nodeLabelEditor != null) {
            this.nodeLabelEditor.dispose();
        }

        if (this.selectionHelper) {
            this.selectionHelper.destroy();
        }
    }
}
