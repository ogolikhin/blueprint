import {IShape} from "../impl/models";
import {Shapes} from "./utils/constants";
import {AbstractShapeFactory, IShapeTemplates} from "./abstract-diagram-factory";

export class GenericDiagramShapeFactory extends AbstractShapeFactory {
    public initTemplates(templates: IShapeTemplates) {
        templates[Shapes.TRIANGLE_DOWN] = this.triangleDown;
        templates[Shapes.TRIANGLE_UP] = this.triangleUp;
        templates[Shapes.TRIANGLE_LEFT] = this.triangleLeft;
        templates[Shapes.TRIANGLE_Right] = this.triangleRight;
        templates[Shapes.CYLINDER_VERTICAL] = this.cylinderVertical;
        templates[Shapes.CYLINDER_HONRIZONTAL] = this.cylinderHorizontal;
        templates[Shapes.PAGE] = this.page;
        templates[Shapes.DOCUMENT] = this.document;
        templates[Shapes.TRAPEZOID] = this.trapezoid;
        templates[Shapes.PARALLELOGRAM] = this.parallelogram;
        templates[Shapes.ROUNDED_RECTANGLE] = this.roundedRectangle;
        templates[Shapes.DIAMOND] = this.diamond;

        return templates;
    }

    private triangleDown = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, "triangledown");
        return super.createDefaultVertex(shape, style);
    };

    private triangleUp = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, "triangleup");
        return super.createDefaultVertex(shape, style);
    };

    private triangleLeft = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, "triangleleft");
        return super.createDefaultVertex(shape, style);
    };

    private triangleRight = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, "triangleright");
        return super.createDefaultVertex(shape, style);
    };

    private cylinderVertical = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, "verticalcylinder");
        return super.createDefaultVertex(shape, style);
    };

    private cylinderHorizontal = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, "horizontalcylinder");
        return super.createDefaultVertex(shape, style);
    };

    private page = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, "page");
        return super.createDefaultVertex(shape, style);
    };

    private document = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, "document");
        return super.createDefaultVertex(shape, style);
    };

    private trapezoid = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, "trapezoid");
        return super.createDefaultVertex(shape, style);
    };

    private parallelogram = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, "parallelogram");
        return super.createDefaultVertex(shape, style);
    };

    private roundedRectangle = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, "rectangle");
        style[mxConstants.STYLE_ROUNDED] = 1;
        return super.createDefaultVertex(shape, style);
    };

    private diamond = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, "diamond");
        return super.createDefaultVertex(shape, style);
    };
}
