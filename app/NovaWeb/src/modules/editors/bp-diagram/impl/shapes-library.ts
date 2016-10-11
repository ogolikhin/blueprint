//fixme: this file is js not TS. needs to be re-written properly
/* tslint:disable */
import {Shapes, ShapeProps, ArrowHeads, UIMockupShapes} from "./utils/constants";
import {MxFactory, MathExtensions, DiagramHelper} from "./utils/helpers";
import {HttpStatusCode} from "../../../core/http";

/*
 * Class: CalloutHelper
 *
 * Holds helpers methods to draw callout
 */
class CalloutHelper {
    private static connectionPoints: MxPoint[] = [
        MxFactory.point(0.25, 0.0),
        MxFactory.point(0.75, 0.0),
        MxFactory.point(0.25, 1.0),
        MxFactory.point(0.75, 1.0),
        MxFactory.point(0.0, 0.25),
        MxFactory.point(0.0, 0.75),
        MxFactory.point(1.0, 0.25),
        MxFactory.point(1.0, 0.75)
    ];

    public static closestConnectionPoint(relativePoint: MxPoint): MxPoint {
        return MathExtensions.closestConnectionPoint(relativePoint, CalloutHelper.connectionPoints);
    }

    public static isInside(point: MxPoint): boolean {
        if (Math.abs(point.x - 0.5) <= 0.5) {
            return Math.abs(point.y - 0.5) <= 0.5;
        }
        return false;
    }

    public static computeCorners(w: number, h: number, radius: number): Array<MxPoint> {
        let points: Array<MxPoint> = [
            MxFactory.point(0, radius),
            MxFactory.point(radius, 0),
            MxFactory.point(w - radius, 0),
            MxFactory.point(w, radius),
            MxFactory.point(w, h - radius),
            MxFactory.point(w - radius, h),
            MxFactory.point(radius, h),
            MxFactory.point(0, h - radius)
        ];
        return points;
    }
}

/*
 * Class: CalloutShape
 *
 * Implements rounded callout.
 * This shape is registered under CalloutShape.NAME in mxCellRenderer.
 */
export class CalloutShape extends mxActor {

    public static get getName(): string {
        return Shapes.CALLOUT;
    }

    public redrawPath(path, x: number, y: number, w: number, h: number) {

        let anchorRelativePoint = MxFactory.point(mxUtils.getValue(this.style, "x", 0), mxUtils.getValue(this.style, "y", 0));

        let connectionPoint = CalloutHelper.closestConnectionPoint(anchorRelativePoint);
        let isInside = CalloutHelper.isInside(anchorRelativePoint);
        let anchorPoint = MxFactory.point(anchorRelativePoint.x * w, anchorRelativePoint.y * h);

        let radius = Math.min(w, h) / 10.0;
        let corners = CalloutHelper.computeCorners(w, h, radius);
        path.begin();
        path.moveTo(corners[0].x, corners[0].y);
        DiagramHelper.drawArcSegment(path, corners[1], radius);
        this.drawSegment(path, corners[1], corners[2], anchorPoint, connectionPoint.x, !isInside && connectionPoint.y === 0.0);
        DiagramHelper.drawArcSegment(path, corners[3], radius);
        this.drawSegment(path, corners[3], corners[4], anchorPoint, connectionPoint.y, !isInside && connectionPoint.x === 1.0);
        DiagramHelper.drawArcSegment(path, corners[5], radius);
        this.drawSegment(path, corners[5], corners[6], anchorPoint, 1.0 - connectionPoint.x, !isInside && connectionPoint.y === 1.0);
        DiagramHelper.drawArcSegment(path, corners[7], radius);
        this.drawSegment(path, corners[7], corners[0], anchorPoint, 1.0 - connectionPoint.y, !isInside && connectionPoint.x === 0.0);
        path.fillAndStroke();
    }

