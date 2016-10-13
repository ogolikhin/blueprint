import * as angular from "angular";
import {IDiagram, IConnection, IDiagramElement, IHierarchyDiagram, IHierarchyElement, IPoint, IProp} from "../models";

export class MxFactory {

    public static graph(container: HTMLElement): MxGraph {
        return new mxGraph(container, new mxGraphModel());
    }

    public static point(x?: number, y?: number): MxPoint {
        return new mxPoint(x, y);
    }

    public static cell(value?: any, geometry?: MxGeometry, style?: string) {
        return new mxCell(value, geometry, style);
    }

    public static edge(value?: any, geometry?: MxGeometry, style?: string) {
        let mxCell = MxFactory.cell(value, geometry, style);
        mxCell.setEdge(true);
        return mxCell;
    }

    public static vertex(value?: any, geometry?: MxGeometry, style?: string) {
        let mxCell = MxFactory.cell(value, geometry, style);
        mxCell.vertex = true;
        return mxCell;
    }

    public static geometry(x?: number, y?: number, width?: number, height?: number): MxGeometry {
        return new mxGeometry(x, y, width, height);
    }

    public static rectangle(x: number, y: number, width: number, height: number): MxRectangle {
        return new mxRectangle(x, y, width, height);
    }
}

export class ConnectionExtensions {
    public static defaultConnectionPoints = [
        MxFactory.point(0.5, 0.0),
        MxFactory.point(1.0, 0.5),
        MxFactory.point(0.5, 1.0),
        MxFactory.point(0.0, 0.5)
    ];

    public static decoratorSize = 12;

    public static closestConnectionPoint(relativePoint: MxPoint): MxPoint {
        return MathExtensions.closestConnectionPoint(relativePoint, ConnectionExtensions.defaultConnectionPoints);
    }

    public static transformToConnectionPoint(connection: IConnection, labelSize: MxRectangle, isSource: boolean): MxPoint {
        if (connection.points && connection.points.length >= 2) {
            let first: IPoint;
            let second: IPoint;
            if (isSource) {
                first = connection.points[0];
                second = connection.points[1];
            } else {
                first = connection.points[connection.points.length - 1];
                second = connection.points[connection.points.length - 2];
            }
            let position = MxFactory.point(first.x, first.y);

            let vector: MxPoint = MathExtensions.subtract(position, MxFactory.point(second.x, second.y));
            let angle: number = MathExtensions.getAngleBetween(MxFactory.point(0, 1), vector) % 360;

            let thickness = connection.strokeWidth ? connection.strokeWidth : 1;

            return ConnectionExtensions.transformToConnectionPointInternal(position, angle, labelSize, thickness, ConnectionExtensions.decoratorSize + 4);
        }
        return null;
    }

    private static transformToConnectionPointInternal(position: MxPoint,
                                                      angle: number,
                                                      labelSize: MxRectangle,
                                                      thickness: number,
                                                      decoratorSize: number): MxPoint {
        let index: number = 1;
        let offset: MxPoint = MxFactory.point(0, 0);

        let factor1: number = index > 0 ? 0.0 : index < 0 ? -1.0 : -0.5;
        let factor2: number = index > 0 ? 1.0 : index < 0 ? -1.0 : 0.0;

        let transformPoint: MxPoint = MxFactory.point(0, 0);
        if (angle > 45 && angle < 135) {
            transformPoint.x = position.x + offset.x + decoratorSize;
            transformPoint.y = position.y + offset.y + labelSize.height * factor1 + thickness * factor2;
        } else if (angle >= 135 && angle < 225) {
            transformPoint.x = position.x + offset.x + labelSize.width * factor1 + thickness * factor2;
            transformPoint.y = position.y + offset.y + decoratorSize;
        } else if (angle > 225 && angle < 315) {
            transformPoint.x = position.x - offset.x - labelSize.width - decoratorSize;
            transformPoint.y = position.y - offset.y + labelSize.height * factor1 + thickness * factor2;
        } else {
            transformPoint.x = position.x - offset.x + labelSize.width * factor1 + thickness * factor2;
            transformPoint.y = position.y - offset.y - labelSize.height - decoratorSize;
        }
        return transformPoint;
    }

