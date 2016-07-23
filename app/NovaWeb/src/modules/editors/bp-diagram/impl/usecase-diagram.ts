import {AbstractShapeFactory, IShapeTemplates } from "./abstract-diagram-factory";
import {IShape, IConnection} from "./models";
import {Shapes, ShapeProps} from "./utils/constants";
import {Style, Styles} from "./utils/style-builder";
import {MxFactory, ShapeExtensions} from "./utils/helpers";
import {UCDLinkType} from "./utils/enums";


export class UseCaseDiagramShapeFactory extends AbstractShapeFactory {
    public initTemplates(templates: IShapeTemplates) {
        templates[Shapes.ACTOR] = this.actor;
        templates[Shapes.USECASE] = this.usecase;
        templates[Shapes.BOUNDARY] = this.boundary;
    }

    private actor = (shape: IShape): MxCell => {
        var style = new Style();

        style[mxConstants.STYLE_VERTICAL_LABEL_POSITION] = mxConstants.ALIGN_BOTTOM;
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;

        var imageUrl = ShapeExtensions.getPropertyByName(shape, ShapeProps.IMAGE);
        if (imageUrl != null) {
            style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_IMAGE;
            style[mxConstants.STYLE_IMAGE] = imageUrl;
        } else {
            style[mxConstants.STYLE_SHAPE] = "defaultactor";
        }
        return this.createDefaultVertex(shape, style);
    };

    private usecase = (shape: IShape): MxCell => {
        var style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_ELLIPSE);
        style[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_CENTER;
        return this.createDefaultVertex(shape, style);
    };

    private boundary = (shape: IShape): MxCell => {
        var style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;

        return this.createDefaultVertex(shape, style);
    };

    //Overrides createConnector of AbstractShapeFactory
    protected createConnector = (connection: IConnection): MxCell => {
        var style = this.styleBuilder.createDefaultConnectionStyle(connection);
        var edge = MxFactory.edge(connection, MxFactory.geometry());
        var linkType = ShapeExtensions.getPropertyByName(connection, ShapeProps.LINK_TYPE);
        if (linkType != null) {
            style[Styles.STYLE_SELECTABLE] = 0;
            if (linkType === UCDLinkType.Include) {
                style[mxConstants.STYLE_ENDARROW] = mxConstants.ARROW_CLASSIC;
                edge.insert(this.includeMarker());
            }
            if (linkType === UCDLinkType.Extended) {
                style[mxConstants.STYLE_STARTARROW] = mxConstants.ARROW_CLASSIC;
                edge.insert(this.extendMarker());
            }
            if (linkType === UCDLinkType.IncludeAndExtended) {
                style[mxConstants.STYLE_ENDARROW] = mxConstants.ARROW_CLASSIC;
                edge.insert(this.includeMarker());
                style[mxConstants.STYLE_STARTARROW] = mxConstants.ARROW_CLASSIC;
                edge.insert(this.extendMarker());
            }
            if (linkType === UCDLinkType.ActorInheritance) {
                style[mxConstants.STYLE_ENDARROW] = mxConstants.ARROW_OPEN;
            }
        }
        edge.setStyle(style.convertToString());
        edge.geometry.relative = true;
        return edge;
    };

    private includeMarker = (): MxCell => {
        var style = new Style();
        style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_ELLIPSE;
        style[mxConstants.STYLE_STROKEWIDTH] = 2;
        return this.marker(14, 14, style);
    };

    private extendMarker = (): MxCell => {
        var style = new Style();
        style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RHOMBUS;
        style[mxConstants.STYLE_FILLCOLOR] = "black";
        return this.marker(18, 18, style);
    };

    private marker = (width: number, height: number, style: Style): MxCell => {
        style[Styles.STYLE_SELECTABLE] = 0;
        var markerGeometry = MxFactory.geometry(0, 0, width, height);
        markerGeometry.offset = MxFactory.point(-width / 2, -height / 2);
        markerGeometry.relative = true;
        var marker = MxFactory.vertex(null, markerGeometry, style.convertToString());
        marker.connectable = false;
        return marker;
    };
    }
