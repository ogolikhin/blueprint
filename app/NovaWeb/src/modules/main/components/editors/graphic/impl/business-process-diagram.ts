import {IShape} from "../impl/models";
import {Shapes} from "./utils/constants";
import {Style} from "./utils/style-builder";
import {MxFactory, ShapeExtensions} from "./utils/helpers";
import {AbstractShapeFactory, IShapeTemplates} from "./abstract-diagram-factory";

export class BusinessProcessShapeFactory extends AbstractShapeFactory {
    public initTemplates(templates: IShapeTemplates) {
        templates[Shapes.POOL] = this.pool;
        templates[Shapes.LANE] = this.lane;
        templates[Shapes.MESSAGE] = this.message;
        templates[Shapes.DATA_OBJECT] = this.dataObject;
        templates[Shapes.DATA_STORE] = this.dataStore;
        templates[Shapes.GATEWAY] = this.gateway;
        templates[Shapes.TASK] = this.taskOrSubprocess;
        templates[Shapes.EXPANDED_SUBPROCESS] = this.taskOrSubprocess;
        templates[Shapes.EVENT] = this.event;
        templates[Shapes.TEXT_ANNOTATION] = this.annotationShape;
        templates[Shapes.GROUP_SHAPE] = this.groupShape;

        return templates;
    }

    private createSwimlaneDefaultStyle(styleBuilder?: Style): Style {
        if (styleBuilder === null) {
            styleBuilder = new Style();
        }
        styleBuilder[mxConstants.STYLE_HORIZONTAL] = 0;
        styleBuilder[mxConstants.STYLE_STARTSIZE] = 55;
        styleBuilder[mxConstants.STYLE_FOLDABLE] = 0;
        styleBuilder[mxConstants.STYLE_SWIMLANE_FILLCOLOR] = mxConstants.NONE;
        styleBuilder[mxConstants.STYLE_FOLDABLE] = 0;
        return styleBuilder;
    }

    private pool = (shape: IShape): MxCell => {
        var style = this.createSwimlaneDefaultStyle(this.styleBuilder.createDefaultShapeStyle(shape, "swimlane"));
        var pool = super.createDefaultVertex(shape, style);
        return pool;
    };

    private lane = (shape: IShape): MxCell => {
        var style = this.createSwimlaneDefaultStyle(this.styleBuilder.createDefaultShapeStyle(shape, "swimlane"));
        var lane = super.createDefaultVertex(shape, style);
        return lane;
    };

