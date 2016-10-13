/* tslint:disable:max-file-line-count */

//fixme: this file is far to large. consider splitting it up
import {UIMockupShapes, UIMockupShapeProps} from "./utils/constants";
import {Style, Styles, MenuStyleObject} from "./utils/style-builder";
import {ShapeExtensions, MxFactory, DiagramHelper} from "./utils/helpers";
import {IconShape, CalloutShape, HighlightEllipse, CheckboxShape, TableCursorShape} from "./shapes-library";
import {AbstractShapeFactory, IShapeTemplates} from "./abstract-diagram-factory";
import {IShape, IProp} from "./models";
import {Helper} from "../../../shared";

export class UiMockupShapeFactory extends AbstractShapeFactory {
    public static highlightStrokeWidth = 2;
    public static highlightStrokeColor = "blue";
    public static disableStateOpacity = 55;
    public static disableStateFillColor = "#F6F6F6";

    public initTemplates(templates: IShapeTemplates) {
        templates[UIMockupShapes.HOTSPOT] = this.hotspot;
        templates[UIMockupShapes.HYPERLINK] = this.hyperlinkLabelParagraph;
        templates[UIMockupShapes.LABEL] = this.hyperlinkLabelParagraph;
        templates[UIMockupShapes.PARAGRAPH] = this.hyperlinkLabelParagraph;
        templates[UIMockupShapes.TEXTBOX] = this.textBox;
        templates[UIMockupShapes.BUTTON] = this.button;
        templates[UIMockupShapes.DROPDOWNBUTTON] = this.dropdownButton;
        templates[UIMockupShapes.NUMERIC_SPINNER] = this.numericSpinner;
        templates[UIMockupShapes.CHECKBOX] = this.checkbox;
        templates[UIMockupShapes.RADIOBUTTON] = this.radioButton;
        templates[UIMockupShapes.SPLITBUTTON] = this.splitButton;
        templates[UIMockupShapes.DATE_TIME_PICKER] = this.dateTimePicker;
        templates[UIMockupShapes.FRAME] = this.frame;
        templates[UIMockupShapes.DROPDOWN_LIST] = this.dropDownList;
        templates[UIMockupShapes.TEXT_AREA] = this.uIMockuptextArea;
        templates[UIMockupShapes.MENU] = this.uIMockupmenu;
        templates[UIMockupShapes.BROWSER] = this.browser;
        templates[UIMockupShapes.WINDOW] = this.window;
        templates[UIMockupShapes.SLIDER] = this.slider;
        templates[UIMockupShapes.SCROLLBAR] = this.scrollbar;
        templates[UIMockupShapes.PROGRESSBAR] = this.progressbar;
        templates[UIMockupShapes.LIST] = this.list;
        templates[UIMockupShapes.ACCORDION] = this.accordion;
        templates[UIMockupShapes.TAB] = this.tab;
        templates[UIMockupShapes.CONTEXTMENU] = this.contextMenu;
        templates[IconShape.shapeName] = this.iconShape;
        templates[UIMockupShapes.TREEVIEW] = this.treeview;
        templates[UIMockupShapes.TABLE] = this.table;
        return templates;
    }

    public customizeCalloutStyle(shape: IShape, style: Style) {
        this.applyDisabledStateForText(shape, style);
    }

    public customizeCallout(shape: IShape, callout: MxCell) {
        this.applyHighlightedDisabledStates(shape, callout, CalloutShape.getName);
    }

