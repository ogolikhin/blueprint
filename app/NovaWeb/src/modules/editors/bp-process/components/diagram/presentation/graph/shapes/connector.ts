import {IProcessLinkModel} from "../../../../../models/process-models";
import {IProcessGraph, IDiagramNode} from "../models/";
import {NodeType} from "../models/";
import {CS_VERTICAL, CS_RIGHT, CS_LEFT} from "./connector-styles";
import {DiagramLink} from "./diagram-link";
import {SystemTask} from "./system-task";


export class ConnectorOverlay extends mxCellOverlay {

    constructor(image?: MxImage, tooltip?: string, align?: string, verticalAlign?: string, offset?: number, cursor?: string) {
        super(image, tooltip, align, verticalAlign, offset, cursor);
        this.cursor = "hand";
        this.offset = 0;
    }

    public getBounds(state): MxRectangle {

        var bounds: MxRectangle = super.getBounds(state);

        if (state.view.graph.getModel().isEdge(state.cell)) {
            var edge = state.cell;
            var source = edge.source;
            var target = edge.target;
            var sourceX: number = source.geometry.getCenterX();
            var sourceY: number = source.geometry.getCenterY();
            var targetY: number = target.geometry.getCenterY();

            if (source.parent.getNodeType && source.parent.getNodeType() === NodeType.SystemTask) {
                sourceX = source.getParent().geometry.getCenterX();
                sourceY = sourceY / 2 + source.getParent().geometry.getCenterY() - 3;
            }

            if (target.parent.getNodeType && target.parent.getNodeType() === NodeType.SystemTask) {
                sourceX = sourceX + 35;
                targetY = targetY / 2 + target.getParent().geometry.getCenterY() - 3;
            }

            if (source.getNodeType) {
                if (source.getNodeType() === NodeType.UserDecision) {
                    sourceX = sourceX + 15;
                } else if (source.getNodeType() === NodeType.MergingPoint) {
                    sourceX = sourceX - 10;
                }
            }

            if (target.getNodeType) {
                if (target.getNodeType() === NodeType.UserDecision) {
                    sourceX = sourceX - 15;
                } else if (target.getNodeType() === NodeType.SystemDecision) {
                    sourceX = sourceX + 15;
                }
            }

            bounds.x = sourceX + 60 - bounds.width / 2;

            if (edge.style && edge.style.indexOf(CS_VERTICAL) > -1) {
                bounds.y = Math.max(sourceY, targetY) - bounds.height / 2;
            } else {
                bounds.y = sourceY - bounds.height / 2;
            }
        }

        return bounds;
    }
}
export class Connector {

    public static get LABEL_SIZE(): number { return 10; }
    public static get LABEL_FONT(): string { return "Open Sans"; }

    constructor() {
    }

    static render(graph: IProcessGraph,
        processLink: IProcessLinkModel,
        source: IDiagramNode,
        target: IDiagramNode,
        hasOverlay: boolean,
        label?: string,
        style?: string): MxCell {

        if (!processLink || !source || !target) {
            return;
        }
        var mxGraph = graph.getMxGraph();
        // #UNUSED
        // var model = graph.getMxGraphModel();
        var parent = graph.getDefaultParent();

        if (style !== undefined) {
            style = "fontColor=#999999;strokeColor=#d4d5da;strokeWidth=3";
            style += ";fontSize=" + Connector.LABEL_SIZE + ";fontFamily=" + Connector.LABEL_FONT;
        } else {
            style = "fontColor=#999999;strokeColor=#d4d5da;strokeWidth=3" + style;
            style += ";fontSize=" + Connector.LABEL_SIZE + ";fontFamily=" + Connector.LABEL_FONT;
        }

        if (target.getNodeType() === NodeType.SystemDecision ||
            target.getNodeType() === NodeType.SystemTask ||
            (source.getNodeType() === NodeType.SystemTask && (<SystemTask>source).isPrecondition())) {
            style += ";endArrow=none";
        } else {
            style += ";endArrow=open";
        }

        if (source.getNodeType() === NodeType.ProcessStart) {
            hasOverlay = false;
        }

        if ((source.getNodeType() === NodeType.UserDecision ||
            source.getNodeType() === NodeType.SystemDecision) &&
            source.getX() < target.getX() &&
            source.getY() < target.getY()
        ) {
            style += ";edgeStyle=" + CS_VERTICAL;
        } else if (source.getX() < target.getX()) {
            style += ";edgeStyle=" + CS_RIGHT;
        } else if (source.getX() >= target.getX()) {
            style += ";edgeStyle=" + CS_LEFT;
        }

        var connectorCell = new DiagramLink(processLink, parent, null, new mxGeometry(), style);
        var sourceElement = source.getConnectableElement();
        var targetElement = target.getConnectableElement();
        var edge: MxCell;

        edge = graph.addLink(connectorCell, parent, 0, sourceElement, targetElement);

        connectorCell.initializeLabel(graph, source, target);

        if (hasOverlay && !graph.viewModel.isReadonly) {
            connectorCell.showMenu(mxGraph);
        }

        return edge;
    }

    static remove(graph: IProcessGraph, edge: MxCell) {
        var model = graph.getMxGraphModel();

        edge.source.removeEdge(edge, true);
        edge.target.removeEdge(edge, false);
        model.remove(edge);
    }
}