module Storyteller {
    export interface ISelectionListener {
        (elements: Array<IProcessShape>): void;
    }
    export class IconRackHelper {
        private isSingleSelection = true;
        private graph: MxGraph;

        public infoIconRack: Shell.IconRack;
        public iconRackListeners: Array<IIconRackListener> = [];
        public selectionListeners: Array<ISelectionListener> = [];

        constructor(graph: MxGraph, public isUserSelectionDisabled: boolean = false, public isSmb: boolean = false) {
            this.graph = graph;
        }

        public isIconRackEnabled(element: IProcessShape): boolean {
            if (this.isUserSelectionDisabled || element == null || element.id < 0) {
                return false;
            }

            return true;
        }

        public createInfoIconRack(action: any): Shell.IconRack {
            return new Shell.IconRack("/Scripts/mxClient/images/information.png", action);
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

        public disposeIconRack() {
            if (this.infoIconRack != null) {
                this.infoIconRack.destroy();
                this.infoIconRack = null;
            }
        }

        public destroy() {
            this.disposeIconRack();
            this.selectionListeners = null;
            this.iconRackListeners = null;
            this.graph.destroy();
        }

        public getLastSelectedCell() {
            let selectedCells = this.graph.getSelectionCells();
            return selectedCells[selectedCells.length - 1];
        }

        public onInfoIconRackClick = () => {
            let diagramElement = this.getDiagramElement(this.getLastSelectedCell());
            if (diagramElement != null) {
                this.iconRackListeners.forEach((listener: IIconRackListener) => listener(diagramElement));
            }
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

                this.iconRackClickBehaviour(evt);
            });
        }

        private hasUnSelectableElement(evt): boolean {
            if (this.hasInvisibleSelectedSystemTask(evt)) {
                return true;
            }

            return evt.properties.removed.filter(e => e instanceof DiagramNode).length !== evt.properties.removed.length;
        }

        private hasInvisibleSelectedSystemTask(evt): boolean {
            return evt.properties.removed.filter(e => e instanceof SystemTask).length > 0 &&
                   !evt.properties.removed.filter(e => e instanceof SystemTask)[0].callout.isVisible();
        }

        public iconRackClickBehaviour = (evt: any) => {
            this.disposeIconRack();

            let cell = this.getLastSelectedCell();
            if (cell != null) {
                if (cell.isEdge()) {
                    this.graph.clearSelection();
                    evt.consume();
                }

                let element = this.getDiagramElement(cell);
                let state = this.graph.getView().getState(cell);

                if (this.isIconRackEnabled(element) && state != null && !this.isSmb) {
                    this.infoIconRack = this.createInfoIconRack(this.onInfoIconRackClick);
                    this.infoIconRack.draw(state);
                }

                this.selectionListeners.forEach((listener: ISelectionListener) => listener([element]));
            }

            evt.consume();
        }
    }
}