    private message = (shape: IShape): MxCell => {
        var style = this.styleBuilder.createDefaultShapeStyle(shape, "message");
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_VERTICAL_LABEL_POSITION] = mxConstants.ALIGN_BOTTOM;
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;
        var message = super.createDefaultVertex(shape, style);
        var isInitiating = ShapeExtensions.getPropertyByName(shape, "IsInitiating");
        if (!isInitiating) {
            var cell = MxFactory.cell(shape, MxFactory.geometry(0, 0, shape.width, shape.height), "fillColor=gray;opacity=20;selectable=0");
            cell.vertex = true;
            cell.geometry.relative = true;
            message.insert(cell);
        }
        return message;
    };

    private dataObject = (shape: IShape): MxCell => {
        var style = this.styleBuilder.createDefaultShapeStyle(shape, "dataobject");
        style[mxConstants.STYLE_PERIMETER] = mxConstants.PERIMETER_RECTANGLE;
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_VERTICAL_LABEL_POSITION] = mxConstants.ALIGN_BOTTOM;
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;
        var dataObject = super.createDefaultVertex(shape, style);
        var isCollection = ShapeExtensions.getPropertyByName(shape, "IsCollection");
        if (isCollection) {
            var markerWidth = shape.width * 0.15;
            var markerHeight = shape.height * 0.2;
            var parallelMarker = MxFactory.cell("", MxFactory.geometry(0.5, 1, markerWidth, markerHeight), "shape=parallelmarker;selectable=0");
            parallelMarker.vertex = true;
            parallelMarker.geometry.relative = true;
            parallelMarker.geometry.offset = MxFactory.point(-markerWidth / 2, -markerHeight);
            dataObject.insert(parallelMarker);
        }
        return dataObject;
    };

    private dataStore = (shape: IShape): MxCell => {
        var style = this.styleBuilder.createDefaultShapeStyle(shape, "datastore");
        var dataStore = super.createDefaultVertex(shape, style);
        return dataStore;
    };

    private gateway = (shape: IShape): MxCell => {
        var style = this.styleBuilder.createDefaultShapeStyle(shape, "gateway");
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_VERTICAL_LABEL_POSITION] = mxConstants.ALIGN_BOTTOM;
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;
        var gateway = super.createDefaultVertex(shape, style);
        var gatewayType = ShapeExtensions.getPropertyByName(shape, "GatewayType");

        if (gatewayType !== "ExclusiveData") {
            var markerWidth = shape.width * 0.6;
            var markerHeight = shape.height * 0.6;
            var markerStrokeWidth = Math.max(1, Math.max(shape.width, shape.height) * 0.015);
            var styleBuilder = new Style();
            styleBuilder["selectable"] = 0;
            styleBuilder[mxConstants.STYLE_STROKEWIDTH] = markerStrokeWidth;
            styleBuilder[mxConstants.STYLE_FILLCOLOR] = "black";
            switch (gatewayType) {
                case "ExclusiveDataWithMarker":
                    markerWidth = shape.width * 0.4;
                    markerHeight = shape.height * 0.5;
                    styleBuilder[mxConstants.STYLE_SHAPE] = "gatewayexclusivedatawithmarker";
                    break;
                case "ExclusiveEvent":
                    styleBuilder[mxConstants.STYLE_SHAPE] = "gatewayexclusiveevent";
                    styleBuilder[mxConstants.STYLE_FILLCOLOR] = "inherit";
                    break;
                case "ExclusiveEventInstantiate":
                    styleBuilder[mxConstants.STYLE_SHAPE] = "gatewayexclusiveeventinstantiate";
                    styleBuilder[mxConstants.STYLE_FILLCOLOR] = "inherit";
                    break;
                case "ParallelEventInstantiate":
                    styleBuilder[mxConstants.STYLE_SHAPE] = "gatewayparelleleventinstantiate";
                    styleBuilder[mxConstants.STYLE_FILLCOLOR] = "inherit";
                    break;
                case "Inclusive":
                    styleBuilder[mxConstants.STYLE_SHAPE] = "gatewayinclusive";
                    break;
                case "Parallel":
                    styleBuilder[mxConstants.STYLE_SHAPE] = "gatewayparallel";
                    break;
                case "Complex":
                    styleBuilder[mxConstants.STYLE_SHAPE] = "gatewaycomplex";
                    break;
                default:
                    break;
            }
            var marker = MxFactory.cell("", MxFactory.geometry(0, 0, markerWidth, markerHeight));
            marker.vertex = true;
            marker.geometry.relative = true;
            marker.geometry.offset = MxFactory.point((shape.width - markerWidth) / 2, (shape.height - markerHeight) / 2);
            gateway.insert(marker);
            marker.setStyle(styleBuilder.convertToString());
        }
        return gateway;
    };

    private annotationShape = (shape: IShape): MxCell => {
        var style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_ROUNDED] = 0;
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_MIDDLE;
        var annotationShape = super.createDefaultVertex(shape, style);
        return annotationShape;
    };

    private taskOrSubprocess = (shape: IShape): MxCell => {
        var loopType = ShapeExtensions.getPropertyByName(shape, "LoopType");
        var boundaryType = ShapeExtensions.getPropertyByName(shape, "BoundaryType");
        var taskType = ShapeExtensions.getPropertyByName(shape, "TaskType");
        var isAdHoc = ShapeExtensions.getPropertyByName(shape, "IsAdHoc");
        var isCompensation = ShapeExtensions.getPropertyByName(shape, "IsCompensation");
        var isCollapsed = ShapeExtensions.getPropertyByName(shape, "IsCollapsed");

        var markerSize = 13;

        var isCompensationMarker = null;
        var isCompensationStyle = new Style();
        var isCompensationMarkerHorizontalOffset = 0;
        var isCompensationMarkerVerticalOffset = shape.height - 25;
        var isCompensationMarkerHorizontalLeftShift = 0;

        var isAdhocMarker = null;
        var isAdhocStyle = new Style();
        var isAdhocMarkerHorizontalOffset = 0;
        var isAdhocMarkerVerticalOffset = shape.height - 25;
        var isAdHocMarkerHorizontalLeftShift = 0;

        var isCollapsedMarker = null;
        var isCollapsedStyle = new Style();
        var isCollapsedMarkerHorizontalOffset = 0;
        var isCollapsedMarkerVerticalOffset = shape.height - 25;
        var isCollapsedMarkerHorizontalLeftShift = 0;

        var tasktypeMarker = null;
        var taskTypeStyle = new Style();

        var loopMarker = null;
        var loopStyle = new Style();
        var loopMarkerHorizontalOffset = (shape.width - markerSize / 2) / 2;
        var loopMarkerVerticalOffset = shape.height - 25;
        var loopMarkerHorizontalLeftShift = 0;

        var style = this.styleBuilder.createDefaultShapeStyle(shape, "rectangle");
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_ROUNDED] = 1;
        style[mxConstants.STYLE_ARCSIZE] = 2;

        if (shape.type === "ExpandedSubProcess") {
            style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;
        }

        var taskOrSubprocess = super.createDefaultVertex(shape, style);

        if (boundaryType != null && boundaryType !== "Default") {
            var boundaryWidth = shape.width - 10;
            var boundaryHeight = shape.height - 10;
            var boundaryStyle = new Style();
            boundaryStyle[mxConstants.STYLE_SHAPE] = "taskrect";
            boundaryStyle["selectable"] = 0;
            boundaryStyle[mxConstants.STYLE_STROKECOLOR] = "inherit";
            boundaryStyle[mxConstants.STYLE_FILLCOLOR] = "inherit";
            var boundary = MxFactory.cell("", MxFactory.geometry(0, 0, boundaryWidth, boundaryHeight));
            switch (boundaryType) {
                case "Call":
                    boundaryStyle[mxConstants.STYLE_STROKEWIDTH] = "3";
                    break;
                case "Event":
                    boundaryStyle[mxConstants.STYLE_STROKEWIDTH] = "1";
                    boundaryStyle[mxConstants.STYLE_DASHED] = "1";
                    break;
                case "Transaction":
                    boundaryStyle[mxConstants.STYLE_STROKEWIDTH] = "1";
                    break;
                default:
                    break;
            }

            this.setMarkerStyleAndInsert(boundary, taskOrSubprocess, (shape.width - boundaryWidth) / 2, (shape.height - boundaryHeight) / 2, boundaryStyle);

        }

        if (taskType != null && taskType !== "None" && !isCollapsed && shape.height > 28) {
            taskTypeStyle["selectable"] = 0;
            taskTypeStyle[mxConstants.STYLE_FILLCOLOR] = "inherit";
            taskTypeStyle[mxConstants.STYLE_STROKECOLOR] = "inherit";
            switch (taskType) {
                case "Service":
                    taskTypeStyle[mxConstants.STYLE_SHAPE] = "taskservice";
                    break;
                case "Receive":
                    taskTypeStyle[mxConstants.STYLE_SHAPE] = "taskreceive";
                    break;
                case "Send":
                    taskTypeStyle[mxConstants.STYLE_SHAPE] = "tasksend";
                    break;
                case "InstantiatingReceive":
                    taskTypeStyle[mxConstants.STYLE_SHAPE] = "taskinstantiatingreceive";
                    break;
                case "Manual":
                    taskTypeStyle[mxConstants.STYLE_SHAPE] = "taskmanual";
                    break;
                case "BusinessRule":
                    taskTypeStyle[mxConstants.STYLE_SHAPE] = "taskbusinessrule";
                    break;
                case "User":
                    taskTypeStyle[mxConstants.STYLE_SHAPE] = "taskuser";
                    break;
                case "Script":
                    taskTypeStyle[mxConstants.STYLE_SHAPE] = "taskscript";
                    break;
                default:
                    break;
            }
            tasktypeMarker = MxFactory.cell("", MxFactory.geometry(0, 0, 15, 15));

            this.setMarkerStyleAndInsert(tasktypeMarker, taskOrSubprocess, 3, 3, taskTypeStyle);
        }

        if (isCompensation != null && isCompensation && shape.height > 28) {
            loopMarkerHorizontalLeftShift++;
            isCollapsedMarkerHorizontalLeftShift--;
            isAdHocMarkerHorizontalLeftShift--;

            isCompensationStyle[mxConstants.STYLE_SHAPE] = "eventcompensation";
            isCompensationStyle["selectable"] = 0;
            if (shape.type === "ExpandedSubProcess") {
                isCompensationStyle[mxConstants.STYLE_FILLCOLOR] = "inherit";
            }

            isCompensationMarker = MxFactory.cell("", MxFactory.geometry(0, 0, markerSize, markerSize));
            isCompensationMarkerHorizontalOffset = (shape.width - markerSize) / 2;
        }

        if (isAdHoc != null && isAdHoc && (isCollapsed || shape.type === "ExpandedSubProcess") && shape.height > 28) {
            loopMarkerHorizontalLeftShift++;
            isCollapsedMarkerHorizontalLeftShift++;
            isCompensationMarkerHorizontalLeftShift++;

            isAdhocStyle[mxConstants.STYLE_SHAPE] = "taskadhoc";
            isAdhocStyle["selectable"] = 0;

            isAdhocMarker = MxFactory.cell("", MxFactory.geometry(0, 0, markerSize, markerSize));
            isAdhocMarkerHorizontalOffset = (shape.width - markerSize) / 2;
        }


        if (isCollapsed != null && isCollapsed && shape.height > 28) {
            loopMarkerHorizontalLeftShift++;
            isCompensationMarkerHorizontalLeftShift++;
            isAdHocMarkerHorizontalLeftShift--;

            isCollapsedStyle[mxConstants.STYLE_SHAPE] = "taskcollapse";
            isCollapsedStyle["selectable"] = 0;
            isCollapsedStyle[mxConstants.STYLE_FILLCOLOR] = "black";

            isCollapsedMarker = MxFactory.cell("", MxFactory.geometry(0, 0, markerSize, markerSize));
            isCollapsedMarkerHorizontalOffset = (shape.width - markerSize) / 2;
        }

        if (loopType != null && loopType !== "None" && shape.height > 28) {
            isCompensationMarkerHorizontalLeftShift--;
            isAdHocMarkerHorizontalLeftShift--;
            isCollapsedMarkerHorizontalLeftShift--;

            loopStyle["selectable"] = 0;
            loopStyle[mxConstants.STYLE_FILLCOLOR] = "black";

            switch (loopType) {
                case "Standard":
                    loopStyle[mxConstants.STYLE_SHAPE] = "taskloop";
                    break;
                case "ParallelMultiInstance":
                    if (shape.type === "Task") {
                        loopStyle[mxConstants.STYLE_SHAPE] = "taskparamulti";
                    } else {
                        loopStyle[mxConstants.STYLE_SHAPE] = "subprocessparamulti";
                    }

                    loopStyle[mxConstants.STYLE_FILLCOLOR] = "black";
                    break;
                case "SequentialMultiInstance":
                    if (shape.type === "Task") {
                        loopStyle[mxConstants.STYLE_SHAPE] = "taskseqmulti";
                    } else {
                        loopStyle[mxConstants.STYLE_SHAPE] = "subprocessseqmulti";
                    }
                    loopStyle[mxConstants.STYLE_FILLCOLOR] = "black";
                    break;
                default:
                    break;
            }
            loopMarker = MxFactory.cell("", MxFactory.geometry(0, 0, markerSize, markerSize));
        }


        this.setMarkerStyleAndInsert (isCompensationMarker,
            taskOrSubprocess,
            isCompensationMarkerHorizontalOffset - isCompensationMarkerHorizontalLeftShift * (markerSize / 2 + 3),
            isCompensationMarkerVerticalOffset,
            isCompensationStyle);

        this.setMarkerStyleAndInsert(isAdhocMarker,
            taskOrSubprocess,
            isAdhocMarkerHorizontalOffset - isAdHocMarkerHorizontalLeftShift * (markerSize / 2 + 3),
            isAdhocMarkerVerticalOffset,
            isAdhocStyle);

        this.setMarkerStyleAndInsert(isCollapsedMarker,
            taskOrSubprocess,
            isCollapsedMarkerHorizontalOffset - isCollapsedMarkerHorizontalLeftShift * (markerSize / 2 + 3),
            isCollapsedMarkerVerticalOffset,
            isCollapsedStyle);

        this.setMarkerStyleAndInsert(loopMarker,
            taskOrSubprocess,
            loopMarkerHorizontalOffset - loopMarkerHorizontalLeftShift * (markerSize / 2 + 4),
            loopMarkerVerticalOffset,
            loopStyle);

        return taskOrSubprocess;
    };

    private setMarkerStyleAndInsert(marker: MxCell, shape: MxCell, horizontalOffset: number, verticalOffset: number, style: Style): void {
        if (!marker) {
            return;
        }

        if (horizontalOffset !== 0) {
            marker.geometry.offset = MxFactory.point(horizontalOffset, verticalOffset);
        }
        marker.vertex = true;
        marker.geometry.relative = true;
        marker.setStyle(style.convertToString());

        shape.insert(marker);
    }

    private event = (shape: IShape): MxCell => {
        var eventType = ShapeExtensions.getPropertyByName(shape, "EventType");
        var eventTrigger = ShapeExtensions.getPropertyByName(shape, "EventTrigger");

        var style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_ELLIPSE);
        style[mxConstants.STYLE_PERIMETER] = mxConstants.PERIMETER_RECTANGLE;
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_VERTICAL_LABEL_POSITION] = mxConstants.ALIGN_BOTTOM;
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;
        var event = super.createDefaultVertex(shape, style);

        if (eventType != null
            && (eventType === "IntermediateThrowing"
                || eventType === "IntermediateCatching" || eventType === "IntermediateNonInterrupting")) {
            var innerEllipseWidth = (shape.width - 5) < 0 ? 0 : shape.width - 5;
            var innerEllipseHeight = (shape.height - 5) < 0 ? 0 : shape.height - 5;
            if (innerEllipseHeight % 2 === 1) {
                innerEllipseHeight += 1;
            }
            var innerEllipse = MxFactory.cell("", MxFactory.geometry(0, 0, innerEllipseWidth, innerEllipseHeight));
            innerEllipse.geometry.offset = MxFactory.point((shape.width - innerEllipseWidth) / 2, (shape.height - innerEllipseHeight) / 2);

            var innerEllipseStyle = new Style();
            innerEllipseStyle[mxConstants.STYLE_SHAPE] = "eventellipse";
            innerEllipseStyle[mxConstants.STYLE_STROKECOLOR] = "inherit";
            innerEllipseStyle[mxConstants.STYLE_FILLCOLOR] = "inherit";
            innerEllipseStyle["selectable"] = 0;
            innerEllipse.setStyle(innerEllipseStyle.convertToString());

            innerEllipse.vertex = true;
            innerEllipse.geometry.relative = true;
            event.insert(innerEllipse);
        }

        if (eventTrigger != null && eventTrigger !== "None") {
            var markerWidth = shape.width * 0.5;
            var markerHeight = shape.height * 0.5;
            var triggerMarker = null;
            var triggerStyle = new Style();
            triggerStyle["selectable"] = 0;

            switch (eventTrigger) {
                case "Error":
                    triggerStyle[mxConstants.STYLE_SHAPE] = "eventerror";
                    if (eventType === "End") {
                        triggerStyle[mxConstants.STYLE_FILLCOLOR] = "black";
                    }
                    triggerMarker = this.createTrigger(shape, markerWidth, markerHeight);
                    break;
                case "Message":
                    triggerStyle[mxConstants.STYLE_SHAPE] = "eventmessage";

                    if (eventType === "End" || eventType === "IntermediateThrowing") {
                        triggerStyle[mxConstants.STYLE_FILLCOLOR] = "black";
                        triggerStyle[mxConstants.STYLE_STROKECOLOR] = "white";
                    }
                    markerHeight = markerHeight / 1.3;
                    triggerMarker = this.createTrigger(shape, markerWidth, markerHeight);
                    break;
                case "Timer":
                    markerWidth = markerWidth * 1.4;
                    markerHeight = markerHeight * 1.4;
                    triggerStyle[mxConstants.STYLE_SHAPE] = "eventtimer";
                    triggerMarker = this.createTrigger(shape, markerWidth, markerHeight);
                    break;
                case "Compensation":
                    triggerStyle[mxConstants.STYLE_SHAPE] = "eventcompensation";
                    if (eventType === "End" || eventType === "IntermediateThrowing") {
                        triggerStyle[mxConstants.STYLE_FILLCOLOR] = "black";
                        triggerStyle[mxConstants.STYLE_STROKECOLOR] = "white";
                    } else {
                        triggerStyle[mxConstants.STYLE_STROKEWIDTH] = "4";
                    }
                    triggerMarker = MxFactory.cell("", MxFactory.geometry(0, 0, markerWidth, markerHeight));
                    triggerMarker.geometry.offset = MxFactory.point((shape.width - markerWidth * 1.1) / 2, (shape.height - markerHeight) / 2);
                    break;
                case "Cancel":
                    triggerStyle[mxConstants.STYLE_SHAPE] = "eventcancel";
                    if (eventType === "End") {
                        triggerStyle[mxConstants.STYLE_FILLCOLOR] = "black";
                        triggerStyle[mxConstants.STYLE_STROKECOLOR] = "white";
                    }
                    markerWidth = markerWidth * 1.2;
                    markerHeight = markerHeight * 1.2;
                    triggerMarker = this.createTrigger(shape, markerWidth, markerHeight);
                    break;
                case "Conditional":
                    markerWidth = markerWidth * 1.5;
                    markerHeight = markerHeight * 1.2;
                    triggerStyle[mxConstants.STYLE_SHAPE] = "eventconditional";
                    triggerMarker = this.createTrigger(shape, markerWidth, markerHeight);
                    break;
                case "Signal":
                    triggerStyle[mxConstants.STYLE_SHAPE] = "eventsignal";
                    if (eventType === "End" || eventType === "IntermediateThrowing") {
                        triggerStyle[mxConstants.STYLE_FILLCOLOR] = "black";
                        triggerStyle[mxConstants.STYLE_STROKECOLOR] = "white";
                    }
                    markerHeight = markerHeight * 0.8;
                    triggerMarker = this.createTrigger(shape, markerWidth, markerHeight);
                    break;
                case "Link":
                    triggerStyle[mxConstants.STYLE_SHAPE] = "eventlink";
                    if (eventType === "IntermediateThrowing") {
                        triggerStyle[mxConstants.STYLE_FILLCOLOR] = "black";
                        triggerStyle[mxConstants.STYLE_STROKECOLOR] = "white";
                    }
                    markerHeight = markerHeight * 0.8;
                    triggerMarker = this.createTrigger(shape, markerWidth, markerHeight);
                    break;
                case "Multiple":
                    triggerStyle[mxConstants.STYLE_SHAPE] = "eventmultiple";
                    if (eventType === "End" || eventType === "IntermediateThrowing") {
                        triggerStyle[mxConstants.STYLE_FILLCOLOR] = "black";
                        triggerStyle[mxConstants.STYLE_STROKECOLOR] = "white";
                    }
                    triggerMarker = this.createTrigger(shape, markerWidth, markerHeight);
                    break;
                case "Escalation":
                    triggerStyle[mxConstants.STYLE_SHAPE] = "eventescalation";
                    if (eventType === "End" || eventType === "IntermediateThrowing") {
                        triggerStyle[mxConstants.STYLE_FILLCOLOR] = "black";
                        triggerStyle[mxConstants.STYLE_STROKECOLOR] = "white";
                    }
                    markerWidth = markerWidth * 1.2;
                    triggerMarker = this.createTrigger(shape, markerWidth, markerHeight);
                    break;
                case "ParallelMultiple":
                    markerWidth = markerWidth * 1.5;
                    markerHeight = markerHeight * 1.5;
                    triggerStyle[mxConstants.STYLE_SHAPE] = "eventparallelmultiple";
                    triggerMarker = this.createTrigger(shape, markerWidth, markerHeight);
                    break;
                case "Terminate":
                    triggerStyle[mxConstants.STYLE_SHAPE] = "eventellipse";
                    triggerStyle[mxConstants.STYLE_FILLCOLOR] = "black";
                    triggerMarker = this.createTrigger(shape, markerWidth, markerHeight);
                    break;
                default:
                    break;
            }
            if (triggerMarker != null) {
                this.setMarkerStyleAndInsert(triggerMarker, event, 0, 0, triggerStyle);
            }
        }

        return event;
    };

    private createTrigger(shape: IShape, markerWidth: number, markerHeight: number): MxCell {
        var triggerMarker = MxFactory.cell("", MxFactory.geometry(0, 0, markerWidth, markerHeight));
        triggerMarker.geometry.offset = MxFactory.point((shape.width - markerWidth) / 2, (shape.height - markerHeight) / 2);
        return triggerMarker;
    }

    private groupShape = (shape: IShape): MxCell => {
        shape.strokeDashPattern = "6 1 2 1";
        var style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_ROUNDED] = 1;
        style[mxConstants.STYLE_ARCSIZE] = 2;
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;
        style[mxConstants.STYLE_FILLCOLOR] = "transparent";
        style.removeProperty(mxConstants.STYLE_GRADIENTCOLOR);
        return super.createDefaultVertex(shape, style);
    };
}