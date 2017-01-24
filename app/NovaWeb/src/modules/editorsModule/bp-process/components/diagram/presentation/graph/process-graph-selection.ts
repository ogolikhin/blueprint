import {Node} from "./../../../../../bp-diagram/impl/usecase/layout/flow-graph-objects";
import {IProcessGraph, IDiagramNode} from "./models/process-graph-interfaces";
import {UserTask} from "./shapes/user-task";
import {SystemTask} from "./shapes/system-task";
import {DiagramNode} from "./shapes/diagram-node";
import {ProcessEvents} from "./../../process-diagram-communication";
import {NodeType} from "./models/process-graph-constants";

export class ProcessGraphSelectionHelper {
    private isProgrammaticSelectionChange: boolean = false; 

    constructor(private processGraph: IProcessGraph) {
    }

    public initSelection(): void {
        new mxRubberband(this.mxGraph);

        this.mxGraph.popupMenuHandler.selectOnPopup = false;
        this.mxGraph.graphHandler.getInitialCellForEvent = this.getSelectableCell;

        this.mxGraph.getSelectionModel().addListener(mxEvent.CHANGE, (sender: any, event: any) => {
            if (this.isProgrammaticSelectionChange) {
                return;
            }
            
            let selectedNodes = this.processGraph.getSelectedNodes();

            if (selectedNodes) {
                this.processGraph.clearCopyGroupHighlight();
                this.processGraph.clearHighlightEdges();

                if (selectedNodes.length > 0) {
                    if (selectedNodes.length > 1) {
                        selectedNodes = selectedNodes.filter(node => this.canMultiSelect(node));
                    } else {
                        selectedNodes = selectedNodes.filter(node => this.canSingleSelect(node));
                    }

                    // fixme: refactor as this might be triggered if the selectedNodes collection doesn't change
                    this.doProgrammaticSelectionChange(() => {
                        this.mxGraph.clearSelection();

                        if (selectedNodes.length > 0) {
                            this.mxGraph.getSelectionModel().addCells(selectedNodes);
                        }
                    });

                    if (selectedNodes.length > 0) {
                        // highlight edges and copy groups, and notify system that the subartifact 
                        // selection has changed. Note: elements array can be empty.
                        this.processGraph.highlightNodeEdges(selectedNodes);
                        this.processGraph.highlightCopyGroups(selectedNodes);
                    }
                }

                this.notifySelectionChanged(selectedNodes);
                this.processGraph.highlightBridges();            
            }
        });
    }

    public destroy() {
        this.processGraph = null;
    }

    private get mxGraph(): MxGraph {
        return this.processGraph.getMxGraph();
    }

    private getSelectableCell = (mouseEvent: MxMouseEvent) => {
        let cell = mouseEvent.getCell();
        let state = this.mxGraph.getView().getState(cell);
        let style = (state != null) ? state.style : this.mxGraph.getCellStyle(cell);

        while (cell != null && style["selectable"] === 0) {
            cell = cell.getParent();
            state = this.mxGraph.getView().getState(cell);
            style = (state != null) ? state.style : this.mxGraph.getCellStyle(cell);

            if (style["selectable"] !== 0) {
                break;
            }
        }

        return cell;
    }

    private canSingleSelect(node: IDiagramNode): boolean {
        switch (node.getNodeType()) {
            case NodeType.MergingPoint:
                return false;

            case NodeType.SystemTask:
                return (<SystemTask>node).callout.isVisible();

            default:
                return true;
        };
    }

    private canMultiSelect(node: IDiagramNode): boolean {
        switch (node.getNodeType()) {
            case NodeType.UserTask:
                return true;

            default:
                return false;
        };
    }

    private doProgrammaticSelectionChange(changeSelection: () => void): void {
        this.isProgrammaticSelectionChange = true;
        
        try {
            changeSelection();
        } finally {
            this.isProgrammaticSelectionChange = false;
        }
    }

    private notifySelectionChanged(nodes: IDiagramNode[]) {
        const communication = this.processGraph.processDiagramCommunication;
        communication.action(ProcessEvents.SelectionChanged, nodes);
    }
}
