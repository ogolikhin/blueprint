import {IProcessGraph, IDiagramNode} from "../models/";
import {IDiagramElement, IMenuContainer} from "../models/";
import {IProcessLinkModel} from "../../../../../models/process-models";
import {NodeType, NodeChange, ElementType} from "../models/";
import {Label, LabelStyle} from "../labels/label";
import {DiagramElement} from "./diagram-element";
import {Connector, ConnectorOverlay} from "./connector";


export interface IDiagramLink extends IDiagramElement, IMenuContainer {
    renderLabel();
    initializeLabel(graph: IProcessGraph, sourceNode: IDiagramNode, targetNode: IDiagramNode);
    label: string;
    sourceNode: IDiagramNode;
    targetNode: IDiagramNode;
    hideMenu(mxGraph: MxGraph);
    showMenu(mxGraph: MxGraph);
    getParentId(): number;
}

export class DiagramLink extends DiagramElement implements IDiagramLink {
    private LABEL_VIEW_MAXLENGTH: number = 25;
    private LABEL_EDIT_MAXLENGTH: number = 40;
    private LABEL_TEXT_ALIGNMENT: string = "left";

    constructor(public model: IProcessLinkModel, parent: any, value?: string, geometry?: MxGeometry, style?: string) {
        super("L" + model.sourceId + "D" + model.destinationId, ElementType.Connector, value, geometry, style);

        this.setParent(parent);
        this.style = style;
        this.geometry = geometry;
        this.value = value;
        this.setEdge(true);
    }

    public renderLabel() {
        if (this.textLabel) {
            this.textLabel.render();
        }
    }

    public initializeLabel(graph: IProcessGraph, sourceNode: IDiagramNode, targetNode: IDiagramNode) {
        if (sourceNode.getNodeType() === NodeType.SystemDecision || sourceNode.getNodeType() === NodeType.UserDecision) {

            let XandY = this.getXandYForLabel(sourceNode, targetNode);
            let width = this.target.getCenter().x - this.target.getWidth() / 2 - (this.source.getCenter().x + this.source.getWidth() / 2);
            const textLabelStyle: LabelStyle = new LabelStyle(
                Connector.LABEL_FONT,
                Connector.LABEL_SIZE,
                "transparent",
                "#999999",
                "",
                XandY.y,
                XandY.x,
                Connector.LABEL_SIZE,
                width,
                "#999999"
            );
            this.textLabel = new Label((value: string) => {
                    this.label = value;
                },
                graph.getHtmlElement(),
                this.model.sourceId + "-" + this.model.destinationId,
                "Label-B" + this.model.sourceId + "-" + this.model.destinationId,
                this.label,
                textLabelStyle,
                this.LABEL_EDIT_MAXLENGTH,
                this.LABEL_VIEW_MAXLENGTH,
                graph.viewModel.isReadonly,
                this.LABEL_TEXT_ALIGNMENT
            );
        }
    }

    private getXandYForLabel(source: IDiagramNode, target: IDiagramNode): { x: number, y: number } {
        let points = {x: 0, y: 0};

        if (source.getNodeType() === NodeType.SystemDecision || source.getNodeType() === NodeType.UserDecision) {
            // height of the connector (ex. the height of 'L' connector)
            //var visibleText = edge.formatElementText(edge, edge.model.label);
            //var sanitizedText = mxUtils.htmlEntities(visibleText, false);

            let sizeOfString = mxUtils.getSizeForString(this.label, Connector.LABEL_SIZE, Connector.LABEL_FONT, null);
            let labelY: number = target.getY();

            if (target.getY() < source.getY() || target.getX() < source.getX()) {
                // conditions are usually at higher Y level than decision point.
                // This scenario happens when there's a do nothing condition within a condition, where the Y should stay at decision level.
                labelY = source.getY();
            }

            points.x = source.getCenter().x + (source.getWidth() / 2);
            points.y = labelY + sizeOfString.height / 2 + 10;
        }

        return points;
    }

    public get label(): string {
        return this.model.label;
    }

    public set label(value: string) {
        if (this.model.label !== value) {
            this.model.label = value;

            if (this.textLabel) {
                this.textLabel.text = value;
            }

            this.notify(NodeChange.Update, true);
        }
    }

    public get sourceNode(): IDiagramNode {
        if (this.source != null && this.source.getNode) {
            return this.source.getNode();
        }

        return null;
    }

    public get targetNode(): IDiagramNode {
        if (this.target != null && this.target.getNode) {
            return this.target.getNode();
        }

        return null;
    }

    public hideMenu(graph: MxGraph) {
        graph.removeCellOverlays(this);
    }

    public showMenu(graph: MxGraph) {
        this.geometry.offset = new mxPoint(0, 30);
        const overlay = new ConnectorOverlay(new mxImage("/novaweb/static/bp-process/images/add-neutral.svg", 16, 16), "Add Task/Decision");
        graph.addCellOverlay(this, overlay);
    }

    public getParentId(): number {
        return this.model.parentId;
    }
}