    public static closestConnectionPoints(sourceGeometry: MxGeometry, targetGeometry: MxGeometry, connectionConstrains?: Array<MxPoint>) {
        connectionConstrains = connectionConstrains == null ? ConnectionExtensions.defaultConnectionPoints : connectionConstrains;
        if (connectionConstrains.length === 0) {
            throw "connection constrains should not be empty";
        }
        let sourcePoints = [];
        let targetPoints = [];
        connectionConstrains.forEach(c => {
            sourcePoints.push(MxFactory.point(sourceGeometry.x + sourceGeometry.width * c.x, sourceGeometry.y + sourceGeometry.height * c.y));
            targetPoints.push(MxFactory.point(targetGeometry.x + targetGeometry.width * c.x, targetGeometry.y + targetGeometry.height * c.y));
        });

        let source: MxPoint = sourcePoints[0];
        let target: MxPoint = targetPoints[0];
        let minDistance = Number.MAX_VALUE;
        for (let i = 0; i < connectionConstrains.length; i++) {
            let p1 = sourcePoints[i];
            for (let j = 0; j < connectionConstrains.length; j++) {
                let p2 = targetPoints[j];
                let distance = MathExtensions.distance(p1, p2);
                if (distance <= minDistance) {
                    minDistance = distance;
                    source = p1;
                    target = p2;
                }
            }
        }
        return [source, target];
    }
}

/*
 * Class: HierarchyHelper
 *
 * Holds helper methods that re-orders shapes & connections according to their z-index
 */
export class HierarchyHelper {
    public static createHierarchy(diagram: IDiagram, orderByZindex?: boolean): IHierarchyDiagram {
        let lookup = {};
        (<IHierarchyDiagram>diagram).children = [];
        // Merge shapes and connections into one array
        let diagramElements = [];
        diagram.connections.forEach((connection: IHierarchyElement) => {
            connection.isShape = false;
            diagramElements.push(connection);
            lookup[connection.id] = connection;
        });
        diagram.shapes.forEach((shape: IHierarchyElement) => {
            shape.isShape = true;
            diagramElements.push(shape);
            lookup[shape.id] = shape;
        });
        // Sort elements by their z-index
        let elements = orderByZindex ? diagramElements.sort(HierarchyHelper.sortByZIndex) : diagramElements;
        elements.forEach((element: IHierarchyElement) => {
            element.children = [];
            (<IHierarchyDiagram>diagram).children.push(element);
            if (element.parentId != null) {
                let key = element.parentId.toString();
                if (lookup.hasOwnProperty(key)) {
                    element.parent = lookup[key];
                }
            }
        });
        // return sorted results
        return <IHierarchyDiagram>diagram;
    }

    public static sortByZIndex(element1: IDiagramElement, element2: IDiagramElement) {
        // Check if null or undefined values are assigned to the element's z-index
        if (element1.zIndex === null) {
            element1.zIndex = 0;
        }
        if (element2.zIndex === null) {
            element2.zIndex = 0;
        }
        // If the z-indices are not the same, return the difference
        if (element1.zIndex !== element2.zIndex) {
            return element1.zIndex - element2.zIndex;
        }
        // If the z-indices are the same and the elements are not the same type, increament the connector z-index by 1
        if ((element1.zIndex === element2.zIndex) && (element1.isShape !== element2.isShape)) {
            if (!element1.isShape) {
                element1.zIndex += 1;
            }
            if (!element2.isShape) {
                element2.zIndex += 1;
            }
        }
        return element1.zIndex - element2.zIndex;
    }
}

export class MathExtensions {

    public static closestConnectionPoint(relativePoint: MxPoint, points: Array<MxPoint>): MxPoint {
        let num1 = Number.MAX_VALUE;
        let point = points[0];
        points.forEach((rhs: MxPoint) => {
            let num2 = MathExtensions.distance(relativePoint, rhs);
            if (num1 > num2) {
                num1 = num2;
                point = rhs;
            }
        });
        return point;
    }

    public static toRelativePoint(origin: MxRectangle, absolutePoint: IPoint): MxPoint {
        return MxFactory.point((absolutePoint.x - origin.x) / origin.width, (absolutePoint.y - origin.y) / origin.height);
    }

    public static distance(lhs: MxPoint, rhs: MxPoint): number {
        let num1 = lhs.x - rhs.x;
        let num2 = lhs.y - rhs.y;
        return Math.sqrt(num1 * num1 + num2 * num2);
    }

    public static lerp(pointA: MxPoint, pointB: MxPoint, alpha: number): MxPoint {
        return MxFactory.point(MathExtensions.linearInterpolation(pointA.x, pointB.x, alpha),
            MathExtensions.linearInterpolation(pointA.y, pointB.y, alpha));
    }

    public static linearInterpolation(x: number, y: number, alpha: number): number {
        return x * (1.0 - alpha) + y * alpha;
    }

    /**
     * Calculates the difference between two points.
     */
    public static subtract(lhs: MxPoint, rhs: MxPoint): MxPoint {
        return MxFactory.point(lhs.x - rhs.x, lhs.y - rhs.y);
    }

    /**
     * Converts the angle between two vectors, represented by simple points.
     */
    public static getAngleBetween(lhs: MxPoint, rhs: MxPoint) {
        let y: number = (lhs.x * rhs.y) - (rhs.x * lhs.y);
        let x: number = (lhs.x * rhs.x) + (lhs.y * rhs.y);

        return MathExtensions.toPositiveDegree(Math.atan2(y, x));
    }

