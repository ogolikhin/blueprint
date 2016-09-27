import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
require("script!mxClient");

import {Shapes, ShapeProps, Diagrams, ConnectorTypes} from "./utils/constants";
import {DiagramMock, Prop} from "../diagram.mock";
import {StencilServiceMock} from "../stencil.svc.mock";
import {DiagramView} from "./diagram-view";
import {UCDLinkType} from "./utils/enums";


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

    it("Use Case Shape Test", () => {
        // Arrange
        const eventShapes = [];

        eventShapes.push(DiagramMock.createShape(Shapes.USECASE));
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.USECASE_DIAGRAM);

        // Act
        diagramView.drawDiagram(diagramMock);
        // Assert        
        const useCaseElement = element.find("ellipse[fill=white][stroke=black]");
        expect(useCaseElement.length).toEqual(1);        
    });

    it("Actor Shape Test", () => {
        // Arrange
        const eventShapes = [];

        eventShapes.push(DiagramMock.createShape(Shapes.ACTOR));
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.USECASE_DIAGRAM);

        // Act
        diagramView.drawDiagram(diagramMock);
        // Assert        
        const actorImages = element.find("image");
        expect(actorImages.length).toEqual(1);        

        const actorText = element.find("foreignObject");
        expect(actorText.length).toEqual(1);
        expect(actorText.text()).toEqual("Actor: x=100; y=100; width=100; height=100");
    });    

    it("Actor Image Shape Test", () => {
        // Arrange
        const eventShapes = [];
        const props = new Array<Prop>();
        const imageUrl = "imageUrl";
        props[0] = { name: ShapeProps.IMAGE, value: imageUrl };        
        eventShapes.push(DiagramMock.createShape(Shapes.ACTOR, props));
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.USECASE_DIAGRAM);

        // Act
        diagramView.drawDiagram(diagramMock);
        // Assert        
        const actorImages = element.find("image");
        expect(actorImages.length).toEqual(1);
        expect(actorImages.attr("xlink:href")).toContain(imageUrl);                
    });

    it("Boundary Shape Test", () => {
        // Arrange
        const eventShapes = [];

        eventShapes.push(DiagramMock.createShape(Shapes.BOUNDARY));
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.USECASE_DIAGRAM);

        // Act
        diagramView.drawDiagram(diagramMock);
        // Assert                
        const useCaseElement = element.find("rect[x=100][y=100][width=100][height=100][fill=white][stroke=black]");
        expect(useCaseElement.length).toEqual(1);
    });

    it("Use Case Include Connector Test", () => {
        // Arrange       
        const props = new Array<Prop>();        
        props[0] = { name: ShapeProps.LINK_TYPE, value: UCDLinkType.Include };     
        var points = [{ x: 100, y: 150 }, { x: 0, y: 0 }];        
        const connections = [];
        connections.push(DiagramMock.createConnection(ConnectorTypes.STRAIGHT, points, props));
        const diagramMock = DiagramMock.createDiagramMock([], connections, Diagrams.USECASE_DIAGRAM);

        // Act
        diagramView.drawDiagram(diagramMock);
        // Assert        
        const include = element.find("ellipse[stroke-width=2]");
        expect(include.length).toEqual(1);
    });

    it("Use Case Extend Connector Test", () => {
        // Arrange       
        const props = new Array<Prop>();
        props[0] = { name: ShapeProps.LINK_TYPE, value: UCDLinkType.Extended };
        var points = [{ x: 100, y: 150 }, { x: 0, y: 0 }];        
        const connections = [];
        connections.push(DiagramMock.createConnection(ConnectorTypes.STRAIGHT, points, props));
        const diagramMock = DiagramMock.createDiagramMock([], connections, Diagrams.USECASE_DIAGRAM);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert        
        const include = element.find("ellipse");
        expect(include.length).toEqual(0);
        const extend = element.find("path[fill=black]");
        // For extend we expect 2 pathes: connectors arrow and extend rhombus
        expect(extend.length).toEqual(2);
    });

    it("Use Case Include and Extend Connector Test", () => {
        // Arrange       
        const props = new Array<Prop>();
        props[0] = { name: ShapeProps.LINK_TYPE, value: UCDLinkType.IncludeAndExtended };
        var points = [{ x: 100, y: 150 }, { x: 0, y: 0 }];
        const connections = [];
        connections.push(DiagramMock.createConnection(ConnectorTypes.STRAIGHT, points, props));
        const diagramMock = DiagramMock.createDiagramMock([], connections, Diagrams.USECASE_DIAGRAM);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert        
        const include = element.find("ellipse");
        expect(include.length).toEqual(1);
        const extend = element.find("path[fill=black]");
        // For include and extend between the same use cases we expect 3 pathes: 2 connectors arrows and extend rhombus
        expect(extend.length).toEqual(3);
    });

    it("Use Case Actor Inheritance Connector Test", () => {
        // Arrange       
        const props = new Array<Prop>();
        props[0] = { name: ShapeProps.LINK_TYPE, value: UCDLinkType.ActorInheritance };
        var points = [{ x: 100, y: 150 }, { x: 0, y: 0 }];
        const connections = [];
        connections.push(DiagramMock.createConnection(ConnectorTypes.STRAIGHT, points, props));
        const diagramMock = DiagramMock.createDiagramMock([], connections, Diagrams.USECASE_DIAGRAM);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert        
        const include = element.find("ellipse");
        expect(include.length).toEqual(0);
        const extend = element.find("path[stroke=black][fill=none]");
        expect(extend.length).toEqual(2);
    });    
});
