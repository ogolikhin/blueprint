import * as angular from "angular";
import {IStep, StepOfType} from "./usecase/models";
import {IUseCaseShape} from "./usecase/usecase-to-diagram";
import {AbstractShapeFactory, IShapeTemplates} from "./abstract-diagram-factory";
import {IConnection} from "./models";
import {Shapes} from "./utils/constants";
import {MxFactory} from "./utils/helpers";
import {Style, Styles} from "./utils/style-builder";

export class UsecaseShapeFactory extends AbstractShapeFactory {

    public static get DEFAULT_CONNECTOR_STROKE_COLOR(): string {
        return "black";
    }

    public static get DEFAULT_CONNECTOR_STROKE_WIDTH(): number {
        return 1;
    }

    public static get DEFAULT_SHAPE_STROKE_WIDTH(): number {
        return 1;
    }

    public static get DEFAULT_SHAPE_STROKE_COLOR(): string {
        return "#002060";
    }

    public static get DEFAULT_SHAPE_FILL_COLOR(): string {
        return "#E1EBF3";
    }

    public static get DEFAULT_SHAPE_GRADIENT_COLOR(): string {
        return "#CBDDEB";
    }

    public static get CONDITION_FILL_COLOR(): string {
        return "#F8F8F8";
    }

    public static get CONDITION_GRADIENT_COLOR(): string {
        return "#CCCCCC";
    }

    public initTemplates(templates: IShapeTemplates) {
        templates[Shapes.PRE_POST_CONDITION] = this.prePostCondition;
        templates[Shapes.STEP] = this.step;
        templates[Shapes.BRANCHING] = this.branching;
        templates[Shapes.EXIT] = this.exit;

        return templates;
    }

    private prePostCondition = (shape: IUseCaseShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_ELLIPSE);
        style[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_CENTER;
        style[mxConstants.STYLE_STROKECOLOR] = UsecaseShapeFactory.DEFAULT_SHAPE_STROKE_COLOR;
        style[mxConstants.STYLE_STROKEWIDTH] = UsecaseShapeFactory.DEFAULT_SHAPE_STROKE_WIDTH;
        style[mxConstants.STYLE_FILLCOLOR] = UsecaseShapeFactory.DEFAULT_SHAPE_FILL_COLOR;
        style[mxConstants.STYLE_GRADIENTCOLOR] = UsecaseShapeFactory.DEFAULT_SHAPE_GRADIENT_COLOR;
        style[mxConstants.STYLE_OVERFLOW] = "hidden";
        return this.createCellWithTooltip(shape, style);
    };

