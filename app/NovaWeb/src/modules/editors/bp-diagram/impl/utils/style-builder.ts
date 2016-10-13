import {IShape, IConnection, ILabelStyle} from "../models";
import {AbstractShapeFactory} from "../abstract-diagram-factory";
import {ArrowHeads, ConnectorTypes} from "./constants";

export class Styles {
    public static get STYLE_SELECTABLE(): string {
        return "selectable";
    }
}

export class Style {
    public convertToString(): string {
        let value = "";
        for (let key in this) {
            if (this.hasOwnProperty(key)) {
                value += key + "=" + this[key] + ";";
            }
        }
        return value;
    }

    public static createFromString(styleStr: string) {
        const style = {};
        if (styleStr != null) {
            const keyValuePairs = styleStr.split(";");
            keyValuePairs.forEach((p) => {
                const keyValue = p.split("=");
                style[keyValue[0]] = keyValue[1];
            });
        }
        return style;
    }

    public removeProperty(propertyName: string) {
        delete this[propertyName];
    }
}


export class MenuStyleObject {
    public styleFontStyle = "";
    public textStyle = "";
    public blankAreaStyle = "";
    public checkMarkStyle = "";
    public rgbaColor = "";
}

export class StyleBuilder {

    public createDefaultShapeStyle = (shape: IShape, shapeKey?: string): Style => {
        const style = new Style();
        if (shapeKey != null) {
            style[mxConstants.STYLE_SHAPE] = shapeKey;
        }
        if (shape.shadow && shape.fillOpacity === 1) {
            style[mxConstants.STYLE_SHADOW] = 1;
        }
        style[mxConstants.STYLE_ALIGN] = shape.labelTextAlignment;
        if (shape.fill != null) {
            style[mxConstants.STYLE_FILLCOLOR] = AbstractShapeFactory.convertToRgbaIfNeeded(shape.fill, shape.fillOpacity);
        }
        if (shape.gradientFill != null && shape.fillOpacity === 1) {
            style[mxConstants.STYLE_GRADIENTCOLOR] = shape.gradientFill;
            if (shape.shadow === false) {
                style[mxConstants.STYLE_GRADIENT_DIRECTION] = mxConstants.DIRECTION_NORTH;
            }
        }
        if (shape.strokeWidth != null) {
            style[mxConstants.STYLE_STROKEWIDTH] = shape.strokeWidth;
        }
        if (shape.stroke != null) {
            if (shape.strokeWidth === 0) {
                style[mxConstants.STYLE_STROKECOLOR] = mxConstants.NONE;
            } else {
                style[mxConstants.STYLE_STROKECOLOR] = AbstractShapeFactory.convertToRgbaIfNeeded(shape.stroke, shape.strokeOpacity);
            }
        }
        if (shape.strokeDashPattern != null) {
            style[mxConstants.STYLE_DASHED] = 1;
            style[mxConstants.STYLE_DASH_PATTERN] = shape.strokeDashPattern;
        }
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_MIDDLE;
        return this.applyLabelStyle(style, shape.labelStyle);
    };

