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
        let id = "123";
        let type = ElementType.SystemTaskHeader;
        let element = new DiagramElement(id, type);

        // Act
        let actual = element.getElementType();

        // Assert
        expect(actual).toEqual(type);
    });
    it("should return width passed in the constructor", () => {
        // Arrange
        let id = "123";
        let width = 15;
        let type = ElementType.SystemTaskHeader;
        let element = new DiagramElement(id, type, undefined, new mxGeometry(0, 0, 15, 35));

        // Act
        let actualWidth = element.getWidth();

        // Assert
        expect(actualWidth).toEqual(width);
    });
    it("should return height passed in the constructor", () => {
        // Arrange
        let id = "123";
        let height = 35;
        let type = ElementType.SystemTaskHeader;
        let element = new DiagramElement(id, type, undefined, new mxGeometry(0, 0, 15, 35));

        // Act
        let actualHeight = element.getHeight();

        // Assert
        expect(actualHeight).toEqual(height);
    });
    describe("when retrieving center point", () => {
        it("returns default point if element is without geometry specification", () => {
            // Arrange
            let element = new DiagramElement("1");
            let expectedCenter = new mxPoint(0, 0);

            // Act
            let center = element.getCenter();

            // Assert
            expect(center).toEqual(expectedCenter);
        });
        it("returns node center point if element is node", () => {
            // Arrange
            let element = new DiagramElement("1", undefined, undefined, new mxGeometry(0, 0, 10, 10));
            let expectedCenter = new mxPoint(5, 5);

            // Act
            let center = element.getCenter();

            // Assert
            expect(center).toEqual(expectedCenter);
        });
    });
});

