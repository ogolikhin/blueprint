import * as angular from "angular";
import {ElementType} from "../models/";
import {DiagramElement, DiagramNodeElement, DiagramNode, UserTask} from "./";
import {ShapeModelMock} from "./shape-model.mock";
import {ProcessShapeModel} from "../../../../../models/process-models";
import {ShapesFactory} from "./shapes-factory";
import {IStatefulArtifactFactory} from "../../../../../../../managers/artifact-manager/";
import {StatefulArtifactFactoryMock} from "../../../../../../../managers/artifact-manager/artifact/artifact.factory.mock";

describe("DiagramElement", () => {
    it("should return type passed in the constructor", () => {
        // Arrange
        const id = "123";
        const type = ElementType.SystemTaskHeader;
        const element = new DiagramElement(id, type);

        // Act
        const actual = element.getElementType();

        // Assert
        expect(actual).toEqual(type);
    });
    it("should return width passed in the constructor", () => {
        // Arrange
        const id = "123";
        const width = 15;
        const type = ElementType.SystemTaskHeader;
        const element = new DiagramElement(id, type, undefined, new mxGeometry(0, 0, 15, 35));

        // Act
        const actualWidth = element.getWidth();

        // Assert
        expect(actualWidth).toEqual(width);
    });
    it("should return height passed in the constructor", () => {
        // Arrange
        const id = "123";
        const height = 35;
        const type = ElementType.SystemTaskHeader;
        const element = new DiagramElement(id, type, undefined, new mxGeometry(0, 0, 15, 35));

        // Act
        const actualHeight = element.getHeight();

        // Assert
        expect(actualHeight).toEqual(height);
    });
    describe("when retrieving center point", () => {
        it("returns default point if element is without geometry specification", () => {
            // Arrange
            const element = new DiagramElement("1");
            const expectedCenter = new mxPoint(0, 0);

            // Act
            const center = element.getCenter();

            // Assert
            expect(center).toEqual(expectedCenter);
        });
        it("returns node center point if element is node", () => {
            // Arrange
            const element = new DiagramElement("1", undefined, undefined, new mxGeometry(0, 0, 10, 10));
            const expectedCenter = new mxPoint(5, 5);

            // Act
            const center = element.getCenter();

            // Assert
            expect(center).toEqual(expectedCenter);
        });
    });
});

describe("DiagramNodeElement", () => {
    describe("when retrieving node", () => {
        it("returns null if parent is null", () => {
            // Arrange
            const element = new DiagramNodeElement("0");

            // Act
            const node = element.getNode();

            // Assert
            expect(node).toBeNull();
        });
        it("returns null if no node is present in hierarchy", () => {
            // Arrange
            const ancestorElement = new DiagramNodeElement("-2");
            const parentElement = new DiagramNodeElement("-1");
            parentElement.setParent(ancestorElement);
            const element = new DiagramNodeElement("0");
            element.setParent(parentElement);

            // Act
            const actualNode = element.getNode();

            // Assert
            expect(actualNode).toBeNull();
        });
        it("returns parent if parent is node", () => {
            // Arrange
            const model = new ProcessShapeModel();
            const node = new DiagramNode(model);
            const parentElement = new DiagramNodeElement("-1");
            parentElement.setParent(node);
            const element = new DiagramNodeElement("0");
            element.setParent(parentElement);

            // Act
            const actualNode = element.getNode();

            // Assert
            expect(actualNode).toEqual(node);
        });
        it("returns ancestor if ancestor is node", () => {
            // Arrange
            const model = new ProcessShapeModel();
            const node = new DiagramNode(model);
            const element = new DiagramNodeElement("0");
            element.setParent(node);

            // Act
            const actualNode = element.getNode();

            // Assert
            expect(actualNode).toEqual(node);
        });
    });
    describe("when retrieving center point", () => {
        it("returns default point if element is node without geometry specification", () => {
            // Arrange
            const element = new DiagramNodeElement("0");
            const expectedCenter = new mxPoint(0, 0);

            // Act
            const center = element.getCenter();

            // Assert
            expect(center).toEqual(expectedCenter);
        });
        it("returns element center point if geometry is specified", () => {
            // Arrange
            const element = new DiagramNodeElement("0", undefined, undefined, new mxGeometry(0, 0, 10, 10));
            const expectedCenter = new mxPoint(5, 5);

            // Act
            const center = element.getCenter();

            // Assert
            expect(center).toEqual(expectedCenter);
        });
        it("returns element center point if is contained in another element with absolute positioning", () => {
            // Arrange
            const parentElement = new DiagramNodeElement("0", undefined, undefined, new mxGeometry(0, 0, 50, 50));
            const element = new DiagramNodeElement("1", undefined, undefined, new mxGeometry(5, 5, 10, 10));
            element.setParent(parentElement);
            const expectedCenter = new mxPoint(10, 10);

            // Act
            const center = element.getCenter();

            // Assert
            expect(center).toEqual(expectedCenter);
        });
        it("returns element center point if is contained in another element with relative positioning", () => {
            // Arrange
            const parentElement = new DiagramNodeElement("0", undefined, undefined, new mxGeometry(0, 0, 50, 50));
            const elementGeometry = new mxGeometry(1, 1, 10, 10);
            elementGeometry.relative = true;
            const element = new DiagramNodeElement("1", undefined, undefined, elementGeometry);
            element.setParent(parentElement);
            const expectedCenter = new mxPoint(55, 55);

            // Act
            const center = element.getCenter();

            // Assert
            expect(center).toEqual(expectedCenter);
        });
        it("returns element center point if is contained in a hierarchy of elements", () => {
            // Arrange
            const ancestorElement = new DiagramNodeElement("0", undefined, undefined, new mxGeometry(10, 20, 100, 100));
            const parentElement = new DiagramNodeElement("1", undefined, undefined, new mxGeometry(15, 5, 30, 50));
            parentElement.setParent(ancestorElement);
            const element = new DiagramNodeElement("2", undefined, undefined, new mxGeometry(5, 5, 10, 10));
            element.setParent(parentElement);
            const expectedCenter = new mxPoint(35, 35);

            // Act
            const center = element.getCenter();

            // Assert
            expect(center).toEqual(expectedCenter);
        });
        it("returns element center point if is contained in a hierarchy of elements with relative positioning", () => {
            // Arrange
            const ancestorElement = new DiagramNodeElement("0", undefined, undefined, new mxGeometry(10, 20, 100, 100));
            const parentGeometry = new mxGeometry(0.5, 0.5, 30, 50);
            parentGeometry.relative = true;
            const parentElement = new DiagramNodeElement("1", undefined, undefined, parentGeometry);
            parentElement.setParent(ancestorElement);
            const element = new DiagramNodeElement("2", undefined, undefined, new mxGeometry(5, 5, 10, 10));
            element.setParent(parentElement);
            const expectedCenter = new mxPoint(60, 60);

            // Act
            const center = element.getCenter();

            // Assert
            expect(center).toEqual(expectedCenter);
        });
    });
    describe("when perfoming text operations", () => {
        let shapesFactory;
        let root: ng.IRootScopeService;

        beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
            $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        }));
        beforeEach(inject(($rootScope: ng.IRootScopeService, statefulArtifactFactory: IStatefulArtifactFactory) => {
            $rootScope["config"] = {
                labels: {
                    "ST_Persona_Label": "Persona",
                    "ST_Colors_Label": "Color",
                    "ST_Comments_Label": "Comments"
                }
            };
            root = $rootScope;
            shapesFactory = new ShapesFactory(root, statefulArtifactFactory);
        }));
    });
});