    private drawSegment(path, start: MxPoint, end: MxPoint, anchor: MxPoint, connection: number, connectToAnchor: boolean): void {
        if (connectToAnchor) {
            let p1 = MathExtensions.lerp(start, end, connection - 0.1);
            path.lineTo(p1.x, p1.y);
            path.lineTo(anchor.x, anchor.y);
            let p2 = MathExtensions.lerp(start, end, connection + 0.1);
            path.lineTo(p2.x, p2.y);
        }
        path.lineTo(end.x, end.y);
    }
}

/*
 * Class: ImageShape
 *
 * Overrides mxImageShape to implement an image shape with custom fallback background.
 * This shape is registered under mxConstants.SHAPE_IMAGE in mxCellRenderer.
 */
export class ImageShape extends mxImageShape {

    public paintVertexShape(c: any, x: number, y: number, w: number, h: number) {

        if (this.image != null) {
            // FlipH/V are implicit via mxShape.updateTransform
            c.image(x, y, w, h, this.image, this.preserveImageAspect, false, false);
            const fill = mxUtils.getValue(this.style, mxConstants.STYLE_IMAGE_BACKGROUND, null);
            const stroke = mxUtils.getValue(this.style, mxConstants.STYLE_IMAGE_BORDER, null);
            const dashed = mxUtils.getValue(this.style, mxConstants.STYLE_DASHED, null);
            const dashPattern = mxUtils.getValue(this.style, mxConstants.STYLE_DASH_PATTERN, null);

            if (fill != null || stroke != null) {
                c.setFillColor(fill);
                c.setStrokeColor(stroke);
                if (dashed) {
                    c.setDashPattern(dashPattern);
                }
                c.rect(x, y, w, h);
                c.fillAndStroke();
            }
        } else {
            ImageShape.drawFallbackImage(c, MxFactory.rectangle(x, y, w, h));
            c.fillAndStroke();
        }
    }

    public static drawFallbackImage(c: MxAbstractCanvas2D, rect: MxRectangle) {
        c.begin();
        c.setDashed(false);
        c.setFillColor("#FFFFFF");
        c.setStrokeColor("#BEBEBE");
        c.setStrokeWidth(Math.max(1, rect.width * 0.006));
        c.moveTo(rect.x, rect.y);
        c.lineTo(rect.x, rect.y + rect.height);
        c.lineTo(rect.x + rect.width, rect.y + rect.height);
        c.lineTo(rect.x + rect.width, rect.y);
        c.lineTo(rect.x, rect.y);
        c.lineTo(rect.x + rect.width, rect.y + rect.height);
        c.moveTo(rect.x + rect.width, rect.y);
        c.lineTo(rect.x, rect.y + rect.height);
    }
}

/*
 * Class: ImageHelper
 *
 * Holds helpers methods to draw image shape
 */
class ImageHelper {
    public static initFallback() {
        let createElement = mxSvgCanvas2D.prototype.createElement;
        //noinspection TsLint
        mxSvgCanvas2D.prototype.createElement = function (tagName: string, namespace: string) {
            let mxSvgCanvas = this;
            let element = createElement.call(mxSvgCanvas, tagName, namespace);
            if (tagName === "image") {
                //workaround for IE
                element.setAttribute("onerror", "this.onerror();");
                //noinspection TsLint
                element.onerror = function () {
                    let x = parseFloat(this.getAttribute("x"));
                    let y = parseFloat(this.getAttribute("y"));
                    let w = parseFloat(this.getAttribute("width"));
                    let h = parseFloat(this.getAttribute("height"));
                    let parent: SVGElement = this.parentNode;
                    let path = createElement.call(mxSvgCanvas, "path", null);

                    // ReSharper disable once InconsistentNaming
                    let c: MxAbstractCanvas2D = new mxAbstractCanvas2D();
                    ImageShape.drawFallbackImage(c, MxFactory.rectangle(x, y, w, h));

                    path.setAttribute("d", c.path.join(" "));
                    path.setAttribute("fill", c.state.fillColor);
                    path.setAttribute("stroke", c.state.strokeColor);
                    path.setAttribute("stroke-width", c.state.strokeWidth);
                    path.setAttribute("stroke-miterlimit", c.state.miterLimit);
                    if (parent != null) {
                        parent.appendChild(path);
                    }
                    element.style.display = "none";
                    //unsibscribe
                    element.onerror = null;
                };
            }
            return element;
        };
    }
}

