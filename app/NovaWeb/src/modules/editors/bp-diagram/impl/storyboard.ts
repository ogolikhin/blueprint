import {Shapes, ShapeProps} from "./utils/constants";
import {MxFactory, ShapeExtensions} from "./utils/helpers";
import {IShape} from "./models";
import {AbstractShapeFactory, IShapeTemplates} from "./abstract-diagram-factory";
import {Style, Styles} from "./utils/style-builder";

export class StoryboardShapeFactory extends AbstractShapeFactory {

    public get frameMargin(): number {
        return 48;
    }

    public initTemplates(templates: IShapeTemplates) {
        templates[Shapes.FRAME] = this.frame;
        return templates;
    }

    private frame = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_STROKECOLOR] = mxConstants.NONE;
        style[mxConstants.STYLE_STROKEWIDTH] = 1;
        style[mxConstants.STYLE_LABEL_POSITION] = mxConstants.ALIGN_CENTER;

        const frame = this.createDefaultVertex(shape, style, true);
        const geometry = MxFactory.geometry(0, 0, shape.width, shape.height - this.frameMargin * 2);
        geometry.relative = true;
        geometry.offset = MxFactory.point(0, this.frameMargin);
        style[mxConstants.STYLE_FILLCOLOR] = mxConstants.NONE;
        style[mxConstants.STYLE_STROKECOLOR] = "black";
        style[Styles.STYLE_SELECTABLE] = 0;
        const border = MxFactory.vertex(null, geometry, style.convertToString());
        frame.insert(border);

        const isFirst = ShapeExtensions.getPropertyByName(shape, ShapeProps.IS_FIRST);
        if (isFirst) {
            frame.insert(this.createIndicator());
        }
        if (shape.label) {
            frame.insert(this.createLabelShape(shape));
        }
        if (shape.description) {
            frame.insert(this.createDescriptionShape(shape, ShapeExtensions.getPropertyByName(shape, ShapeProps.HAS_MOCKUP)));
        }
        return frame;
    };

    private createIndicator(): MxCell {
        const style = new Style();
        style[mxConstants.STYLE_SHAPE] = "first";
        style[Styles.STYLE_SELECTABLE] = 0;
        const indicator = MxFactory.vertex(null, MxFactory.geometry(0, 1, 16, 16), style.convertToString());
        indicator.getGeometry().relative = true;
        indicator.getGeometry().offset = MxFactory.point(0, -16 - this.frameMargin);
        return indicator;
    }

    private createLabelShape(shape: IShape): MxCell {
        const style = new Style();
        style[Styles.STYLE_SELECTABLE] = 0;
        style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        style[mxConstants.STYLE_STROKECOLOR] = mxConstants.NONE;
        style[mxConstants.STYLE_FILLCOLOR] = mxConstants.NONE;
        style[mxConstants.STYLE_ALIGN] = shape.labelTextAlignment;
        style[mxConstants.STYLE_WHITE_SPACE] = "nowrap";
        const labelShape = MxFactory.vertex(shape.label, MxFactory.geometry(0, 0, shape.width, this.frameMargin), style.convertToString());
        labelShape.getGeometry().relative = true;
        return labelShape;
    }

    private createDescriptionShape(shape: IShape, hasMockup: boolean): MxCell {
        const style = new Style();
        style[Styles.STYLE_SELECTABLE] = 0;
        style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        style[mxConstants.STYLE_STROKECOLOR] = mxConstants.NONE;
        style[mxConstants.STYLE_FILLCOLOR] = mxConstants.NONE;
        style[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_LEFT;
        style[mxConstants.STYLE_WHITE_SPACE] = "nowrap";
        let geometry: MxGeometry;
        let offset: MxPoint;
        if (hasMockup) {
            geometry = MxFactory.geometry(0, 0, shape.width, this.frameMargin);
            offset = MxFactory.point(0, shape.height - this.frameMargin);
        } else {
            geometry = MxFactory.geometry(0, 0, shape.width, shape.height - this.frameMargin * 2);
            offset = MxFactory.point(0, this.frameMargin);
        }
        const labelShape = MxFactory.vertex(shape.description, geometry, style.convertToString());
        geometry.relative = true;
        geometry.offset = offset;
        return labelShape;
    }
}
