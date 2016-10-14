import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
require("script!mxClient");

import {DiagramMock, Prop, LabelStyle} from "../diagram.mock";
import {DiagramView} from "./diagram-view";
import {UIMockupShapes, UIMockupShapeProps, Diagrams, Shapes} from "./utils/constants";
import {UiMockupShapeFactory} from "./uimockup-diagram";
import {IShapeTemplates} from "./abstract-diagram-factory";
import {IShape, IProp} from "./models";
import {CalloutShape, IconShape} from "./shapes-library";
import {StencilServiceMock} from "../stencil.svc.mock";

export var uiMockupShapesTestHelper = {
    getStyleObject: (styleString: string): any => {
        const styleListStrings = styleString.split(";");
        const styleList = {};
        for (let i = 0; i < styleListStrings.length; i++) {
            const style = styleListStrings[i].split("=");
            styleList[style[0]] = style[1];
        }
        return styleList;
    }
};

describe("UIMockup", () => {
    let element: ng.IAugmentedJQuery;
    let diagramView: DiagramView;

    beforeEach(angular.mock.module("ngSanitize", ($provide: ng.auto.IProvideService, $compileProvider: ng.ICompileProvider) => {
        $provide.service("stencilService", StencilServiceMock);
    }));

    beforeEach(inject((stencilService: StencilServiceMock) => {
        const divElement = document.createElement("div");
        element = angular.element(divElement);
        diagramView = new DiagramView(element[0], stencilService);
    }));

    it("Hotspot Test, Element Exists", () => {
        // Arrange
        const eventShapes = [];
        eventShapes.push(DiagramMock.createShape(UIMockupShapes.HOTSPOT));
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.UIMOCKUP);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const hotspotElement = element.find("rect");
        expect(hotspotElement.length).toEqual(1);
        const rectNode = hotspotElement[0];
        expect(rectNode.getAttribute("stroke").toLowerCase()).toEqual("#646464");
        expect(rectNode.getAttribute("fill").toLowerCase()).toEqual("#bdd0e6");
    });

    it("Hyperlink Test, Label css", () => {
        // Arrange
        const eventShapes = [];
        eventShapes.push(DiagramMock.createShape(UIMockupShapes.HYPERLINK));
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.UIMOCKUP);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const labelContainer = element.find("div:contains('Hyperlink:')");
        expect(labelContainer.css("vertical-align")).toEqual("top");
        expect(labelContainer.css("overflow")).toEqual("hidden");
    });

    it("TextBox label styling test", () => {
        // Arrange
        const eventShapes = [];
        const shape = DiagramMock.createShape(UIMockupShapes.TEXTBOX);

        const labelStyle = new LabelStyle();
        labelStyle.isItalic = true;
        labelStyle.isBold = true;
        labelStyle.isUnderline = true;
        labelStyle.foreground = "#0000FF";
        labelStyle.fontFamily = "Arial";
        labelStyle.fontSize = "20";
        shape.labelStyle = labelStyle;

        const textProperty = new Prop();
        textProperty.name = UIMockupShapeProps.TEXT;
        textProperty.value = "TextBoxValue";
        shape.props.push(textProperty);

        eventShapes.push(shape);
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.UIMOCKUP);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const textBoxContainer = element.find("div:contains('TextBoxValue')");
        expect(textBoxContainer.css("font-family")).toEqual("Arial");
        expect(textBoxContainer.css("font-size")).toEqual("20px");

        const actualFontWeight = textBoxContainer.css("font-weight");
        // for IE its 700 instead of bold
        expect(actualFontWeight === "bold" || actualFontWeight === "700").toBeTruthy();
        expect(textBoxContainer.css("font-style")).toEqual("italic");
        expect(textBoxContainer.css("text-decoration")).toEqual("underline");

        expect(textBoxContainer.css("color")).toEqual("rgb(0, 0, 255)");
    });

    it("Button Test", () => {
        // Arrange
        const eventShapes = [];
        eventShapes.push(DiagramMock.createShape(UIMockupShapes.BUTTON));
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.UIMOCKUP);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const buttonElement = element.find("rect");
        expect(buttonElement.length).toEqual(1);
        const rectNode = buttonElement[0];
        expect(rectNode.getAttribute("stroke").toLowerCase()).toEqual("#a8bed5");
    });

    it("Drop Down Button Test", () => {
        // Arrange
        const eventShapes = [];
        eventShapes.push(DiagramMock.createShape(UIMockupShapes.DROPDOWNBUTTON));
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.UIMOCKUP);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const buttonElement = element.find("rect");
        expect(buttonElement.length).toEqual(2);
        const rectNode1 = buttonElement[0];
        expect(rectNode1.getAttribute("stroke").toLowerCase()).toEqual("#a8bed5");
        const rectNode2 = buttonElement[1];
        expect(rectNode2.getAttribute("stroke").toLowerCase()).toEqual("none");
        expect(rectNode2.getAttribute("fill").toLowerCase()).toEqual("transparent");
        const markElement = element.find("path");
        expect(markElement.length).toEqual(1);
        const mark = markElement[0];
        expect(mark.getAttribute("stroke").toLowerCase()).toEqual("black");
    });

    it("Split Button Test", () => {
        // Arrange
        const eventShapes = [];
        eventShapes.push(DiagramMock.createShape(UIMockupShapes.SPLITBUTTON));
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.UIMOCKUP);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const buttonElement = element.find("rect");
        expect(buttonElement.length).toEqual(3);
        const rectNode1 = buttonElement[0];
        expect(rectNode1.getAttribute("stroke").toLowerCase()).toEqual("#a8bed5");
        const rectNode2 = buttonElement[1];
        expect(rectNode2.getAttribute("stroke").toLowerCase()).toEqual("#a8bed5");
        expect(rectNode2.getAttribute("fill").toLowerCase()).toEqual("transparent");
        const rectNode3 = buttonElement[1];
        expect(rectNode3.getAttribute("stroke").toLowerCase()).toEqual("#a8bed5");
        expect(rectNode3.getAttribute("fill").toLowerCase()).toEqual("transparent");
        const markElement = element.find("path");
        expect(markElement.length).toEqual(1);
        const mark = markElement[0];
        expect(mark.getAttribute("stroke").toLowerCase()).toEqual("black");
    });


    it("Checkbox checked Test", () => {
        // Arrange
        const eventShapes = [];
        const checkedProperty = new Prop();
        checkedProperty.name = "Checked";
        checkedProperty.value = "true";
        eventShapes.push(DiagramMock.createShape(UIMockupShapes.CHECKBOX, [checkedProperty]));
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.UIMOCKUP);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const path = element.find("path");
        expect(path.length === 1).toBeTruthy();
    });

    it("Checkbox unchecked Test", () => {
        // Arrange
        const eventShapes = [];
        eventShapes.push(DiagramMock.createShape(UIMockupShapes.CHECKBOX));
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.UIMOCKUP);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const checkBoxContainer = element.find("div:contains('CheckBox:')");
        expect(checkBoxContainer !== null).toBeTruthy();
        const path = element.find("path");
        expect(path.length === 0).toBeTruthy();
    });

    it("RadioButton checked Test", () => {
        // Arrange
        const eventShapes = [];
        const checkedProperty = new Prop();
        checkedProperty.name = "Checked";
        checkedProperty.value = "true";
        eventShapes.push(DiagramMock.createShape(UIMockupShapes.RADIOBUTTON, [checkedProperty]));
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.UIMOCKUP);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const ellipse = element.find("ellipse");
        expect(ellipse.length === 3).toBeTruthy();
    });

    it("RadioButton unchecked Test", () => {
        // Arrange
        const eventShapes = [];
        eventShapes.push(DiagramMock.createShape(UIMockupShapes.RADIOBUTTON));
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.UIMOCKUP);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const ellipse = element.find("ellipse");
        expect(ellipse.length === 2).toBeTruthy();
    });


    it("frame Test", () => {
        // Arrange
        const eventShapes = [];
        eventShapes.push(DiagramMock.createShape(UIMockupShapes.FRAME));
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.UIMOCKUP);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const frameElement = element.find("rect");
        expect(frameElement.length).toEqual(4);
        const rectNode = frameElement[2];
        expect(rectNode.getAttribute("stroke").toLowerCase()).toEqual("#a9bfd6");
    });


    it("date time picker test", inject(($injector) => {
        // Arrange
        $.fn.injector = () => $injector;
        const eventShapes = [];
        eventShapes.push(DiagramMock.createShape(UIMockupShapes.DATE_TIME_PICKER));
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.UIMOCKUP);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const rect = element.find("rect");
        expect(rect.length).toEqual(3);

        //
        const rectNode0 = rect[0];
        expect(rectNode0.getAttribute("stroke").toLowerCase()).toEqual("#a8bed5");
        //
        const rectNode1 = rect[1];
        expect(rectNode1.getAttribute("stroke").toLowerCase()).toEqual("#a8bed5");
        //
        const rectNode2 = rect[2];
        expect(rectNode2.getAttribute("fill").toLowerCase()).toEqual("transparent");
    }));

    it("Numeric spinner Test", () => {
        // Arrange
        const eventShapes = [];
        eventShapes.push(DiagramMock.createShape(UIMockupShapes.NUMERIC_SPINNER));
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.UIMOCKUP);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const triangle = element.find("path");
        expect(triangle.length === 2).toBeTruthy();
    });


    it("Text Area", () => {
        // Arrange
        const eventShapes = [];
        eventShapes.push(DiagramMock.createShape(UIMockupShapes.TEXT_AREA));
        const diagramMock = DiagramMock.createDiagramMock(eventShapes, [], Diagrams.UIMOCKUP);

        // Act
        diagramView.drawDiagram(diagramMock);

        // Assert
        const textAreaElement = element.find("rect");
        expect(textAreaElement.length).toEqual(2);
        const rectNode = textAreaElement[0];
        expect(rectNode.getAttribute("stroke").toLowerCase()).toEqual("#7f98a9");
    });

    it("Menu General Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);

        const labelStyle = new LabelStyle();
        labelStyle.isItalic = true;
        labelStyle.isBold = true;
        labelStyle.isUnderline = true;
        labelStyle.foreground = "#0000FF";
        labelStyle.fontFamily = "Arial";
        labelStyle.fontSize = "20";

        //mock a shape here.
        const shape = {
            labelStyle: labelStyle,
            props: [
                {name: "length", value: "3"},
                {
                    name: "TreeItem",
                    value: {isEnabled: true, isSeparator: false, text: "File", isSelected: false}
                },
                {
                    name: "TreeItem",
                    value: {isEnabled: false, isSeparator: false, text: "View", isSelected: true}
                },
                {
                    name: "TreeItem",
                    value: {isEnabled: true, isSeparator: true, text: "View", isSelected: false}
                },
                {name: "Orientation", value: "Vertical"}
            ]
        };

        //act
        const menuShapeBiu = templates["Menu"](<IShape>shape);

        //assert
        const menuStyleString = menuShapeBiu["style"];
        const menuStyle = uiMockupShapesTestHelper.getStyleObject(menuStyleString);
        expect(menuStyle["strokeWidth"]).toEqual("1");
        expect(menuStyle["fillColor"]).toEqual("#F1F5FB");

        expect(menuShapeBiu["children"].length).toBeGreaterThan(0);
        const contentStyleString = menuShapeBiu["children"][0].getStyle();
        const contentStyle = uiMockupShapesTestHelper.getStyleObject(contentStyleString);
        expect(contentStyle["strokeWidth"]).toEqual("1");
        expect(contentStyle["verticalAlign"]).toEqual("top");

        const menuLabel: string = menuShapeBiu["children"][0].value;
        expect(menuLabel.indexOf("File")).toBeGreaterThan(0);
        expect(menuLabel.indexOf("View")).toBeGreaterThan(0);
        expect(menuLabel.indexOf("&#10004;")).toBeGreaterThan(0);
    });

    it("Menu Bold Underlined Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);

        const labelStyle = new LabelStyle();
        labelStyle.isItalic = false;
        labelStyle.isBold = true;
        labelStyle.isUnderline = true;
        labelStyle.foreground = "#0000FF";
        labelStyle.fontFamily = "Arial";
        labelStyle.fontSize = "20";

        //mock a shape here.
        const shape = {
            labelStyle: labelStyle,
            props: [
                {name: "length", value: "3"},
                {
                    name: "TreeItem",
                    value: {isEnabled: true, isSeparator: false, text: "File", isSelected: false}
                },
                {
                    name: "TreeItem",
                    value: {isEnabled: false, isSeparator: false, text: "View", isSelected: true}
                },
                {
                    name: "TreeItem",
                    value: {isEnabled: true, isSeparator: true, text: "View", isSelected: false}
                },
                {name: "Orientation", value: "Vertical"}
            ]
        };

        //act
        const menuShapeBiu = templates["Menu"](<IShape>shape);

        //assert
        const menuStyleString = menuShapeBiu["style"];
        const menuStyle = uiMockupShapesTestHelper.getStyleObject(menuStyleString);
        expect(menuStyle["strokeWidth"]).toEqual("1");
        expect(menuStyle["fillColor"]).toEqual("#F1F5FB");
        expect(menuStyle["fontStyle"]).toEqual("5");

    });

    it("Menu Underlined Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);

        const labelStyle = new LabelStyle();
        labelStyle.isItalic = false;
        labelStyle.isBold = false;
        labelStyle.isUnderline = true;
        labelStyle.foreground = "#0000FF";
        labelStyle.fontFamily = "Arial";
        labelStyle.fontSize = "20";

        //mock a shape here.
        const shape = {
            labelStyle: labelStyle,
            props: [
                {name: "length", value: "3"},
                {
                    name: "TreeItem",
                    value: {isEnabled: true, isSeparator: false, text: "File", isSelected: false}
                },
                {
                    name: "TreeItem",
                    value: {isEnabled: false, isSeparator: false, text: "View", isSelected: true}
                },
                {
                    name: "TreeItem",
                    value: {isEnabled: true, isSeparator: true, text: "View", isSelected: false}
                },
                {name: "Orientation", value: "Vertical"}
            ]
        };

        //act
        const menuShapeBiu = templates["Menu"](<IShape>shape);

        //assert
        const menuStyleString = menuShapeBiu["style"];
        const menuStyle = uiMockupShapesTestHelper.getStyleObject(menuStyleString);
        expect(menuStyle["strokeWidth"]).toEqual("1");
        expect(menuStyle["fillColor"]).toEqual("#F1F5FB");
        expect(menuStyle["fontStyle"]).toEqual("4");

    });

    it("Menu Italic Underlined Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);

        const labelStyle = new LabelStyle();
        labelStyle.isItalic = true;
        labelStyle.isBold = false;
        labelStyle.isUnderline = true;
        labelStyle.foreground = "#0000FF";
        labelStyle.fontFamily = "Arial";
        labelStyle.fontSize = "20";

        //mock a shape here.
        const shape = {
            labelStyle: labelStyle,
            props: [
                {name: "length", value: "3"},
                {
                    name: "TreeItem",
                    value: {isEnabled: true, isSeparator: false, text: "File", isSelected: false}
                },
                {
                    name: "TreeItem",
                    value: {isEnabled: false, isSeparator: false, text: "View", isSelected: true}
                },
                {
                    name: "TreeItem",
                    value: {isEnabled: true, isSeparator: true, text: "View", isSelected: false}
                },
                {name: "Orientation", value: "Vertical"}
            ]
        };

        //act
        const menuShapeBiu = templates["Menu"](<IShape>shape);

        //assert
        const menuStyleString = menuShapeBiu["style"];
        const menuStyle = uiMockupShapesTestHelper.getStyleObject(menuStyleString);
        expect(menuStyle["strokeWidth"]).toEqual("1");
        expect(menuStyle["fillColor"]).toEqual("#F1F5FB");
        expect(menuStyle["fontStyle"]).toEqual("6");

    });

    it("Menu Italic Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);

        const labelStyle = new LabelStyle();
        labelStyle.isItalic = true;
        labelStyle.isBold = false;
        labelStyle.isUnderline = false;
        labelStyle.foreground = "#0000FF";
        labelStyle.fontFamily = "Arial";
        labelStyle.fontSize = "20";

        //mock a shape here.
        const shape = {
            labelStyle: labelStyle,
            props: [
                {name: "length", value: "3"},
                {
                    name: "TreeItem",
                    value: {isEnabled: true, isSeparator: false, text: "File", isSelected: false}
                },
                {
                    name: "TreeItem",
                    value: {isEnabled: false, isSeparator: false, text: "View", isSelected: true}
                },
                {
                    name: "TreeItem",
                    value: {isEnabled: true, isSeparator: true, text: "View", isSelected: false}
                },
                {name: "Orientation", value: "Vertical"}
            ]
        };

        //act
        const menuShapeBiu = templates["Menu"](<IShape>shape);

        //assert
        const menuStyleString = menuShapeBiu["style"];
        const menuStyle = uiMockupShapesTestHelper.getStyleObject(menuStyleString);
        expect(menuStyle["strokeWidth"]).toEqual("1");
        expect(menuStyle["fillColor"]).toEqual("#F1F5FB");
        expect(menuStyle["fontStyle"]).toEqual("2");

    });

    it("Menu Horizontal Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);

        const labelStyle = new LabelStyle();
        labelStyle.isItalic = true;
        labelStyle.isBold = true;
        labelStyle.isUnderline = true;
        labelStyle.foreground = "#0000FF";
        labelStyle.fontFamily = "Arial";
        labelStyle.fontSize = "20";

        //mock a shape here.
        const shape = {
            labelStyle: labelStyle,
            props: [
                {name: "length", value: "3"},
                {
                    name: "TreeItem",
                    value: {isEnabled: true, isSeparator: false, text: "File", isSelected: false}
                },
                {
                    name: "TreeItem",
                    value: {isEnabled: false, isSeparator: false, text: "View", isSelected: true}
                },
                {
                    name: "TreeItem",
                    value: {isEnabled: true, isSeparator: true, text: "View", isSelected: false}
                },
                {name: "Orientation", value: "Horizontal"}
            ]
        };

        //act
        const menuShapeBiu = templates["Menu"](<IShape>shape);
        //assert
        const menuStyleString = menuShapeBiu["style"];
        const menuStyle = uiMockupShapesTestHelper.getStyleObject(menuStyleString);
        expect(menuStyle["strokeWidth"]).toEqual("1");
        expect(menuStyle["fillColor"]).toEqual("#F1F5FB");
    });

    it("Alpha Hex Color To RGB Test", () => {
        // Arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const normalHexColor = "#52d57e";
        const wrongHexColor = "52d57e";
        const shortHexColor = "#52d";
        const emptyHexColor = "";

        // Act
        const normalHexColorResult = uiMockupShapeFactory.convertAlphaHexToRgb(0.5, normalHexColor);
        const wrongHexColorResult = uiMockupShapeFactory.convertAlphaHexToRgb(0.5, wrongHexColor);
        const shortHexColorResult = uiMockupShapeFactory.convertAlphaHexToRgb(0.5, shortHexColor);
        const emptyHexColorResult = uiMockupShapeFactory.convertAlphaHexToRgb(0.5, emptyHexColor);

        // Assert
        expect(normalHexColorResult).not.toEqual(normalHexColor);
        expect(wrongHexColorResult).toEqual(wrongHexColor);
        expect(shortHexColorResult).toEqual(shortHexColor);
        expect(emptyHexColorResult).toEqual(emptyHexColor);
    });

    it("Browser Template Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);
        //mock a shape here.
        const shape = {
            props: [
                {name: "length", value: "2"}
            ]
        };

        //act
        const browserShape = templates["Browser"](<IShape>shape);
        //assert
        const browserStyleString = browserShape["style"];
        const browserStyle = uiMockupShapesTestHelper.getStyleObject(browserStyleString);
        expect(browserStyle["strokeWidth"]).toEqual("2");
        expect(browserStyle["strokeColor"]).toEqual("#A9BFD6");
        expect(browserStyle["fillColor"]).toEqual("none");

        const topBarStyleString = browserShape["children"][0].getStyle();
        const topBarStyle = uiMockupShapesTestHelper.getStyleObject(topBarStyleString);
        expect(topBarStyle["strokeWidth"]).toEqual("2");
        expect(topBarStyle["strokeColor"]).toEqual("#A9BFD6");
        expect(topBarStyle["fillColor"]).toEqual("#DBE5F3");

        const contentBoxStyleString = browserShape["children"][1].getStyle();
        const contentBoxStyle = uiMockupShapesTestHelper.getStyleObject(contentBoxStyleString);
        expect(contentBoxStyle["strokeColor"]).toEqual("#A9BFD6");
    });

    it("Browser Template Test Scroll Bar", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);
        //mock a shape here.
        const shape = {
            props: [
                {name: "length", value: "2"},
                {name: "ScrollBar", value: "true"}
            ]
        };

        //act
        const browserShape = templates["Browser"](<IShape>shape);
        //assert
        const browserStyleString = browserShape["style"];
        const browserStyle = uiMockupShapesTestHelper.getStyleObject(browserStyleString);
        expect(browserStyle["strokeWidth"]).toEqual("2");
        expect(browserStyle["strokeColor"]).toEqual("#A9BFD6");
        expect(browserStyle["fillColor"]).toEqual("none");

        const topBarStyleString = browserShape["children"][0].getStyle();
        const topBarStyle = uiMockupShapesTestHelper.getStyleObject(topBarStyleString);
        expect(topBarStyle["strokeWidth"]).toEqual("2");
        expect(topBarStyle["strokeColor"]).toEqual("#A9BFD6");
        expect(topBarStyle["fillColor"]).toEqual("#DBE5F3");

        const contentBoxStyleString = browserShape["children"][1].getStyle();
        const contentBoxStyle = uiMockupShapesTestHelper.getStyleObject(contentBoxStyleString);
        expect(contentBoxStyle["strokeColor"]).toEqual("#A9BFD6");
    });

    it("window shape test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);
        //mock a shape here.
        const shape = {
            props: [
                {
                    "name": "IsKeepAspectRatio",
                    "value": false
                },
                {
                    "name": "IsNodeHighlighted",
                    "value": "true"
                },
                {
                    "name": "State",
                    "value": "Disabled"
                },
                {
                    "name": "FocusControl",
                    "value": "NotInFocus"
                },
                {
                    "name": "Visible",
                    "value": "true"
                },
                {
                    "name": "ScrollBar",
                    "value": "true"
                },
                {
                    "name": "ShowButtons",
                    "value": "All"
                }
            ]
        };

        //act
        const windowShape = <any>templates["Window"](<IShape>shape);

        //assert
        const windowStyle = uiMockupShapesTestHelper.getStyleObject(windowShape["style"]);
        //
        expect(windowStyle.strokeWidth).toEqual("2");
        expect(windowStyle.strokeColor).toEqual("#A8BED5");

        const topBarStyle = uiMockupShapesTestHelper.getStyleObject(windowShape.children[0].style);
        expect(topBarStyle.strokeWidth).toEqual("2");
        expect(topBarStyle.strokeColor).toEqual("#A8BED5");
        expect(topBarStyle.fillColor).toEqual("#DBE6F4");


        //strokeColor=#A9BFD6;foldable=0;selectable=0;
        const childShape = uiMockupShapesTestHelper.getStyleObject(windowShape.children[1].style);
        //children are not selectable
        expect(childShape.selectable).toEqual("0");
    });

    it("Highlight Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);
        //mock a shape here.
        const shape = <IShape>{};
        const highlightProp = <IProp>{};
        highlightProp.name = "IsNodeHighlighted";
        highlightProp.value = "true";
        shape.props = [highlightProp];
        //act
        const mxCell = templates[UIMockupShapes.BUTTON](shape);
        //assert
        expect(mxCell).not.toBeNull();
        const cellChildren = mxCell["children"];
        expect(mxCell).not.toBeNull();
        expect(cellChildren.length > 0).toBeTruthy();
        const firstChild = cellChildren[0];
        const style = uiMockupShapesTestHelper.getStyleObject(firstChild["style"]);
        expect(style["strokeWidth"]).toEqual(UiMockupShapeFactory.highlightStrokeWidth.toString());
        expect(style["strokeColor"]).toEqual(UiMockupShapeFactory.highlightStrokeColor);
    });

    it("Highlight callout Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initDefaultTemplates(templates);
        //uiMockupShapeFactory.initTemplates(templates);
        //mock a shape here.
        const shape = <IShape>{};
        const highlightProp = <IProp>{};
        highlightProp.name = "IsNodeHighlighted";
        highlightProp.value = "true";
        shape.props = [highlightProp];
        //act
        const mxCell = templates[Shapes.CALLOUT](shape);
        //assert
        expect(mxCell).not.toBeNull();
        const cellChildren = mxCell["children"];
        expect(cellChildren).not.toBeNull();
        expect(cellChildren.length > 0).toBeTruthy();
        const firstChild = cellChildren[0];
        const style = uiMockupShapesTestHelper.getStyleObject(firstChild["style"]);
        expect(style["strokeWidth"]).toEqual(UiMockupShapeFactory.highlightStrokeWidth.toString());
        expect(style["strokeColor"]).toEqual(UiMockupShapeFactory.highlightStrokeColor);
        expect(style["shape"]).toEqual(CalloutShape.getName);

    });

    it("Disable Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);
        //mock a shape here.
        const shape = <IShape>{};
        const stateProperty = <IProp>{};
        stateProperty.name = "State";
        stateProperty.value = "Disabled";
        shape.props = [stateProperty];
        //act
        const mxCell = templates[UIMockupShapes.NUMERIC_SPINNER](shape);
        //assert
        expect(mxCell).not.toBeNull();
        const cellChildren = mxCell["children"];
        expect(cellChildren).not.toBeNull();
        expect(cellChildren.length > 3).toBeTruthy();
        const firstChild = cellChildren[3];
        const style = uiMockupShapesTestHelper.getStyleObject(firstChild["style"]);
        expect(style["fillColor"]).toEqual(UiMockupShapeFactory.disableStateFillColor);
        expect(style["opacity"]).toEqual(UiMockupShapeFactory.disableStateOpacity.toString());
    });

    it("DropDown List Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);
        //mock a shape properties here.
        const shape = <IShape>{};
        const listItemProp = <IProp>{};
        listItemProp.name = "ListItem";
        const itemName = "ItemName";
        const value = {
            checked: true,
            name: itemName
        };
        listItemProp.value = value;
        shape.props = [listItemProp];
        //act
        const mxCell = templates[UIMockupShapes.DROPDOWN_LIST](shape);
        //assert
        expect(mxCell).not.toBeNull();
        const cellChildren = mxCell["children"];
        expect(cellChildren).not.toBeNull();
        expect(cellChildren.length > 0).toBeTruthy();
        const lastChild = cellChildren[cellChildren.length - 1];
        expect(lastChild.value).toEqual(itemName);
    });

    it("Text Area No Scroll Bar", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);
        //mock a shape here.
        const shape = {
            props: [
                {
                    "name": "IsKeepAspectRatio",
                    "value": false
                },
                {
                    "name": "ScrollBar",
                    "value": "false"
                }
            ]
        };
        //act
        const mxCell = templates[UIMockupShapes.TEXT_AREA](<IShape>shape);
        //assert
        expect(mxCell).not.toBeNull();
        const style = uiMockupShapesTestHelper.getStyleObject(mxCell["style"]);
        expect(style.strokeWidth).toEqual("2");
        expect(style.strokeColor).toEqual("#7F98A9");
        expect(mxCell["children"].length).toEqual(1);
    });

    it("Text Area Scroll Bar", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);
        //mock a shape here.
        const shape = {
            height: 60,
            props: [
                {
                    "name": "IsKeepAspectRatio",
                    "value": false
                },
                {
                    "name": "ScrollBar",
                    "value": "true"
                }
            ]
        };
        //act
        const mxCell = templates[UIMockupShapes.TEXT_AREA](<IShape>shape);
        //assert
        expect(mxCell).not.toBeNull();
        const style = uiMockupShapesTestHelper.getStyleObject(mxCell["style"]);
        expect(style.strokeWidth).toEqual("2");
        expect(style.strokeColor).toEqual("#7F98A9");
        expect(mxCell["children"].length).toEqual(2);
    });

    it("Slider Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);

        //mock a shape here.
        const shape = {
            //labelStyle: labelStyle,
            props: [
                {name: "length", value: "2"},
                {name: "Orientation", value: "Vertical"},
                {name: "SliderValue", value: "50"}
            ]
        };

        //act
        const sliderShape = templates["Slider"](<IShape>shape);

        //assert

        expect(sliderShape["children"].length).toBeGreaterThan(0);
        const contentStyleString = sliderShape["children"][0].getStyle();
        const contentStyle = uiMockupShapesTestHelper.getStyleObject(contentStyleString);
        expect(contentStyle["strokeWidth"]).toEqual("1");
        expect(contentStyle["rounded"]).toEqual("1");
    });

    it("Scrollbar Vertical Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);

        //mock a shape here.
        const shape = {
            //labelStyle: labelStyle,
            props: [
                {name: "length", value: "2"},
                {name: "Orientation", value: "Vertical"},
                {name: "ScrollValue", value: "50"}
            ]
        };

        //act
        const scrollShape = templates["Scrollbar"](<IShape>shape);

        //assert

        expect(scrollShape["children"].length).toBeGreaterThan(0);
        const contentStyleString = scrollShape["children"][0].getStyle();
        const contentStyle = uiMockupShapesTestHelper.getStyleObject(contentStyleString);
        expect(contentStyle["strokeWidth"]).toEqual("1");
        expect(contentStyle["shape"]).toEqual("triangle");
    });

    it("Scrollbar Horizontal Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);

        //mock a shape here.
        const shape = {
            //labelStyle: labelStyle,
            props: [
                {name: "length", value: "2"},
                {name: "Orientation", value: "Horizontal"},
                {name: "ScrollValue", value: "50"}
            ]
        };

        //act
        const scrollShape = templates["Scrollbar"](<IShape>shape);

        //assert

        expect(scrollShape["children"].length).toBeGreaterThan(0);
        const contentStyleString = scrollShape["children"][0].getStyle();
        const contentStyle = uiMockupShapesTestHelper.getStyleObject(contentStyleString);
        expect(contentStyle["strokeWidth"]).toEqual("1");
        expect(contentStyle["shape"]).toEqual("triangle");
    });

    it("ProgressBar Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);

        //mock a shape here.
        const shape = {
            //labelStyle: labelStyle,
            props: [
                {name: "length", value: "2"},
                {name: "ProgressBarStyle", value: "Standard"},
                {name: "ProgressValue", value: "75"}
            ]
        };

        //act
        const progressShape = templates["ProgressBar"](<IShape>shape);

        //assert

        expect(progressShape["children"].length).toBeGreaterThan(0);
        const contentStyleString = progressShape["children"][0].getStyle();
        const contentStyle = uiMockupShapesTestHelper.getStyleObject(contentStyleString);
        expect(contentStyle["strokeWidth"]).toEqual("0");
        expect(contentStyle["shape"]).toEqual("highlightEllipse");
    });

    it("List Box", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);
        //mock a shape here.
        const shape = {
            height: 60,
            props: [
                {
                    "name": "IsKeepAspectRatio",
                    "value": false
                },
                {
                    "name": "ListItem",
                    "value": {name: "checked", value: true}
                }
            ]
        };
        //act
        const mxCell = templates[UIMockupShapes.LIST](<IShape>shape);
        //assert
        expect(mxCell).not.toBeNull();
        const style = uiMockupShapesTestHelper.getStyleObject(mxCell["style"]);
        expect(style.strokeWidth).toEqual("1");
        expect(style.strokeColor).toEqual("#808080");
        expect(mxCell["children"].length).toEqual(2);
    });

    it("Accordion Normal Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);

        //mock a shape here.
        const shape = {
            width: 300,
            height: 450,
            props: [
                {name: "length", value: "3"},
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer1"}
                },
                {
                    name: "ListItem",
                    value: {checked: true, name: "Drawer2 which is going to make accordion item so big"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer3"}
                }
            ]
        };

        //act
        const accordionShape = templates["Accordion"](<IShape>shape);

        //assert

        expect(accordionShape["children"].length).toBeGreaterThan(0);
        const contentStyleString = accordionShape["children"][0].getStyle();
        const contentStyle = uiMockupShapesTestHelper.getStyleObject(contentStyleString);
        expect(contentStyle["strokeWidth"]).toEqual("1");
        expect(contentStyle["shape"]).toEqual("rectangle");
    });

    it("Accordion With Scrollbars Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);

        const labelStyle = new LabelStyle();
        labelStyle.isItalic = true;
        labelStyle.isBold = true;
        labelStyle.isUnderline = true;
        labelStyle.foreground = "#0000FF";
        labelStyle.fontFamily = "Arial";
        labelStyle.fontSize = "20";

        //mock a shape here.
        const shape = {
            labelStyle: labelStyle,
            width: 200,
            height: 150,
            props: [
                {name: "length", value: "3"},
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer1"}
                },
                {
                    name: "ListItem",
                    value: {checked: true, name: "Drawer2 which is going to make accordion item so big"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer3"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer4"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer5"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer6"}
                }
            ]
        };

        //act
        const accordionShape = templates["Accordion"](<IShape>shape);

        //assert

        expect(accordionShape["children"].length).toBeGreaterThan(0);
        const contentStyleString = accordionShape["children"][0].getStyle();
        const contentStyle = uiMockupShapesTestHelper.getStyleObject(contentStyleString);
        expect(contentStyle["strokeWidth"]).toEqual("0");
        expect(contentStyle["shape"]).toEqual("rectangle");
    });

    it("Tab", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);
        //mock a shape here.

        const labelStyle = new LabelStyle();
        labelStyle.isItalic = false;
        labelStyle.isBold = true;
        labelStyle.isUnderline = true;
        labelStyle.foreground = "#0000FF";
        labelStyle.fontFamily = "Arial";
        labelStyle.fontSize = "20";

        const shape = {
            height: 60,
            labelStyle: labelStyle,
            props: [
                {
                    "name": "TabBar",
                    "value": {name: "checked", value: true}
                },
                {
                    name: "Scrollbar",
                    value: false
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer1"}
                },
                {
                    name: "ListItem",
                    value: {checked: true, name: "Drawer2 which is going to make accordion item so big"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer3"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer4"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer5"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer6"}
                }
            ]
        };
        //act
        const mxCell = templates[UIMockupShapes.TAB](<IShape>shape);
        //assert
        expect(mxCell).not.toBeNull();
        const style = uiMockupShapesTestHelper.getStyleObject(mxCell["style"]);
        expect(style.strokeWidth).toEqual("2");
        expect(mxCell["children"].length).toEqual(2);
        expect(mxCell["children"][1].children.length).toEqual(6);
    });

    it("Tab Orientation left", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);
        //mock a shape here.

        const labelStyle = new LabelStyle();
        labelStyle.isItalic = false;
        labelStyle.isBold = true;
        labelStyle.isUnderline = true;
        labelStyle.foreground = "#0000FF";
        labelStyle.fontFamily = "Arial";
        labelStyle.fontSize = "20";

        const shape = {
            height: 60,
            width: 200,
            labelStyle: labelStyle,
            props: [
                {
                    "name": "TabBar",
                    "value": {name: "checked", value: false}
                },
                {
                    name: "Scrollbar",
                    value: true
                },
                {
                    name: "Orientation",
                    value: "Left"
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer1"}
                },
                {
                    name: "ListItem",
                    value: {checked: true, name: "Drawer2 which is going to make accordion item so big"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer3"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer4"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer5"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer6"}
                }
            ]
        };
        //act
        const mxCell = templates[UIMockupShapes.TAB](<IShape>shape);
        //assert
        expect(mxCell).not.toBeNull();
        const style = uiMockupShapesTestHelper.getStyleObject(mxCell["style"]);
        expect(style.strokeWidth).toEqual("2");
        expect(mxCell["children"].length).toEqual(3);
        expect(mxCell["children"][1].children.length).toEqual(5);
    });

    it("Tab Orientation right", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);
        //mock a shape here.

        const labelStyle = new LabelStyle();
        labelStyle.isItalic = false;
        labelStyle.isBold = true;
        labelStyle.isUnderline = true;
        labelStyle.foreground = "#0000FF";
        labelStyle.fontFamily = "Arial";
        labelStyle.fontSize = "20";

        const shape = {
            height: 60,
            width: 200,
            labelStyle: labelStyle,
            props: [
                {
                    "name": "TabBar",
                    "value": {name: "checked", value: false}
                },
                {
                    name: "Scrollbar",
                    value: true
                },
                {
                    name: "Orientation",
                    value: "Right"
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer1Drawer3Drawer3Drawer3"}
                },
                {
                    name: "ListItem",
                    value: {checked: true, name: "Drawer2 which is going to make accordion item so big"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer3Drawer3Drawer3"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer4"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer5Drawer3"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer6"}
                }
            ]
        };
        //act
        const mxCell = templates[UIMockupShapes.TAB](<IShape>shape);
        //assert
        expect(mxCell).not.toBeNull();
        expect(mxCell["children"][1].children.length).toEqual(3);
    });

    it("Tab Orientation botttom", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);
        //mock a shape here.

        const labelStyle = new LabelStyle();
        labelStyle.isItalic = false;
        labelStyle.isBold = true;
        labelStyle.isUnderline = true;
        labelStyle.foreground = "#0000FF";
        labelStyle.fontFamily = "Arial";
        labelStyle.fontSize = "20";

        const shape = {
            height: 60,
            width: 200,
            labelStyle: labelStyle,
            props: [
                {
                    "name": "TabBar",
                    "value": {name: "checked", value: false}
                },
                {
                    name: "Scrollbar",
                    value: true
                },
                {
                    name: "Orientation",
                    value: "Bottom"
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer1"}
                },
                {
                    name: "ListItem",
                    value: {checked: true, name: "Drawer2 which is going to make accordion item so big"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer3"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer4"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer5"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer6"}
                }
            ]
        };
        //act
        const mxCell = templates[UIMockupShapes.TAB](<IShape>shape);
        //assert
        expect(mxCell).not.toBeNull();

        expect(mxCell["children"][1].children.length).toEqual(5);
    });

    it("Tab Orientation top", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);
        //mock a shape here.

        const labelStyle = new LabelStyle();
        labelStyle.isItalic = false;
        labelStyle.isBold = true;
        labelStyle.isUnderline = true;
        labelStyle.foreground = "#0000FF";
        labelStyle.fontFamily = "Arial";
        labelStyle.fontSize = "20";

        const shape = {
            height: 60,
            width: 200,
            labelStyle: labelStyle,
            props: [
                {
                    "name": "TabBar",
                    "value": {name: "checked", value: false}
                },
                {
                    name: "Scrollbar",
                    value: true
                },
                {
                    name: "Orientation",
                    value: "Top"
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer1"}
                },
                {
                    name: "ListItem",
                    value: {checked: true, name: "Drawer2 which is going to make accordion item so big"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer3"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer4"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer5"}
                },
                {
                    name: "ListItem",
                    value: {checked: false, name: "Drawer6"}
                }
            ]
        };
        //act
        const mxCell = templates[UIMockupShapes.TAB](<IShape>shape);
        //assert
        expect(mxCell).not.toBeNull();
        expect(mxCell["children"][1].children.length).toEqual(5);
    });

    it("ContextMenu General Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);

        const labelStyle = new LabelStyle();
        labelStyle.isItalic = true;
        labelStyle.isBold = true;
        labelStyle.isUnderline = true;
        labelStyle.foreground = "#0000FF";
        labelStyle.fontFamily = "Arial";
        labelStyle.fontSize = "20";

        //mock a shape here.
        const shape = {
            width: 300,
            height: 450,
            labelStyle: labelStyle,
            props: [
                {name: "length", value: "3"},
                {
                    name: "TreeItem",
                    value: {
                        isEnabled: true,
                        isSeparator: false,
                        text: "File",
                        isSelected: false,
                        hasChildTreeItems: true
                    }
                },
                {
                    name: "TreeItem",
                    value: {
                        isEnabled: false,
                        isSeparator: false,
                        text: "View",
                        isSelected: true,
                        hasChildTreeItems: false
                    }
                },
                {
                    name: "TreeItem",
                    value: {
                        isEnabled: false,
                        isSeparator: true,
                        text: "View",
                        isSelected: false,
                        hasChildTreeItems: true
                    }
                },
                {name: "Orientation", value: "Vertical"}
            ]
        };

        //act
        const contextMenuShape = templates["ContextMenu"](<IShape>shape);

        //assert
        const contextMenuStyleString = contextMenuShape["style"];
        const contextMenuStyle = uiMockupShapesTestHelper.getStyleObject(contextMenuStyleString);
        expect(contextMenuStyle["strokeWidth"]).toEqual("1");
        expect(contextMenuStyle["fillColor"]).toEqual("#FCFCFC");

        expect(contextMenuShape["children"].length).toBeGreaterThan(0);
        const contentStyleString = contextMenuShape["children"][0].getStyle();
        const contentStyle = uiMockupShapesTestHelper.getStyleObject(contentStyleString);
        expect(contentStyle["strokeWidth"]).toEqual("1");
        expect(contentStyle["shape"]).toEqual("rectangle");

    });

    it("ContextMenu Small Shape Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);

        const labelStyle = new LabelStyle();
        labelStyle.isItalic = true;
        labelStyle.isBold = true;
        labelStyle.isUnderline = true;
        labelStyle.foreground = "#0000FF";
        labelStyle.fontFamily = "Arial";
        labelStyle.fontSize = "20";

        //mock a shape here.
        const shape = {
            width: 90,
            height: 90,
            labelStyle: labelStyle,
            props: [
                {name: "length", value: "3"},
                {
                    name: "TreeItem",
                    value: {
                        isEnabled: true,
                        isSeparator: false,
                        text: "File",
                        isSelected: false,
                        hasChildTreeItems: true
                    }
                },
                {
                    name: "TreeItem",
                    value: {
                        isEnabled: false,
                        isSeparator: false,
                        text: "View",
                        isSelected: true,
                        hasChildTreeItems: false
                    }
                },
                {
                    name: "TreeItem",
                    value: {
                        isEnabled: false,
                        isSeparator: true,
                        text: "View",
                        isSelected: false,
                        hasChildTreeItems: true
                    }
                },
                {name: "Orientation", value: "Vertical"}
            ]
        };

        //act
        const contextMenuShape = templates["ContextMenu"](<IShape>shape);

        //assert
        const contextMenuStyleString = contextMenuShape["style"];
        const contextMenuStyle = uiMockupShapesTestHelper.getStyleObject(contextMenuStyleString);
        expect(contextMenuStyle["strokeWidth"]).toEqual("1");
        expect(contextMenuStyle["fillColor"]).toEqual("#FCFCFC");

        expect(contextMenuShape["children"].length).toBeGreaterThan(0);
        const contentStyleString = contextMenuShape["children"][0].getStyle();
        const contentStyle = uiMockupShapesTestHelper.getStyleObject(contentStyleString);
        expect(contentStyle["strokeWidth"]).toEqual("1");
        expect(contentStyle["shape"]).toEqual("rectangle");

    });

    it("icon shape data's IconKey can be extracted and passed alone", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = {};
        uiMockupShapeFactory.initTemplates(<IShapeTemplates>templates);

        const shape = {
            "type": "Icon",
            "props": [
                {
                    "name": "IconKey",
                    "value": "_new1"
                }
            ]
        };

        //act
        const iconShape = templates[IconShape.shapeName](<IShape>shape);
        //
        const iconStyle = uiMockupShapesTestHelper.getStyleObject(iconShape.style);
        expect(iconStyle.IconKey).toEqual("_new1");
        expect(iconStyle.shape).toEqual(IconShape.shapeName);
    });

    it("TreeView General Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);

        const labelStyle = new LabelStyle();
        labelStyle.isItalic = true;
        labelStyle.isBold = true;
        labelStyle.isUnderline = true;
        labelStyle.foreground = "#0000FF";
        labelStyle.fontFamily = "Arial";
        labelStyle.fontSize = "20";

        //mock a shape here.
        const shape = {
            width: 300,
            height: 450,
            labelStyle: labelStyle,
            props: [
                {name: "length", value: "3"},
                {name: "TreeIcon", value: "Triangle"},
                {name: "Folder", value: "Show"},
                {name: "ConnectingLines", value: "Show"},
                {
                    name: "TreeItem",
                    value: {
                        isEnabled: true,
                        isSeparator: false,
                        text: "File",
                        isSelected: false,
                        hasChildTreeItems: true,
                        level: "0"
                    }
                },
                {
                    name: "TreeItem",
                    value: {
                        isEnabled: false,
                        isSeparator: false,
                        text: "View",
                        isSelected: true,
                        hasChildTreeItems: false,
                        level: "1"
                    }
                },
                {
                    name: "TreeItem",
                    value: {
                        isEnabled: false,
                        isSeparator: false,
                        text: "View",
                        isSelected: false,
                        hasChildTreeItems: true,
                        level: "2"
                    }
                },
                {
                    name: "ChildTreeItem",
                    value: {
                        isEnabled: true,
                        isSeparator: false,
                        text: "View",
                        isSelected: false,
                        hasChildTreeItems: true,
                        level: "0.0"
                    }
                },
                {
                    name: "ChildTreeItem",
                    value: {
                        isEnabled: true,
                        isSeparator: false,
                        text: "View",
                        isSelected: false,
                        hasChildTreeItems: false,
                        level: "0.1"
                    }
                },
                {
                    name: "ChildTreeItem",
                    value: {
                        isEnabled: true,
                        isSeparator: false,
                        text: "View",
                        isSelected: false,
                        hasChildTreeItems: false,
                        level: "0.0.0"
                    }
                }
            ]
        };

        //act
        const treeViewShape = templates["Tree"](<IShape>shape);

        //assert
        const contextMenuStyleString = treeViewShape["style"];
        const contextMenuStyle = uiMockupShapesTestHelper.getStyleObject(contextMenuStyleString);
        expect(contextMenuStyle["strokeWidth"]).toEqual("1");
        expect(contextMenuStyle["fillColor"]).toEqual("#FFFFFF");

        expect(treeViewShape["children"].length).toBeGreaterThan(0);
        const contentStyleString = treeViewShape["children"][0].getStyle();
        const contentStyle = uiMockupShapesTestHelper.getStyleObject(contentStyleString);
        expect(contentStyle["strokeWidth"]).toEqual("0");
        expect(contentStyle["shape"]).toEqual("rectangle");

    });

    it("TreeView Small Width & Height Test", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);

        const labelStyle = new LabelStyle();
        labelStyle.isItalic = true;
        labelStyle.isBold = true;
        labelStyle.isUnderline = true;
        labelStyle.foreground = "#0000FF";
        labelStyle.fontFamily = "Arial";
        labelStyle.fontSize = "20";

        //mock a shape here.
        const shape = {
            width: 150,
            height: 200,
            labelStyle: labelStyle,
            props: [
                {name: "length", value: "3"},
                {name: "TreeIcon", value: "PlusMinus"},
                {name: "Folder", value: "Show"},
                {name: "ConnectingLines", value: "Show"},
                {
                    name: "TreeItem",
                    value: {
                        isEnabled: true,
                        isSeparator: false,
                        text: "File",
                        isSelected: false,
                        hasChildTreeItems: true,
                        level: "0"
                    }
                },
                {
                    name: "TreeItem",
                    value: {
                        isEnabled: false,
                        isSeparator: false,
                        text: "View",
                        isSelected: true,
                        hasChildTreeItems: false,
                        level: "1"
                    }
                },
                {
                    name: "TreeItem",
                    value: {
                        isEnabled: false,
                        isSeparator: false,
                        text: "View",
                        isSelected: false,
                        hasChildTreeItems: true,
                        level: "2"
                    }
                },
                {
                    name: "ChildTreeItem",
                    value: {
                        isEnabled: true,
                        isSeparator: false,
                        text: "View",
                        isSelected: false,
                        hasChildTreeItems: true,
                        level: "0.0"
                    }
                },
                {
                    name: "ChildTreeItem",
                    value: {
                        isEnabled: true,
                        isSeparator: false,
                        text: "View which is going to be greater that the prepaid width",
                        isSelected: false,
                        hasChildTreeItems: false,
                        level: "0.1"
                    }
                },
                {
                    name: "ChildTreeItem",
                    value: {
                        isEnabled: true,
                        isSeparator: false,
                        text: "View",
                        isSelected: false,
                        hasChildTreeItems: false,
                        level: "0.0.0"
                    }
                },
                {
                    name: "ChildTreeItem",
                    value: {
                        isEnabled: true,
                        isSeparator: false,
                        text: "View",
                        isSelected: false,
                        hasChildTreeItems: false,
                        level: "0.0.1"
                    }
                },
                {
                    name: "ChildTreeItem",
                    value: {
                        isEnabled: true,
                        isSeparator: false,
                        text: "View",
                        isSelected: false,
                        hasChildTreeItems: false,
                        level: "0.0.2"
                    }
                },
                {
                    name: "ChildTreeItem",
                    value: {
                        isEnabled: true,
                        isSeparator: false,
                        text: "View",
                        isSelected: false,
                        hasChildTreeItems: false,
                        level: "0.0.3"
                    }
                },
                {
                    name: "ChildTreeItem",
                    value: {
                        isEnabled: true,
                        isSeparator: false,
                        text: "View",
                        isSelected: false,
                        hasChildTreeItems: false,
                        level: "0.2"
                    }
                }
            ]
        };

        //act
        const treeViewShape = templates["Tree"](<IShape>shape);

        //assert
        const contextMenuStyleString = treeViewShape["style"];
        const contextMenuStyle = uiMockupShapesTestHelper.getStyleObject(contextMenuStyleString);
        expect(contextMenuStyle["strokeWidth"]).toEqual("1");
        expect(contextMenuStyle["fillColor"]).toEqual("#FFFFFF");

        expect(treeViewShape["children"].length).toBeGreaterThan(0);
        const contentStyleString = treeViewShape["children"][0].getStyle();
        const contentStyle = uiMockupShapesTestHelper.getStyleObject(contentStyleString);
        expect(contentStyle["strokeWidth"]).toEqual("0");
        expect(contentStyle["shape"]).toEqual("rectangle");
    });

    it("Table Test, no horizontal scroll bar.", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);

        const labelStyle = new LabelStyle();
        labelStyle.isItalic = true;
        labelStyle.isBold = true;
        labelStyle.isUnderline = true;
        labelStyle.foreground = "#0000FF";
        labelStyle.fontFamily = "Arial";
        labelStyle.fontSize = "20";

        //mock a shape here.
        const shape = {
            "id": 151,
            "name": "Table",
            "parentId": 51,
            "type": "Table",
            "height": 235,
            "width": 294,
            "x": 230,
            "y": 170,
            "zIndex": 1,
            "angle": 0,
            "stroke": "Transparent",
            "strokeOpacity": 1,
            "strokeWidth": 1,
            "strokeDashPattern": null,
            "fill": "Transparent",
            "gradientFill": null,
            "isGradient": false,
            "fillOpacity": 1,
            "shadow": false,
            "label": null,
            "labelTextAlignment": "left",
            "description": null,
            "props": [
                {
                    "name": "IsKeepAspectRatio",
                    "value": false
                },
                {
                    "name": "IsNodeHighlighted",
                    "value": "false"
                },
                {
                    "name": "State",
                    "value": "Enabled"
                },
                {
                    "name": "FocusControl",
                    "value": "NotInFocus"
                },
                {
                    "name": "Visible",
                    "value": "true"
                },
                {
                    "name": "OnClick",
                    "value": "DoNothing"
                },
                {
                    "name": "TableStyle",
                    "value": "AlternatingRows"
                },
                {
                    "name": "IncludeHeaderRow",
                    "value": "true"
                },
                {
                    "name": "ShowBorder",
                    "value": "true"
                },
                {
                    "name": "ShowScrollBars",
                    "value": "true"
                },
                {
                    "name": "TableColumnHeaders",
                    "value": [
                        {
                            "uniqueName": "05db9f17-f033-4777-af60-9967f0c71cd9",
                            "valueType": 1,
                            "header": "Column 1"
                        },
                        {
                            "uniqueName": "ea18101d-de25-4f01-9f91-695fe7eaaf4d",
                            "valueType": 1,
                            "header": "Column 2"
                        },
                        {
                            "uniqueName": "a4cafd16-3d56-4fd6-84ac-5d2fd024e1ff",
                            "valueType": 1,
                            "header": "Column 3"
                        }
                    ]
                },
                {
                    "name": "TableDataObject",
                    "value": [
                        {
                            "tableCellValue": {
                                "isNullable": false,
                                "contentText": "",
                                "value": "",
                                "maximumLength": 256
                            },
                            "uniqueName": "05db9f17-f033-4777-af60-9967f0c71cd9",
                            "valueType": 1,
                            "columnNumber": 1
                        },
                        {
                            "tableCellValue": {
                                "isNullable": false,
                                "contentText": "dsa",
                                "value": "dsa",
                                "maximumLength": 256
                            },
                            "uniqueName": "ea18101d-de25-4f01-9f91-695fe7eaaf4d",
                            "valueType": 1,
                            "columnNumber": 2
                        },
                        {
                            "tableCellValue": {
                                "isNullable": false,
                                "contentText": "",
                                "value": null,
                                "maximumLength": 256
                            },
                            "uniqueName": "a4cafd16-3d56-4fd6-84ac-5d2fd024e1ff",
                            "valueType": 1,
                            "columnNumber": 3
                        },
                        {
                            "tableCellValue": {
                                "isNullable": false,
                                "contentText": "dsa",
                                "value": "dsa",
                                "maximumLength": 256
                            },
                            "uniqueName": "05db9f17-f033-4777-af60-9967f0c71cd9",
                            "valueType": 1,
                            "columnNumber": 1
                        },
                        {
                            "tableCellValue": {
                                "isNullable": false,
                                "contentText": "das",
                                "value": "das",
                                "maximumLength": 256
                            },
                            "uniqueName": "ea18101d-de25-4f01-9f91-695fe7eaaf4d",
                            "valueType": 1,
                            "columnNumber": 2
                        },
                        {
                            "tableCellValue": {
                                "isNullable": false,
                                "contentText": "dsa",
                                "value": "dsa",
                                "maximumLength": 256
                            },
                            "uniqueName": "a4cafd16-3d56-4fd6-84ac-5d2fd024e1ff",
                            "valueType": 1,
                            "columnNumber": 3
                        }
                    ]
                }
            ],
            "labelStyle": {
                "textAlignment": null,
                "fontFamily": "Arial",
                "fontSize": "13.33",
                "isItalic": true,
                "isBold": true,
                "isUnderline": true,
                "foreground": "#000000"
            },
            "children": [],
            "isShape": true
        };

        //act
        const tableShape = templates["Table"](<IShape>shape);

        //assert
        const tableStyle = tableShape["style"];
        const tableStyleString = uiMockupShapesTestHelper.getStyleObject(tableStyle);
        expect(tableStyleString["strokeWidth"]).toEqual("1");
        expect(tableStyleString["strokeColor"]).toEqual("#91C3FF");
        expect(tableStyleString["fillColor"]).toEqual("white");

        expect(tableShape["children"].length).toEqual(3); //no horizontal scroll bar.
        const innerBoxStyleString = tableShape["children"][0].getStyle();
        const innerBoxStyle = uiMockupShapesTestHelper.getStyleObject(innerBoxStyleString);
        expect(innerBoxStyle["strokeWidth"]).toEqual("1");
        expect(innerBoxStyle["strokeColor"]).toEqual("Transparent");
        expect(innerBoxStyle["fillColor"]).toEqual("Transparent");
        expect(innerBoxStyle["shape"]).toEqual("rectangle");
    });


    it("Table Test, horizontal scroll bar.", () => {
        //arrange
        const uiMockupShapeFactory = new UiMockupShapeFactory();
        const templates = <IShapeTemplates>{};
        uiMockupShapeFactory.initTemplates(templates);

        const labelStyle = new LabelStyle();
        labelStyle.isItalic = true;
        labelStyle.isBold = true;
        labelStyle.isUnderline = true;
        labelStyle.foreground = "#0000FF";
        labelStyle.fontFamily = "Arial";
        labelStyle.fontSize = "20";

        //mock a shape here.
        const shape = {
            "id": 151,
            "name": "Table",
            "parentId": 51,
            "type": "Table",
            "height": 235,
            "width": 100,
            "x": 230,
            "y": 170,
            "zIndex": 1,
            "angle": 0,
            "stroke": "Transparent",
            "strokeOpacity": 1,
            "strokeWidth": 1,
            "strokeDashPattern": null,
            "fill": "Transparent",
            "gradientFill": null,
            "isGradient": false,
            "fillOpacity": 1,
            "shadow": false,
            "label": null,
            "labelTextAlignment": "left",
            "description": null,
            "props": [
                {
                    "name": "IsKeepAspectRatio",
                    "value": false
                },
                {
                    "name": "IsNodeHighlighted",
                    "value": "false"
                },
                {
                    "name": "State",
                    "value": "Enabled"
                },
                {
                    "name": "FocusControl",
                    "value": "NotInFocus"
                },
                {
                    "name": "Visible",
                    "value": "true"
                },
                {
                    "name": "OnClick",
                    "value": "DoNothing"
                },
                {
                    "name": "TableStyle",
                    "value": "AlternatingRows"
                },
                {
                    "name": "IncludeHeaderRow",
                    "value": "true"
                },
                {
                    "name": "ShowBorder",
                    "value": "true"
                },
                {
                    "name": "ShowScrollBars",
                    "value": "true"
                },
                {
                    "name": "TableColumnHeaders",
                    "value": [
                        {
                            "uniqueName": "05db9f17-f033-4777-af60-9967f0c71cd9",
                            "valueType": 1,
                            "header": "Column 1"
                        },
                        {
                            "uniqueName": "ea18101d-de25-4f01-9f91-695fe7eaaf4d",
                            "valueType": 1,
                            "header": "Column 2"
                        },
                        {
                            "uniqueName": "a4cafd16-3d56-4fd6-84ac-5d2fd024e1ff",
                            "valueType": 1,
                            "header": "Column 3"
                        }
                    ]
                },
                {
                    "name": "TableDataObject",
                    "value": [
                        {
                            "tableCellValue": {
                                "isNullable": false,
                                "contentText": "",
                                "value": "",
                                "maximumLength": 256
                            },
                            "uniqueName": "05db9f17-f033-4777-af60-9967f0c71cd9",
                            "valueType": 1,
                            "columnNumber": 1
                        },
                        {
                            "tableCellValue": {
                                "isNullable": false,
                                "contentText": "dsa",
                                "value": "dsa",
                                "maximumLength": 256
                            },
                            "uniqueName": "ea18101d-de25-4f01-9f91-695fe7eaaf4d",
                            "valueType": 1,
                            "columnNumber": 2
                        },
                        {
                            "tableCellValue": {
                                "isNullable": false,
                                "contentText": "",
                                "value": null,
                                "maximumLength": 256
                            },
                            "uniqueName": "a4cafd16-3d56-4fd6-84ac-5d2fd024e1ff",
                            "valueType": 1,
                            "columnNumber": 3
                        },
                        {
                            "tableCellValue": {
                                "isNullable": false,
                                "contentText": "dsa",
                                "value": "dsa",
                                "maximumLength": 256
                            },
                            "uniqueName": "05db9f17-f033-4777-af60-9967f0c71cd9",
                            "valueType": 1,
                            "columnNumber": 1
                        },
                        {
                            "tableCellValue": {
                                "isNullable": false,
                                "contentText": "das",
                                "value": "das",
                                "maximumLength": 256
                            },
                            "uniqueName": "ea18101d-de25-4f01-9f91-695fe7eaaf4d",
                            "valueType": 1,
                            "columnNumber": 2
                        },
                        {
                            "tableCellValue": {
                                "isNullable": false,
                                "contentText": "dsa",
                                "value": "dsa",
                                "maximumLength": 256
                            },
                            "uniqueName": "a4cafd16-3d56-4fd6-84ac-5d2fd024e1ff",
                            "valueType": 1,
                            "columnNumber": 3
                        }
                    ]
                }
            ],
            "labelStyle": {
                "textAlignment": null,
                "fontFamily": "Arial",
                "fontSize": "13.33",
                "isItalic": true,
                "isBold": true,
                "isUnderline": true,
                "foreground": "#000000"
            },
            "children": [],
            "isShape": true
        };

        //act
        const tableShape = templates["Table"](<IShape>shape);

        //assert
        const tableStyle = tableShape["style"];
        const tableStyleString = uiMockupShapesTestHelper.getStyleObject(tableStyle);
        expect(tableStyleString["strokeWidth"]).toEqual("1");
        expect(tableStyleString["strokeColor"]).toEqual("#91C3FF");
        expect(tableStyleString["fillColor"]).toEqual("white");

        expect(tableShape["children"].length).toEqual(4); //horizontal scroll bar is the extra shape.
        const innerBoxStyleString = tableShape["children"][0].getStyle();
        const innerBoxStyle = uiMockupShapesTestHelper.getStyleObject(innerBoxStyleString);
        expect(innerBoxStyle["strokeWidth"]).toEqual("1");
        expect(innerBoxStyle["strokeColor"]).toEqual("Transparent");
        expect(innerBoxStyle["fillColor"]).toEqual("Transparent");
        expect(innerBoxStyle["shape"]).toEqual("rectangle");
    });

});
