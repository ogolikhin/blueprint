import {ISelectionListener} from "./models/";
import {IProcessShape} from "./models/";
import {IDiagramNode} from "./models/";
import {NodeType} from "./models/";
import {SystemTask, DiagramNode} from "./shapes/";


export class ProcessGraphSelectionHelper {

    private isSingleSelection = true;
    private graph: MxGraph;
    private selectionListeners: Array<ISelectionListener> = [];

    constructor(graph: MxGraph) {
        this.graph = graph;
    }

    public addSelectionListener(listener: ISelectionListener) {
        if (listener != null) {
            this.selectionListeners.push(listener);
        }
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
        let elements = <Array<IDiagramNode>>this.graph.getSelectionCells();
        if (elements) {
            elements = elements.filter(e => e instanceof DiagramNode);
        }
        return elements;
    }

    public destroy() {
        this.selectionListeners = null;
        this.graph = null;
    }

    public getLastSelectedCell() {
        let selectedCells = this.graph.getSelectionCells();
        return selectedCells[selectedCells.length - 1];
    }

    public getInitialCellForEvent = (me: MxMouseEvent) => {
        let cell = me.getCell();
        let state = this.graph.getView().getState(cell);
        let style = (state != null) ? state.style : this.graph.getCellStyle(cell);

        while (cell != null && style["selectable"] === 0) {
            cell = cell.getParent();
            state = this.graph.getView().getState(cell);
            style = (state != null) ? state.style : this.graph.getCellStyle(cell);

            if (style["selectable"] !== 0) {
                break;
            }
        }

        if (cell instanceof SystemTask && (<SystemTask>cell).callout.isVisible() === false) {
            if (this.isSingleSelection) {
                this.graph.clearSelection();
            }

            return null;
        }

        return cell;
    }

    public initSelection() {
        this.graph.getSelectionModel().setSingleSelection(this.isSingleSelection);
        let baseIsEventIgnored = this.graph.isEventIgnored;
        this.graph.isEventIgnored = (evtName, me, sender) => {
            return baseIsEventIgnored.call(this.graph, evtName, me, sender);
        };
        this.graph.popupMenuHandler.selectOnPopup = false;
        this.graph.graphHandler.getInitialCellForEvent = this.getInitialCellForEvent;
        this.graph.getSelectionModel().addListener(mxEvent.CHANGE, (sender, evt) => {
            if (evt.properties.removed && evt.properties.removed.length > 0) {
                if (this.hasUnSelectableElement(evt)) {
                    this.graph.clearSelection();
                }
            }
            let elements = this.getSelectedNodes();
            if (elements) {
                elements = elements.filter(e => e instanceof DiagramNode);
                this.selectionListeners.forEach((listener: ISelectionListener) => {
                    listener(elements);
                });
            }
        });
    }

    private hasUnSelectableElement(evt): boolean {
        if (this.hasInvisibleSelectedSystemTask(evt)) {
            return true;
        }

        return evt.properties.removed.filter(e => e instanceof DiagramNode).length !== evt.properties.removed.length;
    }

    private hasInvisibleSelectedSystemTask(evt): boolean {
        return  evt.properties.removed.filter(e => e instanceof SystemTask).length > 0 &&
        !evt.properties.removed.filter(e => e instanceof SystemTask)[0].callout.isVisible();
    }

}