/*
 * Class: ExternalFlowIndicatorShape
 *
 * Overrides mxActor to implement an indicator of external flow.
 * This shape is registered under Shapes.EXTERNAL_FLOW_INDICATOR in mxCellRenderer.
 */
export class ExternalFlowIndicatorShape extends mxActor {

    public static get getName(): string {
        return Shapes.EXTERNAL_FLOW_INDICATOR;
    }

    public paintVertexShape(c: any, x: number, y: number, w: number, h: number) {
        c.begin();
        c.moveTo(x + w / 2, y);
        c.lineTo(x + w / 2, y + h);
        c.moveTo(x, y + h);
        c.lineTo(x, y + h / 2);
        c.lineTo(x + w, y + h / 2);
        c.lineTo(x + w, y + h);
        c.stroke();
    }
}

/*
 * Class: SvgImageShape
 *
 * Extends mxActor to draw a vector image.
 */
export class SvgImageShape extends mxActor {

    public static get getName(): string {
        return "svgimage";
    }

    public paintVertexShape(canvas: any, x: number, y: number, w: number, h: number) {
        let svgPath = mxUtils.getValue(this.style, ShapeProps.PATH, null);
        if (svgPath != null) {
            canvas.begin();
            canvas.path = svgPath.split(" ");
            canvas.fillAndStroke();

            let nbbox = canvas.node.getBBox();
            let sX = (w / nbbox.width) * this.scale;
            let sY = (h / nbbox.height) * this.scale;

            let tX = x * this.scale - nbbox.x * sX;
            let tY = y * this.scale - nbbox.y * sY;

            if (isFinite(sX) && isFinite(sY)) {
                canvas.node.setAttribute("transform", "translate(" + tX + ", " + tY + ") scale(" + sX + ", " + sY + ")");
            }

            let shadow = mxUtils.getValue(this.style, mxConstants.STYLE_SHADOW, 0);
            if (shadow) {
                let n = <SVGElement>canvas.node;
                let shadowNode = n.previousSibling;
                if (shadowNode != null) {

                    let shadowDx = canvas.state.shadowDx * canvas.state.scale;
                    let shadowDy = canvas.state.shadowDy * canvas.state.scale;

                    if (isFinite(sX) && isFinite(sY)) {
                        let attr = "translate(" + (tX + shadowDx) + ", " + (tY + shadowDy) + ") scale(" + sX + ", " + sY + ")";
                        (<any>shadowNode).setAttribute("transform", attr);
                    }
                }
            }
        }
    }

    public static hasClosePathOp(path: string) {
        return path != null && path.length > 0 && path[path.length - 1] !== "Z";
    }
}

export class IconShape extends mxActor {

    public static shapeName = "Icon";

    public static iconData;

