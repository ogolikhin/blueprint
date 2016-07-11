import {IShape, IConnection, IPoint} from "../diagram.svc";
import {Shapes, ShapeProps} from "./utils/constants";
import {MxFactory, ShapeExtensions, Color} from "./utils/helpers";
import {Style, StyleBuilder} from "./utils/style-builder";
import {CalloutShape, SvgImageShape} from "./shapes-library";

export interface IShapeTemplates {
    [key: string]: (shape: IShape) => MxCell;
}

export interface IShapeTemplateFactory {
    createShapeTemplate(shapeType: string): (shape: IShape) => MxCell;
    createConnectorTemplate(): (connection: IConnection) => MxCell;
    enabledToolTips(): boolean;
}

/*
* The class must be implemented as abstract one.
*/
export class AbstractShapeFactory implements IShapeTemplateFactory {

    private templates: IShapeTemplates = null;

    protected styleBuilder: StyleBuilder = new StyleBuilder();

    public createShapeTemplate(shapeType: string): (shape: IShape) => MxCell {
        if (this.templates == null) {
            this.templates = <IShapeTemplates>{};
            this.initDefaultTemplates(this.templates);
            this.initTemplates(this.templates);
        }
        var template = this.templates[shapeType];
        if (template != null) {
            return template;
        }
        //return function 'rectangle'
        return this.fallback;
    }

    public initDefaultTemplates(templates: IShapeTemplates) {
        templates[Shapes.IMAGE] = this.image;
        templates[Shapes.CALLOUT] = this.callout;
        templates[Shapes.TEXTAREA] = this.textArea;
        templates[Shapes.GROUP] = this.group;
        templates[Shapes.RECTANGLE] = this.rectangle;
        templates[Shapes.ELLIPSE] = this.ellipse;
        templates[Shapes.GENERIC_SHAPE] = this.genericShape;
    }

    public enabledToolTips() {
        return false;
    }

    /*
    * The method must be implemented as abstract one.
    */
    public initTemplates(templates: IShapeTemplates) {
    }

    public createDefaultVertex(shape: IShape, style?: Style, disableLabel?: boolean): MxCell {
        var styleStr: string;
        if (style) {
            styleStr = style.convertToString();
        } else {
            styleStr = this.styleBuilder.createDefaultShapeStyle(shape).convertToString();
        }
        var cell = MxFactory.vertex(shape, MxFactory.geometry(shape.x, shape.y, shape.width, shape.height), styleStr);
        if (disableLabel) {
            cell.getLabel = () => { return null; };
        }
        return cell;
    }

    public createConnectorTemplate(): (connection: IConnection) => MxCell {
        return this.createConnector;
    }

    protected createConnector = (connection: IConnection): MxCell => {
        var style = this.styleBuilder.createDefaultConnectionStyle(connection);
        var edge = MxFactory.edge(connection, MxFactory.geometry(), style.convertToString());
        edge.geometry.relative = true;
        return edge;
    };

    public static convertToRgbaIfNeeded(color: string, opacity: number): string {
        if (opacity != null && opacity !== 1 && !Color.isTransparent(color)) {
            var c = Color.parseHex(color);
            c.a = opacity;
            return c.toRgba();
        }
        return color;
    }

    private image = (shape: IShape): MxCell => {
        var style = new Style();
        style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_IMAGE;
        var imageUrl = ShapeExtensions.getPropertyByName(shape, ShapeProps.IMAGE);
        if (imageUrl != null) {
            style[mxConstants.STYLE_IMAGE] = imageUrl;
        }
        var aspect = ShapeExtensions.getPropertyByName(shape, ShapeProps.IS_KEEP_ASPECT_RATIO);
        if (aspect != null && !aspect) {
            style[mxConstants.STYLE_IMAGE_ASPECT] = 0;
        }
        return this.createDefaultVertex(shape, style);
    };

    private callout = (shape: IShape): MxCell => {
        var style = this.styleBuilder.createDefaultShapeStyle(shape);
        style[mxConstants.STYLE_SHAPE] = CalloutShape.getName;
        style[mxConstants.STYLE_FOLDABLE] = 0;
        this.moveCalloutAnchorPosition(shape, style);
        this.customizeCalloutStyle(shape, style);
        var callout = this.createDefaultVertex(shape, style);
        this.customizeCallout(shape, callout);
        return callout;
    };

    /*
    * Virtual method, do not delete.
    */
    public customizeCallout(shape: IShape, callout: MxCell) {
    }

    /*
    * Virtual method, do not delete
    */
    public customizeCalloutStyle(shape: IShape, style: Style) {
    }

    public moveCalloutAnchorPosition(shape: IShape, style: Style) {
        var anchorPosition: IPoint = ShapeExtensions.getPropertyByName(shape, ShapeProps.ANCHOR_POSITION);
        if (anchorPosition != null) {
            style["x"] = anchorPosition.x;
            style["y"] = anchorPosition.y;
        }
    }

    private textArea = (shape: IShape): MxCell => {
        var style = this.styleBuilder.createDefaultShapeStyle(shape);
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;
        return this.createDefaultVertex(shape, style);
    };

    private group = (shape: IShape): MxCell => {
        var style = this.styleBuilder.createDefaultShapeStyle(shape);
        style[mxConstants.STYLE_FOLDABLE] = 0;
        return this.createDefaultVertex(shape, style);
    };

    private rectangle = (shape: IShape): MxCell => {
        var style = this.styleBuilder.createDefaultShapeStyle(shape);
        style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        return this.createDefaultVertex(shape, style);
    };

    private ellipse = (shape: IShape): MxCell => {
        var style = this.styleBuilder.createDefaultShapeStyle(shape);
        style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_ELLIPSE;
        return this.createDefaultVertex(shape, style);
    };

    private genericShape = (shape: IShape): MxCell => {
        var imageUrl = ShapeExtensions.getPropertyByName(shape, ShapeProps.IMAGE);
        if (imageUrl != null) {
            return this.image(shape);
        }
        var style = this.styleBuilder.createDefaultShapeStyle(shape);
        var rectGeometry = ShapeExtensions.getPropertyByName(shape, ShapeProps.RECT_GEOMETRY);
        if (rectGeometry != null) {
            style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
            if (rectGeometry.radiusX > 0 || rectGeometry.radiusY > 0) {
                style[mxConstants.STYLE_ROUNDED] = 1;
            }
        } else {
            style[mxConstants.STYLE_SHAPE] = SvgImageShape.getName;
            var path: string = ShapeExtensions.getPropertyByName(shape, ShapeProps.PATH);
            if (path != null) {
                style[ShapeProps.PATH] = path;
                if (SvgImageShape.hasClosePathOp(path)) {
                    style.removeProperty(mxConstants.STYLE_FILLCOLOR);
                }
            }
        }
        return this.createDefaultVertex(shape, style);
    };

    private fallback = (shape: IShape): MxCell => {
        var style = this.styleBuilder.createDefaultShapeStyle(shape);
        style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        style[mxConstants.STYLE_STROKECOLOR] = "black";
        style[mxConstants.STYLE_FILLCOLOR] = "white";
        return this.createDefaultVertex(shape, style);
    };
}