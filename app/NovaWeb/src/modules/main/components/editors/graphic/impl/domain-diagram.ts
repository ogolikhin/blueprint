import {IShape} from "../diagram.svc";
import {Shapes} from "./utils/constants";
import {AbstractShapeFactory, IShapeTemplates} from "./abstract-diagram-factory";

export class DomainDiagramShapeFactory extends AbstractShapeFactory {
    public initTemplates(templates: IShapeTemplates) {
        templates[Shapes.ENTITY] = this.entity;
        templates[Shapes.ELEMENT] = this.element;
        return templates;
    }

    private entity = (shape: IShape): MxCell => {
        var style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_SWIMLANE);
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_STARTSIZE] = 47;
        style[mxConstants.STYLE_HORIZONTAL] = 1;
        var entity = super.createDefaultVertex(shape, style);

        return entity;
    };
    private element = (shape: IShape): MxCell => {
        var style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_STROKEWIDTH] = 2;
        var element = super.createDefaultVertex(shape, style);
        return element;
    };
}