describe("DiagramNodeElement", () => {
    describe("when retrieving node", () => {
        it("returns null if parent is null", () => {
            // Arrange
            let element = new DiagramNodeElement("0");

            // Act
            let node = element.getNode();

            // Assert
            expect(node).toBeNull();
        });
        it("returns null if no node is present in hierarchy", () => {
            // Arrange
            let ancestorElement = new DiagramNodeElement("-2");
            let parentElement = new DiagramNodeElement("-1");
            parentElement.setParent(ancestorElement);
            let element = new DiagramNodeElement("0");
            element.setParent(parentElement);

            // Act
            let actualNode = element.getNode();

            // Assert
            expect(actualNode).toBeNull();
        });
        it("returns parent if parent is node", () => {
            // Arrange
            let model = new ProcessShapeModel();
            let node = new DiagramNode(model);
            let parentElement = new DiagramNodeElement("-1");
            parentElement.setParent(node);
            let element = new DiagramNodeElement("0");
            element.setParent(parentElement);

            // Act
            let actualNode = element.getNode();

            // Assert
            expect(actualNode).toEqual(node);
        });
        it("returns ancestor if ancestor is node", () => {
            // Arrange
            let model = new ProcessShapeModel();
            let node = new DiagramNode(model);
            let element = new DiagramNodeElement("0");
            element.setParent(node);

            // Act
            let actualNode = element.getNode();

            // Assert
            expect(actualNode).toEqual(node);
        });
    });
    describe("when retrieving center point", () => {
        it("returns default point if element is node without geometry specification", () => {
            // Arrange
            let element = new DiagramNodeElement("0");
            let expectedCenter = new mxPoint(0, 0);

            // Act
            let center = element.getCenter();

            // Assert
            expect(center).toEqual(expectedCenter);
        });
        it("returns element center point if geometry is specified", () => {
            // Arrange
            let element = new DiagramNodeElement("0", undefined, undefined, new mxGeometry(0, 0, 10, 10));
            let expectedCenter = new mxPoint(5, 5);

            // Act
            let center = element.getCenter();

            // Assert
            expect(center).toEqual(expectedCenter);
        });
        it("returns element center point if is contained in another element with absolute positioning", () => {
            // Arrange
            let parentElement = new DiagramNodeElement("0", undefined, undefined, new mxGeometry(0, 0, 50, 50));
            let element = new DiagramNodeElement("1", undefined, undefined, new mxGeometry(5, 5, 10, 10));
            element.setParent(parentElement);
            let expectedCenter = new mxPoint(10, 10);

            // Act
            let center = element.getCenter();

            // Assert
            expect(center).toEqual(expectedCenter);
        });
        it("returns element center point if is contained in another element with relative positioning", () => {
            // Arrange
            let parentElement = new DiagramNodeElement("0", undefined, undefined, new mxGeometry(0, 0, 50, 50));
            let elementGeometry = new mxGeometry(1, 1, 10, 10);
            elementGeometry.relative = true;
            let element = new DiagramNodeElement("1", undefined, undefined, elementGeometry);
            element.setParent(parentElement);
            let expectedCenter = new mxPoint(55, 55);

            // Act
            let center = element.getCenter();

            // Assert
            expect(center).toEqual(expectedCenter);
        });
        it("returns element center point if is contained in a hierarchy of elements", () => {
            // Arrange
            let ancestorElement = new DiagramNodeElement("0", undefined, undefined, new mxGeometry(10, 20, 100, 100));
            let parentElement = new DiagramNodeElement("1", undefined, undefined, new mxGeometry(15, 5, 30, 50));
            parentElement.setParent(ancestorElement);
            let element = new DiagramNodeElement("2", undefined, undefined, new mxGeometry(5, 5, 10, 10));
            element.setParent(parentElement);
            let expectedCenter = new mxPoint(35, 35);

            // Act
            let center = element.getCenter();

            // Assert
            expect(center).toEqual(expectedCenter);
        });
        it("returns element center point if is contained in a hierarchy of elements with relative positioning", () => {
            // Arrange
            let ancestorElement = new DiagramNodeElement("0", undefined, undefined, new mxGeometry(10, 20, 100, 100));
            let parentGeometry = new mxGeometry(0.5, 0.5, 30, 50);
            parentGeometry.relative = true;
            let parentElement = new DiagramNodeElement("1", undefined, undefined, parentGeometry);
            parentElement.setParent(ancestorElement);
            let element = new DiagramNodeElement("2", undefined, undefined, new mxGeometry(5, 5, 10, 10));
            element.setParent(parentElement);
            let expectedCenter = new mxPoint(60, 60);

            // Act
            let center = element.getCenter();

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
        it("set text element - parent element", () => {
            // Arrange
            let testUserTask = ShapeModelMock.instance().UserTaskMock();
            let parentElement = new UserTask(testUserTask, root, null, shapesFactory);

            let element = new DiagramNodeElement("2", ElementType.Shape, null, new mxGeometry());
            element.setParent(parentElement);
            let spyParent = spyOn(parentElement, "setElementText");
            let textInput = "testing 123";
            // Act
            element.setElementText(element, textInput);

            // Assert
            expect(spyParent).toHaveBeenCalledWith(element, textInput);
        });
        it("get text element length - parent element", () => {
            // Arrange
            let testUserTask = ShapeModelMock.instance().UserTaskMock();
            let parentElement = new UserTask(testUserTask, root, null, shapesFactory);

            let element = new DiagramNodeElement("2", ElementType.Shape, null, new mxGeometry());
            element.setParent(parentElement);

            let spyParent = spyOn(parentElement, "getElementTextLength");
            // Act
            element.getElementTextLength(element);

            // Assert
            expect(spyParent).toHaveBeenCalledWith(element);
        });
        it("format text element - parent element", () => {
            // Arrange
            let testUserTask = ShapeModelMock.instance().UserTaskMock();
            let parentElement = new UserTask(testUserTask, root, null, shapesFactory);

            let element = new DiagramNodeElement("2", ElementType.Shape, null, new mxGeometry());
            element.setParent(parentElement);

            let spyParent = spyOn(parentElement, "formatElementText");
            let textInput = "testing 123";

            // Act
            element.formatElementText(element, textInput);

            // Assert
            expect(spyParent).toHaveBeenCalledWith(element, textInput);
        });
    });
});
