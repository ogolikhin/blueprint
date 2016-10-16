import {ProcessGraph} from "./process-graph";
import {NodeType, IDiagramNodeElement} from "./models/";

export interface IDragDropHandler {
    moveCell: MxCell;
    createDragPreview();
    reset();
    isValidDropSource(dropSource: MxCell);
    highlightDropTarget(me);
    dispose();
};

export class DragDropHandler implements IDragDropHandler {

    public graph: MxGraph = null;
    private PREVIEW_WIDTH = 60;
    private PREVIEW_HEIGHT = 75;
    private DROP_TARGET_COLOR = "#3074b6";
    public moveCell: MxCell = null;
    private cell: MxCell = null;
    private currentState = null;
    private saveFill = null;
    private dragPreview: HTMLDivElement = null;
    private layout = null;
    private previousFill = null;

    constructor(private processGraph: ProcessGraph) {
        this.init();
    }

    private init() {
        this.graph = this.processGraph.getMxGraph();
        this.layout = this.processGraph.layout;
        // Disable drag and drop if process is read-only
        if (!this.processGraph.viewModel.isReadonly) {
            this.installMouseDragDropListener();
        }
    }

    public reset() {
        this.clearHighlight();
        this.cell = null;
        this.moveCell = null;
        this.currentState = null;
        this.saveFill = null;
        if (this.dragPreview) {
            this.graph.container.removeChild(this.dragPreview);
        }
        this.dragPreview = null;
    }

    public isValidDropSource(dropSource: MxCell) {
        // check if valid drop source - only user tasks can be dropped 
        let isDropSource: boolean = false;
        if (dropSource && dropSource.isVertex) {
            let diagramNodeElement = <IDiagramNodeElement>dropSource;
            if (diagramNodeElement && diagramNodeElement.getNode) {
                if (diagramNodeElement.getNode().getNodeType() === NodeType.UserTask) {
                    isDropSource = true;
                }
            }
        }
        return isDropSource;
    }

    public highlightDropTarget(me) {
        let state = this.layout.getDropEdgeState(this.getConvertedPoint(me));
        if (state != null) {
            if (!state.cell.edge) {
                return;
            }
            let cellId = Number((<IDiagramNodeElement>this.moveCell).getNode().getId());
            if (!this.layout.isValidForDrop(cellId, state.cell)) {
                if (this.currentState) {
                    this.currentState.shape.stroke = this.previousFill;
                    this.currentState.shape.reconfigure();
                    this.currentState = null;
                }
                this.updateDragElt(false);
                return;
            }
            if (this.currentState == null) {
                this.currentState = state;
                this.previousFill = state.shape.stroke;
                state.shape.stroke = this.DROP_TARGET_COLOR;
                this.updateDragElt(true);
                state.shape.reconfigure();
            } else if (this.currentState !== state) {
                this.currentState.shape.stroke = this.previousFill;
                this.currentState.shape.reconfigure();
                this.currentState = state;
                this.previousFill = state.shape.stroke;
                state.shape.stroke = this.DROP_TARGET_COLOR;
                this.updateDragElt(true);
                state.shape.reconfigure();
            }
        } else if (this.currentState != null) {
            let pt = mxUtils.convertPoint(this.graph.container, me.getX(), me.getY());
            if (this.graph.getCellAt(pt.x, pt.y) !== this.currentState.cell) {
                this.currentState.shape.stroke = this.previousFill;
                this.currentState.shape.reconfigure();
                this.currentState = null;
                this.updateDragElt(false);
            }
        }
    }

    private clearHighlight() {
        if (this.currentState) {
            this.currentState.shape.stroke = this.saveFill;
            this.currentState.shape.reconfigure();
        }
    }

    public createDragPreview() {
        // Creates the element that is used for the drag preview.
        this.dragPreview = document.createElement("div");
        this.dragPreview.style.width = this.PREVIEW_WIDTH + "px";
        this.dragPreview.style.height = this.PREVIEW_HEIGHT + "px";
        this.dragPreview.style.position = "absolute";
        this.dragPreview.style.background = "url('/novaweb/static/bp-process/images/draggable-icon-gray.png') no-repeat center center";
        this.dragPreview.style.borderStyle = "dashed";
        this.dragPreview.style.borderWidth = "thin";
        this.graph.container.appendChild(this.dragPreview);
        return this.dragPreview;
    }

    private updateDragElt(isActive: boolean) {
        if (isActive) {
            this.dragPreview.style.background = "url('/novaweb/static/bp-process/images/draggable-icon-blue.png') no-repeat center center";
        } else {
            this.dragPreview.style.background = "url('/novaweb/static/bp-process/images/draggable-icon-gray.png') no-repeat center center";
        }
    }

    private getConvertedPoint(me) {
        let pt = mxUtils.convertPoint(this.graph.container, me.getX(), me.getY());
        pt.x = pt.x + this.PREVIEW_WIDTH / 2;
        pt.y = pt.y + this.PREVIEW_HEIGHT / 2;
        return pt;
    }

    private showDragPreview(me) {
        let pt = mxUtils.convertPoint(this.graph.container, me.getX(), me.getY());
        let offset = mxUtils.getDocumentScrollOrigin(document);

        this.dragPreview.style.left = (pt.x + offset.x) + "px";
        this.dragPreview.style.top = (pt.y + offset.y) + "px";
    }

    private installMouseDragDropListener() {
        let h = this;
        this.graph.addMouseListener({
            mouseDown: function (sender, me) {
                h.cell = me.getCell();
                if (h.cell && h.cell !== h.moveCell) {
                    if (h.isValidDropSource(h.cell)) {
                        // start drag
                        h.moveCell = h.cell;
                    }
                }
                h.cell = null;
            },
            mouseMove: function (sender, me) {
                if (h.moveCell && h.graph.isMouseDown) {
                    // dragging
                    if (h.dragPreview == null) {
                        h.createDragPreview();
                    }
                    h.showDragPreview(me);
                    h.highlightDropTarget(me);
                }
                h.cell = null;
            },
            mouseUp: function (sender, me) {
                if (h.moveCell) {
                    let node = (<IDiagramNodeElement>h.moveCell).getNode();
                    if (h.currentState && h.currentState.cell.isEdge()) {
                        // drop
                        let edge = h.currentState.cell;
                        let cellId = Number(node.getId());
                            
                        // reset drag state
                        h.reset();

                        h.layout.handleUserTaskDragDrop(cellId, edge);
                    }
                    else {
                        // reset drag state
                        h.reset();
                    }
                }
            }
        });
    };

    public dispose() {
        this.moveCell = null; 
    }
}
