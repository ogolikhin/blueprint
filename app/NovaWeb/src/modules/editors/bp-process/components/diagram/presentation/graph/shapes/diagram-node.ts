import {IProcessShape} from "../../../../../models/process-models";
import {IProcessGraph, IDiagramNode} from "./../models/";
import {IDiagramLink, IDiagramNodeElement} from "./../models/";
import {DiagramNodeElement} from "./diagram-element";
import {ElementType, Direction, NodeType, NodeChange} from "./../models/";
import {IDialogParams} from "../../../../messages/message-dialog";


export class DiagramNode<T extends IProcessShape> extends DiagramNodeElement implements IDiagramNode {

    direction: Direction;
    model: T;
    private nodeType: NodeType;

    public get newShapeColor(): string {
        return "#F7F1CF";
    }

    constructor(model: T, nodeType: NodeType = NodeType.Undefined) {
        super(model.id.toString(), ElementType.Shape);

        this.model = model;
        this.nodeType = nodeType;
    }

    public getNode(): IDiagramNode {
        return this;
    }

    public getId(): string {
        return this.id;
    }

    public setId(value: string) {
        this.id = value;
    }

    protected getLabelPropertyValue(): string {
        return this.getPropertyValue("label");
    }

    protected setLabelPropertyValue(value: string) {
        const valueChanged = this.setPropertyValue("label", value);
        if (valueChanged) {
            this.notify(NodeChange.Update);
        }
    }

    protected updateCellLabel(value: string) {
        // this method is overriden in UserDecision and SystemDecision
    }

    protected setModelName(value: string, redrawCellLabel: boolean) {
        if (this.model != null && this.model.name !== value) {
            this.model.name = value;
            if (redrawCellLabel) {
                this.updateCellLabel(value);
            } else {
                this.notify(NodeChange.Update);
            }
        }
    }

    protected getModelName(): string {
        return this.model.name;
    }

    public get action(): string {
        return this.getLabelPropertyValue();
    }

    public set action(value: string) {
        this.setLabelPropertyValue(value);
    }

    public get label(): string {
        return this.getModelName();
    }

    public set label(value: string) {
        this.setModelName(value, false);
    }

    public get row(): number {
        return this.getPropertyValue("y");
    }

    public set row(value: number) {
        this.setPropertyValue("y", value);
    }

    public get column(): number {
        return this.getPropertyValue("x");
    }

    public get isNew(): boolean {
        return this.model && this.model.id < 0;
    }

    public set column(value: number) {
        this.setPropertyValue("x", value);
    }

    public getConnectableElement(): IDiagramNodeElement {
        return this;
    }

    public getCenter(): MxPoint {
        const geometry = this.getGeometry();
        if (geometry) {
            const point = new mxPoint(geometry.getCenterX(), geometry.getCenterY());
            return point;
        }

        return new mxPoint(0, 0);
    }

    // returns array of incoming diagram links ordered by asc. order index
    public getIncomingLinks(graphModel: MxGraphModel): IDiagramLink[] {
        return <IDiagramLink[]>graphModel
            .getIncomingEdges(this.getConnectableElement())
            .sort((link1: IDiagramLink, link2: IDiagramLink) => {
                return link1.model.orderindex - link2.model.orderindex;
            });
    }

    // returns array of outgoing diagram links ordered by asc. order index
    public getOutgoingLinks(graphModel: MxGraphModel): IDiagramLink[] {
        return <IDiagramLink[]>graphModel
            .getOutgoingEdges(this.getConnectableElement())
            .sort((link1: IDiagramLink, link2: IDiagramLink) => {
                return link1.model.orderindex - link2.model.orderindex;
            });
    }

    // returns array of connected sourceses
    public getSources(graphModel: MxGraphModel): IDiagramNode[] {
        const sources: IDiagramNode[] = [];
        const incomingLinks = this.getIncomingLinks(graphModel);

        incomingLinks.forEach((link: IDiagramLink) => {
            if (link.sourceNode != null) {
                sources.push(link.sourceNode);
            }
        });

        return sources;
    }

    // return array of connected targets
    public getTargets(graphModel: MxGraphModel): IDiagramNode[] {
        const targets: IDiagramNode[] = [];
        const outgoingLinks = this.getOutgoingLinks(graphModel);

        outgoingLinks.forEach((link: IDiagramLink) => {
            if (link.targetNode != null) {
                targets.push(link.targetNode);
            }
        });

        return targets;
    }

    public addNode(graph: IProcessGraph): IDiagramNode {
        throw new Error("This method is abstract!");
    }

    public deleteNode(graph: IProcessGraph) {
        throw new Error("This method is abstract!");
    }

