import "angular";
import "angular-mocks";
import "angular-sanitize";
require("script!mxClient");

import {Shapes, ShapeProps} from "./utils/constants";
import {IShape, IConnection} from "../impl/models";
import {StepOfType} from "../impl/usecase/models";
import {Style} from "./utils/style-builder";
import {UsecaseToDiagram, IUseCaseShape} from "./usecase/usecase-to-diagram";
import {UsecaseShapeFactory} from "./activity-flow-diagram";
import {IShapeTemplates} from "./abstract-diagram-factory";


describe("UsecaseShapeFactory ", () => {
    it("PrePostCondition template Test", () => {
        // Arrange
        var factory = new UsecaseShapeFactory();
        var templates = <IShapeTemplates>{};
        factory.initTemplates(templates);
        var shape = {};

        // Act
        var template = templates[Shapes.PRE_POST_CONDITION](<IShape>shape);

        // Assert
        var style = Style.createFromString(template.getStyle());

        expect(style[mxConstants.STYLE_SHAPE]).toEqual(mxConstants.SHAPE_ELLIPSE);
        expect(style[mxConstants.STYLE_ALIGN]).toEqual(mxConstants.ALIGN_CENTER);
        expect(style[mxConstants.STYLE_STROKECOLOR]).toEqual(UsecaseShapeFactory.DEFAULT_SHAPE_STROKE_COLOR);
        expect(style[mxConstants.STYLE_STROKEWIDTH]).toEqual(UsecaseShapeFactory.DEFAULT_SHAPE_STROKE_WIDTH.toString());
        expect(style[mxConstants.STYLE_FILLCOLOR]).toEqual(UsecaseShapeFactory.DEFAULT_SHAPE_FILL_COLOR);
        expect(style[mxConstants.STYLE_GRADIENTCOLOR]).toEqual(UsecaseShapeFactory.DEFAULT_SHAPE_GRADIENT_COLOR);
        expect(style[mxConstants.STYLE_OVERFLOW]).toEqual("hidden");
    });

    it("Branching step template Test", () => {
        // Arrange
        var factory = new UsecaseShapeFactory();
        var templates = <IShapeTemplates>{};
        factory.initTemplates(templates);
        var shape = {};

        // Act
        var template = templates[Shapes.BRANCHING](<IShape>shape);

        // Assert
        var style = Style.createFromString(template.getStyle());

        expect(style[mxConstants.STYLE_SHAPE]).toEqual(mxConstants.SHAPE_RHOMBUS);
        expect(style[mxConstants.STYLE_STROKECOLOR]).toEqual(UsecaseShapeFactory.DEFAULT_SHAPE_STROKE_COLOR);
        expect(style[mxConstants.STYLE_STROKEWIDTH]).toEqual(UsecaseShapeFactory.DEFAULT_SHAPE_STROKE_WIDTH.toString());
        expect(style[mxConstants.STYLE_FILLCOLOR]).toEqual(UsecaseShapeFactory.DEFAULT_SHAPE_FILL_COLOR);
        expect(style[mxConstants.STYLE_GRADIENTCOLOR]).toEqual(UsecaseShapeFactory.DEFAULT_SHAPE_GRADIENT_COLOR);
    });

    it("Exit step template Test", () => {
        // Arrange
        var factory = new UsecaseShapeFactory();
        var templates = <IShapeTemplates>{};
        factory.initTemplates(templates);
        var shape = {};

        // Act
        var template = templates[Shapes.EXIT](<IShape>shape);

        // Assert
        var style = Style.createFromString(template.getStyle());

        expect(style[mxConstants.STYLE_SHAPE]).toEqual(mxConstants.SHAPE_ELLIPSE);
        expect(style[mxConstants.STYLE_STROKECOLOR]).toEqual(UsecaseShapeFactory.DEFAULT_SHAPE_STROKE_COLOR);
        expect(style[mxConstants.STYLE_STROKEWIDTH]).toEqual(UsecaseShapeFactory.DEFAULT_SHAPE_STROKE_WIDTH.toString());
        expect(style[mxConstants.STYLE_FILLCOLOR]).toEqual(UsecaseShapeFactory.DEFAULT_SHAPE_FILL_COLOR);
        expect(style[mxConstants.STYLE_GRADIENTCOLOR]).toEqual(UsecaseShapeFactory.DEFAULT_SHAPE_GRADIENT_COLOR);
    });

    it("Condition step template Test", () => {
        // Arrange
        var factory = new UsecaseShapeFactory();
        var templates = <IShapeTemplates>{};
        factory.initTemplates(templates);
        var shape: any = {
            element: {
                condition: true
            }
        };

        // Act
        var template = templates[Shapes.STEP](<IUseCaseShape>shape);

        // Assert
        var style = Style.createFromString(template.getStyle());

        expect(style[mxConstants.STYLE_SHAPE]).toEqual(mxConstants.SHAPE_RECTANGLE);
        expect(style[mxConstants.STYLE_STROKECOLOR]).toEqual(UsecaseShapeFactory.DEFAULT_SHAPE_STROKE_COLOR);
        expect(style[mxConstants.STYLE_STROKEWIDTH]).toEqual(UsecaseShapeFactory.DEFAULT_SHAPE_STROKE_WIDTH.toString());
        expect(style[mxConstants.STYLE_FILLCOLOR]).toEqual(UsecaseShapeFactory.CONDITION_FILL_COLOR);
        expect(style[mxConstants.STYLE_GRADIENTCOLOR]).toEqual(UsecaseShapeFactory.CONDITION_GRADIENT_COLOR);
        expect(style[mxConstants.STYLE_VERTICAL_ALIGN]).toEqual(mxConstants.ALIGN_TOP);

        expect(style[mxConstants.STYLE_OVERFLOW]).toEqual("hidden");
    });

    it("System step template Test", () => {
        // Arrange
        var factory = new UsecaseShapeFactory();
        var templates = <IShapeTemplates>{};
        factory.initTemplates(templates);
        var shape: any = {
            element: {
                stepOf: StepOfType.System
            }
        };

        // Act
        var template = templates[Shapes.STEP](<IUseCaseShape>shape);

        // Assert
        var style = Style.createFromString(template.getStyle());

        expect(style[mxConstants.STYLE_SHAPE]).toEqual(mxConstants.SHAPE_RECTANGLE);
        expect(style[mxConstants.STYLE_STROKECOLOR]).toEqual(UsecaseShapeFactory.DEFAULT_SHAPE_STROKE_COLOR);
        expect(style[mxConstants.STYLE_STROKEWIDTH]).toEqual(UsecaseShapeFactory.DEFAULT_SHAPE_STROKE_WIDTH.toString());
        expect(style[mxConstants.STYLE_FILLCOLOR]).toEqual(UsecaseShapeFactory.DEFAULT_SHAPE_FILL_COLOR);
        expect(style[mxConstants.STYLE_GRADIENTCOLOR]).toEqual(UsecaseShapeFactory.DEFAULT_SHAPE_GRADIENT_COLOR);
        expect(style[mxConstants.STYLE_VERTICAL_ALIGN]).toEqual(mxConstants.ALIGN_TOP);

        expect(style[mxConstants.STYLE_OVERFLOW]).toEqual("hidden");
    });

    it("Connector template Test", () => {
        // Arrange
        var factory = new UsecaseShapeFactory();
        var templates = <IShapeTemplates>{};
        factory.initTemplates(templates);
        var connection: any = {};

        // Act
        var template = factory.createConnectorTemplate()(<IConnection>connection);

        // Assert
        var style = Style.createFromString(template.getStyle());

        expect(style[mxConstants.STYLE_SHAPE]).toEqual(mxConstants.SHAPE_CONNECTOR);
        expect(style[mxConstants.STYLE_STROKEWIDTH]).toEqual(UsecaseShapeFactory.DEFAULT_CONNECTOR_STROKE_WIDTH.toString());
        expect(style[mxConstants.STYLE_STROKECOLOR]).toEqual(UsecaseShapeFactory.DEFAULT_CONNECTOR_STROKE_COLOR);
        expect(style[mxConstants.STYLE_ENDARROW]).toEqual(mxConstants.ARROW_CLASSIC);
    });
});
