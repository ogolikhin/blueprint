import {IProcessGraph, IDiagramNode} from "./models/process-graph-interfaces";
import {UserTask} from "./shapes/user-task";
import {SystemTask} from "./shapes/system-task";
import {DiagramNode} from "./shapes/diagram-node";
import {ProcessEvents} from "./../../process-diagram-communication";
import {NodeType} from "./models/process-graph-constants";

export class ProcessGraphSelectionHelper {
    private isSingleSelection: boolean = true;
    private isProgrammaticSelectionChange: boolean = false; 

    constructor(private processGraph: IProcessGraph) {
    }
     
    private get mxGraph(): MxGraph {
        return this.processGraph.getMxGraph();
    }

    public destroy() {
        this.processGraph = null;
    }

    public getLastSelectedCell() {
        let selectedCells = this.mxGraph.getSelectionCells();
        return selectedCells[selectedCells.length - 1];
    }

    public getInitialCellForEvent = (mouseEvent: MxMouseEvent) => {
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

        if (cell instanceof SystemTask && !(<SystemTask>cell).callout.isVisible()) {
            if (this.isSingleSelection) {
                this.mxGraph.clearSelection();
            }

            return null;
        }

        return cell;
    }

    public initSelection(): void {
        new mxRubberband(this.mxGraph);

        const baseIsEventIgnored = this.mxGraph.isEventIgnored;
        this.mxGraph.isEventIgnored = (eventName, mouseEvent, sender): void => {
            return baseIsEventIgnored.call(this.mxGraph, eventName, mouseEvent, sender);
        };
        this.mxGraph.popupMenuHandler.selectOnPopup = false;
        this.mxGraph.graphHandler.getInitialCellForEvent = this.getInitialCellForEvent;

        this.mxGraph.getSelectionModel().addListener(mxEvent.CHANGE, (sender: any, event: any) => {
            if (this.isProgrammaticSelectionChange) {
                return;
            }
            
            let selectedNodes = this.processGraph.getSelectedNodes();

            if (event.properties.removed && event.properties.removed.length > 0) {
                if (this.hasUnSelectableElement(event)) {
                   this.mxGraph.clearSelection();
                }
            }

            if (selectedNodes) {
                if (selectedNodes.length > 1) {
                    selectedNodes = selectedNodes.filter(node => this.processGraph.canMultiSelect(node));
                    this.doProgrammaticSelectionChange(() => {
                        this.mxGraph.clearSelection();
                        this.mxGraph.getSelectionModel().addCells(selectedNodes);
                    });
                }
            
                // highlight edges and copy groups, and notify system that the subartifact 
                // selection has changed. Note: elements array can be empty.
                this.processGraph.highlightNodeEdges(selectedNodes);
                this.processGraph.highlightCopyGroups(selectedNodes);
                this.notifySelectionChanged(selectedNodes);
            }
        });
    }

    private doProgrammaticSelectionChange(changeSelection: () => void): void {
        this.isProgrammaticSelectionChange = true;
        changeSelection();
        this.isProgrammaticSelectionChange = false;
    }

    private notifySelectionChanged(nodes: IDiagramNode[]) {
        const communication = this.processGraph.processDiagramCommunication;
        communication.action(ProcessEvents.SelectionChanged, nodes);
    }

    private hasUnSelectableElement(event: any): boolean {
        if (this.hasInvisibleSelectedSystemTask(event)) {
            return true;
        }

        return event.properties.removed.filter(cell => cell instanceof DiagramNode).length !== event.properties.removed.length;
    }

    private hasInvisibleSelectedSystemTask(event: any): boolean {
        //using variables as alias due to line length restrictions
        const systemTasks = event.properties.removed.filter(cell => cell instanceof SystemTask);

        if (systemTasks.length > 0) {
            const isInvisible = !systemTasks[0].callout.isVisible();
            return isInvisible;
        }

        return false;
    }
}