    public render(graph: IProcessGraph, col: number, row: number, justCreated: boolean): IDiagramNode {
        throw new Error("This method is abstract!");
    }

    public renderLabels(): void {
        //throw new Error("This method is abstract!");
    }

    public addOverlay(mxgraph: MxGraph, parentCell: MxCell, imageURL: string, imageWidth: number, imageHeight: number,
                      toolTip: string, align: string, verticalAlign: string,
                      offsetX: number, offsetY: number, cursor: string = "default"): mxCellOverlay {
        let overlay = new mxCellOverlay(new mxImage(imageURL, imageWidth, imageHeight), toolTip);
        overlay.align = align;
        overlay.verticalAlign = verticalAlign;
        overlay.offset = new mxPoint(offsetX, offsetY);
        overlay.cursor = cursor;
        mxgraph.addCellOverlay(parentCell, overlay);
        return overlay;
    }

    public insertVertex(mxgraph: MxGraph, id, value, x: number, y: number, width: number, height: number, style: string) {
        const parent = mxgraph.getDefaultParent();
        this.geometry = new mxGeometry(x - width / 2, y - height / 2, width, height);
        this.geometry.relative = false;
        this.style = style;
        this.setId(id);
        this.setVertex(true);
        this.setConnectable(true);
        this.setValue(value);
        mxgraph.addCell(this, parent);
    }

    public getNodeType(): NodeType {
        return this.nodeType;
    }

    public setElementText(cell: MxCell, text: string) {
        // override in descendant classes
    }

    public getElementTextLength(cell: MxCell): number {
        // override in descendant classes
        return null;
    }

    public formatElementText(cell: MxCell, text: string): string {
        // override in descendant classes
        return null;
    }

    public getNextNodes(): IDiagramNode[] {
        const edges = this.getConnectableElement().edges;
        if (edges) {
            const nextNodes = edges.filter(edge => {
                const sourceNode = (<IDiagramNodeElement>edge.source).getNode();
                return sourceNode.model.id === this.model.id;
            }).map((edge) => {
                return (<IDiagramNodeElement>edge.target).getNode();
            });
            return <IDiagramNode[]>nextNodes;
        }
        return undefined;
    }

    public getPreviousNodes(): IDiagramNode[] {
        const edges = this.getConnectableElement().edges;
        if (edges) {
            const previousNodes = edges.filter(edge => {
                const targetNode = (<IDiagramNodeElement>edge.target).getNode();
                return targetNode.model.id === this.model.id;
            }).map((edge) => {
                return (<IDiagramNodeElement>edge.source).getNode();
            });
            return <IDiagramNode[]>previousNodes;
        }
        return undefined;
    }

    protected getPropertyValue(propertyName: string) {
        if (this.model == null || this.model.propertyValues == null || this.model.propertyValues[propertyName] == null) {
            return null;
        }

        return this.model.propertyValues[propertyName].value;
    }

    protected setPropertyValue(propertyName: string, newValue: any): boolean {
        if (this.model == null || this.model.propertyValues == null || this.model.propertyValues[propertyName] == null) {
            return false;
        }

        const previousValue = this.model.propertyValues[propertyName].value;
        if (previousValue !== newValue) {
            this.model.propertyValues[propertyName].value = newValue;
            return true;
        }

        return false;
    }

    protected getParentId(): number {
        return this.model.parentId;
    }

    public getDeleteDialogParameters(): IDialogParams {
        return {};
    }

    public getLabelCell(): MxCell {
        return this;
    }

    public canDelete(): boolean {
        return false;
    }

    public canGenerateUserStory(): boolean {
        return false;
    }

    public getImageSource(image: string) {
        return "/novaweb/static/bp-process/images/" + image;
    }

    // TODO: communication with utility panel is different in Nova
    public openPropertiesDialog(scope: ng.IRootScopeService, tab: string) {

        //if (!scope)
        //    return;
        //var utilityPanel: Shell.IPropertiesMw = scope["propertiesSvc"]();
        //if (utilityPanel != null) {
        //    utilityPanel.openModalDialogWithInfo({
        //        id: this.model.id,
        //        containerId: this.model.parentId,
        //        name: this.model.name,
        //        typePrefix: this.model.typePrefix,
        //        predefined: this.model.baseItemTypePredefined,
        //        isDiagram: false,
        //        itemStateIndicators: BluePrintSys.RC.Business.Internal.Components.RapidReview.Models.ItemIndicatorFlags.None,
        //        typeId: undefined
        //    },
        //        tab /*preselected tab*/,
        //        true /*includeDraft*/);
        //}

    }
}
