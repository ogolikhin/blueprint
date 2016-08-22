import {IDiagramNode} from "./models/";
import {NodeType} from "./models/";


export class ProcessCellRenderer extends mxCellRenderer {
   
    public installCellOverlayListeners(state, overlay, shape) {
        super.installCellOverlayListeners(state, overlay, shape);

        var graph = state.view.graph;
        mxEvent.addGestureListeners(shape.node, function (evt) {
            // set a flag on the event object to show the 'Add Task/Decision' 
            // pop-up menu if the source of this mouse click is an overlay on
            // an edge

            var diagramNode = <IDiagramNode>state.cell;
            if (state.cell.edge === true ||
                (diagramNode &&
                    diagramNode.getNodeType &&
                    (diagramNode.getNodeType() === NodeType.UserDecision ||
                        diagramNode.getNodeType() === NodeType.SystemDecision))) {
                evt["InsertNodeIcon"] = true;
            }

            graph.fireMouseEvent(mxEvent.MOUSE_DOWN, new mxMouseEvent(evt, state));
        });
    }
}