    private step = (shape: IUseCaseShape): MxCell => {
        let style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_STROKECOLOR] = UsecaseShapeFactory.DEFAULT_SHAPE_STROKE_COLOR;
        style[mxConstants.STYLE_STROKEWIDTH] = UsecaseShapeFactory.DEFAULT_SHAPE_STROKE_WIDTH;
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;
        style[mxConstants.STYLE_OVERFLOW] = "hidden";
        const external = this.isExternalFlowShape(<IStep>shape.element);
        if (this.isCondition(<IStep>shape.element)) {
            style[mxConstants.STYLE_FILLCOLOR] = UsecaseShapeFactory.CONDITION_FILL_COLOR;
            style[mxConstants.STYLE_GRADIENTCOLOR] = UsecaseShapeFactory.CONDITION_GRADIENT_COLOR;
        }
        if (this.getStepType(<IStep>shape.element) === StepOfType.System || external) {
            style[mxConstants.STYLE_FILLCOLOR] = UsecaseShapeFactory.DEFAULT_SHAPE_FILL_COLOR;
            style[mxConstants.STYLE_GRADIENTCOLOR] = UsecaseShapeFactory.DEFAULT_SHAPE_GRADIENT_COLOR;
        }
        let cell = this.createCellWithTooltip(shape, style);
        if (external) {
            const geometry = MxFactory.geometry(1, 1, 12, 25);
            geometry.relative = true;
            geometry.offset = MxFactory.point(-15, -28);
            style = new Style();
            style[Styles.STYLE_SELECTABLE] = 0;
            style[mxConstants.STYLE_STROKECOLOR] = "black";
            style[mxConstants.STYLE_SHAPE] = Shapes.EXTERNAL_FLOW_INDICATOR;
            const indicator = MxFactory.vertex(null, geometry, style.convertToString());
            cell.insert(indicator);
        }
        return cell;
    };

    private branching = (shape: IUseCaseShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RHOMBUS);
        style[Styles.STYLE_SELECTABLE] = 0;
        style[mxConstants.STYLE_STROKECOLOR] = UsecaseShapeFactory.DEFAULT_SHAPE_STROKE_COLOR;
        style[mxConstants.STYLE_STROKEWIDTH] = UsecaseShapeFactory.DEFAULT_SHAPE_STROKE_WIDTH;
        style[mxConstants.STYLE_FILLCOLOR] = UsecaseShapeFactory.DEFAULT_SHAPE_FILL_COLOR;
        style[mxConstants.STYLE_GRADIENTCOLOR] = UsecaseShapeFactory.DEFAULT_SHAPE_GRADIENT_COLOR;
        return super.createDefaultVertex(shape, style);
    };

    private exit = (shape: IUseCaseShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_ELLIPSE);
        style[Styles.STYLE_SELECTABLE] = 0;
        style[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_CENTER;
        style[mxConstants.STYLE_STROKECOLOR] = UsecaseShapeFactory.DEFAULT_SHAPE_STROKE_COLOR;
        style[mxConstants.STYLE_STROKEWIDTH] = UsecaseShapeFactory.DEFAULT_SHAPE_STROKE_WIDTH;
        style[mxConstants.STYLE_FILLCOLOR] = UsecaseShapeFactory.DEFAULT_SHAPE_FILL_COLOR;
        style[mxConstants.STYLE_GRADIENTCOLOR] = UsecaseShapeFactory.DEFAULT_SHAPE_GRADIENT_COLOR;
        return super.createDefaultVertex(shape, style);
    };

    private isCondition(element: IStep) {
        return element.condition;
    }

    private getStepType(element: IStep) {
        return element != null ? element.stepOf : StepOfType.Actor;
    }

    private isExternalFlowShape(step: IStep) {
        return step != null && step.external;
    }

    //Overrides createConnector of AbstractShapeFactory
    protected createConnector = (connection: IConnection): MxCell => {
        const style = this.styleBuilder.createDefaultConnectionStyle(connection);
        style[mxConstants.STYLE_STROKECOLOR] = UsecaseShapeFactory.DEFAULT_CONNECTOR_STROKE_COLOR;
        style[mxConstants.STYLE_STROKEWIDTH] = UsecaseShapeFactory.DEFAULT_CONNECTOR_STROKE_WIDTH;
        style[mxConstants.STYLE_ENDARROW] = mxConstants.ARROW_CLASSIC;

        const edge = MxFactory.edge(connection, MxFactory.geometry(), style.convertToString());
        edge.geometry.relative = true;
        return edge;
    };

    public enabledToolTips() {
        return true;
    }

    private createCellWithTooltip(shape: IUseCaseShape, style?: Style) {
        const cell = super.createDefaultVertex(shape, style);
        cell.getTooltip = () => {
            if (shape.element != null && (<IStep>shape.element).description != null) {
                return this.getTooltip((<IStep>shape.element).description, shape.width, shape.height);
            }
            return null;
        };
        return cell;
    }

    private getTooltip(html: string, width: number, height: number) {
        if (html) {
            const bbox = this.getLabelSize(html, width);
            if (bbox.height > height) {
                return html;
            }
        }
        return null;
    }

    private getLabelSize(label: string, width: number) {
        const size = new mxRectangle(0, 0, 0, 0);
        const element = angular.element(label);
        if (element.length > 0) {
            const div = document.createElement("div");
            div.style.position = "absolute";
            div.style.visibility = "hidden";
            div.style.zoom = "1";
            div.style.width = width + "px";
            div.style.wordWrap = "break-word";
            div.appendChild(element[0]);
            document.body.appendChild(div);
            const clientRect = div.getBoundingClientRect();
            size.width = clientRect.width;
            size.height = clientRect.height;
            document.body.removeChild(div);
        }

        return size;
    }
}
