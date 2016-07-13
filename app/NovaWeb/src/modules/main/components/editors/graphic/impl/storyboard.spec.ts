import "angular";
import "angular-mocks";
import "angular-sanitize";

import {Shapes, ShapeProps, Diagrams, ConnectorTypes} from "./utils/constants";
import {DiagramServiceMock, Prop} from '../diagram.svc.mock';
import {BPDiagram} from "../../../../components/editors/graphic/bp-diagram";
import {StencilServiceMock} from '../stencil.svc.mock';
require("script!mxClient");

describe("Rendering common shapes", () => {
    var validUseDirectiveHtml = "<div data-diagram></div>";

    var element: ng.IAugmentedJQuery;

    beforeEach(angular.mock.module("ngSanitize", ($provide: ng.auto.IProvideService, $compileProvider: ng.ICompileProvider) => {
        $compileProvider.component("diagram", <any>new BPDiagram());
        $provide.service("stencilService", StencilServiceMock);
       // $provide.service("artifactSelector", ArtifactSelector);
    }));

    xit("Frame Shape Test, First Frame With Mockup", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        var scope = $rootScope.$new();
        element = $compile(validUseDirectiveHtml)(scope);
        scope.$digest();

        // Act
        var props = new Array<Prop>();
        props[0] = { name: ShapeProps.HAS_MOCKUP, value: false };
        props[1] = { name: ShapeProps.IS_FIRST, value: true };


        var frameShapes = [];
        frameShapes.push(DiagramServiceMock.createShape(Shapes.FRAME, props, 1, 100, 100, 100, 250));

        scope["diagram"] = DiagramServiceMock.createDiagramMock(frameShapes, [], Diagrams.STORYBOARD);
        $rootScope.$apply();

        // Assert

        var frameBoundary = element.find("rect[x='100'][y='100'][width='100'][height='250'][stroke='none']");
        expect(frameBoundary.length).toEqual(1);

        var border = element.find("rect[x='100'][y='148'][width='100'][height='154'][stroke='black']");
        expect(border.length).toEqual(1);

        var indicator = element.find("image[x='100'][y='286'][width='16'][height='16']");
        expect(indicator.length).toEqual(1);

        var labelShape = element.find("rect[x='100'][y='100'][width='100'][height='48'][fill='none'][stroke='none']");
        expect(labelShape.length).toEqual(1);
    }));

    xit("Frame Shape Test, Not First Frame With Mockup", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        var scope = $rootScope.$new();
        element = $compile(validUseDirectiveHtml)(scope);
        scope.$digest();

        // Act
        var props = new Array<Prop>();
        props[0] = { name: ShapeProps.HAS_MOCKUP, value: true };
        props[1] = { name: ShapeProps.IS_FIRST, value: false };


        var frameShapes = [];
        frameShapes.push(DiagramServiceMock.createShape(Shapes.FRAME, props, 1, 100, 100, 100, 250));

        scope["diagram"] = DiagramServiceMock.createDiagramMock(frameShapes, [], Diagrams.STORYBOARD);
        $rootScope.$apply();

        // Assert
        var frameBoundary = element.find("rect[x='100'][y='100'][width='100'][height='250'][stroke='none']");
        expect(frameBoundary.length).toEqual(1);

        var border = element.find("rect[x='100'][y='148'][width='100'][height='154'][stroke='black']");
        expect(border.length).toEqual(1);

        var indicator = element.find("rect[width='16'][height='16'][fill='#c3e4f5'][stroke='#455261']");
        expect(indicator.length).toEqual(0);

        var labelShape = element.find("rect[x='100'][y='100'][width='100'][height='48'][fill='none'][stroke='none']");
        expect(labelShape.length).toEqual(1);
    }));

    xit("Frame Shape Test, Description at the bottom of the shape", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        var scope = $rootScope.$new();
        element = $compile(validUseDirectiveHtml)(scope);
        scope.$digest();

        // Act
        var props = new Array<Prop>();
        props[0] = { name: ShapeProps.HAS_MOCKUP, value: true };
        props[1] = { name: ShapeProps.IS_FIRST, value: false };

        var frameShapes = [];
        var shape = DiagramServiceMock.createShape(Shapes.FRAME, props, 1, 100, 100, 100, 250);
        shape.description = DiagramServiceMock.createRichText("test description text");
        frameShapes.push(shape);

        scope["diagram"] = DiagramServiceMock.createDiagramMock(frameShapes, [], Diagrams.STORYBOARD);
        $rootScope.$apply();

        // Assert
        var descriptionShape = element.find("rect[x='100'][y='302'][width='100'][height='48'][fill='none'][stroke='none']");
        expect(descriptionShape.length).toEqual(1);
    }));

    xit("Frame Shape Test, Description at the center of the shape", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        var scope = $rootScope.$new();
        element = $compile(validUseDirectiveHtml)(scope);
        scope.$digest();

        // Act
        var props = new Array<Prop>();
        props[0] = { name: ShapeProps.HAS_MOCKUP, value: false };
        props[1] = { name: ShapeProps.IS_FIRST, value: false };

        var frameShapes = [];
        var shape = DiagramServiceMock.createShape(Shapes.FRAME, props, 1, 100, 100, 100, 250);
        shape.description = DiagramServiceMock.createRichText("test description text");
        frameShapes.push(shape);

        scope["diagram"] = DiagramServiceMock.createDiagramMock(frameShapes, [], Diagrams.STORYBOARD);
        $rootScope.$apply();

        // Assert
        var descriptionShape = element.find("rect[x='100'][y='148'][width='100'][height='154'][fill='none'][stroke='none']");
        expect(descriptionShape.length).toEqual(1);
    }));

    xit("Frame Shape Test, Description at the center of the shape", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        var scope = $rootScope.$new();
        element = $compile(validUseDirectiveHtml)(scope);
        scope.$digest();

        // Act
        var props = new Array<Prop>();
        props[0] = { name: ShapeProps.HAS_MOCKUP, value: false };
        props[1] = { name: ShapeProps.IS_FIRST, value: false };

        var frameShapes = [];
        var shape = DiagramServiceMock.createShape(Shapes.FRAME, props, 1, 100, 100, 100, 250);
        shape.description = DiagramServiceMock.createRichText("test description text");
        frameShapes.push(shape);

        scope["diagram"] = DiagramServiceMock.createDiagramMock(frameShapes, [], Diagrams.STORYBOARD);
        $rootScope.$apply();

        // Assert
        var descriptionShape = element.find("rect[x='100'][y='148'][width='100'][height='154'][fill='none'][stroke='none']");
        expect(descriptionShape.length).toEqual(1);
    }));

    xit("Frame Shape Test, Two connected frames", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        var scope = $rootScope.$new();
        element = $compile(validUseDirectiveHtml)(scope);
        scope.$digest();

        // Act
        var props = new Array<Prop>();
        props[0] = { name: ShapeProps.HAS_MOCKUP, value: false };
        props[1] = { name: ShapeProps.IS_FIRST, value: false };

        var frameShapes = [];
        frameShapes.push(DiagramServiceMock.createShape(Shapes.FRAME, props, 1, 1, 50, 100, 250));
        frameShapes.push(DiagramServiceMock.createShape(Shapes.FRAME, props, 2, 300, 50, 100, 250));

        var connections = [];
        var points = [{ x: 51, y: 252 }, { x: 51, y: 310 }, { x: 150, y: 310 }, { x: 150, y: 25 }, { x: 350, y: 25 }, { x: 350, y: 98 }];
        var connection = DiagramServiceMock.createConnection(ConnectorTypes.RIGHT_ANGLED, points);
        connection.sourceId = 1;
        connection.targetId = 1;

        connections.push(connection);

        scope["diagram"] = DiagramServiceMock.createDiagramMock(frameShapes, connections, Diagrams.STORYBOARD);
        $rootScope.$apply();

        // Assert
        var frame1 = element.find("rect[x='1'][y='50'][width='100'][height='250'][fill='white'][stroke='none']");
        expect(frame1.length).toEqual(1);

        var frame2 = element.find("rect[x='300'][y='50'][width='100'][height='250'][fill='white'][stroke='none']");
        expect(frame2.length).toEqual(1);

        var pathElement = element.find("path");
        expect(pathElement.length).toBe(2);

        var visibility = pathElement[0].getAttribute("visibility");
        expect(visibility).toEqual("hidden");

        var dAttribute = pathElement[0].getAttribute("d");
        expect(dAttribute).toEqual("M 51 252 L 51 310 L 150 310 L 150 25 L 350 25 L 350 98");

        visibility = pathElement[1].getAttribute("visibility");
        expect(visibility).toBeNull();
        dAttribute = pathElement[1].getAttribute("d");
        expect(dAttribute).toEqual("M 51 252 L 51 310 L 150 310 L 150 25 L 350 25 L 350 98");

    }));
});