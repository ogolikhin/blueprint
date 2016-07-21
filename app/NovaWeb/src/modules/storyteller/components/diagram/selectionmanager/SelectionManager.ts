module Storyteller {

    export interface ISelectionManager
    {
        setSelectedNodes(value: Array<IDiagramNode>);
        getSelectedNodes(): Array<IDiagramNode>;

        highlightNodeEdges(value: Array<IDiagramElement>, graph: ProcessGraph);

        updateUtilityPanel(value: Array<IDiagramNode>, graph: ProcessGraph, utilityPanel: Shell.IPropertiesMw);

        clearHighlightEdges();

        onSelectionChanged: (elements: Array<IDiagramNode>) => void;
        destroy();
    }

    export class SelectionManager implements ISelectionManager {
        public static $inject = ["$rootScope"];

        private highlightedEdgeStates: any[] = [];

        private selectedNodes: Array<IDiagramNode>;

        constructor(private $rootScope) {
            this.selectedNodes = [];
        }
        

        public setSelectedNodes(value: Array<IDiagramNode>) {

            // use the event bus to notify subscribers that
            // the selection has changed

            this.selectedNodes = value;
            if (this.$rootScope.publish) {
                this.$rootScope.publish("SelectionManager:SelectionChanged", this.selectedNodes);
            }
        }
        public highlightNodeEdges(nodes: Array<IDiagramNode>, graph: ProcessGraph) {
            this.clearHighlightEdges();
            if (nodes.length > 0) {
                let selectedNode: IDiagramNode = nodes[0];

                let highLightEdges = this.getHighlightScope(selectedNode, graph.graph.getModel());
                for (let edge of highLightEdges) {
                    this.highlightEdge(edge, graph);
                }
                graph.graph.orderCells(false, highLightEdges);
            }
        }
        private getHighlightScope(diagramNode: IDiagramNode, graphModel: MxGraphModel) : MxCell[] {

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

        private highlightEdge(edge: MxCell, graph: ProcessGraph) {
            let state: any = graph.graph.getView().getState(edge);
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
        public getSelectedNodes() {
            return this.selectedNodes;
        }
        public updateUtilityPanel(value: Array<IDiagramNode>, graph: ProcessGraph, utilityPanel: Shell.IPropertiesMw) {
            if (utilityPanel != null) {
                if (utilityPanel.isModalDialogOpen() && value.length > 0) {
                    let element: IProcessShape = value[0].model;
                    if (graph.iconRackHelper.isIconRackEnabled(element)) {
                        utilityPanel.openModalDialogWithProcessShape(element);
                    } else {
                        utilityPanel.closeModalDialog();
                    }
                }
            }
        }
        public destroy() {
            if (this.selectedNodes)
                this.selectedNodes.length = 0;
        }

        public onSelectionChanged(elements: IDiagramNode[]): void {}
    }

    var app = angular.module("Storyteller");
    app.service("selectionManager", SelectionManager);
}