    public createDefaultConnectionStyle = (connection: IConnection): Style => {
        const style = new Style();
        style[mxConstants.STYLE_SHAPE] = mxConstants.SHAPE_CONNECTOR;
        style[mxConstants.STYLE_STARTARROW] = mxConstants.NONE;
        style[mxConstants.STYLE_ENDARROW] = mxConstants.NONE;
        style[mxConstants.STYLE_STROKECOLOR] = AbstractShapeFactory.convertToRgbaIfNeeded(connection.stroke, connection.strokeOpacity);
        style[mxConstants.STYLE_STROKEWIDTH] = connection.strokeWidth;
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;
        style[mxConstants.STYLE_SPACING_TOP] = 5;
        style[mxConstants.STYLE_VERTICAL_LABEL_POSITION] = mxConstants.ALIGN_BOTTOM;
        style[mxConstants.STYLE_EXIT_PERIMETER] = 0;
        style[mxConstants.STYLE_ENTRY_PERIMETER] = 0;
        style[mxConstants.STYLE_ALIGN] = mxConstants.ALIGN_CENTER;

        if (connection.strokeDashPattern != null) {
            style[mxConstants.STYLE_DASHED] = 1;
            style[mxConstants.STYLE_DASH_PATTERN] = connection.strokeDashPattern;
        }
        if (connection.type) {
            switch (connection.type.toLowerCase()) {
                case ConnectorTypes.CURVED:
                    style[mxConstants.STYLE_CURVED] = 1;
                    break;
                case ConnectorTypes.RIGHT_ANGLED:
                    style[mxConstants.STYLE_EDGE] = mxConstants.EDGESTYLE_ORTHOGONAL;
                    break;
                default:
                    break;
            }
        }
        this.initArrowHeadStyle(style, connection.startArrow, true);
        this.initArrowHeadStyle(style, connection.endArrow, false);
        return style;
    };

    public applyLabelStyle(style: Style, labelStyle: ILabelStyle): Style {
        if (!labelStyle) {
            return style;
        }

        if (labelStyle.fontFamily) {
            style[mxConstants.STYLE_FONTFAMILY] = labelStyle.fontFamily;
        }

        if (labelStyle.fontSize) {
            style[mxConstants.STYLE_FONTSIZE] = labelStyle.fontSize;
        }

        let fontStyle = mxConstants.DEFAULT_FONTSTYLE;

        /* tslint:disable */
        if (labelStyle.isItalic) {
            fontStyle |= mxConstants.FONT_ITALIC;
        }
        if (labelStyle.isBold) {
            fontStyle |= mxConstants.FONT_BOLD;
        }
        if (labelStyle.isUnderline) {
            fontStyle |= mxConstants.FONT_UNDERLINE;
        }
        /* tslint:enable */

        if (fontStyle !== mxConstants.DEFAULT_FONTSTYLE) {
            style[mxConstants.STYLE_FONTSTYLE] = fontStyle;
        }

        if (labelStyle.foreground) {
            style[mxConstants.STYLE_FONTCOLOR] = AbstractShapeFactory.convertToRgbaIfNeeded(labelStyle.foreground, 1);
        }

        return style;
    }

    private initArrowHeadStyle(style: Style, arrowHeadType: string, start: boolean) {
        const arrow = start ? mxConstants.STYLE_STARTARROW : mxConstants.STYLE_ENDARROW;
        const fill = start ? mxConstants.STYLE_STARTFILL : mxConstants.STYLE_ENDFILL;
        if (arrowHeadType) {
            switch (arrowHeadType.toLowerCase()) {
                case ArrowHeads.CIRCLE:
                    style[arrow] = mxConstants.ARROW_OVAL;
                    style[fill] = 0;
                    break;
                case ArrowHeads.ARROW:
                    style[arrow] = mxConstants.ARROW_OPEN;
                    break;
                case ArrowHeads.BLACK_ARROW:
                    style[arrow] = mxConstants.ARROW_CLASSIC;
                    break;
                case ArrowHeads.RHOMBUS:
                    style[arrow] = mxConstants.ARROW_DIAMOND;
                    style[fill] = 0;
                    break;
                case ArrowHeads.FILLED_RHOMBUS:
                    style[arrow] = mxConstants.ARROW_DIAMOND;
                    break;
                case ArrowHeads.OPEN_ARROW:
                    style[arrow] = mxConstants.ARROW_BLOCK;
                    style[fill] = 0;
                    break;
                case ArrowHeads.SLASH:
                    style[arrow] = ArrowHeads.SLASH;
                    break;
                default:
                    style[arrow] = mxConstants.NONE;
            }
        }
        style[mxConstants.STYLE_ENDSIZE] = 8;
        style[mxConstants.STYLE_STARTSIZE] = 8;
    }
}
