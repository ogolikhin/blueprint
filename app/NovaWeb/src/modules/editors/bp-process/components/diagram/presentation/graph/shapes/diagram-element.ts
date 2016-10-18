import {IDiagramElement, IDiagramNode} from "./../models/";
import {IDiagramNodeElement, ILabel} from "./../models/";
import {ElementType, NodeChange} from "./../models/";
import {IProcessDiagramCommunication} from "../../../process-diagram-communication";

export class DiagramElement extends mxCell implements IDiagramElement {
    private elementType: ElementType;
    textLabel: ILabel;
    protected processDiagramManager: IProcessDiagramCommunication;

    constructor(id: any, type: ElementType = ElementType.Undefined, value?: string, geometry?: MxGeometry, style?: string) {
        super(value, geometry, style);

        this.setId(id);
        this.elementType = type;
    }

    public getElementType(): ElementType {
        return this.elementType;
    }

    public isHtmlElement(): boolean {
        return this.getElementType() === ElementType.Shape;
    }

    public getX(): number {
        return this.getCenter().x;
    }

    public getY(): number {
        return this.getCenter().y;
    }

    public getHeight(): number {
        const geometry = <MxGeometry>this.getGeometry();
        return geometry.height;
    }

    public getWidth(): number {
        const geometry = <MxGeometry>this.getGeometry();
        return geometry.width;
    }

    public getCenter(): MxPoint {
        const geometry = this.getGeometry();
        if (geometry) {
            const point = new mxPoint(geometry.getCenterX(), geometry.getCenterY());
            return point;
        }

        return new mxPoint(0, 0);
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

    protected getParentId(): number {
        // override in descendant classes
        return null;
    }


    private _redraw: boolean;
    private _isNotificationPending: boolean = false;
    
    public getImageSource(image: string) {
        return "/novaweb/static/bp-process/images/" + image;
    }
}

export class DiagramNodeElement extends DiagramElement implements IDiagramNodeElement {
    public getNode(): IDiagramNode {
        let parent = this.parent;

        while (parent) {
            if (parent["getNodeType"]) {
                return parent;
            }
            parent = (<mxCell>parent).parent;
        }
        return null;
    }

    public getCenter(): MxPoint {
        const geometry = <MxGeometry>this.geometry;
        if (geometry) {
            if (this.parent) {
                const parentCenterX = (<IDiagramNodeElement>this.parent).getCenter().x;
                const parentCenterY = (<IDiagramNodeElement>this.parent).getCenter().y;
                const parentX = parentCenterX - (this.parent.getWidth() / 2);
                const parentY = parentCenterY - (this.parent.getHeight() / 2);

                if (geometry.relative) {
                    return new mxPoint(geometry.x * this.parent.getWidth() + geometry.width / 2, geometry.y * this.parent.getHeight() + geometry.height / 2);
                } else {
                    return new mxPoint(geometry.getCenterX() + parentX, geometry.getCenterY() + parentY);
                }
            }

            return new mxPoint(geometry.getCenterX(), geometry.getCenterY());
        }

        return new mxPoint(0, 0);
    }

    public setElementText(cell: MxCell, text: string) {
        if (this.getNode()) {
            this.getNode().setElementText(cell, text);
        }
    }

    public getElementTextLength(cell: MxCell): number {
        if (this.getNode()) {
            return this.getNode().getElementTextLength(cell);
        }
        return null;
    }

    public formatElementText(cell: MxCell, text: string): string {

        if (this.getNode()) {
            return this.getNode().formatElementText(cell, text);
        }
        return null;
    }
}
