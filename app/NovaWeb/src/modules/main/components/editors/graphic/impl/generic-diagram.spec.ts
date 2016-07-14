import "angular";
import "angular-mocks";
import "angular-sanitize";
require("script!mxClient");

import {Shapes, Diagrams} from "./utils/constants";
import {DiagramServiceMock} from "../diagram.svc.mock";
import {StencilServiceMock} from "../stencil.svc.mock";
import {DiagramView} from "./diagram-view";

describe("Rendering common shapes", () => {
    let element: ng.IAugmentedJQuery;
    let diagramView: DiagramView;

    beforeEach(angular.mock.module("ngSanitize", ($provide: ng.auto.IProvideService) => {
        $provide.service("stencilService", StencilServiceMock);
    }));

        beforeEach(inject((stencilService: StencilServiceMock) => {
        const divElement = document.createElement("div");
        element = angular.element(divElement);
        diagramView = new DiagramView(element[0], stencilService);
    }));

    it("Triangle Down Shape Test", () => {
        // Arrange
        var triangleDownShapes = [];
        triangleDownShapes.push(DiagramServiceMock.createShape(Shapes.TRIANGLE_DOWN));
        const diagramMock = DiagramServiceMock.createDiagramMock(triangleDownShapes, [], Diagrams.GENERIC_DIAGRAM);
        // Act
        diagramView.drawDiagram(diagramMock);
        // Assert
        // element that is part of the pool shape, this changes if the pool shape template changes
        var triangleDownElement = element.find("path[d='M 100 100 L 200 100 L 150 200 Z']");
        expect(triangleDownElement.length).toEqual(1);
    });

    it("Triangle Up Shape Test", () => {
        // Arrange
        var triangleUpShapes = [];
        triangleUpShapes.push(DiagramServiceMock.createShape(Shapes.TRIANGLE_UP));

        const diagramMock = DiagramServiceMock.createDiagramMock(triangleUpShapes, [], Diagrams.GENERIC_DIAGRAM);
        // Act
        diagramView.drawDiagram(diagramMock);
        // Assert
        // element that is part of the pool shape, this changes if the pool shape template changes
        var triangleUpElement = element.find("path[d='M 150 100 L 200 200 L 100 200 Z']");
        expect(triangleUpElement.length).toEqual(1);
    });

    it("Triangle Left Shape Test", () => {
        // Arrange
        var triangleLeftShapes = [];
        triangleLeftShapes.push(DiagramServiceMock.createShape(Shapes.TRIANGLE_LEFT));
        const diagramMock = DiagramServiceMock.createDiagramMock(triangleLeftShapes, [], Diagrams.GENERIC_DIAGRAM);
        // Act
        diagramView.drawDiagram(diagramMock);
        // Assert
        // element that is part of the pool shape, this changes if the pool shape template changes
        var triangleLeftElement = element.find("path[d='M 100 150 L 200 100 L 200 200 Z']");
        expect(triangleLeftElement.length).toEqual(1);
    });

    it("Triangle Right Shape Test", () => {
        // Arrange
        var triangleRightShapes = [];
        triangleRightShapes.push(DiagramServiceMock.createShape(Shapes.TRIANGLE_Right));
        const diagramMock = DiagramServiceMock.createDiagramMock(triangleRightShapes, [], Diagrams.GENERIC_DIAGRAM);
        // Act
        diagramView.drawDiagram(diagramMock);
        // Assert
        // element that is part of the pool shape, this changes if the pool shape template changes
        var triangleRightElement = element.find("path[d='M 200 150 L 100 200 L 100 100 Z']");
        expect(triangleRightElement.length).toEqual(1);
    });

    it("Vertical Cylinder Shape Test", () => {
        // Arrange
        var cylinderVerticalShapes = [];
        cylinderVerticalShapes.push(DiagramServiceMock.createShape(Shapes.CYLINDER_VERTICAL));
        const diagramMock = DiagramServiceMock.createDiagramMock(cylinderVerticalShapes, [], Diagrams.GENERIC_DIAGRAM);
        // Act
        diagramView.drawDiagram(diagramMock);
        // Assert
        // element that is part of the pool shape, this changes if the pool shape template changes
        var cylinderVerticalElement1 = element.find("path[d='M 100 114 C 100 95.4 200 95.4 200 114 L 200 186 C 200 204.6 100 204.6 100 186 L 100 114']");
        expect(cylinderVerticalElement1.length).toEqual(2);

        var cylinderVerticalElement2 = element.find("path[d='M 100 114 C 100 128 200 128 200 114']");
        expect(cylinderVerticalElement2.length).toEqual(2);
    });

    it("Horizontal Cylinder Shape Test", () => {
        // Arrange
        var cylinderHorizontalShapes = [];
        cylinderHorizontalShapes.push(DiagramServiceMock.createShape(Shapes.CYLINDER_HONRIZONTAL));
        const diagramMock = DiagramServiceMock.createDiagramMock(cylinderHorizontalShapes, [], Diagrams.GENERIC_DIAGRAM);
        // Act
        diagramView.drawDiagram(diagramMock);
        // Assert
        // element that is part of the pool shape, this changes if the pool shape template changes
        var cylinderHorizontalElement1 = element.find("path[d='M 114 100 C 95.4 100 95.4 200 114 200 L 186 200 C 204.6 200 204.6 100 186 100 L 114 100']");
        expect(cylinderHorizontalElement1.length).toEqual(2);

        var cylinderHorizontalElement2 = element.find("path[d='M 186 100 C 172 100 172 200 186 200']");
        expect(cylinderHorizontalElement2.length).toEqual(2);
    });

    it("Page Shape Test", () => {
        // Arrange
        var pageShapes = [];
        pageShapes.push(DiagramServiceMock.createShape(Shapes.PAGE));
        const diagramMock = DiagramServiceMock.createDiagramMock(pageShapes, [], Diagrams.GENERIC_DIAGRAM);
        // Act
        diagramView.drawDiagram(diagramMock);
        // Assert
        // element that is part of the pool shape, this changes if the pool shape template changes
        var pageElement1 = element.find("path[d='M 100 100 L 180 100 L 200 120 L 200 200 L 100 200 Z']");
        expect(pageElement1.length).toEqual(2);

        var pageElement2 = element.find("path[d='M 180 100 L 180 120 L 200 120']");
        expect(pageElement2.length).toEqual(2);
    });

    it("Document Shape Test", () => {
        // Arrange
        var documentShapes = [];
        documentShapes.push(DiagramServiceMock.createShape(Shapes.DOCUMENT));
        const diagramMock = DiagramServiceMock.createDiagramMock(documentShapes, [], Diagrams.GENERIC_DIAGRAM);
        // Act
        diagramView.drawDiagram(diagramMock);
        // Assert
        // element that is part of the pool shape, this changes if the pool shape template changes
        var documentElement = element.find("path[d='M 100 100 L 200 100 L 200 185 Q 175 158 150 185 Q 125 212 100 185 L 100 115 Z']");
        expect(documentElement.length).toEqual(1);
    });

    it("Trapezoid Shape Test", () => {
        // Arrange
        var trapezoidShapes = [];
        trapezoidShapes.push(DiagramServiceMock.createShape(Shapes.TRAPEZOID));
        const diagramMock = DiagramServiceMock.createDiagramMock(trapezoidShapes, [], Diagrams.GENERIC_DIAGRAM);
        // Act
        diagramView.drawDiagram(diagramMock);
        // Assert
        // element that is part of the pool shape, this changes if the pool shape template changes
        var trapezoidElement = element.find("path[d='M 100 200 L 123.5 100 L 173.5 100 L 200 200 Z']");
        expect(trapezoidElement.length).toEqual(1);
    });

    it("Parallelogram Shape Test", () => {
        // Arrange
        var parallelogramShapes = [];
        parallelogramShapes.push(DiagramServiceMock.createShape(Shapes.PARALLELOGRAM));
        const diagramMock = DiagramServiceMock.createDiagramMock(parallelogramShapes, [], Diagrams.GENERIC_DIAGRAM);
        // Act
        diagramView.drawDiagram(diagramMock);
        // Assert
        // element that is part of the pool shape, this changes if the pool shape template changes
        var parallelogramElement = element.find("path[d='M 100 200 L 123.5 100 L 197 100 L 173.5 200 Z']");
        expect(parallelogramElement.length).toEqual(1);
    });

    it("RoundedRectangle Shape Test", () => {
        // Arrange
        var roundedRectangleShapes = [];
        roundedRectangleShapes.push(DiagramServiceMock.createShape(Shapes.ROUNDED_RECTANGLE));
        const diagramMock = DiagramServiceMock.createDiagramMock(roundedRectangleShapes, [], Diagrams.GENERIC_DIAGRAM);
        // Act
        diagramView.drawDiagram(diagramMock);
        // Assert
        // element that is part of the pool shape, this changes if the pool shape template changes
        var roundedRectangleElement = element.find("rect[x='100'][y='100'][width='100'][height='100'][rx='15'][ry='15']");
        expect(roundedRectangleElement.length).toEqual(1);
    });

    it("Gradient & Shadow Test", () => {
        // Arrange
        var parallelogramShapes = [];
        var parallelogram = DiagramServiceMock.createShape(Shapes.PARALLELOGRAM);
        parallelogram.gradientFill = "true";
        parallelogram.shadow = false;
        parallelogram.fillOpacity = 1;
        parallelogramShapes.push(parallelogram);
        const diagramMock = DiagramServiceMock.createDiagramMock(parallelogramShapes, [], Diagrams.GENERIC_DIAGRAM);
        // Act
        diagramView.drawDiagram(diagramMock);
        // Assert
        // element that is part of the pool shape, this changes if the pool shape template changes
        var parallelogramElement = element.find("path[d='M 100 200 L 123.5 100 L 197 100 L 173.5 200 Z']");
        expect(parallelogramElement.length).toEqual(1);
        expect(diagramMock.shapes[0].gradientFill).toEqual("true");
        expect(diagramMock.shapes[0].fillOpacity).toEqual(1);
    });
});