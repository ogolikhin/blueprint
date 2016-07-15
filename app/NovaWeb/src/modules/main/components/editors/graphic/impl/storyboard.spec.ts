import "angular";
import "angular-mocks";
import "angular-sanitize";
require("script!mxClient");

import {Shapes, ShapeProps, Diagrams, ConnectorTypes} from "./utils/constants";
import {DiagramServiceMock, Prop} from '../diagram.svc.mock';
import {StencilServiceMock} from '../stencil.svc.mock';
import {Point} from "../impl/models";
import {ProjectManager} from "../../../../services/project-manager";
import {ProjectRepository} from "../../../../services/project-repository";
import {MessageServiceMock} from "../../../../../shell/messages/message.mock";
import {LocalizationServiceMock} from "../../../../../core/localization.mock";
import {DiagramView} from "./diagram-view";

describe("Rendering common shapes", () => {
    let element: ng.IAugmentedJQuery;
    let diagramView: DiagramView;

    beforeEach(angular.mock.module("ngSanitize", ($provide: ng.auto.IProvideService, $compileProvider: ng.ICompileProvider) => {     
        $provide.service("stencilService", StencilServiceMock);
        $provide.service("diagramService", DiagramServiceMock);
        $provide.service("projectManager", ProjectManager);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("projectRepository", ProjectRepository);
    }));

    beforeEach(inject((stencilService: StencilServiceMock) => {
        const divElement = document.createElement("div");
        element = angular.element(divElement);
        diagramView = new DiagramView(element[0], stencilService);
    }));


    it("Frame Shape Test, First Frame With Mockup", inject(($compile: ng.ICompileService, diagramService: DiagramServiceMock, $rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
        // Arrange
        const eventShapes = [];
        const props = new Array<Prop>();
        props[0] = { name: ShapeProps.HAS_MOCKUP, value: false };
        props[1] = { name: ShapeProps.IS_FIRST, value: true };
        eventShapes.push(DiagramServiceMock.createShape(Shapes.FRAME, props, 1, 100, 100, 100, 250));
        const diagramMock = DiagramServiceMock.createDiagramMock(eventShapes, [], Diagrams.STORYBOARD);      

        // Act
        diagramView.drawDiagram(diagramMock);
        
        // Assert
        const frameBoundary = element.find("rect[x='100'][y='100'][width='100'][height='250'][stroke='none']");
        expect(frameBoundary.length).toEqual(1);

        const border = element.find("rect[x='100'][y='148'][width='100'][height='154'][stroke='black']");
        expect(border.length).toEqual(1);

        const indicator = element.find("image[x='100'][y='286'][width='16'][height='16']");
        expect(indicator.length).toEqual(1);

        const labelShape = element.find("rect[x='100'][y='100'][width='100'][height='48'][fill='none'][stroke='none']");
        expect(labelShape.length).toEqual(1);
    }));

    it("Frame Shape Test, Not First Frame With Mockup", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, diagramService: DiagramServiceMock, projectManager: ProjectManager) => {
        // Arrange
        const eventShapes = [];
        const props = new Array<Prop>();
        props[0] = { name: ShapeProps.HAS_MOCKUP, value: true };
        props[1] = { name: ShapeProps.IS_FIRST, value: false };
        eventShapes.push(DiagramServiceMock.createShape(Shapes.FRAME, props, 1, 100, 100, 100, 250));
        const diagramMock = DiagramServiceMock.createDiagramMock(eventShapes, [], Diagrams.STORYBOARD);
     
        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const frameBoundary = element.find("rect[x='100'][y='100'][width='100'][height='250'][stroke='none']");
        expect(frameBoundary.length).toEqual(1);

        const border = element.find("rect[x='100'][y='148'][width='100'][height='154'][stroke='black']");
        expect(border.length).toEqual(1);

        const indicator = element.find("rect[width='16'][height='16'][fill='#c3e4f5'][stroke='#455261']");
        expect(indicator.length).toEqual(0);

        const labelShape = element.find("rect[x='100'][y='100'][width='100'][height='48'][fill='none'][stroke='none']");
        expect(labelShape.length).toEqual(1);
    }));

    it("Frame Shape Test, Description at the bottom of the shape", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, diagramService: DiagramServiceMock, projectManager: ProjectManager) => {
        // Arrange
        const eventShapes = [];
        const props = new Array<Prop>();
        props[0] = { name: ShapeProps.HAS_MOCKUP, value: true };
        props[1] = { name: ShapeProps.IS_FIRST, value: false };
        const shape = DiagramServiceMock.createShape(Shapes.FRAME, props, 1, 100, 100, 100, 250);
        shape.description = DiagramServiceMock.createRichText("test description text");       
        eventShapes.push(shape);
        const diagramMock = DiagramServiceMock.createDiagramMock(eventShapes, [], Diagrams.STORYBOARD);
        
        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const descriptionShape = element.find("rect[x='100'][y='302'][width='100'][height='48'][fill='none'][stroke='none']");
        expect(descriptionShape.length).toEqual(1);
    }));

    it("Frame Shape Test, Description at the center of the shape", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, diagramService: DiagramServiceMock, projectManager: ProjectManager) => {
        // Arrange
        const eventShapes = [];
        const props = new Array<Prop>();
        props[0] = { name: ShapeProps.HAS_MOCKUP, value: false };
        props[1] = { name: ShapeProps.IS_FIRST, value: false };
        const shape = DiagramServiceMock.createShape(Shapes.FRAME, props, 1, 100, 100, 100, 250);
        shape.description = DiagramServiceMock.createRichText("test description text");
        eventShapes.push(shape);
        const diagramMock = DiagramServiceMock.createDiagramMock(eventShapes, [], Diagrams.STORYBOARD);
       
        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const descriptionShape = element.find("rect[x='100'][y='148'][width='100'][height='154'][fill='none'][stroke='none']");
        expect(descriptionShape.length).toEqual(1);
    }));

    it("Frame Shape Test, Description at the center of the shape", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, diagramService: DiagramServiceMock, projectManager: ProjectManager) => {
        // Arrange
        const eventShapes = [];
        const props = new Array<Prop>();
        props[0] = { name: ShapeProps.HAS_MOCKUP, value: false };
        props[1] = { name: ShapeProps.IS_FIRST, value: false };
        const shape = DiagramServiceMock.createShape(Shapes.FRAME, props, 1, 100, 100, 100, 250);
        shape.description = DiagramServiceMock.createRichText("test description text");
        eventShapes.push(shape);
        const diagramMock = DiagramServiceMock.createDiagramMock(eventShapes, [], Diagrams.STORYBOARD);
        
        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const descriptionShape = element.find("rect[x='100'][y='148'][width='100'][height='154'][fill='none'][stroke='none']");
        expect(descriptionShape.length).toEqual(1);
    }));

    it("Frame Shape Test, Two connected frames", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, diagramService: DiagramServiceMock, projectManager: ProjectManager) => {
        // Arrange
        const eventShapes = [];
        const props = new Array<Prop>();
        props[0] = { name: ShapeProps.HAS_MOCKUP, value: false };
        props[1] = { name: ShapeProps.IS_FIRST, value: false };
       
        eventShapes.push(DiagramServiceMock.createShape(Shapes.FRAME, props, 1, 1, 50, 100, 250));
        eventShapes.push(DiagramServiceMock.createShape(Shapes.FRAME, props, 2, 300, 50, 100, 250));

        const connections = [];
        const points = [{ x: 51, y: 252 }, { x: 51, y: 310 }, { x: 150, y: 310 }, { x: 150, y: 25 }, { x: 350, y: 25 }, { x: 350, y: 98 }];
        const connection = DiagramServiceMock.createConnection(ConnectorTypes.RIGHT_ANGLED, points);
        connection.sourceId = 1;
        connection.targetId = 1;

        connections.push(connection);

        const diagramMock = DiagramServiceMock.createDiagramMock(eventShapes, connections, Diagrams.STORYBOARD);
       
        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const frame1 = element.find("rect[x='1'][y='50'][width='100'][height='250'][fill='white'][stroke='none']");
        expect(frame1.length).toEqual(1);

        const frame2 = element.find("rect[x='300'][y='50'][width='100'][height='250'][fill='white'][stroke='none']");
        expect(frame2.length).toEqual(1);

        const pathElement = element.find("path");
        expect(pathElement.length).toBe(2);

        let visibility = pathElement[0].getAttribute("visibility");
        expect(visibility).toEqual("hidden");

        let dAttribute = pathElement[0].getAttribute("d");
        expect(dAttribute).toEqual("M 51 252 L 51 310 L 150 310 L 150 25 L 350 25 L 350 98");

        visibility = pathElement[1].getAttribute("visibility");
        expect(visibility).toBeNull();
        dAttribute = pathElement[1].getAttribute("d");
        expect(dAttribute).toEqual("M 51 252 L 51 310 L 150 310 L 150 25 L 350 25 L 350 98");

    }));
});