    public drawIcon(canvas: any, xContainer: number, yContainer: number, wContainer: number, hContainer: number) {
        let iconKey = this.style.IconKey;
        if (iconKey === "undefined") {
            return;
        }

        canvas.begin();
        let iconData = IconShape.iconData[iconKey];
        canvas.node.setAttribute("fill-rule", iconData.fillRule);
        canvas.path = [iconData.data];
        canvas.fillAndStroke();
        //canvas.fill();
        //canvas.stroke();

        let originalBoundingBox = canvas.node.getBBox();
        //
        let scaleX, scaleY, translateX, translateY;

        scaleX = (wContainer / originalBoundingBox.width) * this.scale;
        scaleY = (hContainer / originalBoundingBox.height) * this.scale;

        //try to use the smaller scale as common scale
        //so that the shape aspect is respected
        if (scaleX < scaleY) {
            scaleY = scaleX;
            translateX = xContainer * this.scale - originalBoundingBox.x * scaleX;
            //adjust the offset by (hContainer - originalBoundingBox.height * scaleY) / 2
            //this is because the we use the smaller scaleX, so the translateY need to be move down a little more
            translateY = yContainer * this.scale - originalBoundingBox.y * scaleY + (hContainer * this.scale - originalBoundingBox.height * scaleY) / 2;
        } else {
            scaleX = scaleY;
            translateY = yContainer * this.scale - originalBoundingBox.y * scaleY;
            translateX = xContainer * this.scale - originalBoundingBox.x * scaleX + (wContainer * this.scale - originalBoundingBox.width * scaleX) / 2;
        }

        let state = canvas.state;
        //adjustedStokeRatio is really a magic number, silveright's stroke width with path
        // is not compatible with svg, tweak it if it is necessary
        let adjustedStokeRatio = 20;
        canvas.node.setAttribute("stroke-width", state.strokeWidth / adjustedStokeRatio);

        let strokeDashArray = canvas.node.getAttribute("stroke-dasharray");
        if (strokeDashArray) {
            let newStrockDashArray = [];
            strokeDashArray.split(" ").forEach( (value) => {
                newStrockDashArray.push(value / adjustedStokeRatio);
            });
            canvas.node.setAttribute("stroke-dasharray", newStrockDashArray.join(" "));
        }
        if (isFinite(scaleX) && isFinite(scaleY)) {
            canvas.node.setAttribute("transform", "translate(" + translateX + ", " + translateY + ") scale(" + scaleX + ", " + scaleY + ")");
        }

        let shadow = mxUtils.getValue(this.style, mxConstants.STYLE_SHADOW, 0);
        let stroke = mxUtils.getValue(this.style, mxConstants.STYLE_STROKEWIDTH, 0);
        if (shadow) {
            let n = <SVGElement>canvas.node;
            let shadowNode = n.previousSibling;

            if (shadowNode != null) {
                if (stroke) {
                    (<any>shadowNode).setAttribute("stroke-width", 0);
                }
                let shadowDx = canvas.state.shadowDx * canvas.state.scale;
                let shadowDy = canvas.state.shadowDy * canvas.state.scale;

                if (isFinite(scaleX) && isFinite(scaleY)) {
                    let attr = "translate(" + (translateX + shadowDx) + ", " + (translateY + shadowDy) + ") scale(" + scaleX + ", " + scaleY + ")";
                    (<any>shadowNode).setAttribute("transform", attr);
                }
            }
        }
    }

    public paintVertexShape(canvas: any, x: number, y: number, w: number, h: number) {
        if (!IconShape.iconData) {
            const xmlhttp = new XMLHttpRequest();
            const url = mxBasePath + "/icons/main.json";

            xmlhttp.onreadystatechange =  () => {
                if (xmlhttp.readyState === 4 && xmlhttp.status === HttpStatusCode.Success) {
                    IconShape.iconData = JSON.parse(xmlhttp.responseText);
                }
            };
            xmlhttp.open("GET", url, false);
            xmlhttp.send();
        }
        this.drawIcon(canvas, x, y, w, h);
    }
}

export class CheckboxShape extends mxActor {

    public static get getName(): string {
        return UIMockupShapes.CHECKBOX;
    }

    public redrawPath(path, x: number, y: number, w: number, h: number) {
        path.begin();
        path.moveTo(1, 5);
        path.lineTo(4, 7);
        path.lineTo(8, 2);
        path.fillAndStroke();
    }
}

export class TableCursorShape extends mxActor {

    public static get getName(): string {
        return UIMockupShapes.TABLE;
    }

    public redrawPath(path, x: number, y: number, w: number, h: number) {
        path.begin();
        path.moveTo(14, 10);
        path.lineTo(19, 14);
        path.lineTo(14, 18);
        path.stroke();
    }
}

export class HighlightEllipse extends mxActor {

    public static get getName(): string {
        return "highlightEllipse";
    }

