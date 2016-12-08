import {IDiagramElement, IDiagramNode, IDiagramNodeElement, ElementType} from "./../models/";
import {IProcessDiagramCommunication} from "../../../process-diagram-communication";
import {ILabel} from "../labels/label";

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

    protected getParentId(): number {
        // override in descendant classes
        return null;
    }

    public getImageSource(image: string) {
        return "/novaweb/static/bp-process/images/" + image;
    }

    public setElementStyle(name: string, value: any) {
        let str: string = this.getStyle();
        let styles = this.createStyleDictionary(str);

        if (styles[name]) {
            styles[name] = value;
            str = this.createStyleString(styles);
            this.setStyle(str);
        }
    }

    public getElementStyle(name: string): any {
        let val = undefined;
        let str: string = this.getStyle();
        let styles = this.createStyleDictionary(str);
        if (styles[name]) {
            val = styles[name];
        }
        return val;
    }

    public getElementStyles(): any {
        let str: string = this.getStyle();
        let styles = this.createStyleDictionary(str);
        
        return styles;
    }

    private createStyleDictionary(styleString: string) {
        let styles = {};
        let s = styleString.split(";"); 
        for (let i = 0; i < s.length; i++) {
            let p = s[i].split("=");
            if (p.length === 2) {
                styles[p[0]] = p[1];
            }
        }
        return styles;
    }
    private createStyleString(styles) {
        let str: string = "";
        let names = Object.getOwnPropertyNames(styles);

        for (let i = 0; i < names.length; i++) {
            str += names[i] + "=" + styles[names[i]] + ";";   
        }
        str = str.slice(0, str.length - 1);
        return str;
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
}