    private hotspot = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_STROKEWIDTH] = 1;
        style[mxConstants.STYLE_STROKECOLOR] = "#646464";
        style[mxConstants.STYLE_FILLCOLOR] = "#BDD0E6";
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_OPACITY] = 40;
        let vertex = super.createDefaultVertex(shape, style);
        this.applyHighlightedDisabledStates(shape, vertex);
        return vertex;
    };

    private hyperlinkLabelParagraph = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;
        style[mxConstants.STYLE_OVERFLOW] = "hidden";
        style[mxConstants.STYLE_FOLDABLE] = 0;
        this.applyDisabledStateForText(shape, style);
        let textContainer = super.createDefaultVertex(shape, style);
        this.applyHighlightedDisabledStates(shape, textContainer);
        return textContainer;
    };

    private textBox = (shape: IShape): MxCell => {
        const textProperty = ShapeExtensions.getPropertyByName(shape, UIMockupShapeProps.TEXT);
        shape.label = textProperty;

        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);

        if (!shape.strokeWidth || shape.stroke === "Transparent") {
            style[mxConstants.STYLE_STROKEWIDTH] = 1;
            style[mxConstants.STYLE_STROKECOLOR] = "#A8BED5";
        }
        style[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_LEFT;
        style[mxConstants.STYLE_FOLDABLE] = 0;
        this.applyDisabledStateForText(shape, style);
        let vertex = super.createDefaultVertex(shape, style);
        this.applyHighlightedDisabledStates(shape, vertex);
        return vertex;
    };

    private button = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_STROKEWIDTH] = 1;
        style[mxConstants.STYLE_STROKECOLOR] = "#A8BED5";
        style[mxConstants.STYLE_FILLCOLOR] = "#DBE6F4";
        style[mxConstants.STYLE_GRADIENTCOLOR] = "#FBFDFE";
        style[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_NORTH;
        style[mxConstants.STYLE_FOLDABLE] = 0;
        this.applyDisabledStateForText(shape, style);
        let button = super.createDefaultVertex(shape, style);
        this.applyHighlightedDisabledStates(shape, button);
        return button;
    };

    private dropdownButton = (shape: IShape): MxCell => {
        const ddbstyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        ddbstyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        ddbstyle[mxConstants.STYLE_STROKECOLOR] = "#A8BED5";
        ddbstyle[mxConstants.STYLE_FILLCOLOR] = "white";
        ddbstyle[mxConstants.STYLE_GRADIENTCOLOR] = "#DBE6F4";
        ddbstyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_NORTH;
        ddbstyle[mxConstants.STYLE_FOLDABLE] = 0;
        const markSize = {height: 3, width: 6};
        const dropdownButton = this.createDefaultVertex(shape, ddbstyle, true);
        //mark
        const markStyle = new Style();
        markStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
        markStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        markStyle[mxConstants.STYLE_STROKECOLOR] = "black";
        markStyle[mxConstants.STYLE_FILLCOLOR] = "black";
        markStyle[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_SOUTH;
        markStyle[Styles.STYLE_SELECTABLE] = 0;
        const markGeometry = MxFactory.geometry(
            (1 - (markSize.width / shape.width)) - 0.01, 0.5 - (markSize.height / shape.height) / 2,
            markSize.width,
            markSize.height);
        markGeometry.relative = true;
        const mark = MxFactory.vertex(null, markGeometry, markStyle.convertToString());
        dropdownButton.insert(mark);

        let labelText = "";
        for (let i = 0; i < shape.props.length; i++) {
            if (shape.props[i].name === "ListItem" && shape.props[i].value.checked) {
                labelText = Helper.escapeHTMLText(shape.props[i].value.name);
            }
        }
        //label
        const labelStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        labelStyle[mxConstants.STYLE_STROKECOLOR] = mxConstants.NONE;
        labelStyle[mxConstants.STYLE_FILLCOLOR] = "transparent";
        labelStyle[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_LEFT;
        labelStyle[Styles.STYLE_SELECTABLE] = 0;
        this.applyDisabledStateForText(shape, labelStyle);
        const labelGeometry = MxFactory.geometry(0, 0.05, shape.width * 0.9, shape.height * 0.9);
        labelGeometry.relative = true;
        const label = MxFactory.vertex(labelText, labelGeometry, labelStyle.convertToString());
        dropdownButton.insert(label);

        this.applyHighlightedDisabledStates(shape, dropdownButton);
        return dropdownButton;
    };

    private frame = (shape: IShape): MxCell => {

        const style = this.styleBuilder.createDefaultShapeStyle.call(shape, shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_STROKEWIDTH] = 2;
        style[mxConstants.STYLE_STROKECOLOR] = mxConstants.NONE;
        style[mxConstants.STYLE_FILLCOLOR] = "#FFFFFF";
        style[mxConstants.STYLE_FOLDABLE] = 0;
        const frame = this.createDefaultVertex(shape, style, true);

        let geometry = MxFactory.geometry(0, 0, shape.width, shape.height - 20);
        geometry.relative = true;
        geometry.offset = MxFactory.point(0, 20);
        style[mxConstants.STYLE_FILLCOLOR] = mxConstants.NONE;
        style[mxConstants.STYLE_STROKEWIDTH] = 2;
        style[mxConstants.STYLE_STROKECOLOR] = "#A9BFD6";
        style[Styles.STYLE_SELECTABLE] = 0;
        const border = MxFactory.vertex(null, geometry, style.convertToString());
        frame.insert(border);

        const rect = mxUtils.getSizeForString(shape.label, style["fontSize"], style["fontFamily"], null);
        let labelWidth = rect.width + 5;
        if (labelWidth > shape.width - 30) {
            labelWidth = shape.width - 30;
        }
        geometry = MxFactory.geometry(0, 0, labelWidth, rect.height * 1.22);
        geometry.relative = true;
        geometry.offset = MxFactory.point(20, -(rect.height / 2 - 16));
        style[mxConstants.STYLE_FILLCOLOR] = "#FFFFFF";
        style[mxConstants.STYLE_STROKEWIDTH] = 2;
        style[mxConstants.STYLE_STROKECOLOR] = mxConstants.NONE;
        style[Styles.STYLE_SELECTABLE] = 0;
        this.applyDisabledStateForText(shape, style);
        const labelShape = MxFactory.vertex(shape.label, geometry, style.convertToString());
        labelShape.getGeometry().relative = true;
        frame.insert(labelShape);

        this.applyHighlightedDisabledStates(shape, frame);
        return frame;
    };

    private dateTimePicker = (shape: IShape): MxCell => {
        //
        const calendarStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        calendarStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        calendarStyle[mxConstants.STYLE_STROKECOLOR] = "#A8BED5";
        calendarStyle[mxConstants.STYLE_FOLDABLE] = 0;
        //
        const calendar = this.createDefaultVertex(shape, calendarStyle, true);
        //
        //create calendarButton and add it to calendar
        //
        const calendarButtonStyle = new Style();
        calendarButtonStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        calendarButtonStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        calendarButtonStyle[mxConstants.STYLE_STROKECOLOR] = "#A8BED5";
        calendarButtonStyle[mxConstants.STYLE_FILLCOLOR] = "#DBE6F4";
        calendarButtonStyle[mxConstants.STYLE_GRADIENTCOLOR] = "#FBFDFE";
        calendarButtonStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_NORTH;
        calendarButtonStyle[mxConstants.STYLE_FOLDABLE] = 0;
        calendarButtonStyle[Styles.STYLE_SELECTABLE] = 0;
        //
        const calendarButton = MxFactory.vertex(null,
            MxFactory.geometry(shape.width - 20, //X
                0, //Y
                20, //W
                shape.height),
            calendarButtonStyle.convertToString());


        const calendarIconStyle = new Style();
        calendarIconStyle[Styles.STYLE_SELECTABLE] = 0;
        calendarIconStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        calendarIconStyle[mxConstants.STYLE_FILLCOLOR] = "black";
        calendarIconStyle[mxConstants.STYLE_SHAPE] = "calendaricon";
        calendarIconStyle[Styles.STYLE_SELECTABLE] = 0;


        const calendarIcon = MxFactory.cell("", MxFactory.geometry((20 - 10) / 2, (shape.height - 10) / 2, 10, 10));
        calendarIcon.vertex = true;
        calendarButton.insert(calendarIcon);
        calendarIcon.setStyle(calendarIconStyle.convertToString());
        calendar.insert(calendarButton);

        //
        //create a lable and add to calendar
        //

        const dateTimeString = DiagramHelper.findValueByName(shape.props, "DateTimeValue");
        const dateTimeValue = DiagramHelper.normalizeDateFormat(dateTimeString);
        const inputMode = DiagramHelper.findValueByName(shape.props, "InputMode");
        const displayFormat = DiagramHelper.findValueByName(shape.props, "DisplayFormat");

        //use angular date filter
        const labelText = DiagramHelper.formatDateTime(dateTimeValue, displayFormat, inputMode);

        const labelStyle = new Style();
        labelStyle[mxConstants.STYLE_STROKECOLOR] = mxConstants.NONE;
        labelStyle[mxConstants.STYLE_FILLCOLOR] = "transparent";
        labelStyle[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_LEFT;
        labelStyle[Styles.STYLE_SELECTABLE] = 0;
        labelStyle[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_MIDDLE;
        this.applyDisabledStateForText(shape, labelStyle);
        const labelGeometry = MxFactory.geometry(
            0,
            0,
            shape.width,
            shape.height);

        labelGeometry.relative = true;

        calendar.insert(
            MxFactory.vertex(
                labelText,
                labelGeometry,
                labelStyle.convertToString()
            )
        );

        this.applyHighlightedDisabledStates(shape, calendar);
        return calendar;
    };

    private uIMockuptextArea = (shape: IShape): MxCell => {
        const textAreaStyle = new Style();
        textAreaStyle[mxConstants.STYLE_STROKEWIDTH] = 2;
        textAreaStyle[mxConstants.STYLE_STROKECOLOR] = "#7F98A9";
        textAreaStyle[mxConstants.STYLE_FOLDABLE] = 0;
        const textArea = this.createDefaultVertex(shape, textAreaStyle, true);

        const scrollBar = ShapeExtensions.getPropertyByName(shape, "ScrollBar") === "true";
        let labelWidth: number;
        if (scrollBar) {
            labelWidth = shape.width - 22;
        } else {
            labelWidth = shape.width;
        }

        const labelStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        labelStyle[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_LEFT;
        labelStyle[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;
        labelStyle[Styles.STYLE_SELECTABLE] = 0;
        labelStyle[mxConstants.STYLE_OVERFLOW] = "hidden";
        this.applyDisabledStateForText(shape, labelStyle);
        const labelGeometry = MxFactory.geometry(1, 1, labelWidth - 2, shape.height - 2); //offsets the container boarder width
        labelGeometry.relative = false;
        const label = MxFactory.vertex(shape.label, labelGeometry, labelStyle.convertToString());
        textArea.insert(label);

        //scroll bar
        if (scrollBar) {
            const scrollBarStyle = new Style();
            scrollBarStyle[mxConstants.STYLE_STROKECOLOR] = "#7F98A9";
            scrollBarStyle[mxConstants.STYLE_FILLCOLOR] = "white";
            scrollBarStyle[mxConstants.STYLE_GRADIENTCOLOR] = "#F2F4F6";
            scrollBarStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_WEST;
            scrollBarStyle[Styles.STYLE_SELECTABLE] = 0;
            scrollBarStyle[mxConstants.STYLE_FOLDABLE] = 0;
            const scrollBarGeometry = MxFactory.geometry(labelWidth, 0, 20, shape.height - 1);
            scrollBarGeometry.relative = false;
            const scrollBarBox = MxFactory.vertex(null, scrollBarGeometry, scrollBarStyle.convertToString());

            const markSize = {height: 3, width: 6};

            //mark
            const mark1Style = new Style();
            mark1Style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
            mark1Style[mxConstants.STYLE_STROKEWIDTH] = 1;
            mark1Style[mxConstants.STYLE_STROKECOLOR] = "black";
            mark1Style[mxConstants.STYLE_FILLCOLOR] = "black";
            mark1Style[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_SOUTH;
            mark1Style[Styles.STYLE_SELECTABLE] = 0;
            const mark1Geometry = MxFactory.geometry(7, shape.height - 11, markSize.width, markSize.height);
            mark1Geometry.relative = false;
            const mark1 = MxFactory.vertex(null, mark1Geometry, mark1Style.convertToString());
            scrollBarBox.insert(mark1);

            //mark2
            const mark2Style = new Style();
            mark2Style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
            mark2Style[mxConstants.STYLE_STROKEWIDTH] = 1;
            mark2Style[mxConstants.STYLE_STROKECOLOR] = "black";
            mark2Style[mxConstants.STYLE_FILLCOLOR] = "black";
            mark2Style[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_NORTH;
            mark2Style[Styles.STYLE_SELECTABLE] = 0;
            const mark2Geometry = MxFactory.geometry(7, 10, markSize.width, markSize.height);
            mark2Geometry.relative = false;
            const mark2 = MxFactory.vertex(null, mark2Geometry, mark2Style.convertToString());
            scrollBarBox.insert(mark2);

            //scrollbar stub
            if (shape.height > 55) {
                const scrollBarStubStyle = new Style();
                scrollBarStubStyle[mxConstants.STYLE_STROKECOLOR] = "#7F98A9";
                scrollBarStubStyle[mxConstants.STYLE_STROKEWIDTH] = 2;
                scrollBarStubStyle[mxConstants.STYLE_FILLCOLOR] = "white";
                scrollBarStubStyle[mxConstants.STYLE_GRADIENTCOLOR] = "#F2F4F6";
                scrollBarStubStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_WEST;
                scrollBarStubStyle[Styles.STYLE_SELECTABLE] = 0;
                scrollBarStubStyle[mxConstants.STYLE_FOLDABLE] = 0;
                const scrollBarStubGeometry = MxFactory.geometry(0, 20, 19, shape.height / 5);
                scrollBarStubGeometry.relative = false;
                const scrollBarStub = MxFactory.vertex(null, scrollBarStubGeometry, scrollBarStyle.convertToString());
                scrollBarBox.insert(scrollBarStub);
            }
            textArea.insert(scrollBarBox);

        }
        this.applyHighlightedDisabledStates(shape, textArea);
        return textArea;
    };

    private splitButton = (shape: IShape): MxCell => {
        const splitButtonstyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        splitButtonstyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        splitButtonstyle[mxConstants.STYLE_STROKECOLOR] = "#A8BED5";
        splitButtonstyle[mxConstants.STYLE_FILLCOLOR] = "white";
        splitButtonstyle[mxConstants.STYLE_GRADIENTCOLOR] = "#DBE6F4";
        splitButtonstyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_NORTH;
        splitButtonstyle[mxConstants.STYLE_FOLDABLE] = 0;
        const markSize = {height: 3, width: 6};
        const splitButton = this.createDefaultVertex(shape, splitButtonstyle, true);
        //mark
        const markStyle = new Style();
        markStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
        markStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        markStyle[mxConstants.STYLE_STROKECOLOR] = "black";
        markStyle[mxConstants.STYLE_FILLCOLOR] = "black";
        markStyle[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_SOUTH;
        markStyle[Styles.STYLE_SELECTABLE] = 0;
        const markGeometry = MxFactory.geometry(
            (1 - (markSize.width / shape.width)) - 0.01, 0.5 - (markSize.height / shape.height) / 2,
            markSize.width,
            markSize.height);
        markGeometry.relative = true;
        const mark = MxFactory.vertex(null, markGeometry, markStyle.convertToString());
        splitButton.insert(mark);
        let labelText = "";
        for (let i = 0; i < shape.props.length; i++) {
            if (shape.props[i].name === "ListItem" && shape.props[i].value.checked) {
                labelText = Helper.escapeHTMLText(shape.props[i].value.name);
            }
        }
        //box
        const boxStyle = new Style();
        boxStyle[mxConstants.STYLE_STROKECOLOR] = "#A8BED5";
        boxStyle[mxConstants.STYLE_FILLCOLOR] = "transparent";
        boxStyle[Styles.STYLE_SELECTABLE] = 0;
        const boxGeometry = MxFactory.geometry(0.94, 0, shape.width * 0.06, shape.height);
        boxGeometry.relative = true;
        const box = MxFactory.vertex(null, boxGeometry, boxStyle.convertToString());
        splitButton.insert(box);

        //label
        const labelStyle = new Style();
        this.styleBuilder.applyLabelStyle(labelStyle, shape.labelStyle);
        labelStyle[mxConstants.STYLE_STROKECOLOR] = mxConstants.NONE;
        labelStyle[mxConstants.STYLE_FILLCOLOR] = "transparent";
        labelStyle[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_CENTER;
        labelStyle[Styles.STYLE_SELECTABLE] = 0;
        this.applyDisabledStateForText(shape, labelStyle);
        const labelGeometry = MxFactory.geometry(0, 0.06, shape.width * 0.94, shape.height * 0.9);
        labelGeometry.relative = true;
        const label = MxFactory.vertex(labelText, labelGeometry, labelStyle.convertToString());
        splitButton.insert(label);
        this.applyHighlightedDisabledStates(shape, splitButton);
        return splitButton;
    };


    private dropDownList = (shape: IShape): MxCell => {
        const ddbstyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        ddbstyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        ddbstyle[mxConstants.STYLE_STROKECOLOR] = "#A8BED5";
        ddbstyle[mxConstants.STYLE_FOLDABLE] = 0;
        //
        const markSize = {height: 3, width: 6};
        const dropdownButton = this.createDefaultVertex(shape, ddbstyle, true);

        //box
        const boxStyle = new Style();
        boxStyle[mxConstants.STYLE_STROKECOLOR] = "#A8BED5";
        boxStyle[mxConstants.STYLE_FILLCOLOR] = "#DBE6F4";
        boxStyle[mxConstants.STYLE_GRADIENTCOLOR] = "#FBFDFE";
        boxStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_NORTH;

        boxStyle[Styles.STYLE_SELECTABLE] = 0;
        const boxGeometry = MxFactory.geometry(0.94, 0, shape.width * 0.06, shape.height);
        boxGeometry.relative = true;
        const box = MxFactory.vertex(null, boxGeometry, boxStyle.convertToString());
        dropdownButton.insert(box);
        //mark
        const markStyle = new Style();
        markStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
        markStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        markStyle[mxConstants.STYLE_STROKECOLOR] = "black";
        markStyle[mxConstants.STYLE_FILLCOLOR] = "black";
        markStyle[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_SOUTH;
        markStyle[Styles.STYLE_SELECTABLE] = 0;
        const markGeometry = MxFactory.geometry(
            (1 - (markSize.width / shape.width)) - 0.01, 0.5 - (markSize.height / shape.height) / 2,
            markSize.width,
            markSize.height);
        markGeometry.relative = true;
        const mark = MxFactory.vertex(null, markGeometry, markStyle.convertToString());
        dropdownButton.insert(mark);
        let labelText = "";
        for (let i = 0; i < shape.props.length; i++) {
            if (shape.props[i].name === "ListItem" && shape.props[i].value.checked) {
                labelText = Helper.escapeHTMLText(shape.props[i].value.name);
            }
        }
        //label
        const labelStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        labelStyle[mxConstants.STYLE_STROKECOLOR] = mxConstants.NONE;
        labelStyle[mxConstants.STYLE_FILLCOLOR] = "transparent";
        labelStyle[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_CENTER;
        labelStyle[Styles.STYLE_SELECTABLE] = 0;
        this.applyDisabledStateForText(shape, labelStyle);
        const labelGeometry = MxFactory.geometry(0, 0.06, shape.width * 0.94, shape.height * 0.9);
        labelGeometry.relative = true;
        const label = MxFactory.vertex(labelText, labelGeometry, labelStyle.convertToString());
        dropdownButton.insert(label);
        this.applyHighlightedDisabledStates(shape, dropdownButton);
        return dropdownButton;
    };

    private checkbox = (shape: IShape): MxCell => {
        const checkBoxSize = 12;
        const innerRectSize = 8;
        const innerRectOffset = 2;
        const labelOffset = 2;
        const borderColor = "#A0AFC3";
        const checkMarkColor = "#1E395B";
        const brightGradientColor = "#F9FAFC";
        const darkGradientColor = "#C7D5E9";

        const checkbox = this.createCheckboxShape(shape, checkBoxSize, labelOffset);

        const rectangleStyle = new Style();
        const rectangle = this.createCheckboxRectangle(shape, rectangleStyle, checkBoxSize, borderColor);
        checkbox.insert(rectangle);

        const innerRect = this.createCheckboxInnerRectangle(shape, rectangleStyle, brightGradientColor, darkGradientColor, innerRectOffset, innerRectSize);
        rectangle.insert(innerRect);

        const isChecked = ShapeExtensions.getPropertyByName(shape, "Checked");
        if (isChecked === "true") {
            innerRect.insert(this.createCheckboxMark(shape, checkMarkColor, innerRectSize));
        }

        this.applyHighlightedDisabledStates(shape, checkbox);
        return checkbox;
    };

    private createHighlightedDisabledStyle(shapeModel: IShape): Style {
        const style = new Style();
        style[mxConstants.STYLE_FILLCOLOR] = "transparent";
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[Styles.STYLE_SELECTABLE] = 0;
        style[mxConstants.STYLE_STROKEWIDTH] = 0;
        return style;
    }

    private createHighlightedShapeStyle(shapeModel: IShape, style: Style, shapeStyle: string) {
        if (shapeStyle === CalloutShape.getName) {
            style[mxConstants.STYLE_SHAPE] = shapeStyle;
            this.moveCalloutAnchorPosition(shapeModel, style);
        } else {
            style[mxConstants.STYLE_SHAPE] = HighlightEllipse.getName;
        }
    }

    private createDisabledShapeStyle(shapeModel: IShape, style: Style, shapeStyle: string) {
        style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        if (shapeStyle !== null) {
            style[mxConstants.STYLE_SHAPE] = shapeStyle;
        }
        if (shapeStyle === CalloutShape.getName) {
            this.moveCalloutAnchorPosition(shapeModel, style);
        }
    }

    private applyDisabledStateForText(shapeModel: IShape, textStyle: Style) {
        const nodeState = ShapeExtensions.getPropertyByName(shapeModel, "State");
        if (nodeState === "Disabled") {
            textStyle[mxConstants.STYLE_TEXT_OPACITY] = 40;
        }
    }

    private applyHighlightedDisabledStates(shapeModel: IShape, mxShape: MxCell, shapeStyle: string = null) {
        const isNodeHighlighted = ShapeExtensions.getPropertyByName(shapeModel, "IsNodeHighlighted");
        const nodeState = ShapeExtensions.getPropertyByName(shapeModel, "State");
        if (isNodeHighlighted === "true" || nodeState === "Disabled") {
            let geometry, state, style;

            if (nodeState === "Disabled") {
                style = this.createHighlightedDisabledStyle(shapeModel);
                style[mxConstants.STYLE_FILLCOLOR] = UiMockupShapeFactory.disableStateFillColor;
                style[mxConstants.STYLE_OPACITY] = UiMockupShapeFactory.disableStateOpacity;
                style[mxConstants.STYLE_STROKECOLOR] = "transparent";
                this.createDisabledShapeStyle(shapeModel, style, shapeStyle);
                geometry = MxFactory.geometry(-1, -1, shapeModel.width + 2, shapeModel.height + 2);
                state = MxFactory.vertex(null, geometry, style.convertToString());
                mxShape.insert(state);
            }

            if (isNodeHighlighted === "true") {
                style = this.createHighlightedDisabledStyle(shapeModel);
                style[mxConstants.STYLE_STROKEWIDTH] = UiMockupShapeFactory.highlightStrokeWidth;
                style[mxConstants.STYLE_STROKECOLOR] = UiMockupShapeFactory.highlightStrokeColor;
                this.createHighlightedShapeStyle(shapeModel, style, shapeStyle);
                geometry = MxFactory.geometry(-1, -1, shapeModel.width + 2, shapeModel.height + 2);
                state = MxFactory.vertex(null, geometry, style.convertToString());
                mxShape.insert(state);
            }
        }

    }

    private createCheckboxShape(shape: IShape, checkBoxSize: number, labelOffset: number): MxCell {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_SPACING_LEFT] = checkBoxSize + labelOffset;
        if (shape.fill == null) {
            style[mxConstants.STYLE_FILLCOLOR] = "transparent";
        }
        this.applyDisabledStateForText(shape, style);

        const checkbox = this.createDefaultVertex(shape, style);
        return checkbox;
    }

    private createCheckboxInnerRectangle(shape: IShape,
                                         rectangleStyle: Style,
                                         brightGradientColor: string,
                                         darkGradientColor: string,
                                         innerRectOffset: number,
                                         innerRectSize: number): MxCell {
        rectangleStyle[mxConstants.STYLE_FILLCOLOR] = brightGradientColor;
        rectangleStyle[mxConstants.STYLE_GRADIENTCOLOR] = darkGradientColor;
        rectangleStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_NORTH;
        const innerRectGeometry = MxFactory.geometry(innerRectOffset, innerRectOffset, innerRectSize, innerRectSize);
        const innerRect = MxFactory.vertex(null, innerRectGeometry, rectangleStyle.convertToString());
        return innerRect;
    }

    private createCheckboxRectangle(shape: IShape, rectangleStyle: Style, checkBoxSize: number, borderColor: string): MxCell {
        rectangleStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        rectangleStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        rectangleStyle[mxConstants.STYLE_FOLDABLE] = 0;
        rectangleStyle[mxConstants.STYLE_STROKECOLOR] = borderColor;
        rectangleStyle[Styles.STYLE_SELECTABLE] = 0;
        const checkBoxY = shape.height / 2 - checkBoxSize / 2;
        const rectangleGeometry = MxFactory.geometry(0, checkBoxY, checkBoxSize, checkBoxSize);
        const rectangle = MxFactory.vertex(null, rectangleGeometry, rectangleStyle.convertToString());
        return rectangle;
    }

    private createCheckboxMark(shape: IShape, checkMarkColor: string, innerRectSize: number): MxCell {
        const checkMarkStyle = new Style();
        checkMarkStyle[mxConstants.STYLE_STROKEWIDTH] = 2;
        checkMarkStyle[mxConstants.STYLE_FOLDABLE] = 0;
        checkMarkStyle[mxConstants.STYLE_STROKECOLOR] = checkMarkColor;
        checkMarkStyle[mxConstants.STYLE_SHAPE] = CheckboxShape.getName;
        checkMarkStyle[Styles.STYLE_SELECTABLE] = 0;
        const checkMarkGeometry = MxFactory.geometry(0, 0, innerRectSize, innerRectSize);
        const checkMarkRect = MxFactory.vertex(null, checkMarkGeometry, checkMarkStyle.convertToString());
        return checkMarkRect;
    }


    private radioButton = (shape: IShape): MxCell => {
        const circleBoxSize = 12;
        const innerCircleSize = 8;
        const dotSize = 4;
        const innerCircleOffset = 2;
        const labelOffset = 2;
        const borderColor = "#A0AFC3";

        const brightGradientColor = "#F9FAFC";
        const darkGradientColor = "#C7D5E9";

        const dotBrightMarkColor = "#98A8BE";
        const dotDarkMarkColor = "#2C4667";

        const radioButton = this.createRadioButtonShape(shape, circleBoxSize, labelOffset);

        const circleStyle = new Style();
        const rectangle = this.createRadioButtonCircle(shape, circleStyle, circleBoxSize, borderColor);
        radioButton.insert(rectangle);

        const innerRect = this.createRadioButtonInnerCircle(shape, circleStyle, brightGradientColor, darkGradientColor, innerCircleOffset, innerCircleSize);
        rectangle.insert(innerRect);

        const isChecked = ShapeExtensions.getPropertyByName(shape, "Checked");
        if (isChecked === "true") {
            innerRect.insert(this.createRadioButtonMark(shape, dotBrightMarkColor, dotDarkMarkColor, borderColor, dotSize, innerCircleOffset));
        }

        this.applyHighlightedDisabledStates(shape, radioButton);
        return radioButton;
    };

    private createRadioButtonShape(shape: IShape, circleBoxSize: number, labelOffset: number): MxCell {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_SPACING_LEFT] = circleBoxSize + labelOffset;
        if (shape.fill == null) {
            style[mxConstants.STYLE_FILLCOLOR] = "transparent";
        }
        this.applyDisabledStateForText(shape, style);
        const checkbox = this.createDefaultVertex(shape, style);
        return checkbox;
    }

    private createRadioButtonInnerCircle(shape: IShape,
                                         circleStyle: Style,
                                         brightGradientColor: string,
                                         darkGradientColor: string,
                                         innerRectOffset: number,
                                         innerRectSize: number): MxCell {
        circleStyle[mxConstants.STYLE_FILLCOLOR] = brightGradientColor;
        circleStyle[mxConstants.STYLE_GRADIENTCOLOR] = darkGradientColor;
        circleStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_ELLIPSE;
        circleStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_NORTH;
        const innerRectGeometry = MxFactory.geometry(innerRectOffset, innerRectOffset, innerRectSize, innerRectSize);
        const innerCircle = MxFactory.vertex(null, innerRectGeometry, circleStyle.convertToString());
        return innerCircle;
    }

    private createRadioButtonCircle(shape: IShape, circleStyle: Style, checkBoxSize: number, borderColor: string): MxCell {
        circleStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        circleStyle[mxConstants.STYLE_FOLDABLE] = 0;
        circleStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_ELLIPSE;
        circleStyle[mxConstants.STYLE_STROKECOLOR] = borderColor;
        circleStyle[Styles.STYLE_SELECTABLE] = 0;
        const checkBoxY = shape.height / 2 - checkBoxSize / 2;
        const rectangleGeometry = MxFactory.geometry(0, checkBoxY, checkBoxSize, checkBoxSize);
        const circle = MxFactory.vertex(null, rectangleGeometry, circleStyle.convertToString());
        return circle;
    }


    private createRadioButtonMark(shape: IShape,
                                  dotBrightMarkColor: string,
                                  dotDarkMarkColor: string,
                                  borderColor: string,
                                  dotSize: number,
                                  innerCircleOffset: number): MxCell {
        const checkMarkStyle = new Style();
        checkMarkStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        checkMarkStyle[mxConstants.STYLE_FOLDABLE] = 0;
        checkMarkStyle[mxConstants.STYLE_FILLCOLOR] = dotBrightMarkColor;
        checkMarkStyle[mxConstants.STYLE_GRADIENTCOLOR] = dotDarkMarkColor;
        checkMarkStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_NORTH;
        checkMarkStyle[mxConstants.STYLE_STROKECOLOR] = borderColor;
        checkMarkStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_ELLIPSE;
        checkMarkStyle[Styles.STYLE_SELECTABLE] = 0;
        const checkMarkGeometry = MxFactory.geometry(innerCircleOffset, innerCircleOffset, dotSize, dotSize);
        const checkMarkRect = MxFactory.vertex(null, checkMarkGeometry, checkMarkStyle.convertToString());
        return checkMarkRect;
    }

    private numericSpinner = (shape: IShape): MxCell => {

        const borderColor = "#A8BED5";
        const buttonFillColor = "#DBE6F4";
        const buttonGradientColor = "#FBFDFE";
        const spinnerButtonWidth = 14;
        const markColor = "Black";
        const markSize = {height: 2, width: 4};
        const spinnerInnerButtonHeight = shape.height / 2;

        const style = new Style();
        style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        style[mxConstants.STYLE_STROKEWIDTH] = 1;
        style[mxConstants.STYLE_STROKECOLOR] = borderColor;
        style[mxConstants.STYLE_FOLDABLE] = 0;
        const numericSpinner = this.createDefaultVertex(shape, style, true);

        // buttons
        const buttonStyle = new Style();
        buttonStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        buttonStyle[mxConstants.STYLE_STROKEWIDTH] = 0.5;
        buttonStyle[mxConstants.STYLE_STROKECOLOR] = borderColor;
        buttonStyle[mxConstants.STYLE_FOLDABLE] = 0;
        buttonStyle[mxConstants.STYLE_FILLCOLOR] = buttonFillColor;
        buttonStyle[mxConstants.STYLE_GRADIENTCOLOR] = buttonGradientColor;
        buttonStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_NORTH;
        buttonStyle[Styles.STYLE_SELECTABLE] = 0;

        const topButtonGeometry = MxFactory.geometry(shape.width - spinnerButtonWidth, 0, spinnerButtonWidth, spinnerInnerButtonHeight);
        const topButton = MxFactory.vertex(null, topButtonGeometry, buttonStyle.convertToString());

        const bottomButtonGeometry = MxFactory.geometry(shape.width - spinnerButtonWidth, shape.height / 2, spinnerButtonWidth, spinnerInnerButtonHeight);
        const bottomButton = MxFactory.vertex(null, bottomButtonGeometry, buttonStyle.convertToString());


        //mark
        const markStyle = new Style();
        markStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
        markStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        markStyle[mxConstants.STYLE_STROKECOLOR] = markColor;
        markStyle[mxConstants.STYLE_FILLCOLOR] = markColor;
        markStyle[Styles.STYLE_SELECTABLE] = 0;
        markStyle[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_NORTH;
        const markGeometry = MxFactory.geometry(
            spinnerButtonWidth / 2 - markSize.width / 2, spinnerInnerButtonHeight / 2 - markSize.height / 2,
            markSize.width,
            markSize.height);
        const topMark = MxFactory.vertex(null, markGeometry, markStyle.convertToString());
        topButton.insert(topMark);
        numericSpinner.insert(topButton);

        markStyle[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_SOUTH;
        const bottomMark = MxFactory.vertex(null, markGeometry, markStyle.convertToString());
        bottomButton.insert(bottomMark);
        numericSpinner.insert(bottomButton);


        //label
        const spinnerValue = ShapeExtensions.getPropertyByName(shape, "Defaultnumber");
        const spinnerIntValue = parseInt(spinnerValue, 10);

        const labelStyle = new Style();
        this.styleBuilder.applyLabelStyle(labelStyle, shape.labelStyle);
        labelStyle[mxConstants.STYLE_STROKECOLOR] = mxConstants.NONE;
        labelStyle[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_RIGHT;
        labelStyle[Styles.STYLE_SELECTABLE] = 0;
        this.applyDisabledStateForText(shape, labelStyle);
        const labelGeometry = MxFactory.geometry(0, 0, shape.width - spinnerButtonWidth, shape.height);
        const label = MxFactory.vertex(spinnerIntValue.toLocaleString(), labelGeometry, labelStyle.convertToString());
        numericSpinner.insert(label);
        this.applyHighlightedDisabledStates(shape, numericSpinner);
        return numericSpinner;
    };

    private browser = (shape: IShape): MxCell => {
        const browserStyle = new Style();
        browserStyle[mxConstants.STYLE_STROKEWIDTH] = 2;
        browserStyle[mxConstants.STYLE_STROKECOLOR] = "#A9BFD6";
        browserStyle[mxConstants.STYLE_FILLCOLOR] = "none";
        browserStyle[mxConstants.STYLE_FOLDABLE] = 0;
        const browser = this.createDefaultVertex(shape, browserStyle, true);
        //top bar
        const topBarStyle = new Style();
        topBarStyle[mxConstants.STYLE_STROKECOLOR] = "#A9BFD6";
        topBarStyle[mxConstants.STYLE_STROKEWIDTH] = 2;
        topBarStyle[mxConstants.STYLE_FILLCOLOR] = "#DBE5F3";
        topBarStyle[Styles.STYLE_SELECTABLE] = 0;
        topBarStyle[mxConstants.STYLE_FOLDABLE] = 0;
        const topBarGeometry = MxFactory.geometry(0, 0, shape.width, 70);
        topBarGeometry.relative = false;
        const topBar = MxFactory.vertex(null, topBarGeometry, topBarStyle.convertToString());

        //icons
        const backButtonStyle = new Style();
        backButtonStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_IMAGE;
        backButtonStyle[mxConstants.STYLE_PERIMETER] = mxPerimeter.RectanglePerimeter;
        backButtonStyle[mxConstants.STYLE_IMAGE] = "/Scripts/mxClient/images/Back32.png";
        backButtonStyle[Styles.STYLE_SELECTABLE] = 0;
        const backButtonGeometry = MxFactory.geometry(0, 42.5, 25, 25);
        const backButton = MxFactory.vertex(null, backButtonGeometry, backButtonStyle.convertToString());

        const forwardButtonStyle = new Style();
        forwardButtonStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_IMAGE;
        forwardButtonStyle[mxConstants.STYLE_PERIMETER] = mxPerimeter.RectanglePerimeter;
        forwardButtonStyle[mxConstants.STYLE_IMAGE] = "/Scripts/mxClient/images/Forward32.png";
        forwardButtonStyle[Styles.STYLE_SELECTABLE] = 0;
        const forwardButtonGeometry = MxFactory.geometry(25, 42.5, 25, 25);
        const forwardButton = MxFactory.vertex(null, forwardButtonGeometry, forwardButtonStyle.convertToString());

        topBar.insert(backButton);
        topBar.insert(forwardButton);

        //navigation label
        const urlText: string = ShapeExtensions.getPropertyByName(shape, "URL");
        const navigationLabelStyle = new Style();
        navigationLabelStyle[Styles.STYLE_SELECTABLE] = 0;
        navigationLabelStyle[mxConstants.STYLE_FOLDABLE] = 0;
        navigationLabelStyle[mxConstants.STYLE_FILLCOLOR] = "white";
        navigationLabelStyle[mxConstants.STYLE_STROKECOLOR] = mxConstants.NONE;
        navigationLabelStyle[mxConstants.STYLE_OVERFLOW] = "hidden";
        navigationLabelStyle[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_LEFT;
        navigationLabelStyle[Styles.STYLE_SELECTABLE] = 0;
        this.applyDisabledStateForText(shape, navigationLabelStyle);
        const navigationLabelGeometry = MxFactory.geometry(51, 42.5, (shape.width - 125) * 14 / 19, 22.5);
        navigationLabelGeometry.relative = false;
        const navigationLabel = MxFactory.vertex(urlText, navigationLabelGeometry, navigationLabelStyle.convertToString());
        topBar.insert(navigationLabel);

        const refreshButtonStyle = new Style();
        refreshButtonStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_IMAGE;
        refreshButtonStyle[mxConstants.STYLE_PERIMETER] = mxPerimeter.RectanglePerimeter;
        refreshButtonStyle[mxConstants.STYLE_IMAGE] = "/Scripts/mxClient/images/Refresh32.png";
        refreshButtonStyle[Styles.STYLE_SELECTABLE] = 0;
        const refreshButtonGeometry = MxFactory.geometry(navigationLabel.geometry.width + 51, 42.5, 25, 25);
        const refreshButton = MxFactory.vertex(null, refreshButtonGeometry, refreshButtonStyle.convertToString());

        const stopButtonStyle = new Style();
        stopButtonStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_IMAGE;
        stopButtonStyle[mxConstants.STYLE_PERIMETER] = mxPerimeter.RectanglePerimeter;
        stopButtonStyle[mxConstants.STYLE_IMAGE] = "/Scripts/mxClient/images/Stop32.png";
        stopButtonStyle[Styles.STYLE_SELECTABLE] = 0;
        const stopButtonGeometry = MxFactory.geometry(navigationLabel.geometry.width + 76, 42.5, 25, 25);
        const stopButton = MxFactory.vertex(null, stopButtonGeometry, stopButtonStyle.convertToString());

        topBar.insert(refreshButton);
        topBar.insert(stopButton);

        //search label
        const searchText: string = ShapeExtensions.getPropertyByName(shape, "SearchText");
        const searchLabelStyle = new Style();
        searchLabelStyle[Styles.STYLE_SELECTABLE] = 0;
        searchLabelStyle[mxConstants.STYLE_FOLDABLE] = 0;
        searchLabelStyle[mxConstants.STYLE_FILLCOLOR] = "white";
        searchLabelStyle[mxConstants.STYLE_STROKECOLOR] = mxConstants.NONE;
        searchLabelStyle[mxConstants.STYLE_OVERFLOW] = "hidden";
        searchLabelStyle[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_LEFT;
        this.applyDisabledStateForText(shape, searchLabelStyle);
        const searchLabelGeometry = MxFactory.geometry((shape.width - 125) * 14 / 19 + 100, 42.5, (shape.width - 125) * 5 / 19, 22.5);
        searchLabelGeometry.relative = false;
        const searchLabel = MxFactory.vertex(searchText, searchLabelGeometry, searchLabelStyle.convertToString());
        topBar.insert(searchLabel);

        const searchButtonStyle = new Style();
        searchButtonStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_IMAGE;
        searchButtonStyle[mxConstants.STYLE_PERIMETER] = mxPerimeter.RectanglePerimeter;
        searchButtonStyle[mxConstants.STYLE_IMAGE] = "/Scripts/mxClient/images/Search32.png";
        searchButtonStyle[Styles.STYLE_SELECTABLE] = 0;
        const searchButtonGeometry = MxFactory.geometry(navigationLabel.geometry.width + searchLabel.geometry.width + 100, 42.5, 25, 25);
        const searchButton = MxFactory.vertex(null, searchButtonGeometry, searchButtonStyle.convertToString());
        topBar.insert(searchButton);

        //label
        const browserLabelStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        browserLabelStyle[Styles.STYLE_SELECTABLE] = 0;
        browserLabelStyle[mxConstants.STYLE_FOLDABLE] = 0;
        browserLabelStyle[mxConstants.STYLE_FILLCOLOR] = "white";
        browserLabelStyle[mxConstants.STYLE_GRADIENTCOLOR] = "#E8F1FA";
        browserLabelStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_SOUTH;
        browserLabelStyle[mxConstants.STYLE_STROKECOLOR] = mxConstants.NONE;
        browserLabelStyle[mxConstants.STYLE_OVERFLOW] = "hidden";
        browserLabelStyle[Styles.STYLE_SELECTABLE] = 0;
        this.applyDisabledStateForText(shape, browserLabelStyle);
        const browserLabelGeometry = MxFactory.geometry(1, 1, shape.width - 3, 35);
        browserLabelGeometry.relative = false;
        const browserLabel = MxFactory.vertex(shape.label, browserLabelGeometry, browserLabelStyle.convertToString());
        topBar.insert(browserLabel);
        browser.insert(topBar);

        //content box
        const contentBoxStyle = new Style();
        contentBoxStyle[mxConstants.STYLE_STROKECOLOR] = "#A9BFD6";
        contentBoxStyle[mxConstants.STYLE_FOLDABLE] = 0;
        contentBoxStyle[Styles.STYLE_SELECTABLE] = 0;
        const contentBoxGeometry = MxFactory.geometry(4, 74, shape.width - 9, shape.height - 79);
        const contentBox = MxFactory.vertex(null, contentBoxGeometry, contentBoxStyle.convertToString());
        const scrollBar = ShapeExtensions.getPropertyByName(shape, "ScrollBar") === "true";
        if (scrollBar) {
            //scroll bar
            if (scrollBar) {
                const scrollBarStyle = new Style();
                scrollBarStyle[mxConstants.STYLE_STROKECOLOR] = "#A9BFD6";
                scrollBarStyle[mxConstants.STYLE_FILLCOLOR] = "#F6F9FE";
                scrollBarStyle[Styles.STYLE_SELECTABLE] = 0;
                scrollBarStyle[mxConstants.STYLE_FOLDABLE] = 0;
                const scrollBarGeometry = MxFactory.geometry(contentBox.geometry.width - 18, 0, 18, contentBox.geometry.height);
                scrollBarGeometry.relative = false;
                const scrollBarBox = MxFactory.vertex(null, scrollBarGeometry, scrollBarStyle.convertToString());
                const scrollbarMarkSize = {height: 3, width: 6};

                //mark
                const mark1Style = new Style();
                mark1Style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
                mark1Style[mxConstants.STYLE_STROKEWIDTH] = 1;
                mark1Style[mxConstants.STYLE_STROKECOLOR] = "black";
                mark1Style[mxConstants.STYLE_FILLCOLOR] = "black";
                mark1Style[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_SOUTH;
                mark1Style[Styles.STYLE_SELECTABLE] = 0;
                const mark1Geometry = MxFactory.geometry(6, contentBox.geometry.height - 11, scrollbarMarkSize.width, scrollbarMarkSize.height);
                mark1Geometry.relative = false;
                const mark1 = MxFactory.vertex(null, mark1Geometry, mark1Style.convertToString());
                scrollBarBox.insert(mark1);

                //mark2
                const mark2Style = new Style();
                mark2Style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
                mark2Style[mxConstants.STYLE_STROKEWIDTH] = 1;
                mark2Style[mxConstants.STYLE_STROKECOLOR] = "black";
                mark2Style[mxConstants.STYLE_FILLCOLOR] = "black";
                mark2Style[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_NORTH;
                mark2Style[Styles.STYLE_SELECTABLE] = 0;
                const mark2Geometry = MxFactory.geometry(6, 10, scrollbarMarkSize.width, scrollbarMarkSize.height);
                mark2Geometry.relative = false;
                const mark2 = MxFactory.vertex(null, mark2Geometry, mark2Style.convertToString());
                scrollBarBox.insert(mark2);

                contentBox.insert(scrollBarBox);
            }
        }
        browser.insert(contentBox);
        this.applyHighlightedDisabledStates(shape, browser);
        return browser;
    };

    private uIMockupmenu = (shape: IShape): MxCell => {
        shape.gradientFill = null;
        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        const isVertical = (ShapeExtensions.getPropertyByName(shape, "Orientation") === "Vertical");
        const containerMargin = 5;
        style[mxConstants.STYLE_STROKEWIDTH] = 1;
        style[mxConstants.STYLE_STROKECOLOR] = "#A8BED5";
        style[mxConstants.STYLE_FILLCOLOR] = "#F1F5FB";
        style[mxConstants.STYLE_WHITE_SPACE] = "nowrap";
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;
        style[mxConstants.STYLE_FOLDABLE] = 0;

        let labelText = "";
        const menuStyle = new MenuStyleObject();
        this.setMenuFontStyle(shape, menuStyle, isVertical);
        for (let i = 0; i < shape.props.length; i++) {
            if (shape.props[i].name === "TreeItem") {
                if (shape.props[i].value.isSeparator) {
                    labelText = this.menuAddSeparatorSpace(isVertical, menuStyle, labelText);
                    continue;
                }
                labelText = this.menuAddVhSpace(isVertical, menuStyle, labelText);
                labelText = this.menuAddCheckMark(shape.props[i], menuStyle, labelText);
                labelText = this.menuAddMenuText(shape.props[i], menuStyle, labelText);
            }
        }

        const labelcontainer = this.createMenuLabelContainer(shape, containerMargin, labelText, menuStyle);
        const menu = this.createDefaultVertex(shape, style, true);
        menu.insert(labelcontainer);
        this.applyHighlightedDisabledStates(shape, menu);
        return menu;
    };

    private setMenuFontStyle(shape: IShape, menuStyle: MenuStyleObject, isVertical: boolean) {
        const rectangleStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        menuStyle.styleFontStyle = rectangleStyle[mxConstants.STYLE_FONTSTYLE];
        let isUnderline = false;
        if (shape.labelStyle) {
            isUnderline = shape.labelStyle.isUnderline;
            menuStyle.rgbaColor = this.convertAlphaHexToRgb(0.6, shape.labelStyle.foreground);
        }
        if (isUnderline) {
            menuStyle.textStyle = "text-decoration: underline;";
            if (shape.labelStyle.isBold && shape.labelStyle.isItalic) { //BIU
                menuStyle.styleFontStyle = "3"; //BI
            } else if (shape.labelStyle.isBold) { //BU
                menuStyle.styleFontStyle = "1"; //B
            } else if (shape.labelStyle.isItalic) { //IU
                menuStyle.styleFontStyle = "2"; //I
            } else {
                menuStyle.styleFontStyle = ""; //Null
            }
        } else {
            menuStyle.textStyle = "text-decoration: none;";
        }
        menuStyle.blankAreaStyle = "font-weight: normal; font-size: 16px; line-height: 2px;";
        menuStyle.checkMarkStyle = "font-weight: normal; font-size: 14px;";
        if (isVertical === false) {
            menuStyle.textStyle += " display: inline-block; margin-bottom: 10px;";
            menuStyle.blankAreaStyle += " display: inline-block; margin-bottom: 10px; min-width: 15px;";
            menuStyle.checkMarkStyle += " display: inline-block; margin-bottom: 10px;";
        }
    }

    private menuAddSeparatorSpace(isVertical: boolean, menuStyle: MenuStyleObject, labelText: string): string {
        if (isVertical === false) {
            labelText += "<span style='" + menuStyle.blankAreaStyle + "'>&nbsp;&nbsp;</span>";
        } else {
            labelText += "<p style='" + menuStyle.blankAreaStyle + "'>\n</p>";
        }
        return labelText;
    }

    private menuAddVhSpace(isVertical: boolean, menuStyle: MenuStyleObject, labelText: string): string {
        if (labelText.length > 0) {
            if (isVertical === false) {
                labelText += "<span style='" + menuStyle.blankAreaStyle + "'>&nbsp;&nbsp;</span>";
            } else {
                labelText += "<p style='" + menuStyle.blankAreaStyle + "'></p>";
            }
        }
        return labelText;
    }

    private menuAddCheckMark(prop: IProp, menuStyle: MenuStyleObject, labelText: string): string {
        if (prop.value.isSelected) {
            //✔
            labelText += "<span style='" + menuStyle.checkMarkStyle + "'>&#10004;&nbsp;</span>";
        }
        return labelText;
    }

    private menuAddMenuText(prop: IProp, menuStyle: MenuStyleObject, labelText: string): string {
        const isEnabled = prop.value.isEnabled;
        const formattedText = Helper.escapeHTMLText(prop.value.text).replace(/\s/g, "&nbsp;");
        const opacityStyle = (menuStyle.rgbaColor.length > 0) ? "color: " + menuStyle.rgbaColor + ";" : "opacity: 0.6;";
        const textStyle = isEnabled ? menuStyle.textStyle : menuStyle.textStyle + opacityStyle;
        //mxUtils.getSizeForString
        labelText += (menuStyle.textStyle === "") ? formattedText : "<span style='" + textStyle + "'>" + formattedText + "</span>";
        return labelText;
    }

    public convertAlphaHexToRgb(alpha: number, hexColor: string): string {
        if (hexColor.indexOf("#") !== -1 && hexColor.length >= 6) {
            hexColor = hexColor.replace(new RegExp("[^0-9A-F]", "gi"), "");

            let r = parseInt(hexColor.substring(0, 2), 16);
            r = Math.round(((alpha * (r / 255)) + 1 - alpha) * 255);
            let g = parseInt(hexColor.substring(2, 4), 16);
            g = Math.round(((alpha * (g / 255)) + 1 - alpha) * 255);
            let b = parseInt(hexColor.substring(4, 6), 16);
            b = Math.round(((alpha * (b / 255)) + 1 - alpha) * 255);

            return "rgb(" + r + "," + g + "," + b + ")";
        }

        return hexColor;
    }

    private createMenuLabelContainer(shape: IShape, containerMargin: number, labelText: string, menuStyle: MenuStyleObject): MxCell {
        const rectangleStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        rectangleStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        rectangleStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        rectangleStyle[mxConstants.STYLE_FOLDABLE] = 0;
        rectangleStyle[mxConstants.STYLE_OVERFLOW] = "hidden";
        rectangleStyle[mxConstants.STYLE_WHITE_SPACE] = "nowrap";
        rectangleStyle[mxConstants.STYLE_FONTSTYLE] = menuStyle.styleFontStyle;
        rectangleStyle[mxConstants.STYLE_FILLCOLOR] = "#F1F5FB";
        rectangleStyle[Styles.STYLE_SELECTABLE] = 0;
        rectangleStyle[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;
        this.applyDisabledStateForText(shape, rectangleStyle);
        const geometry = MxFactory.geometry(containerMargin, containerMargin, shape.width - (containerMargin * 2), shape.height - (containerMargin * 2));
        const labelContainer = MxFactory.vertex(labelText, geometry, rectangleStyle.convertToString());
        return labelContainer;
    }

    private window = (shape: IShape): MxCell => {

        const windowStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        windowStyle[mxConstants.STYLE_STROKEWIDTH] = 2;
        windowStyle[mxConstants.STYLE_STROKECOLOR] = "#A8BED5";
        windowStyle[mxConstants.STYLE_FOLDABLE] = 0;

        const windowShape = this.createDefaultVertex(shape, windowStyle, true);

        const headerStyle = new Style();
        headerStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        headerStyle[mxConstants.STYLE_STROKEWIDTH] = 2;
        headerStyle[mxConstants.STYLE_STROKECOLOR] = "#A8BED5";
        headerStyle[mxConstants.STYLE_FILLCOLOR] = "#DBE6F4";
        headerStyle[mxConstants.STYLE_GRADIENTCOLOR] = "#FBFDFE";
        headerStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_NORTH;
        headerStyle[mxConstants.STYLE_FOLDABLE] = 0;
        headerStyle[Styles.STYLE_SELECTABLE] = 0;
        //
        const headerShape = MxFactory.vertex(null,
            MxFactory.geometry(0, //X
                0, //Y
                shape.width, //W
                36),
            headerStyle.convertToString());


        windowShape.insert(headerShape);
        const showButtons = DiagramHelper.findValueByName(shape.props, "ShowButtons");
        let titleWidth = shape.width;
        if (showButtons === "All" || showButtons === "CloseOnly") {
            const closeButtonStyle = new Style();
            closeButtonStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_IMAGE;
            closeButtonStyle[mxConstants.STYLE_PERIMETER] = mxPerimeter.RectanglePerimeter;
            closeButtonStyle[mxConstants.STYLE_IMAGE] = "/Scripts/mxClient/images/window-close.png";
            closeButtonStyle[mxConstants.STYLE_FONTCOLOR] = "#FFFFFF";
            closeButtonStyle[Styles.STYLE_SELECTABLE] = 0;


            headerShape.insert(MxFactory.vertex(null,
                MxFactory.geometry(shape.width - 41, //X
                    0, //Y
                    39, //W
                    16), //H
                closeButtonStyle.convertToString()));
            //

            titleWidth -= 40;
        }

        if (showButtons === "All") {

            const resizeButtonStyle = new Style();
            resizeButtonStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_IMAGE;
            resizeButtonStyle[mxConstants.STYLE_PERIMETER] = mxPerimeter.RectanglePerimeter;
            resizeButtonStyle[mxConstants.STYLE_IMAGE] = "/Scripts/mxClient/images/window-resize.png";
            resizeButtonStyle[mxConstants.STYLE_FONTCOLOR] = "#FFFFFF";
            resizeButtonStyle[Styles.STYLE_SELECTABLE] = 0;

            headerShape.insert(MxFactory.vertex(null,
                MxFactory.geometry(shape.width - 91, //X
                    0, //Y
                    49, //W
                    16), //H
                resizeButtonStyle.convertToString()));
            //

            titleWidth -= 49;


        }

        const scrollBar = DiagramHelper.findValueByName(shape.props, "ScrollBar");
        if (scrollBar === "true") {
            const scrollBarStyle = new Style();
            scrollBarStyle[mxConstants.STYLE_STROKECOLOR] = "#7F98A9";
            scrollBarStyle[mxConstants.STYLE_FILLCOLOR] = "white";
            scrollBarStyle[mxConstants.STYLE_GRADIENTCOLOR] = "#F2F4F6";
            scrollBarStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_WEST;
            scrollBarStyle[Styles.STYLE_SELECTABLE] = 0;
            scrollBarStyle[mxConstants.STYLE_FOLDABLE] = 0;

            const scrollBarGeometry = MxFactory.geometry(shape.width - 21, 36, 20, shape.height - 37);

            const scrollBarShape = MxFactory.vertex(null, scrollBarGeometry, scrollBarStyle.convertToString());

            windowShape.insert(scrollBarShape);

            const markSize = {height: 3, width: 6};

            //upTriangle
            const upTriangleStyle = new Style();
            upTriangleStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
            upTriangleStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
            upTriangleStyle[mxConstants.STYLE_STROKECOLOR] = "black";
            upTriangleStyle[mxConstants.STYLE_FILLCOLOR] = "black";
            upTriangleStyle[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_NORTH;
            upTriangleStyle[Styles.STYLE_SELECTABLE] = 0;

            const upTriangleGeometry = MxFactory.geometry(7, 11, markSize.width, markSize.height);
            upTriangleGeometry.relative = false;
            //
            scrollBarShape.insert(MxFactory.vertex(null, upTriangleGeometry, upTriangleStyle.convertToString()));


            const downTriangleStyle = new Style();
            downTriangleStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
            downTriangleStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
            downTriangleStyle[mxConstants.STYLE_STROKECOLOR] = "black";
            downTriangleStyle[mxConstants.STYLE_FILLCOLOR] = "black";
            downTriangleStyle[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_SOUTH;
            downTriangleStyle[Styles.STYLE_SELECTABLE] = 0;

            const downTriangleGeometry = MxFactory.geometry(7, shape.height - 36 - 11, markSize.width, markSize.height);
            downTriangleGeometry.relative = false;
            //
            scrollBarShape.insert(MxFactory.vertex(null, downTriangleGeometry, downTriangleStyle.convertToString()));
        }

        if (shape.label) {
            const titleStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
            titleStyle[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_LEFT;
            titleStyle[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;

            titleStyle[Styles.STYLE_SELECTABLE] = 0;

            titleStyle[mxConstants.STYLE_STROKECOLOR] = mxConstants.NONE;

            titleStyle[mxConstants.STYLE_FILLCOLOR] = "transparent";
            titleStyle[mxConstants.STYLE_OVERFLOW] = "hidden";
            const labelGeometry = MxFactory.geometry(1, 0, titleWidth - 2, 36 - 2); //offsets the container boarder width
            labelGeometry.relative = false;
            const label = MxFactory.vertex(shape.label, labelGeometry, titleStyle.convertToString());
            headerShape.insert(label);
        }
        this.applyHighlightedDisabledStates(shape, windowShape);
        return windowShape;

    };

    private slider = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        const isVertical = (ShapeExtensions.getPropertyByName(shape, "Orientation") === "Vertical");
        style[mxConstants.STYLE_FOLDABLE] = 0;
        const slider = this.createDefaultVertex(shape, style, true);
        const rectMargin = 4;
        const mainRect = this.createSliderMainRect(shape, isVertical, rectMargin);
        const valueRect = this.createSliderValueRect(shape, isVertical, rectMargin);
        slider.insert(mainRect);
        slider.insert(valueRect);
        this.applyHighlightedDisabledStates(shape, slider);
        return slider;
    };

    private createSliderMainRect(shape: IShape, isVertical: boolean, rectMargin: number): MxCell {
        const rectangleStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        rectangleStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        rectangleStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        rectangleStyle[mxConstants.STYLE_STROKECOLOR] = "#A3AEB9";
        rectangleStyle[mxConstants.STYLE_FOLDABLE] = 0;
        rectangleStyle[mxConstants.STYLE_FILLCOLOR] = "#EBEBEB";
        rectangleStyle[mxConstants.STYLE_ROUNDED] = 1;
        rectangleStyle[Styles.STYLE_SELECTABLE] = 0;
        this.applyDisabledStateForText(shape, rectangleStyle);
        const rectSize = 3;
        let rectHeight = rectSize;
        let rectWidth = shape.width - rectMargin * 2;
        let rectX = rectMargin;
        let rectY = (shape.height / 2) - (rectHeight / 2);
        if (isVertical) {
            rectHeight = shape.height - rectMargin * 2;
            rectWidth = rectSize;
            rectX = (shape.width / 2) - (rectWidth / 2);
            rectY = rectMargin;
        }
        const geometry = MxFactory.geometry(rectX, rectY, rectWidth, rectHeight);
        const mainRect = MxFactory.vertex(null, geometry, rectangleStyle.convertToString());
        return mainRect;
    }

    private createSliderValueRect(shape: IShape, isVertical: boolean, rectMargin: number): MxCell {
        const rectangleStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        rectangleStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        rectangleStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        rectangleStyle[mxConstants.STYLE_STROKECOLOR] = "#687B8B";
        rectangleStyle[mxConstants.STYLE_FOLDABLE] = 0;
        rectangleStyle[mxConstants.STYLE_FILLCOLOR] = "#FFFFFF";
        rectangleStyle[mxConstants.STYLE_ROUNDED] = 1;
        rectangleStyle[Styles.STYLE_SELECTABLE] = 0;
        const rectGreaterSize = 18;
        const rectSmallerSize = 10;
        let sliderValue = 0;
        for (let i = 0; i < shape.props.length; i++) {
            if (shape.props[i].name === "SliderValue") {
                sliderValue = shape.props[i].value;
            }
        }
        let rectHeight = rectGreaterSize;
        let rectWidth = rectSmallerSize;
        let rectX = (sliderValue / 100) * (shape.width - rectMargin);
        let rectY = (shape.height / 2) - (rectGreaterSize / 2);
        if (isVertical) {
            rectHeight = rectSmallerSize;
            rectWidth = rectGreaterSize;
            rectX = (shape.width / 2) - (rectGreaterSize / 2);
            rectY = ((100 - sliderValue) / 100) * (shape.height - rectMargin);
        }
        let geometry = MxFactory.geometry(rectX, rectY, rectWidth, rectHeight);
        const mainRect = MxFactory.vertex(null, geometry, rectangleStyle.convertToString());

        //Inside Rectangle
        rectangleStyle[mxConstants.STYLE_STROKEWIDTH] = 0;
        rectangleStyle[mxConstants.STYLE_STROKECOLOR] = "transparent";
        rectangleStyle[mxConstants.STYLE_FILLCOLOR] = "white";
        rectangleStyle[mxConstants.STYLE_GRADIENTCOLOR] = "#DBE6F4";
        //rectangleStyle[mxConstants.STYLE_ROUNDED] = 0;
        rectangleStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_NORTH;

        const insideRectDiffSize = 3;
        rectHeight -= insideRectDiffSize;
        rectWidth -= insideRectDiffSize;
        geometry = MxFactory.geometry(insideRectDiffSize / 2, insideRectDiffSize / 2, rectWidth, rectHeight);
        const insideRect = MxFactory.vertex(null, geometry, rectangleStyle.convertToString());
        mainRect.insert(insideRect);

        return mainRect;
    }

    private scrollbar = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        style[mxConstants.STYLE_STROKEWIDTH] = 1;
        style[mxConstants.STYLE_STROKECOLOR] = "#91C3FF";
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_FILLCOLOR] = "#F6F9FE";
        //style[Styles.STYLE_SELECTABLE] = 0;
        const isVertical = (ShapeExtensions.getPropertyByName(shape, "Orientation") === "Vertical");
        const scrollbar = this.createDefaultVertex(shape, style, true);
        const firstTriangle = this.createScrollTriangle(shape, isVertical, true);
        scrollbar.insert(firstTriangle);
        const secondTriangle = this.createScrollTriangle(shape, isVertical, false);
        scrollbar.insert(secondTriangle);
        const stud = this.createScrollStud(shape, isVertical);
        scrollbar.insert(stud);
        this.applyHighlightedDisabledStates(shape, scrollbar);
        return scrollbar;
    };

    private createScrollTriangle(shape: IShape, isVertical: boolean, firstOne: boolean): MxCell {
        const triangleStyle = new Style();
        triangleStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
        triangleStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        triangleStyle[mxConstants.STYLE_STROKECOLOR] = "black";
        triangleStyle[mxConstants.STYLE_FILLCOLOR] = "black";
        triangleStyle[mxConstants.STYLE_FOLDABLE] = 0;
        if (firstOne) {
            triangleStyle[mxConstants.STYLE_DIRECTION] = isVertical ? mxConstants.DIRECTION_NORTH : mxConstants.DIRECTION_WEST;
        } else {
            triangleStyle[mxConstants.STYLE_DIRECTION] = isVertical ? mxConstants.DIRECTION_SOUTH : mxConstants.DIRECTION_EAST;
        }
        triangleStyle[Styles.STYLE_SELECTABLE] = 0;
        const triangleMargin = 5;
        let triangleHeight = 6;
        let triangleWidth = 3;
        let triangleX = firstOne ? triangleMargin : shape.width - triangleWidth - triangleMargin;
        let triangleY = (shape.height / 2) - (triangleHeight / 2);
        if (isVertical) {
            triangleHeight = 3;
            triangleWidth = 6;
            triangleX = (shape.width / 2) - (triangleWidth / 2);
            triangleY = firstOne ? triangleMargin : shape.height - triangleHeight - triangleMargin;
        }
        const triangleGeometry = MxFactory.geometry(triangleX, triangleY, triangleWidth, triangleHeight);
        triangleGeometry.relative = false;
        const triangle = MxFactory.vertex(null, triangleGeometry, triangleStyle.convertToString());
        return triangle;
    }

    private createScrollStud(shape: IShape, isVertical: boolean): MxCell {
        const rectangleStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        rectangleStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        rectangleStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        rectangleStyle[mxConstants.STYLE_STROKECOLOR] = "#BBC6D5";
        rectangleStyle[mxConstants.STYLE_FOLDABLE] = 0;
        rectangleStyle[mxConstants.STYLE_FILLCOLOR] = "white";
        rectangleStyle[mxConstants.STYLE_GRADIENTCOLOR] = "#DBE6F4";
        rectangleStyle[mxConstants.STYLE_ROUNDED] = 1;
        rectangleStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_NORTH;
        rectangleStyle[Styles.STYLE_SELECTABLE] = 0;
        const rectSmallerSize = 18;
        let scrollValue = 0;
        for (let i = 0; i < shape.props.length; i++) {
            if (shape.props[i].name === "ScrollValue") {
                scrollValue = shape.props[i].value;
            }
        }
        let rectHeight = shape.height;
        let rectWidth = rectSmallerSize;
        let startMargin = 25;
        let rectX = startMargin + (scrollValue / 100) * (shape.width - startMargin * 2) - (rectWidth / 2);
        let rectY = 0;
        if (isVertical) {
            rectHeight = rectSmallerSize;
            rectWidth = shape.width;
            rectX = 0;
            rectY = startMargin + (scrollValue / 100) * (shape.height - startMargin * 2) - (rectHeight / 2);
        }
        let geometry = MxFactory.geometry(rectX, rectY, rectWidth, rectHeight);
        const stud = MxFactory.vertex(null, geometry, rectangleStyle.convertToString());

        //Inside Lines
        rectangleStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_LINE;
        rectangleStyle[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_EAST;
        rectangleStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        rectangleStyle[mxConstants.STYLE_STROKECOLOR] = "black";
        rectangleStyle[mxConstants.STYLE_FILLCOLOR] = "black";
        rectangleStyle[Styles.STYLE_SELECTABLE] = 0;
        rectangleStyle[mxConstants.STYLE_FOLDABLE] = 0;

        const eachLineSpace = 3;
        const lineWidth = 7;
        let startX = (rectWidth / 2) - (lineWidth / 2);
        let startY = (rectHeight / 2) - eachLineSpace;
        if (isVertical === false) {
            startY = (rectHeight / 2) - (lineWidth / 2);
            startX = (rectWidth / 2) - eachLineSpace;
            rectangleStyle[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_SOUTH;
        }
        geometry = isVertical ? MxFactory.geometry(startX, startY, lineWidth, 1) :
            MxFactory.geometry(startX, startY, 1, lineWidth);
        const line1 = MxFactory.vertex(null, geometry, rectangleStyle.convertToString());
        geometry = isVertical ? MxFactory.geometry(startX, startY + eachLineSpace, lineWidth, 1) :
            MxFactory.geometry(startX + eachLineSpace, startY, 1, lineWidth);
        const line2 = MxFactory.vertex(null, geometry, rectangleStyle.convertToString());
        geometry = isVertical ? MxFactory.geometry(startX, startY + (eachLineSpace * 2), lineWidth, 1) :
            MxFactory.geometry(startX + (eachLineSpace * 2), startY, 1, lineWidth);
        const line3 = MxFactory.vertex(null, geometry, rectangleStyle.convertToString());

        stud.insert(line1);
        stud.insert(line2);
        stud.insert(line3);

        return stud;
    }

    private progressbar = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_SHAPE] = HighlightEllipse.getName;
        style[mxConstants.STYLE_STROKEWIDTH] = 1;
        style[mxConstants.STYLE_STROKECOLOR] = "#657581";
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_OVERFLOW] = "hidden";
        let progressBarStyle: string;
        for (let i = 0; i < shape.props.length; i++) {
            if (shape.props[i].name === "ProgressBarStyle") {
                progressBarStyle = shape.props[i].value;
            }
        }

        if (progressBarStyle === "Standard") {
            style[mxConstants.STYLE_FILLCOLOR] = "#FFFFFF";
        } else {
            style[mxConstants.STYLE_FILLCOLOR] = "#000000";
        }
        style[mxConstants.STYLE_GRADIENTCOLOR] = "#E4E6E7";
        style[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_SOUTH;
        //style[mxConstants.STYLE_ROUNDED] = 1;
        const progressbar = this.createDefaultVertex(shape, style, true);
        if (progressBarStyle === "Standard") {
            const valueRect = this.createProgressValueRect(shape);
            progressbar.insert(valueRect);
        }
        this.applyHighlightedDisabledStates(shape, progressbar, style[mxConstants.STYLE_SHAPE]);
        return progressbar;
    };

    private tab = (shape: IShape): MxCell => {
        let innerBoxGeometry, initialHeight = 0, height, length, itemsHeight;

        const style = this.styleBuilder.createDefaultShapeStyle.call(shape, shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_STROKEWIDTH] = 2;
        style[mxConstants.STYLE_STROKECOLOR] = mxConstants.NONE;
        style[mxConstants.STYLE_FILLCOLOR] = "#FFFFFF";
        style[mxConstants.STYLE_FOLDABLE] = 0;
        this.applyDisabledStateForText(shape, style);
        const tab = this.createDefaultVertex(shape, style, true);

        const geometry = MxFactory.geometry(0, 0, shape.width, shape.height);
        style[mxConstants.STYLE_FILLCOLOR] = mxConstants.NONE;
        style[mxConstants.STYLE_STROKEWIDTH] = 2;
        style[mxConstants.STYLE_STROKECOLOR] = "#A9BFD6";
        const border = MxFactory.vertex(null, geometry, style.convertToString());

        let orientation;
        const listItems = [];
        for (let i = 0; i < shape.props.length; i++) {
            if (shape.props[i].name === "ListItem") {
                listItems.push(shape.props[i]);
            }
            if (shape.props[i].name === "Orientation") {
                orientation = shape.props[i].value;
            }
        }

        const innerBoxStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        innerBoxStyle[Styles.STYLE_SELECTABLE] = 0;
        innerBoxStyle[mxConstants.STYLE_FOLDABLE] = 0;

        innerBoxGeometry = MxFactory.geometry(1, 1, shape.width, 50);

        const innerBox = MxFactory.vertex(null, innerBoxGeometry, innerBoxStyle.convertToString());

        const o = this.getTabRows(listItems, shape);
        length = o.listItemRows.length;
        height = o.height;
        itemsHeight = height * (length + 1);

        for (let j = length - 1; j > -1; j--) {
            if (orientation === "Bottom" || orientation === "Left") {
                initialHeight = (length - j) * height;
            } else if (orientation === "Top" || orientation === "Right") {
                initialHeight = (length - j - 1) * height;
            }
            this.insertTabRow(o.listItemRows[j], orientation, shape, initialHeight, height, innerBox);
        }
        if (orientation === "Bottom" || orientation === "Left") {
            initialHeight = 0;
        } else if (orientation === "Top" || orientation === "Right") {
            initialHeight = length * height;
        }
        this.insertTabRow(o.selectedRow, orientation, shape, initialHeight, height, innerBox);

        if (orientation === "Top") {
            border.getGeometry().height = shape.height - itemsHeight;
            border.getGeometry().y = itemsHeight;
        } else if (orientation === "Bottom") {
            border.getGeometry().height = shape.height - itemsHeight;
            innerBox.getGeometry().y = shape.height - itemsHeight;
        } else if (orientation === "Left") {
            border.getGeometry().width = shape.width - itemsHeight;
            border.getGeometry().x = itemsHeight;
            innerBox.getGeometry().x = itemsHeight;
        } else if (orientation === "Right") {
            border.getGeometry().width = shape.width - itemsHeight;
            innerBox.getGeometry().x = shape.width;
        }

        tab.insert(border);
        tab.insert(innerBox);

        const scrollBar = DiagramHelper.findValueByName(shape.props, "Scrollbar");
        if (scrollBar.toString() === "true") {
            const scrollBarStyle = new Style();
            scrollBarStyle[mxConstants.STYLE_STROKECOLOR] = "#7F98A9";
            scrollBarStyle[mxConstants.STYLE_FILLCOLOR] = "white";
            scrollBarStyle[mxConstants.STYLE_GRADIENTCOLOR] = "#F2F4F6";
            scrollBarStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_WEST;
            scrollBarStyle[Styles.STYLE_SELECTABLE] = 0;
            scrollBarStyle[mxConstants.STYLE_FOLDABLE] = 0;

            const scrollBarHeight = border.getGeometry().height;
            let scrollBarGeometry: MxGeometry;
            if (orientation === "Top") {
                scrollBarGeometry = MxFactory.geometry(shape.width - 21, shape.height - scrollBarHeight, 20, scrollBarHeight);
            } else if (orientation === "Bottom" || orientation === "Left") {
                scrollBarGeometry = MxFactory.geometry(shape.width - 21, 0, 20, scrollBarHeight);
            } else if (orientation === "Right") {
                scrollBarGeometry = MxFactory.geometry(border.getGeometry().width - 21, shape.height - scrollBarHeight, 20, scrollBarHeight);
            }

            const scrollBarShape = MxFactory.vertex(null, scrollBarGeometry, scrollBarStyle.convertToString());

            tab.insert(scrollBarShape);

            const markSize = {height: 3, width: 6};

            if (scrollBarHeight > 0) {
                //upTriangle
                const upTriangleStyle = new Style();
                upTriangleStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
                upTriangleStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
                upTriangleStyle[mxConstants.STYLE_STROKECOLOR] = "black";
                upTriangleStyle[mxConstants.STYLE_FILLCOLOR] = "black";
                upTriangleStyle[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_NORTH;
                upTriangleStyle[Styles.STYLE_SELECTABLE] = 0;

                const upTriangleGeometry = MxFactory.geometry(7, 11, markSize.width, markSize.height);
                upTriangleGeometry.relative = false;
                //
                scrollBarShape.insert(MxFactory.vertex(null, upTriangleGeometry, upTriangleStyle.convertToString()));


                const downTriangleStyle = new Style();
                downTriangleStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
                downTriangleStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
                downTriangleStyle[mxConstants.STYLE_STROKECOLOR] = "black";
                downTriangleStyle[mxConstants.STYLE_FILLCOLOR] = "black";
                downTriangleStyle[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_SOUTH;
                downTriangleStyle[Styles.STYLE_SELECTABLE] = 0;

                const downTriangleGeometry = MxFactory.geometry(7, scrollBarHeight - 11, markSize.width, markSize.height);
                downTriangleGeometry.relative = false;
                //
                scrollBarShape.insert(MxFactory.vertex(null, downTriangleGeometry, downTriangleStyle.convertToString()));
            }
        }

        this.applyHighlightedDisabledStates(shape, tab);

        return tab;
    };

    private insertTabRow(row, orientation, shape, startHeight, height, innerBox) {
        let item, runningWidth = 0, runningHeight, listItemEntryGeometry: MxGeometry, listItemEntry: mxCell;
        runningHeight = startHeight;

        if (row.listItemsRow) {
            for (let s = row.listItemsRow.length - 1; s > -1; s--) {
                item = row.listItemsRow[s];
                const listItemEntryStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
                listItemEntryStyle[mxConstants.STYLE_FOLDABLE] = 0;
                listItemEntryStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
                listItemEntryStyle[Styles.STYLE_SELECTABLE] = 0;
                if (orientation === "Left" || orientation === "Right") {
                    listItemEntryStyle[mxConstants.STYLE_ROTATION] = 90;
                }
                if (item.item.value.checked === true) {
                    listItemEntryStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
                    listItemEntryStyle[mxConstants.STYLE_STROKECOLOR] = "#CAE5EE";
                }

                if (runningWidth + item.width > shape.width) {
                    runningHeight = runningHeight + height;
                    runningWidth = 0;
                    listItemEntryGeometry = MxFactory.geometry(runningWidth, runningHeight, item.width, height);
                } else {
                    listItemEntryGeometry = MxFactory.geometry(runningWidth, runningHeight, item.width, height);
                }

                listItemEntry = MxFactory.vertex(
                    {label: item.item.value.name, isRichText: false},
                    listItemEntryGeometry,
                    listItemEntryStyle.convertToString());

                if (orientation === "Left" || orientation === "Right") {
                    listItemEntry.getGeometry().rotate(90, MxFactory.point(0, 0));
                }

                innerBox.insert(listItemEntry);
                runningWidth = runningWidth + item.width;
            }
        }
    }

    private getTabRows(listItems: any, shape: IShape): any {
        let currentRow = 0, selected = false, width, height = 0, row = 0, index = 0, listItemsRow = [], runningWidth = 0, runningHeight = 0, selectedRow = {};
        const listItemRows = [];

        for (let k = listItems.length - 1; k >= -1; k--) {
            if (k === -1) {
                if (selected) {
                    selectedRow = {listItemsRow: listItemsRow};
                } else {
                    if (runningHeight <= shape.height) {
                        listItemRows.push({listItemsRow: listItemsRow});
                    }
                }
                break;
            }

            const o = this.measureWord(listItems[k].value.name, shape);
            width = o.width;
            height = o.height;
            listItems[k].value.name = o.text;

            if (runningWidth + width > shape.width) {
                index = 0;
                row = row + 1;
                runningWidth = 0;
                runningHeight = runningHeight + height;

            } else {
                currentRow = row;
            }

            if (currentRow !== row) {
                if (selected) {
                    selectedRow = {listItemsRow: listItemsRow};
                } else {
                    if (runningHeight <= shape.height) {
                        listItemRows.push({listItemsRow: listItemsRow});
                    }
                }
                selected = false;
                listItemsRow = [];
            }

            if (k !== -1 && listItems[k].value.checked) {
                selected = true;
            }

            listItemsRow.push({width: width, height: height, row: row, index: index, item: listItems[k]});
            runningWidth = runningWidth + width;
            index = index + 1;
        }

        return {listItemRows: listItemRows, height: height, selectedRow: selectedRow};
    }

    private measureWord(orgText, shape): any {
        let t, w;
        const rect = mxUtils.getSizeForString(orgText + "1", shape.labelStyle.fontSize, shape.labelStyle.fontFamily, null);
        const width = rect.width;

        if (width > shape.width) {
            let resText = "";
            for (let j = orgText.length - 1; j > 1; j--) {
                t = orgText.slice(1, j);
                w = mxUtils.getSizeForString(t + "1", shape.labelStyle.fontSize, shape.labelStyle.fontFamily, null);
                if (w > shape.width) {
                    break;
                } else {
                    resText = t;
                }
            }
            return {width: shape.width, height: rect.height, text: resText};
        } else {
            return {width: width, height: rect.height, text: orgText};
        }
    }

    private list = (shape: IShape): MxCell => {
        const ddbstyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        ddbstyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        ddbstyle[mxConstants.STYLE_STROKECOLOR] = "#808080";
        ddbstyle[mxConstants.STYLE_FILLCOLOR] = "white";
        ddbstyle[mxConstants.STYLE_FOLDABLE] = 0;
        const list = this.createDefaultVertex(shape, ddbstyle, true);

        const listItems = [];
        for (let i = 0; i < shape.props.length; i++) {
            if (shape.props[i].name === "ListItem") {
                listItems.push(shape.props[i]);
            }
        }

        const scrollBarSize = 18;
        const innerBoxWidth = shape.width - scrollBarSize - 2;
        let innerBoxHeight = shape.height - 1;
        let textWidth = 0, originalTextHeight = 0, textHeight = 0, minTextHeight = 20, needHorizaontalScroll = false;

        //Calculating the max width and height
        for (let k = 0; k < listItems.length; k++) {
            let textSize;
            let sampleRect = mxUtils.getSizeForString(listItems[k].value.name, ddbstyle["fontSize"], ddbstyle["fontFamily"], textSize);
            originalTextHeight = sampleRect.height * 0.9;
            textHeight = (textHeight < 40) ? sampleRect.height * 1.9 : sampleRect.height * 1.3;
            if (textHeight < minTextHeight) {
                textHeight = minTextHeight;
            }
            if (sampleRect.width > textWidth) {
                textWidth = sampleRect.width;
            }
        }
        //Setting the width for Bold style
        if (["1", "3", "5", "7"].indexOf(ddbstyle["fontStyle"]) >= 0) {
            textWidth = textWidth * 1.15;
        }

        //Setting the InnerBox boundries
        if (textWidth + scrollBarSize > shape.width) {
            needHorizaontalScroll = true;
            innerBoxHeight = shape.height - scrollBarSize;
        }

        //Creating Inner Box
        const innerBoxStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        innerBoxStyle[Styles.STYLE_SELECTABLE] = 0;
        innerBoxStyle[mxConstants.STYLE_FOLDABLE] = 0;
        innerBoxStyle[mxConstants.STYLE_FILLCOLOR] = "#FCFCFC";
        const innerBoxGeometry = MxFactory.geometry(1, 1, innerBoxWidth, innerBoxHeight);
        const innerBox = MxFactory.vertex(null, innerBoxGeometry, innerBoxStyle.convertToString());

        //Creating Items
        for (let j = 0; j < listItems.length; j++) {
            if (j * textHeight < innerBoxHeight) {
                const listItemEntry = this.getListBoxItemEntry(shape, j, textHeight, innerBoxHeight, innerBoxWidth, listItems[j].value);

                //Creating Label
                const labelEntry = this.getListLabelInside(shape, j, textHeight, innerBoxWidth, listItems, listItemEntry, originalTextHeight);
                listItemEntry.insert(labelEntry);

                innerBox.insert(listItemEntry);
            }
        }

        list.insert(innerBox);

        //Creating Vertical ScrollBar
        const verticalScroll = this.getScrollBar(shape, false, needHorizaontalScroll, scrollBarSize);
        this.makeVertexUnselectable(verticalScroll);
        list.insert(verticalScroll);

        //Creating Horizontal ScrollBar
        if (needHorizaontalScroll) {
            const horizontalScroll = this.getScrollBar(shape, true, needHorizaontalScroll, scrollBarSize);
            this.makeVertexUnselectable(horizontalScroll);
            list.insert(horizontalScroll);
            list.insert(this.createSquareBetweenScrolls(shape, scrollBarSize));
        }

        this.applyHighlightedDisabledStates(shape, list);
        return list;
    };

    //Creating listItemEntry
    private getListBoxItemEntry(shape: IShape, j: number, textHeight: number, innerBoxHeight: number, innerBoxWidth: number, itemValue: any): MxCell {
        const listItemEntryStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        listItemEntryStyle[mxConstants.STYLE_FOLDABLE] = 0;
        listItemEntryStyle[mxConstants.STYLE_STROKEWIDTH] = 0;
        listItemEntryStyle[mxConstants.STYLE_STROKECOLOR] = "transparent";
        listItemEntryStyle[Styles.STYLE_SELECTABLE] = 0;
        listItemEntryStyle[mxConstants.STYLE_WHITE_SPACE] = "nowrap";
        listItemEntryStyle[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_MIDDLE;
        if (itemValue.checked) {
            listItemEntryStyle[mxConstants.STYLE_FILLCOLOR] = "#CAE5EE";
        } else {
            listItemEntryStyle[mxConstants.STYLE_FILLCOLOR] = "transparent";
        }
        let adjustedHeight: number;
        if ((j + 1) * textHeight > innerBoxHeight && innerBoxHeight - (j * textHeight) - 1 >= 0) {
            adjustedHeight = innerBoxHeight - (j * textHeight) - 1;
        } else {
            adjustedHeight = textHeight;
        }
        const listItemEntryGeometry = MxFactory.geometry(0, j * textHeight, innerBoxWidth, adjustedHeight);
        const listItemEntry = MxFactory.vertex(null, listItemEntryGeometry, listItemEntryStyle.convertToString());
        return listItemEntry;
    }

    //Creating Label Inside
    private getListLabelInside(shape: IShape, j: number, textHeight: number, innerBoxWidth: number, listItems: any[],
                               listItemEntry: MxCell, originalTextHeight: number): MxCell {
        const labelStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        labelStyle[mxConstants.STYLE_FOLDABLE] = 0;
        labelStyle[mxConstants.STYLE_STROKEWIDTH] = 0;
        labelStyle[mxConstants.STYLE_STROKECOLOR] = "transparent";
        labelStyle[Styles.STYLE_SELECTABLE] = 0;
        labelStyle[mxConstants.STYLE_FILLCOLOR] = "transparent";
        labelStyle[mxConstants.STYLE_WHITE_SPACE] = "nowrap";
        labelStyle[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_MIDDLE;
        this.applyDisabledStateForText(shape, labelStyle);
        let heightDif = textHeight - listItemEntry.getGeometry().height;
        heightDif -= (textHeight - originalTextHeight) / 2;
        const labelX = 1;
        const labelGeometry = MxFactory.geometry(labelX, textHeight / 2 - originalTextHeight / 2, innerBoxWidth - labelX, originalTextHeight - heightDif);
        const labelEntry = MxFactory.vertex(Helper.escapeHTMLText(listItems[j].value.name), labelGeometry, labelStyle.convertToString());
        return labelEntry;
    }

    private makeScrollBar = (xPosition: number, shapeHeight: number, strokeColor: string, makeStub: boolean): MxCell => {
        const scrollBarStyle = new Style();
        //scrollBarStyle[mxConstants.STYLE_STROKECOLOR] = "#7F98A9";
        scrollBarStyle[mxConstants.STYLE_STROKECOLOR] = strokeColor;
        scrollBarStyle[mxConstants.STYLE_FILLCOLOR] = "white";
        scrollBarStyle[mxConstants.STYLE_GRADIENTCOLOR] = "#F2F4F6";
        scrollBarStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_WEST;
        scrollBarStyle[Styles.STYLE_SELECTABLE] = 0;
        scrollBarStyle[mxConstants.STYLE_FOLDABLE] = 0;
        const scrollBarGeometry = MxFactory.geometry(xPosition, 1, 19, shapeHeight - 2);
        scrollBarGeometry.relative = false;
        const scrollBarBox = MxFactory.vertex(null, scrollBarGeometry, scrollBarStyle.convertToString());

        const markSize = {height: 3, width: 6};

        //mark
        const mark1Style = new Style();
        mark1Style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
        mark1Style[mxConstants.STYLE_STROKEWIDTH] = 1;
        mark1Style[mxConstants.STYLE_STROKECOLOR] = "black";
        mark1Style[mxConstants.STYLE_FILLCOLOR] = "black";
        mark1Style[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_SOUTH;
        mark1Style[Styles.STYLE_SELECTABLE] = 0;
        const mark1Geometry = MxFactory.geometry(7, shapeHeight - 11, markSize.width, markSize.height);
        mark1Geometry.relative = false;
        const mark1 = MxFactory.vertex(null, mark1Geometry, mark1Style.convertToString());
        scrollBarBox.insert(mark1);

        //mark2
        const mark2Style = new Style();
        mark2Style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
        mark2Style[mxConstants.STYLE_STROKEWIDTH] = 1;
        mark2Style[mxConstants.STYLE_STROKECOLOR] = "black";
        mark2Style[mxConstants.STYLE_FILLCOLOR] = "black";
        mark2Style[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_NORTH;
        mark2Style[Styles.STYLE_SELECTABLE] = 0;
        const mark2Geometry = MxFactory.geometry(7, 10, markSize.width, markSize.height);
        mark2Geometry.relative = false;
        const mark2 = MxFactory.vertex(null, mark2Geometry, mark2Style.convertToString());
        scrollBarBox.insert(mark2);

        //scrollbar stub
        if (makeStub && shapeHeight > 55) {
            const scrollBarStubStyle = new Style();
            scrollBarStubStyle[mxConstants.STYLE_STROKECOLOR] = "#7F98A9";
            scrollBarStubStyle[mxConstants.STYLE_STROKEWIDTH] = 2;
            scrollBarStubStyle[mxConstants.STYLE_FILLCOLOR] = "white";
            scrollBarStubStyle[mxConstants.STYLE_GRADIENTCOLOR] = "#F2F4F6";
            scrollBarStubStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_WEST;
            scrollBarStubStyle[Styles.STYLE_SELECTABLE] = 0;
            scrollBarStubStyle[mxConstants.STYLE_FOLDABLE] = 0;
            const scrollBarStubGeometry = MxFactory.geometry(0, 20, 19, shapeHeight / 5);
            scrollBarStubGeometry.relative = false;
            const scrollBarStub = MxFactory.vertex(null, scrollBarStubGeometry, scrollBarStyle.convertToString());
            scrollBarBox.insert(scrollBarStub);
        }
        return scrollBarBox;
    };

    private createProgressValueRect(shape: IShape): MxCell {
        const rectangleStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        rectangleStyle[mxConstants.STYLE_SHAPE] = HighlightEllipse.getName;
        rectangleStyle[mxConstants.STYLE_STROKEWIDTH] = 0;
        rectangleStyle[mxConstants.STYLE_FOLDABLE] = 0;
        rectangleStyle[mxConstants.STYLE_FILLCOLOR] = "#FFFFFF";
        rectangleStyle[mxConstants.STYLE_GRADIENTCOLOR] = "#F8C058";
        //rectangleStyle[mxConstants.STYLE_ROTATION] = "-45";
        rectangleStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_SOUTH;
        rectangleStyle[Styles.STYLE_SELECTABLE] = 0;
        rectangleStyle[mxConstants.STYLE_OVERFLOW] = "hidden";
        this.applyDisabledStateForText(shape, rectangleStyle);
        let progressValue = 0;
        for (let i = 0; i < shape.props.length; i++) {
            if (shape.props[i].name === "ProgressValue") {
                progressValue = shape.props[i].value;
            }
        }
        const marginSize = 2;
        const rectWidth = (progressValue / 100) * (shape.width - marginSize * 2);
        const geometry = MxFactory.geometry(marginSize, marginSize, rectWidth, shape.height - marginSize * 2);
        const valueRect = MxFactory.vertex(null, geometry, rectangleStyle.convertToString());
        return valueRect;
    }

    private accordion = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        style[mxConstants.STYLE_STROKEWIDTH] = 1;
        style[mxConstants.STYLE_STROKECOLOR] = "#657581";
        style[mxConstants.STYLE_FILLCOLOR] = "#FCFCFC";
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_OVERFLOW] = "hidden";

        const accordion = this.createDefaultVertex(shape, style, true);

        const listItems = [];
        let minimumBlankHeight = 0;
        for (let i = 0; i < shape.props.length; i++) {
            if (shape.props[i].name === "ListItem") {
                listItems.push(shape.props[i]);
                if (shape.props[i].value.checked) {
                    minimumBlankHeight = 10;
                }
            }
        }

        const scrollBarSize = 18;
        const drawerIconWidth = 35;
        let textWidth = 0;
        let textHeight = 0;
        const minTextHeight = 20;
        let needVerticalScroll = false;
        let needHorizaontalScroll = false;
        let innerBoxHeight = shape.height;
        let innerBoxWidth = shape.width - 1;
        //const allowedCollapse = (ShapeExtensions.getPropertyByName(shape, "AllowCollapse") === "true");

        //Calculating the max width and height
        for (let k = 0; k < listItems.length; k++) {
            let textSize;
            const sampleRect = mxUtils.getSizeForString(listItems[k].value.name, style["fontSize"], style["fontFamily"], textSize);
            textHeight = (textHeight < 30) ? sampleRect.height * 1.32 : sampleRect.height * 1.22;
            if (textHeight < minTextHeight) {
                textHeight = minTextHeight;
            }
            if (sampleRect.width > textWidth) {
                textWidth = sampleRect.width;
            }
        }
        //Setting the width for Bold style
        if (["1", "3", "5", "7"].indexOf(style["fontStyle"]) >= 0) {
            textWidth = textWidth * 1.15;
        }

        //Setting the InnerBox boundries
        if (textWidth + drawerIconWidth + (needVerticalScroll ? scrollBarSize : 0) > shape.width) {
            needHorizaontalScroll = true;
            innerBoxHeight = shape.height - scrollBarSize;
        }
        if (textHeight * listItems.length + minimumBlankHeight + (needHorizaontalScroll ? scrollBarSize : 0) > shape.height) {
            needVerticalScroll = true;
            innerBoxWidth = shape.width - scrollBarSize - 1;
        }
        if (textWidth + drawerIconWidth + (needVerticalScroll ? scrollBarSize : 0) > shape.width) {
            needHorizaontalScroll = true;
            innerBoxHeight = shape.height - scrollBarSize;
        }
        if (textHeight * listItems.length + minimumBlankHeight + (needHorizaontalScroll ? scrollBarSize : 0) > shape.height) {
            needVerticalScroll = true;
            innerBoxWidth = shape.width - scrollBarSize - 1;
        }

        //Creating Inner Box
        let innerBox: MxCell;
        if (needHorizaontalScroll || needVerticalScroll) {
            const innerBoxStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
            innerBoxStyle[Styles.STYLE_SELECTABLE] = 0;
            innerBoxStyle[mxConstants.STYLE_FOLDABLE] = 0;
            innerBoxStyle[mxConstants.STYLE_STROKEWIDTH] = 0;
            innerBoxStyle[mxConstants.STYLE_FILLCOLOR] = "#FCFCFC";
            const innerBoxGeometry = MxFactory.geometry(1, 1, innerBoxWidth, innerBoxHeight);
            innerBox = MxFactory.vertex(null, innerBoxGeometry, innerBoxStyle.convertToString());
        } else {
            innerBox = accordion;
        }

        //Creating Items
        let useBlankSpaceBefore = false;
        for (let j = 0; j < listItems.length; j++) {
            const actualMinBlankHeight = useBlankSpaceBefore ? minimumBlankHeight : 0;
            //Check if the height is enough
            if (j * textHeight + actualMinBlankHeight < innerBoxHeight) {
                const listItemEntryStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
                listItemEntryStyle[mxConstants.STYLE_FOLDABLE] = 0;
                listItemEntryStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
                listItemEntryStyle[Styles.STYLE_SELECTABLE] = 0;
                listItemEntryStyle[mxConstants.STYLE_STROKECOLOR] = "#BBC6D5";
                listItemEntryStyle[mxConstants.STYLE_FILLCOLOR] = "white";
                listItemEntryStyle[mxConstants.STYLE_GRADIENTCOLOR] = "#DBE6F4";
                listItemEntryStyle[mxConstants.STYLE_WHITE_SPACE] = "nowrap";
                listItemEntryStyle[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_MIDDLE;
                listItemEntryStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_SOUTH;
                //Creating Last List Item if the height is not enough
                let adjustedHeight: number;
                if ((j + 1) * textHeight + actualMinBlankHeight > innerBoxHeight) {
                    adjustedHeight = innerBoxHeight - (j * textHeight) - actualMinBlankHeight - 1;
                } else {
                    adjustedHeight = textHeight;
                }
                //Setting the height of blank space inside default Item
                let whiteSpaceHeight = 0;
                if (useBlankSpaceBefore) {
                    if (needVerticalScroll) {
                        whiteSpaceHeight = actualMinBlankHeight;
                    } else {
                        whiteSpaceHeight = innerBoxHeight - listItems.length * textHeight;
                    }
                }
                const listItemEntryGeometry = MxFactory.geometry(0, j * textHeight + whiteSpaceHeight, innerBoxWidth, adjustedHeight);
                //listItemEntryGeometry.relative = true;
                this.applyDisabledStateForText(shape, listItemEntryStyle);
                const listItemEntry = MxFactory.vertex(
                    Helper.escapeHTMLText(listItems[j].value.name),
                    listItemEntryGeometry,
                    listItemEntryStyle.convertToString());
                //Adding Drawer Icon
                if (needHorizaontalScroll === false && listItemEntryGeometry.height === textHeight) {
                    const drawerStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
                    drawerStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_LINE;
                    drawerStyle[mxConstants.STYLE_STROKEWIDTH] = 2;
                    drawerStyle[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_EAST;
                    drawerStyle[mxConstants.STYLE_STROKECOLOR] = "#1E395B";
                    drawerStyle[mxConstants.STYLE_FILLCOLOR] = "#1E395B";
                    drawerStyle[mxConstants.STYLE_FOLDABLE] = 0;
                    drawerStyle[Styles.STYLE_SELECTABLE] = 0;
                    drawerStyle[mxConstants.STYLE_ROTATION] = listItems[j].value.checked ? "45" : "-45";

                    let drawerGeometry = MxFactory.geometry(listItemEntryGeometry.width - (drawerIconWidth / 2) + 5, listItemEntryGeometry.height / 2, 6, 1);
                    let drawerRect = MxFactory.vertex(null, drawerGeometry, drawerStyle.convertToString());
                    listItemEntry.insert(drawerRect);

                    drawerStyle[mxConstants.STYLE_ROTATION] = listItems[j].value.checked ? "-45" : "45";
                    drawerGeometry = MxFactory.geometry(listItemEntryGeometry.width - (drawerIconWidth / 2) + 2, listItemEntryGeometry.height / 2, 6, 1);
                    drawerRect = MxFactory.vertex(null, drawerGeometry, drawerStyle.convertToString());
                    listItemEntry.insert(drawerRect);
                }
                innerBox.insert(listItemEntry);
                if (listItems[j].value.checked) {
                    useBlankSpaceBefore = true;
                }
            }
        }
        //Creating Horizontal ScrollBar
        accordion.insert(innerBox);
        if (needHorizaontalScroll) {
            const horizontalScroll = this.getScrollBar(shape, true, needHorizaontalScroll && needVerticalScroll, scrollBarSize);
            this.makeVertexUnselectable(horizontalScroll);
            accordion.insert(horizontalScroll);
        }
        //Creating Vertical ScrollBar
        if (needVerticalScroll) {
            const verticalScroll = this.getScrollBar(shape, false, needHorizaontalScroll && needVerticalScroll, scrollBarSize);
            this.makeVertexUnselectable(verticalScroll);
            accordion.insert(verticalScroll);
        }
        //Creating Small Square Between ScrollBars
        if (needHorizaontalScroll && needVerticalScroll) {
            accordion.insert(this.createSquareBetweenScrolls(shape, scrollBarSize));
        }


        this.applyHighlightedDisabledStates(shape, accordion);
        return accordion;
    };

    private createSquareBetweenScrolls = (shape: IShape, scrollBarSize: number): MxCell => {
        const squareStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        squareStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        squareStyle[mxConstants.STYLE_STROKEWIDTH] = 0;
        //squareStyle[mxConstants.STYLE_STROKECOLOR] = "#F6F9FE";
        squareStyle[mxConstants.STYLE_FILLCOLOR] = "#F6F9FE";
        squareStyle[mxConstants.STYLE_FOLDABLE] = 0;
        squareStyle[Styles.STYLE_SELECTABLE] = 0;

        const squareGeometry = MxFactory.geometry(shape.width - scrollBarSize, shape.height - scrollBarSize, scrollBarSize, scrollBarSize);
        const squareRect = MxFactory.vertex(null, squareGeometry, squareStyle.convertToString());
        return squareRect;
    };

    //Makes the Vertex Unselectable
    public makeVertexUnselectable = (vertex: MxCell) => {
        let vertexStyle = vertex.getStyle();
        if (vertexStyle.indexOf(Styles.STYLE_SELECTABLE) < 0) {
            const noneSelectable = Styles.STYLE_SELECTABLE + "=0;";
            vertexStyle += noneSelectable;
        }
        vertex.setStyle(vertexStyle);
    };

    //Creates Scrollbar To Be Used By Accordion
    private getScrollBar = (shape: IShape, isHorizontal: boolean, hasBothScrollBars: boolean, scrollBarSize: number): MxCell => {
        const scroll = {
            width: (isHorizontal ? (hasBothScrollBars ? shape.width - scrollBarSize : shape.width) : scrollBarSize),
            height: (isHorizontal ? scrollBarSize : (hasBothScrollBars ? shape.height - scrollBarSize : shape.height)),
            x: (isHorizontal ? 0 : shape.width - scrollBarSize),
            y: (isHorizontal ? shape.height - scrollBarSize : 0),
            props: [
                {name: "length", value: "2"},
                {name: "Orientation", value: (isHorizontal ? "Horizontal" : "Vertical")},
                {name: "ScrollValue", value: "0"}
            ]
        };

        const scrollShape = this.scrollbar(<IShape>scroll);
        return scrollShape;
    };

    private contextMenu = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        style[mxConstants.STYLE_STROKEWIDTH] = 1;
        style[mxConstants.STYLE_STROKECOLOR] = "#657581";
        style[mxConstants.STYLE_FILLCOLOR] = "#FCFCFC";
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_OVERFLOW] = "hidden";
        style[mxConstants.STYLE_WHITE_SPACE] = "nowrap";

        const contextmenu = this.createDefaultVertex(shape, style, true);

        const treeItems = [];
        for (let i = 0; i < shape.props.length; i++) {
            if (shape.props[i].name === "TreeItem") {
                treeItems.push(shape.props[i]);
            }
        }

        let leftRectSize = 28, triangleIconWidth = 35, textWidth = 0, textHeight = 0, minTextHeight = 20, needVerticalScroll = false;
        let needHorizaontalScroll = false, innerBoxHeight = shape.height, verticalDrawerHeight = 10, originalTextHeight = 0;
        const innerBoxWidth = shape.width - 1 - leftRectSize;

        //Calculating the max width and height
        for (let k = 0; k < treeItems.length; k++) {
            let textSize;
            const sampleRect = mxUtils.getSizeForString(treeItems[k].value.text, style["fontSize"], style["fontFamily"], textSize);
            originalTextHeight = sampleRect.height * 1.1;
            textHeight = (textHeight < 30) ? sampleRect.height * 1.8 : sampleRect.height * 1.4;
            if (textHeight < minTextHeight) {
                textHeight = minTextHeight;
            }
            if (sampleRect.width > textWidth) {
                textWidth = sampleRect.width;
            }
        }

        //Setting the width for Bold style
        if (["1", "3", "5", "7"].indexOf(style["fontStyle"]) >= 0) {
            textWidth = textWidth * 1.15;
        }

        //Setting the InnerBox boundries
        if (textWidth + triangleIconWidth + leftRectSize > shape.width) {
            needHorizaontalScroll = true;
        }
        if (textHeight * treeItems.length + 5 > shape.height) {
            needVerticalScroll = true;
            innerBoxHeight = shape.height - verticalDrawerHeight;
        }

        //Creating Left Rectangle
        const leftRect = this.getContextMenuLeftRect(shape, leftRectSize);
        contextmenu.insert(leftRect);

        //Creating Inner Box
        let innerBox: MxCell;
        const innerBoxStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        innerBoxStyle[Styles.STYLE_SELECTABLE] = 0;
        innerBoxStyle[mxConstants.STYLE_FOLDABLE] = 0;
        innerBoxStyle[mxConstants.STYLE_STROKEWIDTH] = 0;
        innerBoxStyle[mxConstants.STYLE_FILLCOLOR] = "#FCFCFC";
        const innerBoxGeometry = MxFactory.geometry(leftRectSize + 1, 1, innerBoxWidth - 1, innerBoxHeight - 2);
        innerBox = MxFactory.vertex(null, innerBoxGeometry, innerBoxStyle.convertToString());

        //Creating Items
        for (let j = 0; j < treeItems.length; j++) {
            //Check if the height is enough
            if (j * textHeight < innerBoxHeight) {
                const listItemEntry = this.getContextMenuListItemEntry(shape, j, textHeight, innerBoxHeight, innerBoxWidth);

                //Creating Label
                const labelEntry = this.getContextMenuLabelInside(shape, j, textHeight, innerBoxWidth, treeItems, listItemEntry, originalTextHeight);
                listItemEntry.insert(labelEntry);

                //Adding Check Mark
                if (treeItems[j].value.isSelected === true) {
                    const markEntry = this.getContextMenuMarkEntry(shape, j, textHeight, innerBoxHeight, treeItems, listItemEntry, labelEntry, leftRectSize);
                    contextmenu.insert(markEntry);
                }

                //Adding Has Child Icon
                if (treeItems[j].value.hasChildTreeItems === true && needHorizaontalScroll === false && textHeight - listItemEntry.getGeometry().height < 10) {
                    const hasChildTriangle = this.getContextMenuHasChild(shape, j, treeItems, listItemEntry, labelEntry, innerBoxWidth);
                    listItemEntry.insert(hasChildTriangle);
                }

                innerBox.insert(listItemEntry);
            }
        }

        //Creating Vertical Drawer
        if (needVerticalScroll) {
            contextmenu.insert(this.getContextMenuVDrawer(shape, verticalDrawerHeight));
        }

        contextmenu.insert(innerBox);
        this.applyHighlightedDisabledStates(shape, contextmenu);
        return contextmenu;
    };

    private table = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        style[mxConstants.STYLE_STROKEWIDTH] = 1;
        style[mxConstants.STYLE_STROKECOLOR] = "#91C3FF";
        style[mxConstants.STYLE_FILLCOLOR] = "white";
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_OVERFLOW] = "hidden";
        style[mxConstants.STYLE_WHITE_SPACE] = "nowrap";
        const table = this.createDefaultVertex(shape, style, true);
        const tableDataObject = ShapeExtensions.getPropertyByName(shape, "TableDataObject");
        const columnHeaders = ShapeExtensions.getPropertyByName(shape, "TableColumnHeaders");
        const includeHeaderRow = ShapeExtensions.getPropertyByName(shape, "IncludeHeaderRow").toLowerCase() === "true";
        const showBorder = ShapeExtensions.getPropertyByName(shape, "ShowBorder").toLowerCase() === "true";
        const showScrollBars = ShapeExtensions.getPropertyByName(shape, "ShowScrollBars").toLowerCase() === "true";
        let showHorizontalScrollBar = false;
        const tableStyle = ShapeExtensions.getPropertyByName(shape, "TableStyle");
        const cellHeight = 25;
        const tableDataRows = [];
        let idx = 0;
        let highlighterHeight = cellHeight; //to be populated when we render the first data row.
        while (idx < tableDataObject.length) {
            const row = [];
            for (let i = 0; i < columnHeaders.length; i++) {
                row.push(tableDataObject[idx]);
                idx++;
            }
            tableDataRows.push(row);
        }
        const matrix = this.generatePreprocessingMatrix(shape, columnHeaders, tableDataRows, includeHeaderRow);
        let processedMatrix = this.generateDimensionProcessedMatrix(matrix);
        let innerBoxWidth = shape.width - 6;
        if (showScrollBars) {
            innerBoxWidth -= 25;
        }
        const innerBoxStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        innerBoxStyle[Styles.STYLE_SELECTABLE] = 0;
        innerBoxStyle[mxConstants.STYLE_FOLDABLE] = 0;
        const innerBoxGeometry = MxFactory.geometry(3, 3, innerBoxWidth, shape.height - 6);
        const innerBox = MxFactory.vertex(null, innerBoxGeometry, innerBoxStyle.convertToString());

        let totalTheoreticalLengthOfData = 0;
        for (let z = 0; z < processedMatrix[0].length; z++) {
            totalTheoreticalLengthOfData += processedMatrix[0][z].width;
        }
        if (totalTheoreticalLengthOfData > innerBoxGeometry.width) {
            showHorizontalScrollBar = true;
        } else { // if we don't need scroll bars, we need to add filler cells.
            processedMatrix = this.addFillerCellsToMatrix(processedMatrix, innerBoxGeometry.width - totalTheoreticalLengthOfData);
        }

        highlighterHeight = processedMatrix[0][0].height;
        let renderedDataIndex = 1;
        if (includeHeaderRow) {
            renderedDataIndex = 0;
            highlighterHeight = processedMatrix[1][0].height;
        }
        let yPosn = 0;
        let nextRowRequired = true; //we always render the next row
        for (let j = 0; j < processedMatrix.length; j++) {
            if (nextRowRequired) {
                let correctedRowHeight = processedMatrix[j][0].height;
                if (yPosn + processedMatrix[j][0].height > innerBoxGeometry.height) {
                    nextRowRequired = false;
                    correctedRowHeight = innerBoxGeometry.height - yPosn;
                }
                const rowShape = this.makeTableRow(
                    shape,
                    processedMatrix[j],
                    processedMatrix[j][0].isHeader,
                    innerBoxGeometry.width,
                    correctedRowHeight,
                    yPosn,
                    tableStyle,
                    showBorder,
                    renderedDataIndex);
                renderedDataIndex++;
                innerBox.insert(rowShape);
                yPosn += processedMatrix[j][0].height;
            }
        }
        //highlight first column
        let highlightYPosn = processedMatrix[0][0].height + 2;
        if (!includeHeaderRow) {
            highlightYPosn = 2;
        }
        const highlightContainerStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        highlightContainerStyle[Styles.STYLE_SELECTABLE] = 0;
        highlightContainerStyle[mxConstants.STYLE_FOLDABLE] = 0;
        highlightContainerStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        highlightContainerStyle[mxConstants.STYLE_STROKECOLOR] = "#7DA2CE";
        const highlightContainerGeometry = MxFactory.geometry(30 + 2, highlightYPosn, innerBox.geometry.width - 30 - 4, highlighterHeight - 4);
        const highlightContainter = MxFactory.vertex(null, highlightContainerGeometry, highlightContainerStyle.convertToString());

        const highlightBoxStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        highlightBoxStyle[mxConstants.STYLE_FILLCOLOR] = "#EBF3FD";
        highlightBoxStyle[mxConstants.STYLE_OPACITY] = 25;
        highlightBoxStyle[Styles.STYLE_SELECTABLE] = 0;
        const highlightBoxGeometry = MxFactory.geometry(0, 0, highlightContainerGeometry.width, highlightContainerGeometry.height);
        const highlightBox = MxFactory.vertex(null, highlightBoxGeometry, highlightBoxStyle.convertToString());
        highlightContainter.insert(highlightBox);

        innerBox.insert(highlightContainter);
        table.insert(innerBox);

        const innerBoxBorderStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        innerBoxBorderStyle[Styles.STYLE_SELECTABLE] = 0;
        innerBoxBorderStyle[mxConstants.STYLE_FOLDABLE] = 0;
        innerBoxBorderStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        innerBoxBorderStyle[mxConstants.STYLE_STROKECOLOR] = "#C0CBD9";
        innerBoxBorderStyle[mxConstants.STYLE_FILLCOLOR] = "transparent";
        const innerBoxBorderGeometry = MxFactory.geometry(3, 3, innerBoxWidth, shape.height - 6);
        const innerBoxBorder = MxFactory.vertex(null, innerBoxBorderGeometry, innerBoxBorderStyle.convertToString());
        table.insert(innerBoxBorder);

        //scrollbar stuff
        if (showScrollBars && shape.height > 20 && shape.width > 20) {
            let scrollBarHeight = shape.height + 1;
            if (showHorizontalScrollBar) {
                scrollBarHeight -= 19;
            }
            const verticalScrollBar = this.makeScrollBar(shape.width - 19, scrollBarHeight, "#91C3FF", true);
            table.insert(verticalScrollBar);
        }
        if (showHorizontalScrollBar && shape.height > 20 && shape.width > 20) {
            let scrollBarWidth = shape.width + 1;
            if (showScrollBars) {
                scrollBarWidth -= 19;
            }
            const horizontalScrollBar = this.makeTableHorizontalScrollBar(shape.height - 19, scrollBarWidth, "#91C3FF", true);
            table.insert(horizontalScrollBar);
        }
        this.applyHighlightedDisabledStates(shape, table);
        return table;
    };

    private generatePreprocessingMatrix(shape: IShape, headerColumn: any[], tableData: any[], showHeaderRow: boolean): any[] {
        const fontSize = shape.labelStyle.fontSize;
        const fontFamily = shape.labelStyle.fontFamily;
        const div = document.createElement("div");
        div.style.fontFamily = fontFamily;
        div.style.fontSize = Math.round(parseInt(fontSize, 10)) + "px";
        div.style.lineHeight = Math.round(parseInt(fontSize, 10) * parseInt(mxConstants.LINE_HEIGHT, 10)) + "px";
        div.style.position = "absolute";
        div.style.visibility = "hidden";
        div.style.display = (mxClient["IS_QUIRKS"]) ? "inline" : "inline-block";
        div.style.zoom = "1";
        div.style.whiteSpace = "nowrap";
        // Adds the text and inserts into DOM for updating of size
        div.innerHTML = "";
        document.body.appendChild(div);
        //we can do stuff here
        const matrix = [];
        let nextRow: any[];
        let contentText: string;
        let widthToUse: number;
        let heightToUse: number;
        if (showHeaderRow) {
            nextRow = [];
            for (let i = 0; i < headerColumn.length; i++) {
                contentText = headerColumn[i].header;
                if (contentText === "") {
                    widthToUse = 70;
                    heightToUse = 25;
                } else {
                    div.innerHTML = contentText;
                    widthToUse = div.offsetWidth * 1.1;
                    heightToUse = div.offsetHeight * 1.4;
                }
                const processedHeaderCell = {
                    displayText: contentText,
                    width: widthToUse,
                    height: heightToUse,
                    isHeader: true
                };
                nextRow.push(processedHeaderCell);
            }
            matrix.push(nextRow);
        }
        for (let j = 0; j < tableData.length; j++) {
            nextRow = [];
            for (let k = 0; k < tableData[j].length; k++) {
                contentText = tableData[j][k].tableCellValue.contentText;
                if (contentText === "") {
                    widthToUse = 70;
                    heightToUse = 25;
                } else {
                    div.innerHTML = contentText;
                    widthToUse = div.offsetWidth * 1.1;
                    heightToUse = div.offsetHeight * 1.4;
                }
                const processedCell = {
                    displayText: contentText,
                    width: widthToUse,
                    height: heightToUse,
                    isHeader: false,
                    rowNumber: j
                };
                nextRow.push(processedCell);
            }
            matrix.push(nextRow);
        }
        document.body.removeChild(div);
        return matrix;
    }

    private generateDimensionProcessedMatrix(preprocessingMatrix: any[]): any[] {
        const numRows = preprocessingMatrix.length;
        const numCols = preprocessingMatrix[0].length;
        const defaultColumnWidth = 70;
        const defaultRowHeight = 25;
        //correct height of rows
        for (let i = 0; i < numRows; i++) {
            const row = preprocessingMatrix[i];
            let maxHeightForRow = defaultRowHeight;
            for (let j = 0; j < numCols; j++) {
                maxHeightForRow = Math.max(maxHeightForRow, row[j].height);
            }
            for (let j = 0; j < row.length; j++) {
                preprocessingMatrix[i][j].height = maxHeightForRow;
            }
        }
        for (let k = 0; k < numCols; k++) {
            const column = this.getColumnAtYIndex(preprocessingMatrix, k, numRows);
            let maxWidthForColumn = defaultColumnWidth;
            for (let l = 0; l < numRows; l++) {
                maxWidthForColumn = Math.max(maxWidthForColumn, column[l].width);
            }
            for (let l = 0; l < numRows; l++) {
                preprocessingMatrix[l][k].width = maxWidthForColumn;
            }
        }
        const finalMatrix = this.addHeaderRowCellsToMatrix(preprocessingMatrix);
        return finalMatrix;
    }

    private addHeaderRowCellsToMatrix(matrix: any[]): any[] {
        for (let i = 0; i < matrix.length; i++) {
            matrix[i].unshift({
                displayText: "",
                width: 30,
                height: matrix[i][0].height,
                isHeader: matrix[i][0].isHeader,
                rowNumber: i
            });
        }
        return matrix;
    }

    private addFillerCellsToMatrix(matrix: any[], cellWidth: number) {
        for (let i = 0; i < matrix.length; i++) {
            matrix[i].push({
                displayText: "",
                width: cellWidth,
                height: matrix[i][0].height,
                isHeader: matrix[i][0].isHeader,
                rowNumber: i
            });
        }
        return matrix;
    }

    private getColumnAtYIndex(matrix: any[], index: number, numRows: number): any[] {
        const columnAtIndex = [];
        for (let i = 0; i < numRows; i++) {
            columnAtIndex.push(matrix[i][index]);
        }
        return columnAtIndex;
    }

    private makeTableRow(shape: IShape,
                         row: any[],
                         isHeader: boolean,
                         containerWidth: number,
                         rowHeight: number,
                         yPosn: number,
                         tableStyle: string,
                         showBorder: boolean,
                         renderedIndex: number) {

        const rowStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        rowStyle[Styles.STYLE_SELECTABLE] = 0;
        rowStyle[mxConstants.STYLE_FOLDABLE] = 0;
        if (isHeader) {
            rowStyle[mxConstants.STYLE_FILLCOLOR] = "white";
            rowStyle[mxConstants.STYLE_GRADIENTCOLOR] = "#EDF2FB";
            rowStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_SOUTH;
        } else {
            if ((tableStyle === "AlternatingRows") && (renderedIndex != null && (renderedIndex % 2 === 0))) {
                rowStyle[mxConstants.STYLE_FILLCOLOR] = "#F5F7FB";
            }
        }
        const rowGeometry = MxFactory.geometry(0, yPosn, containerWidth, rowHeight);
        const rowShape = MxFactory.vertex(null, rowGeometry, rowStyle.convertToString());
        let nextCellRequired = true; //we always make the first cell, regardless of width.
        let xPosn = 0; //keep track of xposition.
        for (let i = 0; i < row.length; i++) {
            if (nextCellRequired) {
                let cellWidth = row[i].width;
                if (xPosn + cellWidth > containerWidth) {
                    nextCellRequired = false;
                    cellWidth = containerWidth - xPosn;
                }
                const text = row[i].displayText;
                const cellStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
                cellStyle[Styles.STYLE_SELECTABLE] = 0;
                cellStyle[mxConstants.STYLE_FOLDABLE] = 0;
                cellStyle[mxConstants.STYLE_FILLCOLOR] = "transparent";
                const cellGeometry = MxFactory.geometry(xPosn, 0, cellWidth, rowHeight);
                if (!isHeader) {
                    if (i === 0) {
                        cellStyle[mxConstants.STYLE_FILLCOLOR] = "#E8EFFC"; //first cell of the row has a different color.
                    }
                } else { //if is header
                    cellStyle[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_CENTER;
                    cellStyle[mxConstants.STYLE_FONTCOLOR] = "#4C607A";
                }
                cellStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
                cellStyle[mxConstants.STYLE_STROKECOLOR] = "#C0CBD9";
                if (!showBorder && !isHeader && i !== 0) {
                    cellStyle[mxConstants.STYLE_STROKEWIDTH] = 0;
                    cellStyle[mxConstants.STYLE_STROKECOLOR] = "none";
                }
                this.applyDisabledStateForText(shape, cellStyle);
                const label = {label: text, isRichText: false};
                const cellShape = MxFactory.vertex(label, cellGeometry, cellStyle.convertToString());
                if (!isHeader && i === 0 && renderedIndex === 1) {
                    const tableCursorMark = this.createTableCursorMark(shape, "#8CA3C2", cellWidth, rowHeight);
                    if (shape.width > 50 && shape.height > 50) {
                        cellShape.insert(tableCursorMark);
                    }
                }
                rowShape.insert(cellShape);
                xPosn += cellWidth;
            }
        }
        return rowShape;
    }

    //makes vertical scroll bar for table shape.
    private makeTableHorizontalScrollBar = (yPosition: number, shapeWidth: number, strokeColor: string, makeStub: boolean): MxCell => {
        const scrollBarStyle = new Style();
        scrollBarStyle[mxConstants.STYLE_STROKECOLOR] = strokeColor;
        scrollBarStyle[mxConstants.STYLE_FILLCOLOR] = "white";
        scrollBarStyle[mxConstants.STYLE_GRADIENTCOLOR] = "#F2F4F6";
        scrollBarStyle[mxConstants.STYLE_OPACITY] = 100;

        scrollBarStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_SOUTH;
        scrollBarStyle[Styles.STYLE_SELECTABLE] = 0;
        scrollBarStyle[mxConstants.STYLE_FOLDABLE] = 0;
        const scrollBarGeometry = MxFactory.geometry(0, yPosition, shapeWidth - 1, 19);
        scrollBarGeometry.relative = false;
        const scrollBarBox = MxFactory.vertex(null, scrollBarGeometry, scrollBarStyle.convertToString());

        const markSize = {height: 6, width: 3};

        //mark
        const mark1Style = new Style();
        mark1Style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
        mark1Style[mxConstants.STYLE_STROKEWIDTH] = 1;
        mark1Style[mxConstants.STYLE_STROKECOLOR] = "black";
        mark1Style[mxConstants.STYLE_FILLCOLOR] = "black";
        mark1Style[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_WEST;
        mark1Style[Styles.STYLE_SELECTABLE] = 0;
        const mark1Geometry = MxFactory.geometry(7, 7, markSize.width, markSize.height);
        mark1Geometry.relative = false;
        const mark1 = MxFactory.vertex(null, mark1Geometry, mark1Style.convertToString());
        scrollBarBox.insert(mark1);

        //mark2
        const mark2Style = new Style();
        mark2Style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
        mark2Style[mxConstants.STYLE_STROKEWIDTH] = 1;
        mark2Style[mxConstants.STYLE_STROKECOLOR] = "black";
        mark2Style[mxConstants.STYLE_FILLCOLOR] = "black";
        mark2Style[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_EAST;
        mark2Style[Styles.STYLE_SELECTABLE] = 0;
        const mark2Geometry = MxFactory.geometry(shapeWidth - 11, 7, markSize.width, markSize.height);
        mark2Geometry.relative = false;
        const mark2 = MxFactory.vertex(null, mark2Geometry, mark2Style.convertToString());
        scrollBarBox.insert(mark2);

        //scrollbar stub
        if (makeStub && shapeWidth > 55) {
            const scrollBarStubStyle = new Style();
            scrollBarStubStyle[mxConstants.STYLE_STROKECOLOR] = "#7F98A9";
            scrollBarStubStyle[mxConstants.STYLE_STROKEWIDTH] = 2;
            scrollBarStubStyle[mxConstants.STYLE_FILLCOLOR] = "white";
            scrollBarStubStyle[mxConstants.STYLE_GRADIENTCOLOR] = "#F2F4F6";
            scrollBarStubStyle[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_WEST;
            scrollBarStubStyle[Styles.STYLE_SELECTABLE] = 0;
            scrollBarStubStyle[mxConstants.STYLE_FOLDABLE] = 0;
            const scrollBarStubGeometry = MxFactory.geometry(20, 0, shapeWidth / 5, 19);
            scrollBarStubGeometry.relative = false;
            const scrollBarStub = MxFactory.vertex(null, scrollBarStubGeometry, scrollBarStyle.convertToString());
            scrollBarBox.insert(scrollBarStub);
        }
        return scrollBarBox;
    };

    private createTableCursorMark(shape: IShape, tableCursorMarkColor: string, innerRectWidth: number, innerRectHeight: number): MxCell {
        const tableCursorMarkStyle = new Style();
        tableCursorMarkStyle[mxConstants.STYLE_STROKEWIDTH] = 2;
        tableCursorMarkStyle[mxConstants.STYLE_FOLDABLE] = 0;
        tableCursorMarkStyle[mxConstants.STYLE_STROKECOLOR] = tableCursorMarkColor;
        tableCursorMarkStyle[mxConstants.STYLE_SHAPE] = TableCursorShape.getName;
        tableCursorMarkStyle[Styles.STYLE_SELECTABLE] = 0;
        tableCursorMarkStyle[mxConstants.STYLE_FILLCOLOR] = "transparent";
        const tableCursorMarkStyleGeometry = MxFactory.geometry(0, 0, innerRectWidth, innerRectHeight);
        const tableCursorMarkRect = MxFactory.vertex(null, tableCursorMarkStyleGeometry, tableCursorMarkStyle.convertToString());
        return tableCursorMarkRect;
    }

    //Creating listItemEntry
    private getContextMenuListItemEntry(shape: IShape, j: number, textHeight: number, innerBoxHeight: number, innerBoxWidth: number): MxCell {
        const listItemEntryStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        listItemEntryStyle[mxConstants.STYLE_FOLDABLE] = 0;
        listItemEntryStyle[mxConstants.STYLE_STROKEWIDTH] = 0;
        listItemEntryStyle[Styles.STYLE_SELECTABLE] = 0;
        listItemEntryStyle[mxConstants.STYLE_FILLCOLOR] = "transparent";
        listItemEntryStyle[mxConstants.STYLE_WHITE_SPACE] = "nowrap";
        listItemEntryStyle[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_MIDDLE;
        //if (treeItems[j].value.isEnabled === false)
        //    listItemEntryStyle[mxConstants.STYLE_TEXT_OPACITY] = 55;

        //Creating Last List Item if the height is not enough
        let adjustedHeight: number;
        if ((j + 1) * textHeight > innerBoxHeight) {
            adjustedHeight = innerBoxHeight - (j * textHeight) - 1;
        } else {
            adjustedHeight = textHeight;
        }
        const listItemEntryGeometry = MxFactory.geometry(0, j * textHeight, innerBoxWidth, adjustedHeight);
        const listItemEntry = MxFactory.vertex(null, listItemEntryGeometry, listItemEntryStyle.convertToString());
        return listItemEntry;
    }

    //Creating Label Inside
    private getContextMenuLabelInside(shape: IShape,
                                      j: number,
                                      textHeight: number,
                                      innerBoxWidth: number,
                                      treeItems: any[],
                                      listItemEntry: MxCell,
                                      originalTextHeight: number): MxCell {
        const labelStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        labelStyle[mxConstants.STYLE_FOLDABLE] = 0;
        labelStyle[mxConstants.STYLE_STROKEWIDTH] = 0;
        labelStyle[Styles.STYLE_SELECTABLE] = 0;
        labelStyle[mxConstants.STYLE_FILLCOLOR] = "transparent";
        labelStyle[mxConstants.STYLE_WHITE_SPACE] = "nowrap";
        labelStyle[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_MIDDLE;
        if (treeItems[j].value.isEnabled === false) {
            labelStyle[mxConstants.STYLE_TEXT_OPACITY] = 55;
        }
        let heightDif = textHeight - listItemEntry.getGeometry().height;
        heightDif -= (textHeight - originalTextHeight) / 2;
        const labelGeometry = MxFactory.geometry(2, textHeight / 2 - originalTextHeight / 2, innerBoxWidth, originalTextHeight - heightDif);
        this.applyDisabledStateForText(shape, labelStyle);
        const labelEntry = MxFactory.vertex(Helper.escapeHTMLText(treeItems[j].value.text), labelGeometry, labelStyle.convertToString());
        return labelEntry;
    }

    //Creating Check Mark
    private getContextMenuMarkEntry(shape: IShape,
                                    j: number,
                                    textHeight: number,
                                    innerBoxWidth: number,
                                    treeItems: any[],
                                    listItemEntry: MxCell,
                                    labelEntry: MxCell,
                                    leftRectSize: number): MxCell {
        const markStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        markStyle[mxConstants.STYLE_FOLDABLE] = 0;
        markStyle[mxConstants.STYLE_STROKEWIDTH] = 0;
        markStyle[Styles.STYLE_SELECTABLE] = 0;
        markStyle[mxConstants.STYLE_FILLCOLOR] = "transparent";
        markStyle[mxConstants.STYLE_WHITE_SPACE] = "nowrap";
        markStyle[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_MIDDLE;
        markStyle[mxConstants.STYLE_FONTSIZE] = 15;
        if (treeItems[j].value.isEnabled === false) {
            markStyle[mxConstants.STYLE_TEXT_OPACITY] = 55;
        }
        const markGeometry = MxFactory.geometry(
            leftRectSize / 2 - 10,
            listItemEntry.getGeometry().y + (listItemEntry.getGeometry().height / 2) - (labelEntry.getGeometry().height / 2) + 3,
            20,
            labelEntry.getGeometry().height);
        const markEntry = MxFactory.vertex("&#10004;", markGeometry, markStyle.convertToString());
        return markEntry;
    }

    //Creating Has Child Triangle
    private getContextMenuHasChild(shape: IShape, j: number, treeItems: any[], listItemEntry: MxCell, labelEntry: MxCell, innerBoxWidth: number): MxCell {
        const hasChildStyle = new Style();
        hasChildStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
        hasChildStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        hasChildStyle[mxConstants.STYLE_STROKECOLOR] = "black";
        hasChildStyle[mxConstants.STYLE_FILLCOLOR] = "black";
        hasChildStyle[mxConstants.STYLE_FOLDABLE] = 0;
        hasChildStyle[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_EAST;
        hasChildStyle[Styles.STYLE_SELECTABLE] = 0;
        if (treeItems[j].value.isEnabled === false) {
            hasChildStyle[mxConstants.STYLE_OPACITY] = 40;
        }
        const hasChildTriangleHeight = 6;
        const hasChildTriangleWidth = 3;
        const hasChildTriangleX = innerBoxWidth - 15;
        const hasChildTriangleY = listItemEntry.getGeometry().height / 2 + labelEntry.getGeometry().y - 5;
        const hasChildTriangleGeometry = MxFactory.geometry(hasChildTriangleX, hasChildTriangleY, hasChildTriangleWidth, hasChildTriangleHeight);
        hasChildTriangleGeometry.relative = false;
        const hasChildTriangle = MxFactory.vertex(null, hasChildTriangleGeometry, hasChildStyle.convertToString());
        return hasChildTriangle;
    }

    //Creating Left Rectangle
    private getContextMenuLeftRect(shape: IShape, leftRectSize: number): MxCell {
        const leftRectStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        leftRectStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        leftRectStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        leftRectStyle[mxConstants.STYLE_STROKECOLOR] = "#657581";
        leftRectStyle[mxConstants.STYLE_FILLCOLOR] = "#F1F5FB";
        leftRectStyle[mxConstants.STYLE_FOLDABLE] = 0;
        leftRectStyle[Styles.STYLE_SELECTABLE] = 0;

        const leftRectGeometry = MxFactory.geometry(0, 0, leftRectSize, shape.height);
        const leftRect = MxFactory.vertex(null, leftRectGeometry, leftRectStyle.convertToString());
        return leftRect;
    }

    //Creating Vertical Drawer
    private getContextMenuVDrawer(shape: IShape, verticalDrawerHeight: number): MxCell {
        const drawerRectStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        drawerRectStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        drawerRectStyle[mxConstants.STYLE_STROKEWIDTH] = 0;
        //drawerRectStyle[mxConstants.STYLE_STROKECOLOR] = "#657581";
        drawerRectStyle[mxConstants.STYLE_FILLCOLOR] = "#EFF7FC";
        drawerRectStyle[mxConstants.STYLE_FOLDABLE] = 0;
        drawerRectStyle[Styles.STYLE_SELECTABLE] = 0;

        const drawerRectGeometry = MxFactory.geometry(1, shape.height - verticalDrawerHeight, shape.width - 2, verticalDrawerHeight);
        const drawerRect = MxFactory.vertex(null, drawerRectGeometry, drawerRectStyle.convertToString());

        //Inserting the triangle
        const triangleStyle = new Style();
        triangleStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
        triangleStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        triangleStyle[mxConstants.STYLE_STROKECOLOR] = "black";
        triangleStyle[mxConstants.STYLE_FILLCOLOR] = "black";
        triangleStyle[mxConstants.STYLE_FOLDABLE] = 0;
        triangleStyle[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_SOUTH;
        triangleStyle[Styles.STYLE_SELECTABLE] = 0;
        const triangleHeight = 2;
        const triangleWidth = 4;
        const triangleX = (drawerRectGeometry.width / 2) - (triangleWidth / 2);
        const triangleY = (drawerRectGeometry.height / 2) - (triangleHeight / 2);
        const triangleGeometry = MxFactory.geometry(triangleX, triangleY, triangleWidth, triangleHeight);
        triangleGeometry.relative = false;
        const triangle = MxFactory.vertex(null, triangleGeometry, triangleStyle.convertToString());
        drawerRect.insert(triangle);

        return drawerRect;
    }

    private iconShape = (shape: IShape): MxCell => {
        const style = <any>this.styleBuilder.createDefaultShapeStyle(shape, IconShape.shapeName);
        style[mxConstants.STYLE_FOLDABLE] = 0;

        style.IconKey = DiagramHelper.findValueByName(shape.props, "IconKey");
        if (!style.IconKey) {
            return null;
        }
        const icon = this.createDefaultVertex(shape, style, true);

        this.applyHighlightedDisabledStates(shape, icon);
        return icon;
    };

    private treeview = (shape: IShape): MxCell => {
        const showLines = (ShapeExtensions.getPropertyByName(shape, "ConnectingLines") === "Show");
        const treeIcon = ShapeExtensions.getPropertyByName(shape, "TreeIcon");
        const showFolder = (ShapeExtensions.getPropertyByName(shape, "Folder") === "Show");
        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_RECTANGLE;
        style[mxConstants.STYLE_STROKEWIDTH] = 1;
        style[mxConstants.STYLE_STROKECOLOR] = "#657581";
        style[mxConstants.STYLE_FILLCOLOR] = "#FFFFFF";
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_OVERFLOW] = "hidden";
        style[mxConstants.STYLE_WHITE_SPACE] = "nowrap";

        const treeview = this.createDefaultVertex(shape, style, true);

        const treeItems = [];
        for (let i = 0; i < shape.props.length; i++) {
            if (shape.props[i].name === "TreeItem") {
                shape.props[i].value.type = "TreeItem";
                treeItems.push(shape.props[i]);
            } else if (shape.props[i].name === "ChildTreeItem") {
                shape.props[i].value.type = "ChildTreeItem";
                treeItems.push(shape.props[i]);
            }
        }

        let levelIndentSize = 20, textWidth = 0, textHeight = 0, minTextHeight = 20, needVerticalScroll = false, sampleRect;
        let needHorizaontalScroll = false, innerBoxHeight = shape.height - 1, originalTextHeight = 0, scrollBarSize = 18, textSize;
        let innerBoxWidth = shape.width - 5, horzLineWidth = 10, folderIconSize = 14;

        //Calculating the max width and height
        for (let j = 0; j < treeItems.length; j++) {
            sampleRect = mxUtils.getSizeForString(treeItems[j].value.text, style["fontSize"], style["fontFamily"], textSize);
            originalTextHeight = sampleRect.height * 1.1;
            textHeight = (textHeight < 30) ? sampleRect.height * 1.7 : sampleRect.height * 1.4;
            if (textHeight < minTextHeight) {
                textHeight = minTextHeight;
            }
            //const effectiveWidth = sampleRect.width + levelIndentSize * (childTreeItems[l].value.level.match(/./g) || []).length;
            const leftIndent = levelIndentSize * (treeItems[j].value.level.split(".").length);
            if (sampleRect.width + leftIndent > textWidth) {
                textWidth = sampleRect.width + leftIndent;
            }
            treeItems[j].value.leftIndent = leftIndent - 5;
        }

        //Setting the width for Bold style
        if (["1", "3", "5", "7"].indexOf(style["fontStyle"]) >= 0) {
            textWidth = textWidth * 1.15;
        }

        //Setting the InnerBox boundries
        if (textWidth + (horzLineWidth + 3) + (needVerticalScroll ? scrollBarSize : 0) + (showFolder ? folderIconSize : 0) > shape.width) {
            needHorizaontalScroll = true;
            innerBoxHeight = shape.height - scrollBarSize;
        }
        if (textHeight * treeItems.length + 5 + (needHorizaontalScroll ? scrollBarSize : 0) > shape.height) {
            needVerticalScroll = true;
            innerBoxWidth = shape.width - scrollBarSize;
        }
        if (textWidth + (horzLineWidth + 3) + (needVerticalScroll ? scrollBarSize : 0) + (showFolder ? folderIconSize : 0) > shape.width) {
            needHorizaontalScroll = true;
            innerBoxHeight = shape.height - scrollBarSize;
        }
        if (textHeight * treeItems.length + 5 + (needHorizaontalScroll ? scrollBarSize : 0) > shape.height) {
            needVerticalScroll = true;
            innerBoxWidth = shape.width - scrollBarSize;
        }

        //Creating Inner Box
        let innerBox: MxCell;
        const innerBoxStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        innerBoxStyle[Styles.STYLE_SELECTABLE] = 0;
        innerBoxStyle[mxConstants.STYLE_FOLDABLE] = 0;
        innerBoxStyle[mxConstants.STYLE_STROKEWIDTH] = 0;
        innerBoxStyle[mxConstants.STYLE_FILLCOLOR] = "#FFFFFF";
        innerBoxStyle[mxConstants.STYLE_STROKECOLOR] = "transparent";
        const innerBoxGeometry = MxFactory.geometry(5, 1, innerBoxWidth - 1, innerBoxHeight - 1);
        innerBox = MxFactory.vertex(null, innerBoxGeometry, innerBoxStyle.convertToString());

        //Creating Items
        let lastItemEntry;
        for (let k = 0; k < treeItems.length; k++) {
            const listItemEntry = this.getTreeListItemEntry(shape, k, textHeight, innerBoxHeight, innerBoxWidth, treeItems[k].value);
            //Check if the height is enough
            if (k * textHeight < innerBoxHeight) {
                treeItems[k].value.itemGenerated = true;

                //Creating Folder
                const folderEntry = this.getTreeFolderInside(shape, k, textHeight, innerBoxWidth, treeItems, listItemEntry,
                    originalTextHeight, showFolder, folderIconSize);
                if (folderEntry) {
                    listItemEntry.insert(folderEntry);
                }

                //Creating Label
                const labelEntry = this.getTreeLabelInside(shape, k, textHeight, innerBoxWidth, treeItems, listItemEntry,
                    originalTextHeight, showFolder, folderIconSize);
                listItemEntry.insert(labelEntry);

                //adding Connecting Lines
                if (showLines) {
                    const reducedHeight = textHeight - listItemEntry.getGeometry().height;
                    if (reducedHeight < 10) {
                        innerBox.insert(this.getTreeHorizontalLine(shape, listItemEntry, horzLineWidth));
                    }
                    if (lastItemEntry) {
                        innerBox.insert(this.getTreeVerticalLine(shape, listItemEntry, lastItemEntry, treeItems, k, horzLineWidth,
                            textHeight, treeIcon, innerBoxHeight));
                    }
                }

                //Adding Has Child Icon
                if (treeItems[k].value.hasChildTreeItems === true && textHeight - listItemEntry.getGeometry().height < 10) {
                    let hasChildShape;
                    if (treeIcon === "Triangle") {
                        hasChildShape = this.getTreeHasChildTriangle(shape, k, treeItems, listItemEntry, horzLineWidth);
                    } else if (treeIcon === "PlusMinus") {
                        hasChildShape = this.getTreeHasChildMinus(shape, k, treeItems, listItemEntry, horzLineWidth);
                    }
                    if (hasChildShape) {
                        innerBox.insert(hasChildShape);
                    }
                }

                lastItemEntry = listItemEntry;
                innerBox.insert(listItemEntry);
            } else if (showLines) {
                const vertLine = this.getTreeVerticalLine(shape, listItemEntry, lastItemEntry, treeItems, k, horzLineWidth,
                    textHeight, treeIcon, innerBoxHeight);
                if (vertLine) {
                    const lineGeometry = vertLine.getGeometry();
                    if (lineGeometry.y + lineGeometry.height < innerBox.getGeometry().height + 5) {
                        innerBox.insert(vertLine);
                    }
                }
            }
        }

        //Creating Horizontal ScrollBar
        treeview.insert(innerBox);
        if (needHorizaontalScroll) {
            const horizontalScroll = this.getScrollBar(shape, true, needHorizaontalScroll && needVerticalScroll, scrollBarSize);
            this.makeVertexUnselectable(horizontalScroll);
            treeview.insert(horizontalScroll);
        }
        //Creating Vertical ScrollBar
        if (needVerticalScroll) {
            const verticalScroll = this.getScrollBar(shape, false, needHorizaontalScroll && needVerticalScroll, scrollBarSize);
            this.makeVertexUnselectable(verticalScroll);
            treeview.insert(verticalScroll);
        }
        //Creating Small Square Between ScrollBars
        if (needHorizaontalScroll && needVerticalScroll) {
            treeview.insert(this.createSquareBetweenScrolls(shape, scrollBarSize));
        }

        this.applyHighlightedDisabledStates(shape, treeview);
        return treeview;
    };

    //Creating HasChild Minus
    private getTreeHasChildMinus(shape: IShape, k: number, treeItems: any[], listItemEntry: MxCell, horzLineWidth: number): MxCell {
        const hasChildStyle = new Style();
        hasChildStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_IMAGE;
        hasChildStyle[mxConstants.STYLE_PERIMETER] = mxPerimeter.RectanglePerimeter;
        hasChildStyle[mxConstants.STYLE_IMAGE] = "/Scripts/mxClient/images/Minus.png";
        hasChildStyle[Styles.STYLE_SELECTABLE] = 0;
        //if (treeItems[k].value.isEnabled === false)
        //    hasChildStyle[mxConstants.STYLE_OPACITY] = 40;
        const hasChildIconHeight = 14;
        const hasChildIconWidth = 14;
        const hasChildIconX = listItemEntry.getGeometry().x - horzLineWidth - hasChildIconWidth / 2 + 3;
        const hasChildIconY = listItemEntry.getGeometry().height / 2 + listItemEntry.getGeometry().y - hasChildIconHeight / 2 + 2;
        const hasChildIconGeometry = MxFactory.geometry(hasChildIconX, hasChildIconY, hasChildIconWidth, hasChildIconHeight);
        hasChildIconGeometry.relative = false;
        const hasChildIcon = MxFactory.vertex(null, hasChildIconGeometry, hasChildStyle.convertToString());
        return hasChildIcon;
    }

    //Creating HasChild Triangle
    private getTreeHasChildTriangle(shape: IShape, k: number, treeItems: any[], listItemEntry: MxCell, horzLineWidth: number): MxCell {
        const hasChildStyle = new Style();
        hasChildStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_TRIANGLE;
        hasChildStyle[mxConstants.STYLE_STROKEWIDTH] = 1;
        hasChildStyle[mxConstants.STYLE_STROKECOLOR] = "black";
        hasChildStyle[mxConstants.STYLE_FILLCOLOR] = "black";
        hasChildStyle[mxConstants.STYLE_FOLDABLE] = 0;
        hasChildStyle[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_EAST;
        hasChildStyle[Styles.STYLE_SELECTABLE] = 0;
        hasChildStyle[mxConstants.STYLE_ROTATION] = 45;
        if (treeItems[k].value.isEnabled === false) {
            hasChildStyle[mxConstants.STYLE_OPACITY] = 40;
        }
        const hasChildTriangleHeight = 8;
        const hasChildTriangleWidth = 4;
        const hasChildTriangleX = listItemEntry.getGeometry().x - horzLineWidth;
        const hasChildTriangleY = listItemEntry.getGeometry().height / 2 + listItemEntry.getGeometry().y - 1;
        const hasChildTriangleGeometry = MxFactory.geometry(hasChildTriangleX, hasChildTriangleY, hasChildTriangleWidth, hasChildTriangleHeight);
        hasChildTriangleGeometry.relative = false;
        const hasChildTriangle = MxFactory.vertex(null, hasChildTriangleGeometry, hasChildStyle.convertToString());
        return hasChildTriangle;
    }

    //Creating Folder Inside
    private getTreeFolderInside(shape: IShape, j: number, textHeight: number, innerBoxWidth: number, treeItems: any[],
                                listItemEntry: MxCell, originalTextHeight: number, showFolder: boolean, folderIconSize: number): MxCell {
        if (treeItems[j].value.hasChildTreeItems === false || showFolder === false) {
            return null;
        }
        const folderStyle = new Style();
        folderStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_IMAGE;
        folderStyle[mxConstants.STYLE_PERIMETER] = mxPerimeter.RectanglePerimeter;
        folderStyle[mxConstants.STYLE_IMAGE] = "/Scripts/mxClient/images/Folder.png";
        folderStyle[Styles.STYLE_SELECTABLE] = 0;
        folderStyle[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_MIDDLE;
        if (treeItems[j].value.isEnabled === false || this.isTreeItemParentDisabled(j, treeItems)) {
            folderStyle[mxConstants.STYLE_OPACITY] = 40;
        }
        const folderHeight = 10;
        const folderX = 2;
        const folderY = textHeight / 2 - folderHeight / 2 + 2;
        const folderGeometry = MxFactory.geometry(folderX, folderY, folderIconSize, folderHeight);
        folderGeometry.relative = false;
        const folderIcon = MxFactory.vertex(null, folderGeometry, folderStyle.convertToString());
        return folderIcon;
    }

    //Creating Label Inside
    private getTreeLabelInside(shape: IShape, j: number, textHeight: number, innerBoxWidth: number, treeItems: any[],
                               listItemEntry: MxCell, originalTextHeight: number, showFolder: boolean, folderIconSize: number): MxCell {
        const labelStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        labelStyle[mxConstants.STYLE_FOLDABLE] = 0;
        labelStyle[mxConstants.STYLE_STROKEWIDTH] = 0;
        labelStyle[mxConstants.STYLE_STROKECOLOR] = "transparent";
        labelStyle[Styles.STYLE_SELECTABLE] = 0;
        labelStyle[mxConstants.STYLE_FILLCOLOR] = "transparent";
        labelStyle[mxConstants.STYLE_WHITE_SPACE] = "nowrap";
        labelStyle[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_MIDDLE;
        if (treeItems[j].value.isEnabled === false || this.isTreeItemParentDisabled(j, treeItems)) {
            labelStyle[mxConstants.STYLE_TEXT_OPACITY] = 55;
        } else {
            this.applyDisabledStateForText(shape, labelStyle);
        }
        let heightDif = textHeight - listItemEntry.getGeometry().height;
        heightDif -= (textHeight - originalTextHeight) / 2;
        let labelX = 2;
        if (treeItems[j].value.hasChildTreeItems && showFolder) {
            labelX += folderIconSize;
        }
        const labelGeometry = MxFactory.geometry(
            labelX,
            textHeight / 2 - originalTextHeight / 2,
            innerBoxWidth - treeItems[j].value.leftIndent - labelX,
            originalTextHeight - heightDif);
        const labelEntry = MxFactory.vertex(Helper.escapeHTMLText(treeItems[j].value.text), labelGeometry, labelStyle.convertToString());
        return labelEntry;
    }

    //Checking if the TreeItem's Parent is Disabled
    private isTreeItemParentDisabled(originalItemIndex: number, treeItems: any[]): boolean {
        for (let i = originalItemIndex - 1; i >= 0; i--) {
            if (treeItems[i].value.leftIndent < treeItems[originalItemIndex].value.leftIndent) {
                if (treeItems[i].value.isEnabled === false) {
                    treeItems[originalItemIndex].value.isEnabled = false;
                    return true;
                }
                break;
            }
        }
        return false;
    }

    //Creating Horizontal Line
    private getTreeHorizontalLine(shape: IShape, listItemEntry: MxCell, horzLineWidth: number): MxCell {
        const horzLineStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        horzLineStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_LINE;
        horzLineStyle[mxConstants.STYLE_STROKEWIDTH] = 2;
        horzLineStyle[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_EAST;
        horzLineStyle[mxConstants.STYLE_STROKECOLOR] = "#E9E9E9";
        horzLineStyle[mxConstants.STYLE_FILLCOLOR] = "#E9E9E9";
        horzLineStyle[mxConstants.STYLE_FOLDABLE] = 0;
        horzLineStyle[Styles.STYLE_SELECTABLE] = 0;

        const horzLineGeometry = MxFactory.geometry(listItemEntry.getGeometry().x - horzLineWidth,
            (listItemEntry.getGeometry().y + listItemEntry.getGeometry().height / 2) + 2,
            horzLineWidth,
            1);
        const horzLine = MxFactory.vertex(null, horzLineGeometry, horzLineStyle.convertToString());
        return horzLine;
    }

    //Creating Vertical Line
    private getTreeVerticalLine(shape: IShape, listItemEntry: MxCell, lastItemEntry: MxCell, treeItems: any[],
                                k: number, horzLineWidth: number, textHeight: number, treeIcon: string, innerBoxHeight): MxCell {
        const vertLineStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        vertLineStyle[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_LINE;
        vertLineStyle[mxConstants.STYLE_STROKEWIDTH] = 2;
        vertLineStyle[mxConstants.STYLE_DIRECTION] = mxConstants.DIRECTION_SOUTH;
        vertLineStyle[mxConstants.STYLE_STROKECOLOR] = "#E9E9E9";
        vertLineStyle[mxConstants.STYLE_FILLCOLOR] = "#E9E9E9";
        vertLineStyle[mxConstants.STYLE_FOLDABLE] = 0;
        vertLineStyle[Styles.STYLE_SELECTABLE] = 0;
        //vertLineStyle[mxConstants.STYLE_] = 0;

        let height = this.getTreeLineHeight(listItemEntry, lastItemEntry, treeItems, k, textHeight, treeIcon);
        if (height <= 0) {
            return null;
        }
        const y = this.getTreeLineY(listItemEntry, lastItemEntry, treeItems, k, textHeight, treeIcon);
        if (y + height > innerBoxHeight) {
            let adjustedHeight = innerBoxHeight - y;
            //y += height - adjustedHeight;
            height = adjustedHeight;
        }
        const vertLineGeometry = MxFactory.geometry(
            listItemEntry.getGeometry().x - horzLineWidth,
            y,
            1,
            height);
        const vertLine = MxFactory.vertex(null, vertLineGeometry, vertLineStyle.convertToString());
        return vertLine;
    }

    private getTreeLineHeight(listItemEntry: MxCell, lastItemEntry: MxCell, treeItems: any[],
                              k: number, textHeight: number, treeIcon: string): number {
        if (treeItems[k].value.leftIndent === treeItems[k - 1].value.leftIndent) {
            const itemsHeightDiff = (listItemEntry.getGeometry().y + listItemEntry.getGeometry().height / 2) -
                (lastItemEntry.getGeometry().y + lastItemEntry.getGeometry().height / 2);
            return itemsHeightDiff + 1 - this.getHasChildDecreaseSize(treeItems[k - 1].value, treeIcon);
        } else if (treeItems[k].value.leftIndent > treeItems[k - 1].value.leftIndent) {
            return (listItemEntry.getGeometry().height / 2) + 3;
        } else { //Means treeItems[k].value.leftIndent < treeItems[k - 1].value.leftIndent
            for (let i = k - 2; i >= 0; i--) {
                if (treeItems[i].value.leftIndent === treeItems[k].value.leftIndent) {
                    return listItemEntry.getGeometry().y - i * textHeight - this.getHasChildDecreaseSize(treeItems[i].value, treeIcon) + 2;
                }
            }
        }
        return 0;
    }

    private getHasChildDecreaseSize(treeValue: any, treeIcon: string): number {
        if (treeValue.hasChildTreeItems === true) {
            if (treeIcon === "Triangle") {
                return 4;
            } else if (treeIcon === "PlusMinus") {
                return 8;
            }
        }
        return 0;
    }

    private getTreeLineY(listItemEntry: MxCell, lastItemEntry: MxCell, treeItems: any[],
                         k: number, textHeight: number, treeIcon: string): number {
        if (treeItems[k].value.leftIndent === treeItems[k - 1].value.leftIndent) {
            return (lastItemEntry.getGeometry().y + lastItemEntry.getGeometry().height / 2) +
                this.getHasChildDecreaseSize(treeItems[k - 1].value, treeIcon) + 2;
        } else if (treeItems[k].value.leftIndent > treeItems[k - 1].value.leftIndent) {
            return listItemEntry.getGeometry().y;
        } else { //Means treeItems[k].value.leftIndent < treeItems[k - 1].value.leftIndent
            for (let i = k - 2; i >= 0; i--) {
                if (treeItems[i].value.leftIndent === treeItems[k].value.leftIndent) {
                    return (i * textHeight + 1 + listItemEntry.getGeometry().height / 2) +
                        this.getHasChildDecreaseSize(treeItems[i].value, treeIcon);
                }
            }
        }
        return 0;
    }

    //Creating listItemEntry
    private getTreeListItemEntry(shape: IShape, j: number, textHeight: number, innerBoxHeight: number, innerBoxWidth: number, itemValue: any): MxCell {
        const listItemEntryStyle = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        listItemEntryStyle[mxConstants.STYLE_FOLDABLE] = 0;
        listItemEntryStyle[mxConstants.STYLE_STROKEWIDTH] = 0;
        listItemEntryStyle[mxConstants.STYLE_STROKECOLOR] = "transparent";
        listItemEntryStyle[Styles.STYLE_SELECTABLE] = 0;
        listItemEntryStyle[mxConstants.STYLE_FILLCOLOR] = "transparent";
        listItemEntryStyle[mxConstants.STYLE_WHITE_SPACE] = "nowrap";
        listItemEntryStyle[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_MIDDLE;
        //if (treeItems[j].value.isEnabled === false)
        //    listItemEntryStyle[mxConstants.STYLE_TEXT_OPACITY] = 55;

        //Creating Last List Item if the height is not enough
        let adjustedHeight: number;
        if ((j + 1) * textHeight > innerBoxHeight && innerBoxHeight - (j * textHeight) - 1 >= 0) {
            adjustedHeight = innerBoxHeight - (j * textHeight) - 1;
        } else {
            adjustedHeight = textHeight;
        }
        const listItemEntryGeometry = MxFactory.geometry(itemValue.leftIndent, j * textHeight - 1, innerBoxWidth - itemValue.leftIndent, adjustedHeight);
        const listItemEntry = MxFactory.vertex(null, listItemEntryGeometry, listItemEntryStyle.convertToString());
        return listItemEntry;
    }
}