    /**
     * Converts an angle in radian to an angle in degree.
     */
    public static toPositiveDegree(rad: number): number {
        let degree: number = rad * 180 / Math.PI;

        if (degree < 0) {
            degree += 360;
        }

        return degree;
    }
}

export class Color {

    public a: number;

    constructor(public r, public g, public b, a?: number) {
        this.a = a ? a : 1;
    }

    public toRgb() {
        return "rgb(" + this.r + "," + this.g + "," + this.b + ")";
    }

    public toRgba() {
        return "rgba(" + this.r + "," + this.g + "," + this.b + "," + this.a + ")";
    }

    /* tslint:disable */
    public static parseHex(hex: string): Color {
        if (hex != null) {
            hex = hex.replace("#", "");
            let bigint = parseInt(hex, 16);
            let r = (bigint >> 16) & 255;
            let g = (bigint >> 8) & 255;
            let b = bigint & 255;
            return new Color(r, g, b);
        }
        return new Color(0, 0, 0);
    }

    /* tslint:enable */

    public static isTransparent(color: string) {
        if (color != null && color.toLowerCase() === "transparent") {
            return true;
        }
        return false;
    }
}

export class ShapeExtensions {
    public static getPropertyByName(el: IDiagramElement, propertyName: string): any {
        for (let i = 0; i < el.props.length; i++) {
            let prop = el.props[i];
            if (prop.name === propertyName) {
                return prop.value;
            }
        }
        return null;
    }
}

export class DiagramHelper {
    public static findValueByName(items: IProp[], key: string): string {
        for (let i = 0; i < items.length; i++) {
            if (items[i].name === key) {
                return items[i] && items[i].value;
            }
        }
    }

    public static drawArcSegment(path, end: MxPoint, radius: number): void {
        path.arcTo(radius, radius, 0, 0, 1, end.x, end.y);
    }

    //server return inconsistent time format so we have to add
    //logics to make it consistent, the format returned from server can be like
    //2015-07-27T06:00:00
    //2015-07-27T15:44:39.9733204-04:00
    public static normalizeDateFormat(dateTimeString: string): Date {
        let finalDateTimeString = "";

        if (dateTimeString) {
            if (dateTimeString.length < 20) {

                let timeZoneOffSet = new Date().getTimezoneOffset(); //240
                let timeZone = timeZoneOffSet / (-60); // -4
                let absTimeZone = Math.abs(timeZone); // 4
                let absTimeZoneString = "" + absTimeZone;
                if (absTimeZoneString.length < 2) {
                    absTimeZoneString = "0" + absTimeZoneString;
                }

                if (timeZone < 0) {
                    finalDateTimeString = dateTimeString + "-" + absTimeZoneString + ":00";
                } else {
                    finalDateTimeString = dateTimeString + "+" + absTimeZoneString + ":00";
                }


            } else {
                finalDateTimeString = dateTimeString;
            }
        }


        let dateTimeValue = new Date(finalDateTimeString);
        if (dateTimeValue.toString() === "Invalid Date") {
            dateTimeValue = new Date(dateTimeString);
        }
        return dateTimeValue;
    }

    public static formatDateTime(dateTimeValue: Date, displayFormat: string, inputMode: string): string {
        let formatDate: (date: Date, format: string) => string;

        try {
            //fixme: this is not the right way to use an angular date filter. you shoud use $filter(filtername)(data)
            formatDate = <(date: Date, format: string) => string>angular.injector(["ng"]).get("$filter")("date");

        } catch (e) {
            //fixme: catch must not be empty on try catch as this is a costly operation and must ALWYS return something
        }

        let rtn = "";

        if (formatDate) {
            if (displayFormat === "Long") {
                if (inputMode === "DatePicker") {
                    //Tuesday, July 21, 2015
                    rtn = formatDate(dateTimeValue, "fullDate");
                } else if (inputMode === "TimePicker") {
                    //1:29:04pm
                    rtn = formatDate(dateTimeValue, "mediumTime");

                } else {
                    //Tuesday, July 21, 2015 1:29:04pm
                    rtn = formatDate(dateTimeValue, "EEEE, MMMM d, y h:mm:ss a");
                }

            } else {

                if (inputMode === "DatePicker") {
                    // 7/21/2015
                    rtn = formatDate(dateTimeValue, "M/d/yyyy");

                } else if (inputMode === "TimePicker") {
                    // 1:29 PM
                    rtn = formatDate(dateTimeValue, "shortTime");
                } else {
                    //7/21/2015 1:29 PM
                    rtn = formatDate(dateTimeValue, "M/d/yyyy h:mm a");

                }
            }
        }
        return rtn;
    }
}
;