    public redrawPath(path, x: number, y: number, w: number, h: number) {
        let radius = Math.min(w, h) < 100 ? Math.min(w, h) / 10.0 : 5;
        let corners = CalloutHelper.computeCorners(w, h, radius);
        path.begin();
        path.moveTo(corners[0].x, corners[0].y);
        DiagramHelper.drawArcSegment(path, corners[1], radius);
        path.lineTo(corners[2].x, corners[2].y);
        DiagramHelper.drawArcSegment(path, corners[3], radius);
        path.lineTo(corners[4].x, corners[4].y);
        DiagramHelper.drawArcSegment(path, corners[5], radius);
        path.lineTo(corners[6].x, corners[6].y);
        DiagramHelper.drawArcSegment(path, corners[7], radius);
        path.lineTo(corners[0].x, corners[0].y);
        path.fillAndStroke();
    }
}

/*
 * Class: Connector
 *
 * Overrides mxConnector to implement custom behaviour for curved connectors.
 * This shape is registered under mxConstants.SHAPE_CONNECTOR in mxCellRenderer.
 */
export class Connector extends mxConnector {

    public paintCurvedLine(c, pts: Array<MxPoint>) {

        if (pts.length >= 4) {
            c.begin();

            let pt = pts[0];
            c.moveTo(pt.x, pt.y);

            let cp1 = pts[1];
            let cp2 = pts[2];
            let endPoint = pts[3];

            c.curveTo(cp1.x, cp1.y, cp2.x, cp2.y, endPoint.x, endPoint.y);
            c.stroke();
        }
    }
}

/*
 * Class: MarkerHelper
 *
 * Implements custom markers for the connections and regester them
 */
class MarkerHelper {

    public static drawSlash(canvas, shape, type, pe: MxPoint, unitX: number, unitY: number, size: number, source, sw, filled: boolean) {
        // The angle of the forward facing arrow sides against the x axis is
        // 26.565 degrees, 1/sin(26.565) = 2.236 / 2 = 1.118 ( / 2 allows for
        // only half the strokewidth is processed ).
        let endOffsetX = unitX * sw * 2.236;
        let endOffsetY = unitY * sw * 2.236;

        unitX = unitX * (size + sw);
        unitY = unitY * (size + sw);

        let pt = pe.clone();
        pt.x += endOffsetX;
        pt.y += endOffsetY;

        return () => {
            canvas.begin();
            canvas.moveTo(pt.x - unitX - unitY / 1.5, pt.y - unitY + unitX / 1.5);
            canvas.lineTo(pt.x - unitX * 1.5 + unitY * 0.75, pt.y - unitY / 0.75 - unitX / 1.5);
            canvas.stroke();
        };
    }

    public static drawOval(canvas, shape, type, pe: MxPoint, unitX: number, unitY: number, size: number, source, sw, filled: boolean) {
        let a = size / 2;

        let pt = pe.clone();
        pe.x -= unitX * size;
        pe.y -= unitY * size;

        return () => {
            canvas.ellipse(pt.x - a - unitX * a, pt.y - a - unitY * a, size, size);

            if (filled) {
                canvas.fillAndStroke();
            } else {
                canvas.stroke();
            }
        };
    }
}

mxCellRenderer.registerShape(mxConstants.SHAPE_IMAGE, ImageShape);
mxCellRenderer.registerShape(IconShape.shapeName, IconShape);
mxCellRenderer.registerShape(CalloutShape.getName, CalloutShape);
mxCellRenderer.registerShape(mxConstants.SHAPE_CONNECTOR, Connector);
mxCellRenderer.registerShape(CheckboxShape.getName, CheckboxShape);
mxCellRenderer.registerShape(HighlightEllipse.getName, HighlightEllipse);
mxCellRenderer.registerShape(SvgImageShape.getName, SvgImageShape);
mxCellRenderer.registerShape(TableCursorShape.getName, TableCursorShape);
mxCellRenderer.registerShape(ExternalFlowIndicatorShape.getName, ExternalFlowIndicatorShape);

mxMarker.addMarker(ArrowHeads.SLASH, MarkerHelper.drawSlash);
mxMarker.addMarker(mxConstants.ARROW_OVAL, MarkerHelper.drawOval);

ImageHelper.initFallback();
