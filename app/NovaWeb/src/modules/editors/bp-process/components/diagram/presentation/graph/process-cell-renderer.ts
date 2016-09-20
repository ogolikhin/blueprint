import {IDiagramNode} from "./models/";
import {NodeType} from "./models/";


export class ProcessCellRenderer extends mxCellRenderer {
   
    public installCellOverlayListeners(state, overlay, shape) {
        super.installCellOverlayListeners(state, overlay, shape);

        var graph = state.view.graph;
        mxEvent.addGestureListeners(shape.node, (evt) => {
            // set a flag on the event object to show the node popup menu
            // if the source of this mouse click is an overlay on
            // an edge or an overlay on a user decision shape or an
            // overlay on a system decision shape

            var diagramNode = <IDiagramNode>state.cell;
            if (state.cell.edge === true || this.isUserDecision(diagramNode) || this.isSystemDecision(diagramNode)) {
                 evt["InsertNodeIcon"] = true;
            }

            graph.fireMouseEvent(mxEvent.MOUSE_DOWN, new mxMouseEvent(evt, state));
            
        });
    }

    private isUserDecision(diagramNode: IDiagramNode): boolean {
        let uc: boolean = false;
        if (diagramNode && diagramNode.getNodeType) {
            uc = diagramNode.getNodeType() === NodeType.UserDecision;
        } 
        return uc;     
    }

    private isSystemDecision(diagramNode: IDiagramNode): boolean {
        let sc: boolean = false;
        if (diagramNode && diagramNode.getNodeType) {
            sc = diagramNode.getNodeType() === NodeType.SystemDecision;
        }
        return sc;
    }
}