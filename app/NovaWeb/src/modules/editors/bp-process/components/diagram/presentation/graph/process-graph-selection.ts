import {IProcessGraph, IProcessShape} from "./models/";
import {ProcessEvents} from "./../../process-diagram-communication";
import {IDiagramNode} from "./models/";
import {NodeType} from "./models/";
import {SystemTask, DiagramNode, UserTask} from "./shapes/";


export class ProcessGraphSelectionHelper {
    private isSingleSelection = true;
    private mxGraph: MxGraph;
    private isProgrammaticSelectionChange: boolean = false; 

    constructor(private processGraph: IProcessGraph) {
        this.mxGraph = processGraph.getMxGraph();
    }
     
    public getDiagramElement(cell: MxCell): IProcessShape {
        if (cell) {
            if (cell.getParent()["getNodeType"]) { //for system tasks to work.
                cell = cell.getParent();
            }

            if (cell["getNode"] && cell["getNodeType"]) {
                let cellNode = <IDiagramNode>cell;
                if (cellNode.getNodeType() !== NodeType.MergingPoint) {
                    return cellNode.getNode().model;
                }
            }
        }
        return null;
    }

    private getSelectedNodes(): Array<IDiagramNode> {
        let elements = <Array<IDiagramNode>>this.mxGraph.getSelectionCells();
        if (elements) {
            elements = elements.filter(e => e instanceof DiagramNode);
        }
        return elements;
    }

    public destroy() {
        this.mxGraph = null;
    }

    public getLastSelectedCell() {
        let selectedCells = this.mxGraph.getSelectionCells();
        return selectedCells[selectedCells.length - 1];
    }

    public getInitialCellForEvent = (me: MxMouseEvent) => {
        let cell = me.getCell();
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

        if (cell instanceof SystemTask && (<SystemTask>cell).callout.isVisible() === false) {
            if (this.isSingleSelection) {
                this.mxGraph.clearSelection();
            }

            return null;
        }
        return cell;
    }

    public initSelection() {
        new mxRubberband(this.mxGraph);
        let baseIsEventIgnored = this.mxGraph.isEventIgnored;
        this.mxGraph.isEventIgnored = (evtName, me, sender) => {
            return baseIsEventIgnored.call(this.mxGraph, evtName, me, sender);
        };
        this.mxGraph.popupMenuHandler.selectOnPopup = false;
        this.mxGraph.graphHandler.getInitialCellForEvent = this.getInitialCellForEvent;
        this.mxGraph.getSelectionModel().addListener(mxEvent.CHANGE, (sender, evt) => {
            if (this.isProgrammaticSelectionChange) {
                this.isProgrammaticSelectionChange = false;
                return;
            }
            let cells = this.mxGraph.getSelectionCells();
            if (evt.properties.removed && evt.properties.removed.length > 0) {
                if (this.hasUnSelectableElement(evt)) {
                   // this.graph.clearSelection();
                }
            }

            if (cells.length > 1) {
                cells = cells.filter(cell => cell instanceof UserTask);
                this.isProgrammaticSelectionChange = true;
                this.mxGraph.clearSelection();
                this.isProgrammaticSelectionChange = true;
                this.mxGraph.getSelectionModel().addCells(cells);
            }
            
            let elements = this.getSelectedNodes();
            if (elements) {
                elements = elements.filter(e => e instanceof DiagramNode);
                // highlight edges and notify system that the subartifact 
                // selection has changed. Note: elements array can be empty.
                this.processGraph.highlightNodeEdges(elements);
                this.notifySelectionChanged(elements);
             }
        });
    }

    private notifySelectionChanged(elements: IDiagramNode[]) {
        const communication = this.processGraph.processDiagramCommunication;
        communication.action(ProcessEvents.SelectionChanged, elements);
    }

    private hasUnSelectableElement(evt): boolean {
        if (this.hasInvisibleSelectedSystemTask(evt)) {
            return true;
        }
        return evt.properties.removed.filter(e => e instanceof DiagramNode).length !== evt.properties.removed.length;
    }

    private hasInvisibleSelectedSystemTask(evt): boolean {
        //using variables as alias due to line length restrictions
        const systemTasks = evt.properties.removed.filter(e => e instanceof SystemTask);

        if (systemTasks.length > 0) {
            const isInvisible = !systemTasks[0].callout.isVisible();
            return isInvisible;
        }

        return false;
    }

}
