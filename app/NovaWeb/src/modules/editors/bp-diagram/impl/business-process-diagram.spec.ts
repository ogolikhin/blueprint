/* tslint:disable:max-line-length */
import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
require("script!mxClient");

import {Diagrams, ConnectorTypes} from "./utils/constants";
import {DiagramMock, Prop} from "../diagram.mock";
import {StencilServiceMock} from "../stencil.svc.mock";
import {Point} from "../impl/models";
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

    it("Pool Shape Test", () => {
        // Arrange
        const eventShapes = [];
        eventShapes.push(DiagramMock.createShape("Pool"));
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);
        // Assert
        // element that is part of the pool shape, this changes if the pool shape template changes
        const poolElement1 = element.find("path[d='M 155 100 L 100 100 L 100 200 L 155 200 Z']");
        expect(poolElement1.length).toEqual(1);
        const poolElement2 = element.find("path[d='M 155 100 L 200 100 L 200 200 L 155 200']");
        expect(poolElement2.length).toEqual(2);
    });

    it("Lane Shape Test", () => {
        // Arrange
        const eventShapes = [];
        eventShapes.push(DiagramMock.createShape("Lane"));
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.BUSINESS_PROCESS);
        // Act
        diagramView.drawDiagram(diagramMock);


        // Assert
        // element that is part of the Lane shape, this changes if the Lane shape template changes
        const laneElement1 = element.find("path[d='M 155 100 L 100 100 L 100 200 L 155 200 Z']");
        expect(laneElement1.length).toEqual(1);
        const laneElement2 = element.find("path[d='M 155 100 L 200 100 L 200 200 L 155 200']");
        expect(laneElement2.length).toEqual(2);
    });

    it("Message Shape Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "IsInitiating", value: true};
        const messageShapes = [];
        messageShapes.push(DiagramMock.createShape("Message", props));
        const diagramMock = DiagramMock.createDiagramMock(messageShapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);


        // Assert
        const messagePathElement1 = element.find("path[d='M 100 100 L 200 100 L 200 200 L 100 200 L 100 100']");
        expect(messagePathElement1.length).toEqual(2);
        const messagePathElement2 = element.find("path[d='M 100 100 L 150 150 L 200 100']");
        expect(messagePathElement2.length).toEqual(2);
    });

    it("Label Test", () => {
        // Arrange
        const props = new Array<Prop>();
        const shapes = [];
        shapes.push(DiagramMock.createShape("DataObject", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);


        // Assert
        const expectedLabelText = "DataObject: x=100; y=100; width=100; height=100";
        expect(element.find("span").text()).toEqual(expectedLabelText);
    });

    it("Data Object Shape Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "IsCollection", value: true};
        const shapes = [];
        shapes.push(DiagramMock.createShape("DataObject", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);


        // Assert
        // elements which are part of the data object shape(3 parts)
        const dataObjectPathElement1 = element.find("path[d='M 100 100 L 100 200 L 200 200 L 200 130 L 162.5 100 L 100 100']");
        expect(dataObjectPathElement1.length).toEqual(2);
        const dataObjectPathElement2 = element.find("path[d='M 162.5 100 L 162.5 130 L 200 130']");
        expect(dataObjectPathElement2.length).toEqual(2);
        const dataObjectPathElement3 = element.find("path[d='M 142.5 180 L 142.5 200 M 150 180 L 150 200 M 157.5 180 L 157.5 200']");
        expect(dataObjectPathElement3.length).toEqual(2);
    });

    it("Data Store Shape Test", () => {
        // Arrange
        const props = new Array<Prop>();
        const shapes = [];
        shapes.push(DiagramMock.createShape("DataStore", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);


        // Assert
        const dataStoreElement = element.find("rect");
        expect(dataStoreElement).toBeDefined();
    });

    it("Gateway Shape Test", () => {
        // Arrange
        const props = new Array<Prop>();
        const shapes = [];
        shapes.push(DiagramMock.createShape("GateWay", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);


        // Assert
        const gatewayElement = element.find("rect");
        expect(gatewayElement).toBeDefined();
    });

    it("Group Shape Test", () => {
        // Arrange
        const props = new Array<Prop>();
        const shapes = [];
        shapes.push(DiagramMock.createShape("Group", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);


        // Assert
        const groupShapeElement = element.find("rect");
        expect(groupShapeElement).toBeDefined();
    });

    it("Task Shape Test", () => {
        // Arrange
        const props = new Array<Prop>();
        const shapes = [];
        shapes.push(DiagramMock.createShape("Task", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);


        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const taskElements = element.find("rect");
        expect(taskElements.length).toEqual(1);
    });

    it("Event Shape(Start Message) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "Start"};
        props[1] = {name: "EventTrigger", value: "Message"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);


        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(1);

        const eventPathElement1 = element.find("path[d='M 125 130.77 L 175 130.77 L 175 169.23 L 125 169.23 L 125 130.77 Z'][fill=white]");
        expect(eventPathElement1.length).toEqual(1);
        const eventPathElement2 = element.find("path[d='M 125 130.77 L 150 150 L 175 130.77']");
        expect(eventPathElement2.length).toEqual(2);
    });

    it("Event Shape(IntermediateThrowing Message) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "IntermediateThrowing"};
        props[1] = {name: "EventTrigger", value: "Message"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(2);

        const eventPathElement1 = element.find("path[d='M 125 130.77 L 175 130.77 L 175 169.23 L 125 169.23 L 125 130.77 Z'][fill=black]");
        expect(eventPathElement1.length).toEqual(1);
        const eventPathElement2 = element.find("path[d='M 125 130.77 L 150 150 L 175 130.77']");
        expect(eventPathElement2.length).toEqual(2);
    });

    it("Event Shape(Start Error) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "Start"};
        props[1] = {name: "EventTrigger", value: "Error"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(1);

        const eventPathElement1 = element.find("path[d='M 141.5 125 L 125 160 L 138 147 L 160 175 L 175 140 L 161 154 L 141.5 125 Z'][fill=white]");
        expect(eventPathElement1.length).toEqual(1);
    });

    it("Event Shape(End Error) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "End"};
        props[1] = {name: "EventTrigger", value: "Error"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(1);

        const eventPathElement1 = element.find("path[d='M 141.5 125 L 125 160 L 138 147 L 160 175 L 175 140 L 161 154 L 141.5 125 Z'][fill=black]");
        expect(eventPathElement1.length).toEqual(1);
    });

    it("Event Shape(Start Timer) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "Start"};
        props[1] = {name: "EventTrigger", value: "Timer"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(2);

        const eventPathElement1 = element.find("path[d='M 149.65 149.65 L 151.05 126.2 M 149.65 149.65 L 165.05 149.65 M 149.65 122.35 L 149.65 125.85 M 163.3 125.92 L 161.34 129.35 M 173.24 135.86 L 169.81 137.96 M 176.95 149.65 L 173.45 149.65 M 173.24 163.44 L 169.81 161.34 M 136 125.92 L 137.96 129.35 M 163.3 173.38 L 161.34 169.95 M 149.65 173.45 L 149.65 176.95 M 136 173.38 L 137.96 169.95 M 126.06 163.44 L 129.49 161.34 M 122.35 149.65 L 125.85 149.65 M 126.06 135.86 L 129.49 137.96']");
        expect(eventPathElement1.length).toEqual(1);
    });

    it("Event Shape(Start Compensation) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "Start"};
        props[1] = {name: "EventTrigger", value: "Compensation"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(1);

        const eventPathElement1 = element.find("path[d='M 122.5 150 L 147.5 125 L 147.5 175 Z M 147.5 150 L 172.5 125 L 172.5 175 Z'][fill=white]");
        expect(eventPathElement1.length).toEqual(1);
    });

    it("Event Shape(End Compensation) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "End"};
        props[1] = {name: "EventTrigger", value: "Compensation"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(1);

        const eventPathElement1 = element.find("path[d='M 122.5 150 L 147.5 125 L 147.5 175 Z M 147.5 150 L 172.5 125 L 172.5 175 Z'][fill=black]");
        expect(eventPathElement1.length).toEqual(1);
    });


    it("Event Shape(IntermediateCatching Cancel) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "IntermediateCatching"};
        props[1] = {name: "EventTrigger", value: "Cancel"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(2);

        const eventPathElement1 = element.find("path[d='M 132 120 L 150 138 L 168 120 L 180 132 L 162 150 L 180 168 L 168 180 L 150 162 L 132 180 L 120 168 L 138 150 L 120 132 L 132 120 Z'][fill=white]");
        expect(eventPathElement1.length).toEqual(1);
    });

    it("Event Shape(End Cancel) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "End"};
        props[1] = {name: "EventTrigger", value: "Cancel"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(1);

        const eventPathElement1 = element.find("path[d='M 132 120 L 150 138 L 168 120 L 180 132 L 162 150 L 180 168 L 168 180 L 150 162 L 132 180 L 120 168 L 138 150 L 120 132 L 132 120 Z'][fill=black]");
        expect(eventPathElement1.length).toEqual(1);
    });

    it("Event Shape(Start Conditional) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "Start"};
        props[1] = {name: "EventTrigger", value: "Conditional"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(1);

        const eventPathElement1 = element.find("path[d='M 123.75 120 L 176.25 120 L 176.25 180 L 123.75 180 L 123.75 120 Z M 127.5 132 L 172.5 132 M 127.5 144 L 172.5 144 M 127.5 156 L 172.5 156 M 127.5 168 L 172.5 168']");
        expect(eventPathElement1.length).toEqual(1);
    });

    it("Event Shape(Start Signal) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "Start"};
        props[1] = {name: "EventTrigger", value: "Signal"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(1);

        const eventPathElement1 = element.find("path[d='M 150 130 L 175 170 L 125 170 L 150 130 Z'][fill=white]");
        expect(eventPathElement1.length).toEqual(1);
    });

    it("Event Shape(End Signal) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "End"};
        props[1] = {name: "EventTrigger", value: "Signal"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(1);

        const eventPathElement1 = element.find("path[d='M 150 130 L 175 170 L 125 170 L 150 130 Z'][fill=black]");
        expect(eventPathElement1.length).toEqual(1);
    });

    it("Event Shape(Intermediate Catching Link) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "IntermediateCatching"};
        props[1] = {name: "EventTrigger", value: "Link"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(2);

        const eventPathElement1 = element.find("path[d='M 125 142 L 160 142 L 160 130 L 175 150 L 160 170 L 160 158 L 125 158 L 125 142 Z'][fill=white]");
        expect(eventPathElement1.length).toEqual(1);
    });

    it("Event Shape(Intermediate Throwing Link) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "IntermediateThrowing"};
        props[1] = {name: "EventTrigger", value: "Link"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(2);

        const eventPathElement1 = element.find("path[d='M 125 142 L 160 142 L 160 130 L 175 150 L 160 170 L 160 158 L 125 158 L 125 142 Z'][fill=black]");
        expect(eventPathElement1.length).toEqual(1);
    });

    it("Event Shape(Start Multiple) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "Start"};
        props[1] = {name: "EventTrigger", value: "Multiple"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(1);

        const eventPathElement1 = element.find("path[d='M 150 125 L 175 144 L 165 175 L 135 175 L 125 144 L 150 125 Z'][fill=white]");
        expect(eventPathElement1.length).toEqual(1);
    });

    it("Event Shape(End Multiple) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "End"};
        props[1] = {name: "EventTrigger", value: "Multiple"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(1);

        const eventPathElement1 = element.find("path[d='M 150 125 L 175 144 L 165 175 L 135 175 L 125 144 L 150 125 Z'][fill=black]");
        expect(eventPathElement1.length).toEqual(1);
    });

    it("Event Shape(Start Escalation) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "Start"};
        props[1] = {name: "EventTrigger", value: "Escalation"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(1);

        const eventPathElement1 = element.find("path[d='M 150 125 L 168 175 L 150 155 L 132 175 L 150 125 Z'][fill=white]");
        expect(eventPathElement1.length).toEqual(1);
    });

    it("Event Shape(End Escalation) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "End"};
        props[1] = {name: "EventTrigger", value: "Escalation"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(1);

        const eventPathElement1 = element.find("path[d='M 150 125 L 168 175 L 150 155 L 132 175 L 150 125 Z'][fill=black]");
        expect(eventPathElement1.length).toEqual(1);
    });

    it("Event Shape(Start ParallelMultiple) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "Start"};
        props[1] = {name: "EventTrigger", value: "ParallelMultiple"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(1);

        const eventPathElement1 = element.find("path[d='M 144 112.5 L 156 112.5 L 156 144 L 187.5 144 L 187.5 156 L 156 156 L 156 187.5 L 144 187.5 L 144 156 L 112.5 156 L 112.5 144 L 144 144 L 144 112.5 Z'][fill=white]");
        expect(eventPathElement1.length).toEqual(1);
    });

    it("Event Shape(End Terminate) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "EventType", value: "End"};
        props[1] = {name: "EventTrigger", value: "Terminate"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Event", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        // elements which are part of the task shape(2 parts, since the mock has LoopType property set to ParallelMultiInstance)
        const ellipseComponent = element.find("ellipse");
        expect(ellipseComponent.length).toEqual(2);

        const filledEllipseElement = element.find("ellipse[rx=25][ry=25][fill=black]");
        expect(filledEllipseElement.length).toEqual(1);
    });

    it("Gateway (ExclusiveData) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "GatewayType", value: "ExclusiveData"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Gateway", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const pathFinder = element.find("path");
        expect(pathFinder.length).toEqual(1);

        const dAttribute = pathFinder[0].getAttribute("d");
        expect(dAttribute).toEqual("M 150 100 L 200 150 L 150 200 L 100 150 Z");

        const visibilityAttribute = pathFinder[0].getAttribute("visibility");
        expect(visibilityAttribute).toBeNull();
    });

    it("Gateway (ExclusiveDataWithMarker) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props.push({name: "GatewayType", value: "ExclusiveDataWithMarker"});
        const shapes = [];
        shapes.push(DiagramMock.createShape("Gateway", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const pathFinder = element.find("path");
        expect(pathFinder.length).toEqual(3);

        let dAttribute = pathFinder[0].getAttribute("d");
        expect(dAttribute).toEqual("M 150 100 L 200 150 L 150 200 L 100 150 Z");
        let visibilityAttribute = pathFinder[0].getAttribute("visibility");
        expect(visibilityAttribute).toBeNull();

        dAttribute = pathFinder[1].getAttribute("d");
        expect(dAttribute).toEqual("M 130 125 L 140.1 125 L 170 175 L 159.9 175 L 130 125 Z M 159.9 125 L 170 125 L 140.1 175 L 130 175 L 159.9 125 Z");
        visibilityAttribute = pathFinder[1].getAttribute("visibility");
        expect(visibilityAttribute).toEqual("hidden");

        dAttribute = pathFinder[2].getAttribute("d");
        expect(dAttribute).toEqual("M 130 125 L 140.1 125 L 170 175 L 159.9 175 L 130 125 Z M 159.9 125 L 170 125 L 140.1 175 L 130 175 L 159.9 125 Z");
        visibilityAttribute = pathFinder[2].getAttribute("visibility");
        expect(visibilityAttribute).toBeNull();
    });

    it("Gateway (ExclusiveEvent) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props.push({name: "GatewayType", value: "ExclusiveEvent"});
        const shapes = [];
        shapes.push(DiagramMock.createShape("Gateway", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const pathFinder = element.find("path");
        expect(pathFinder.length).toEqual(3);

        let dAttribute = pathFinder[0].getAttribute("d");
        expect(dAttribute).toEqual("M 150 100 L 200 150 L 150 200 L 100 150 Z");
        let visibilityAttribute = pathFinder[0].getAttribute("visibility");
        expect(visibilityAttribute).toBeNull();

        dAttribute = pathFinder[1].getAttribute("d");
        expect(dAttribute).toEqual("M 150.3 129.09 L 130.3 143.64 L 137.58 167.27 L 163.03 167.27 L 170.3 143.64 Z");
        visibilityAttribute = pathFinder[1].getAttribute("visibility");
        expect(visibilityAttribute).toEqual("hidden");

        dAttribute = pathFinder[2].getAttribute("d");
        expect(dAttribute).toEqual("M 150.3 129.09 L 130.3 143.64 L 137.58 167.27 L 163.03 167.27 L 170.3 143.64 Z");
        visibilityAttribute = pathFinder[2].getAttribute("visibility");
        expect(visibilityAttribute).toBeNull();

        const ellipseFinder = element.find("ellipse");
        expect(ellipseFinder.length).toEqual(2);

        let cxAttribute = ellipseFinder[0].getAttribute("cx");
        expect(parseFloat(cxAttribute)).toBeCloseTo(150, 0);
        let cyAttribute = ellipseFinder[0].getAttribute("cy");
        expect(parseFloat(cyAttribute)).toBeCloseTo(150, 0);
        let rxAttribute = ellipseFinder[0].getAttribute("rx");
        expect(parseFloat(rxAttribute)).toBeCloseTo(30, 0);
        let ryAttribute = ellipseFinder[0].getAttribute("ry");
        expect(parseFloat(ryAttribute)).toBeCloseTo(30, 0);

        cxAttribute = ellipseFinder[1].getAttribute("cx");
        expect(parseFloat(cxAttribute)).toBeCloseTo(150, 0);
        cyAttribute = ellipseFinder[1].getAttribute("cy");
        expect(parseFloat(cyAttribute)).toBeCloseTo(150, 0);
        rxAttribute = ellipseFinder[1].getAttribute("rx");
        expect(parseFloat(rxAttribute)).toBeCloseTo(25.75, 1);
        ryAttribute = ellipseFinder[1].getAttribute("ry");
        expect(parseFloat(ryAttribute)).toBeCloseTo(25.75, 1);
    });

    it("Gateway (ExclusiveEventInstantiate) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props.push({name: "GatewayType", value: "ExclusiveEventInstantiate"});
        const shapes = [];
        shapes.push(DiagramMock.createShape("Gateway", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const pathFinder = element.find("path");
        expect(pathFinder.length).toEqual(3);

        let dAttribute = pathFinder[0].getAttribute("d");
        expect(dAttribute).toEqual("M 150 100 L 200 150 L 150 200 L 100 150 Z");
        let visibilityAttribute = pathFinder[0].getAttribute("visibility");
        expect(visibilityAttribute).toBeNull();

        dAttribute = pathFinder[1].getAttribute("d");
        expect(dAttribute).toEqual("M 150.3 126.06 L 127.27 143.03 L 135.76 169.7 L 164.85 169.7 L 173.33 143.03 Z");
        visibilityAttribute = pathFinder[1].getAttribute("visibility");
        expect(visibilityAttribute).toEqual("hidden");

        dAttribute = pathFinder[2].getAttribute("d");
        expect(dAttribute).toEqual("M 150.3 126.06 L 127.27 143.03 L 135.76 169.7 L 164.85 169.7 L 173.33 143.03 Z");
        visibilityAttribute = pathFinder[2].getAttribute("visibility");
        expect(visibilityAttribute).toBeNull();

        const ellipseFinder = element.find("ellipse");
        expect(ellipseFinder.length).toEqual(1);

        const cxAttribute = ellipseFinder[0].getAttribute("cx");
        expect(parseFloat(cxAttribute)).toBeCloseTo(150, 0);
        const cyAttribute = ellipseFinder[0].getAttribute("cy");
        expect(parseFloat(cyAttribute)).toBeCloseTo(150, 0);
        const rxAttribute = ellipseFinder[0].getAttribute("rx");
        expect(parseFloat(rxAttribute)).toBeCloseTo(30, 0);
        const ryAttribute = ellipseFinder[0].getAttribute("ry");
        expect(parseFloat(ryAttribute)).toBeCloseTo(30, 0);
    });

    it("Gateway (ParallelEventInstantiate) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props.push({name: "GatewayType", value: "ParallelEventInstantiate"});
        const shapes = [];
        shapes.push(DiagramMock.createShape("Gateway", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const pathFinder = element.find("path");
        expect(pathFinder.length).toEqual(3);

        let dAttribute = pathFinder[0].getAttribute("d");
        expect(dAttribute).toEqual("M 150 100 L 200 150 L 150 200 L 100 150 Z");
        let visibilityAttribute = pathFinder[0].getAttribute("visibility");
        expect(visibilityAttribute).toBeNull();

        dAttribute = pathFinder[1].getAttribute("d");
        expect(dAttribute).toEqual("M 146.36 128.18 L 153.64 128.18 L 153.64 146.36 L 171.82 146.36 L 171.82 153.64 L 153.64 153.64 L 153.64 171.82 L 146.36 171.82 L 146.36 153.64 L 128.18 153.64 L 128.18 146.36 L 146.36 146.36 Z");
        visibilityAttribute = pathFinder[1].getAttribute("visibility");
        expect(visibilityAttribute).toEqual("hidden");

        dAttribute = pathFinder[2].getAttribute("d");
        expect(dAttribute).toEqual("M 146.36 128.18 L 153.64 128.18 L 153.64 146.36 L 171.82 146.36 L 171.82 153.64 L 153.64 153.64 L 153.64 171.82 L 146.36 171.82 L 146.36 153.64 L 128.18 153.64 L 128.18 146.36 L 146.36 146.36 Z");
        visibilityAttribute = pathFinder[2].getAttribute("visibility");
        expect(visibilityAttribute).toBeNull();

        const ellipseFinder = element.find("ellipse");
        expect(ellipseFinder.length).toEqual(1);

        const cxAttribute = ellipseFinder[0].getAttribute("cx");
        expect(parseFloat(cxAttribute)).toBeCloseTo(150, 0);
        const cyAttribute = ellipseFinder[0].getAttribute("cy");
        expect(parseFloat(cyAttribute)).toBeCloseTo(150, 0);
        const rxAttribute = ellipseFinder[0].getAttribute("rx");
        expect(parseFloat(rxAttribute)).toBeCloseTo(30, 0);
        const ryAttribute = ellipseFinder[0].getAttribute("ry");
        expect(parseFloat(ryAttribute)).toBeCloseTo(30, 0);
    });

    it("Gateway (Inclusive) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props.push({name: "GatewayType", value: "Inclusive"});
        const shapes = [];
        shapes.push(DiagramMock.createShape("Gateway", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const pathFinder = element.find("path");
        expect(pathFinder.length).toEqual(2);

        let dAttribute = pathFinder[0].getAttribute("d");
        expect(dAttribute).toEqual("M 150 100 L 200 150 L 150 200 L 100 150 Z");

        dAttribute = pathFinder[1].getAttribute("d");
        expect(dAttribute).toEqual("M 120 150 C 120 133.43 133.43 120 150 120 C 166.57 120 180 133.43 180 150 C 180 166.57 166.57 180 150 180 C 133.43 180 120 166.57 120 150 Z M 127.5 150 C 127.5 162.43 137.57 172.5 150 172.5 C 162.43 172.5 172.5 162.43 172.5 150 C 172.5 137.57 162.43 127.5 150 127.5 C 137.57 127.5 127.5 137.57 127.5 150 Z");
    });

    it("Gateway (Parallel) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props.push({name: "GatewayType", value: "Parallel"});
        const shapes = [];
        shapes.push(DiagramMock.createShape("Gateway", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const pathFinder = element.find("path");
        expect(pathFinder.length).toEqual(3);

        let dAttribute = pathFinder[0].getAttribute("d");
        expect(dAttribute).toEqual("M 150 100 L 200 150 L 150 200 L 100 150 Z");
        let visibilityAttribute = pathFinder[0].getAttribute("visibility");
        expect(visibilityAttribute).toBeNull();

        dAttribute = pathFinder[1].getAttribute("d");
        expect(dAttribute).toEqual("M 143.94 120 L 156.06 120 L 156.06 143.94 L 180 143.94 L 180 156.06 L 156.06 156.06 L 156.06 180 L 143.94 180 L 143.94 156.06 L 120 156.06 L 120 143.94 L 143.94 143.94 Z");
        visibilityAttribute = pathFinder[1].getAttribute("visibility");
        expect(visibilityAttribute).toEqual("hidden");

        dAttribute = pathFinder[2].getAttribute("d");
        expect(dAttribute).toEqual("M 143.94 120 L 156.06 120 L 156.06 143.94 L 180 143.94 L 180 156.06 L 156.06 156.06 L 156.06 180 L 143.94 180 L 143.94 156.06 L 120 156.06 L 120 143.94 L 143.94 143.94 Z");
        visibilityAttribute = pathFinder[2].getAttribute("visibility");
        expect(visibilityAttribute).toBeNull();
    });

    it("Gateway (Complex) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props.push({name: "GatewayType", value: "Complex"});
        const shapes = [];
        shapes.push(DiagramMock.createShape("Gateway", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const pathFinder = element.find("path");
        expect(pathFinder.length).toEqual(3);

        let dAttribute = pathFinder[0].getAttribute("d");
        expect(dAttribute).toEqual("M 150 100 L 200 150 L 150 200 L 100 150 Z");
        let visibilityAttribute = pathFinder[0].getAttribute("visibility");
        expect(visibilityAttribute).toBeNull();

        dAttribute = pathFinder[1].getAttribute("d");
        expect(dAttribute).toEqual("M 120 146.4 L 141.6 146.4 L 126 130.8 L 130.8 126 L 146.4 141.6 L 146.4 120 L 153.6 120 L 153.6 141.6 L 169.2 126 L 174 130.8 L 158.4 146.4 L 180 146.4 L 180 153.6 L 158.4 153.6 L 174 169.2 L 169.2 174 L 153.6 158.4 L 153.6 180 L 146.4 180 L 146.4 158.4 L 130.8 174 L 126 169.2 L 141.6 153.6 L 120 153.6 Z");
        visibilityAttribute = pathFinder[1].getAttribute("visibility");
        expect(visibilityAttribute).toEqual("hidden");

        dAttribute = pathFinder[2].getAttribute("d");
        expect(dAttribute).toEqual("M 120 146.4 L 141.6 146.4 L 126 130.8 L 130.8 126 L 146.4 141.6 L 146.4 120 L 153.6 120 L 153.6 141.6 L 169.2 126 L 174 130.8 L 158.4 146.4 L 180 146.4 L 180 153.6 L 158.4 153.6 L 174 169.2 L 169.2 174 L 153.6 158.4 L 153.6 180 L 146.4 180 L 146.4 158.4 L 130.8 174 L 126 169.2 L 141.6 153.6 L 120 153.6 Z");
        visibilityAttribute = pathFinder[2].getAttribute("visibility");
        expect(visibilityAttribute).toBeNull();
    });

    it("Task Shape(Standard+adHoc+Compensation+Call) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "LoopType", value: "Standard"};
        props[1] = {name: "IsAdHoc", value: true};
        props[2] = {name: "TaskType", value: "None"};
        props[3] = {name: "IsCompensation", value: true};
        props[4] = {name: "IsCollapsed", value: true};
        props[5] = {name: "BoundaryType", value: "Call"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Task", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const rectElement = element.find("rect[x=100][y=100]");
        expect(rectElement.length).toEqual(1);

        const taskPathElement1 = element.find("path[d='M 105 114 C 105 109.03 109.03 105 114 105 L 186 105 C 188.39 105 190.68 105.95 192.36 107.64 C 194.05 109.32 195 111.61 195 114 L 195 186 C 195 188.39 194.05 190.68 192.36 192.36 C 190.68 194.05 188.39 195 186 195 L 114 195 C 111.61 195 109.32 194.05 107.64 192.36 C 105.95 190.68 105 188.39 105 186 Z']");
        expect(taskPathElement1.length).toEqual(1);
        const taskPathElement2 = element.find("path[d='M 134 181.5 L 140.5 175 L 140.5 188 Z M 140.5 181.5 L 147 175 L 147 188 Z']");
        expect(taskPathElement2.length).toEqual(1);
        const taskPathElement3 = element.find("path[d='M 172 181.5 C 172 177.91 173.46 175 175.25 175 C 177.04 175 178.5 177.91 178.5 181.5 C 178.5 185.09 179.96 188 181.75 188 C 183.54 188 185 185.09 185 181.5']");
        expect(taskPathElement3.length).toEqual(2);
        const taskPathElement4 = element.find("path[d='M 158.85 177.6 L 160.15 177.6 L 160.15 180.85 L 163.4 180.85 L 163.4 182.15 L 160.15 182.15 L 160.15 185.4 L 158.85 185.4 L 158.85 182.15 L 155.6 182.15 L 155.6 180.85 L 158.85 180.85 L 158.85 177.6 Z']");
        expect(taskPathElement4.length).toEqual(1);
        const taskPathElement5 = element.find("path[d='M 153 175 L 166 175 L 166 188 L 153 188 L 153 175']");
        expect(taskPathElement5.length).toEqual(2);
        const taskPathElement6 = element.find("path[d='M 118.83 187.4 C 116.25 185.43 115.52 181.85 117.12 179.03 C 118.73 176.2 122.17 175 125.19 176.2 C 128.21 177.41 129.87 180.66 129.08 183.81 C 128.3 186.96 125.3 189.05 122.08 188.7 M 118.83 184.15 L 118.83 187.4 L 115.25 186.43']");
        expect(taskPathElement6.length).toEqual(2);
    });

    it("Task Shape(ParallelMultiInstance+Service+Event) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "LoopType", value: "ParallelMultiInstance"};
        props[1] = {name: "IsAdHoc", value: false};
        props[2] = {name: "TaskType", value: "Service"};
        props[3] = {name: "IsCompensation", value: false};
        props[4] = {name: "IsCollapsed", value: false};
        props[5] = {name: "BoundaryType", value: "Event"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Task", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const rectElement = element.find("rect[x=100][y=100]");
        expect(rectElement.length).toEqual(1);

        const taskPathElement1 = element.find("path[d='M 103.41 107.92 L 105.03 109.19 L 104.86 110.55 L 103 111.28 L 103.59 113.85 L 105.68 113.58 L 106.48 114.7 L 105.71 116.53 L 107.89 117.89 L 109.16 116.27 L 110.58 116.44 L 111.31 118.3 L 113.85 117.71 L 113.61 115.59 L 114.7 114.7 L 116.44 115.62 L 117.92 113.38 L 116.21 112.05 L 116.38 110.72 L 118.3 109.99 L 117.74 107.45 L 115.53 107.69 L 114.76 106.68 L 115.56 104.74 L 113.35 103.44 L 111.96 105.09 L 110.61 104.89 L 109.75 103 L 107.3 103.65 L 107.54 105.71 L 106.63 106.51 L 104.74 105.71 Z M 107.96 110.8 C 107.96 109.31 109.17 108.1 110.66 108.1 C 112.15 108.1 113.36 109.31 113.36 110.8 C 113.36 112.29 112.15 113.5 110.66 113.5 C 109.17 113.5 107.96 112.29 107.96 110.8 Z']");
        expect(taskPathElement1.length).toEqual(1);
        const taskPathElement2 = element.find("path[d='M 103.41 107.92 L 105.03 109.19 L 104.86 110.55 L 103 111.28 L 103.59 113.85 L 105.68 113.58 L 106.48 114.7 L 105.71 116.53 L 107.89 117.89 L 109.16 116.27 L 110.58 116.44 L 111.31 118.3 L 113.85 117.71 L 113.61 115.59 L 114.7 114.7 L 116.44 115.62 L 117.92 113.38 L 116.21 112.05 L 116.38 110.72 L 118.3 109.99 L 117.74 107.45 L 115.53 107.69 L 114.76 106.68 L 115.56 104.74 L 113.35 103.44 L 111.96 105.09 L 110.61 104.89 L 109.75 103 L 107.3 103.65 L 107.54 105.71 L 106.63 106.51 L 104.74 105.71 Z M 107.96 110.8 C 107.96 109.31 109.17 108.1 110.66 108.1 C 112.15 108.1 113.36 109.31 113.36 110.8 C 113.36 112.29 112.15 113.5 110.66 113.5 C 109.17 113.5 107.96 112.29 107.96 110.8 Z']");
        expect(taskPathElement2.length).toEqual(1);
        const taskPathElement4 = element.find("path[d='M 146.75 175 L 149.35 175 L 149.35 188 L 146.75 188 L 146.75 175 Z M 151.95 175 L 154.55 175 L 154.55 188 L 151.95 188 L 151.95 175 Z M 157.15 175 L 159.75 175 L 159.75 188 L 157.15 188 L 157.15 175 Z']");
        expect(taskPathElement4.length).toEqual(1);
    });

    it("Task Shape(SequentialMultiInstance+Receive+Transaction) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "LoopType", value: "SequentialMultiInstance"};
        props[1] = {name: "IsAdHoc", value: false};
        props[2] = {name: "TaskType", value: "Receive"};
        props[3] = {name: "IsCompensation", value: false};
        props[4] = {name: "IsCollapsed", value: false};
        props[5] = {name: "BoundaryType", value: "Transaction"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Task", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const rectElement = element.find("rect[x=100][y=100]");
        expect(rectElement.length).toEqual(1);

        const taskPathElement1 = element.find("path[d='M 105 114 C 105 109.03 109.03 105 114 105 L 186 105 C 188.39 105 190.68 105.95 192.36 107.64 C 194.05 109.32 195 111.61 195 114 L 195 186 C 195 188.39 194.05 190.68 192.36 192.36 C 190.68 194.05 188.39 195 186 195 L 114 195 C 111.61 195 109.32 194.05 107.64 192.36 C 105.95 190.68 105 188.39 105 186 Z']");
        expect(taskPathElement1.length).toEqual(1);
        const taskPathElement2 = element.find("path[d='M 103 103 L 116.5 103 L 116.5 111.44 L 118 111.44 L 118 114.25 L 115 114.25 L 115 115.66 L 113.35 114.25 L 103 114.25 Z M 112 113.03 L 115 110.03 L 115 111.44 L 118 111.44 L 118 114.25 L 115 114.25 L 115 115.66 Z']");
        expect(taskPathElement2.length).toEqual(2);
        const taskPathElement3 = element.find("path[d='M 103.45 103 L 109.75 108.63 L 116.05 103 Z']");
        expect(taskPathElement3.length).toEqual(2);
        const taskPathElement4 = element.find("path[d='M 146.75 175 L 146.75 177.6 L 159.75 177.6 L 159.75 175 L 146.75 175 Z M 146.75 180.2 L 146.75 182.8 L 159.75 182.8 L 159.75 180.2 L 146.75 180.2 Z M 146.75 185.4 L 146.75 188 L 159.75 188 L 159.75 185.4 L 146.75 185.4 Z']");
        expect(taskPathElement4.length).toEqual(1);
    });

    it("Task Shape(Send) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "LoopType", value: "None"};
        props[1] = {name: "IsAdHoc", value: false};
        props[2] = {name: "TaskType", value: "Send"};
        props[3] = {name: "IsCompensation", value: false};
        props[4] = {name: "IsCollapsed", value: false};
        props[5] = {name: "BoundaryType", value: "Default"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Task", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const rectElement = element.find("rect[x=100][y=100]");
        expect(rectElement.length).toEqual(1);

        const taskPathElement1 = element.find("path[d='M 103 103 L 116.5 103 L 116.5 111.44 L 118 113.03 L 115 115.66 L 115 114.25 L 103 114.25 Z M 112 111.44 L 115 111.44 L 115 110.03 L 118 113.03 L 115 115.66 L 115 114.25 L 112 114.25 L 112 111.44']");
        expect(taskPathElement1.length).toEqual(2);
        const taskPathElement2 = element.find("path[d='M 103.45 103 L 109.75 108.63 L 116.05 103 Z']");
        expect(taskPathElement2.length).toEqual(2);
    });

    it("Task Shape(InstantiatingReceive) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "LoopType", value: "None"};
        props[1] = {name: "IsAdHoc", value: false};
        props[2] = {name: "TaskType", value: "InstantiatingReceive"};
        props[3] = {name: "IsCompensation", value: false};
        props[4] = {name: "IsCollapsed", value: false};
        props[5] = {name: "BoundaryType", value: "Default"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Task", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const rectElement = element.find("rect[x=100][y=100]");
        expect(rectElement.length).toEqual(1);

        const taskPathElement1 = element.find("path[d='M 105.55 106.75 L 115.45 106.75 L 115.45 114.25 L 105.55 114.25 Z']");
        expect(taskPathElement1.length).toEqual(2);
        const taskPathElement2 = element.find("path[d='M 105.55 106.75 L 115.45 106.75 L 110.5 110.5 Z']");
        expect(taskPathElement2.length).toEqual(2);
    });

    it("Task Shape(Manual) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "LoopType", value: "None"};
        props[1] = {name: "IsAdHoc", value: false};
        props[2] = {name: "TaskType", value: "Manual"};
        props[3] = {name: "IsCompensation", value: false};
        props[4] = {name: "IsCollapsed", value: false};
        props[5] = {name: "BoundaryType", value: "Default"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Task", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const rectElement = element.find("rect[x=100][y=100]");
        expect(rectElement.length).toEqual(1);

        const taskPathElement1 = element.find("path[d='M 103 106.54 C 103.31 104.82 104.18 103.47 105.3 103 L 111.21 103 C 111.56 103.24 111.8 103.79 111.8 104.39 C 111.8 105 111.56 105.54 111.21 105.78 L 107.27 105.78 L 117.28 105.78 C 117.71 106.05 118 106.7 118 107.43 C 118 108.15 117.71 108.8 117.28 109.07 L 110.39 109.07 L 117.28 109.07 C 117.71 109.34 118 109.99 118 110.72 C 118 111.44 117.71 112.09 117.28 112.36 L 111.04 112.36 L 116.46 112.36 C 117 112.36 117.44 113.04 117.44 113.88 C 117.44 114.72 117 115.4 116.46 115.4 L 110.88 115.4 L 115.31 115.4 C 115.76 115.4 116.13 115.97 116.13 116.66 C 116.13 117.36 115.76 117.93 115.31 117.93 L 104.48 117.93 C 104.13 118 103.78 117.85 103.5 117.52 C 103.22 117.19 103.04 116.7 103 116.16 Z']");
        expect(taskPathElement1.length).toEqual(1);
    });

    it("Task Shape(BusinessRule) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "LoopType", value: "None"};
        props[1] = {name: "IsAdHoc", value: false};
        props[2] = {name: "TaskType", value: "BusinessRule"};
        props[3] = {name: "IsCompensation", value: false};
        props[4] = {name: "IsCollapsed", value: false};
        props[5] = {name: "BoundaryType", value: "Default"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Task", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const rectElement1 = element.find("rect[x=100][y=100]");
        expect(rectElement1.length).toEqual(1);

        const rectElement2 = element.find("rect[width=15][height=15]");
        expect(rectElement2.length).toEqual(1);

        const taskPathElement1 = element.find("path[d='M 103 106.46 L 118 106.46 M 103.15 112.23 L 117.91 112.23 M 106.75 106.46 L 106.75 118']");
        expect(taskPathElement1.length).toEqual(2);
    });

    it("Task Shape(User) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "LoopType", value: "None"};
        props[1] = {name: "IsAdHoc", value: false};
        props[2] = {name: "TaskType", value: "User"};
        props[3] = {name: "IsCompensation", value: false};
        props[4] = {name: "IsCollapsed", value: false};
        props[5] = {name: "BoundaryType", value: "Default"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Task", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const rectElement1 = element.find("rect[x=100][y=100]");
        expect(rectElement1.length).toEqual(1);

        const taskPathElement1 = element.find("path[d='M 103 118 L 103 113.43 C 103.88 111.88 105.22 110.68 106.83 109.99 C 107.29 109.85 107.78 109.8 108.27 109.83 C 108.26 111.25 109.29 112.44 110.66 112.61 C 111.41 112.65 112.14 112.38 112.68 111.86 C 113.23 111.33 113.53 110.6 113.53 109.83 C 114.11 109.76 114.7 109.81 115.26 109.99 C 116.58 110.71 117.57 111.95 118 113.43 L 118 118 Z M 113.53 109.83 C 113.53 110.6 113.23 111.33 112.68 111.86 C 112.14 112.38 111.41 112.65 110.66 112.61 C 109.29 112.44 108.26 111.25 108.27 109.83 C 108.54 109.8 108.8 109.75 109.06 109.67 L 109.22 109.01 C 108.72 108.89 108.3 108.53 108.11 108.03 C 107.69 107.22 108.1 106.32 109.11 105.81 C 110.13 105.3 111.51 105.3 112.52 105.81 C 113.54 106.32 113.95 107.22 113.53 108.03 C 113.27 108.54 112.8 108.89 112.26 109.01 L 112.41 109.67 Z']");
        expect(taskPathElement1.length).toEqual(1);
        const taskPathElement2 = element.find("path[d='M 105.55 115.39 L 105.55 117.84 M 114.97 115.39 L 114.97 117.84']");
        expect(taskPathElement2.length).toEqual(2);
        const taskPathElement3 = element.find("path[d='M 108.11 108.03 C 107.39 107.3 107.2 106.19 107.63 105.26 C 108.17 104.02 109.34 103.2 110.66 103.13 C 112.15 103 113.56 103.85 114.17 105.26 C 114.56 106.23 114.3 107.34 113.53 108.03 C 113.58 107.52 113.47 107.01 113.21 106.56 C 112.54 106.13 111.7 106.07 110.98 106.4 C 110.21 106.01 109.3 106.07 108.59 106.56 C 108.27 106.99 108.11 107.5 108.11 108.03 Z']");
        expect(taskPathElement3.length).toEqual(1);
    });

    it("Task Shape(Script) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "LoopType", value: "None"};
        props[1] = {name: "IsAdHoc", value: false};
        props[2] = {name: "TaskType", value: "Script"};
        props[3] = {name: "IsCompensation", value: false};
        props[4] = {name: "IsCollapsed", value: false};
        props[5] = {name: "BoundaryType", value: "Default"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("Task", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const rectElement1 = element.find("rect[x=100][y=100]");
        expect(rectElement1.length).toEqual(1);

        const taskPathElement1 = element.find("path[d='M 115.61 103 C 113.22 105.19 113.22 108.31 115.61 110.5 C 118 112.69 118 115.81 115.61 118 L 105.39 118 C 107.78 115.81 107.78 112.69 105.39 110.5 C 103 108.31 103 105.19 105.39 103 Z']");
        expect(taskPathElement1.length).toEqual(1);
        const taskPathElement2 = element.find("path[d='M 107.43 110.5 L 113.57 110.5 M 105.8 107.5 L 111.93 107.5 M 106.21 104.5 L 112.34 104.5 M 109.07 113.5 L 115.2 113.5 M 108.66 116.5 L 114.79 116.5']");
        expect(taskPathElement2.length).toEqual(2);
    });


    it("Sub Process Shape(ParallelMultiInstance+Event) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "LoopType", value: "ParallelMultiInstance"};
        props[1] = {name: "IsAdHoc", value: false};
        props[2] = {name: "IsCompensation", value: false};
        props[3] = {name: "BoundaryType", value: "Event"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("ExpandedSubProcess", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const rectElement = element.find("rect[x=100][y=100]");
        expect(rectElement.length).toEqual(1);

        const subProcessPathElement1 = element.find("path[d='M 105 114 C 105 109.03 109.03 105 114 105 L 186 105 C 188.39 105 190.68 105.95 192.36 107.64 C 194.05 109.32 195 111.61 195 114 L 195 186 C 195 188.39 194.05 190.68 192.36 192.36 C 190.68 194.05 188.39 195 186 195 L 114 195 C 111.61 195 109.32 194.05 107.64 192.36 C 105.95 190.68 105 188.39 105 186 Z']");
        expect(subProcessPathElement1.length).toEqual(1);
        const subProcessPathElement2 = element.find("path[d='M 146.75 175 L 148.44 175 L 148.44 188 L 146.75 188 L 146.75 175 Z M 152.41 175 L 154.09 175 L 154.09 188 L 152.41 188 L 152.41 175 Z M 158.06 175 L 159.75 175 L 159.75 188 L 158.06 188 L 158.06 175 Z']");
        expect(subProcessPathElement2.length).toEqual(1);
    });

    it("Sub Process Shape(SequentialMultiInstance+Call+adHoc+compensation) Test", () => {
        // Arrange
        const props = new Array<Prop>();
        props[0] = {name: "LoopType", value: "SequentialMultiInstance"};
        props[1] = {name: "IsAdHoc", value: true};
        props[2] = {name: "IsCompensation", value: true};
        props[3] = {name: "BoundaryType", value: "Event"};
        const shapes = [];
        shapes.push(DiagramMock.createShape("ExpandedSubProcess", props));
        const diagramMock = DiagramMock.createDiagramMock(shapes, [], Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const rectElement = element.find("rect[x=100][y=100]");
        expect(rectElement.length).toEqual(1);

        const subProcessPathElement1 = element.find("path[d='M 105 114 C 105 109.03 109.03 105 114 105 L 186 105 C 188.39 105 190.68 105.95 192.36 107.64 C 194.05 109.32 195 111.61 195 114 L 195 186 C 195 188.39 194.05 190.68 192.36 192.36 C 190.68 194.05 188.39 195 186 195 L 114 195 C 111.61 195 109.32 194.05 107.64 192.36 C 105.95 190.68 105 188.39 105 186 Z']");
        expect(subProcessPathElement1.length).toEqual(1);
        const subProcessPathElement2 = element.find("path[d='M 143.5 181.5 L 150 175 L 150 188 Z M 150 181.5 L 156.5 175 L 156.5 188 Z']");
        expect(subProcessPathElement2.length).toEqual(1);
        const subProcessPathElement3 = element.find("path[d='M 162.5 181.5 C 162.5 177.91 163.96 175 165.75 175 C 167.54 175 169 177.91 169 181.5 C 169 185.09 170.46 188 172.25 188 C 174.04 188 175.5 185.09 175.5 181.5'][stroke=white]");
        expect(subProcessPathElement3.length).toEqual(1);
        const subProcessPathElement4 = element.find("path[d='M 162.5 181.5 C 162.5 177.91 163.96 175 165.75 175 C 167.54 175 169 177.91 169 181.5 C 169 185.09 170.46 188 172.25 188 C 174.04 188 175.5 185.09 175.5 181.5'][stroke=black]");
        expect(subProcessPathElement4.length).toEqual(1);
        const subProcessPathElement5 = element.find("path[d='M 125.75 175 L 125.75 176.69 L 138.75 176.69 L 138.75 175 L 125.75 175 Z M 125.75 180.66 L 125.75 182.34 L 138.75 182.34 L 138.75 180.66 L 125.75 180.66 Z M 125.75 186.31 L 125.75 188 L 138.75 188 L 138.75 186.31 L 125.75 186.31 Z'][fill=black][stroke=black]");
        expect(subProcessPathElement5.length).toEqual(1);
    });

    it("annotation Test", () => {

        // Act
        const props = new Array<Prop>();

        const annotationShapes = [];
        annotationShapes.push(DiagramMock.createShape("TextAnnotation", props));

        const connections = [];
        const points = [];
        points.push(<Point>{x: 100, y: 150});
        points.push(<Point>{x: 0, y: 0});
        const connection = DiagramMock.createConnection(ConnectorTypes.STRAIGHT, points);
        connections.push(connection);

        const diagramMock = DiagramMock.createDiagramMock(annotationShapes, connections, Diagrams.BUSINESS_PROCESS);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const rectElement = element.find("rect[x=100][y=100]");
        expect(rectElement.length).toEqual(1);
        const annotationPathElement1 = element.find("path[d='M 100 150 L 0 0']");
        expect(annotationPathElement1.length).toEqual(2);
    });
});
