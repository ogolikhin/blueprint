import {IShape} from "../impl/models";
import {Shapes} from "./utils/constants";
import {Style, Styles} from "./utils/style-builder";
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
        const style = this.createSwimlaneDefaultStyle(this.styleBuilder.createDefaultShapeStyle(shape, "swimlane"));
        const pool = super.createDefaultVertex(shape, style);
        return pool;
    };

    private lane = (shape: IShape): MxCell => {
        const style = this.createSwimlaneDefaultStyle(this.styleBuilder.createDefaultShapeStyle(shape, "swimlane"));
        const lane = super.createDefaultVertex(shape, style);
        return lane;
    };

    private message = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, "message");
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_VERTICAL_LABEL_POSITION] = mxConstants.ALIGN_BOTTOM;
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;
        const message = super.createDefaultVertex(shape, style);
        const isInitiating = ShapeExtensions.getPropertyByName(shape, "IsInitiating");
        if (!isInitiating) {
            const cell = MxFactory.cell(shape, MxFactory.geometry(0, 0, shape.width, shape.height), "fillColor=gray;opacity=20;selectable=0");
            cell.vertex = true;
            cell.geometry.relative = true;
            message.insert(cell);
        }
        return message;
    };

    private dataObject = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, "dataobject");
        style[mxConstants.STYLE_PERIMETER] = mxConstants.PERIMETER_RECTANGLE;
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_VERTICAL_LABEL_POSITION] = mxConstants.ALIGN_BOTTOM;
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;
        const dataObject = super.createDefaultVertex(shape, style);
        const isCollection = ShapeExtensions.getPropertyByName(shape, "IsCollection");
        if (isCollection) {
            const markerWidth = shape.width * 0.15;
            const markerHeight = shape.height * 0.2;
            const parallelMarker = MxFactory.cell("", MxFactory.geometry(0.5, 1, markerWidth, markerHeight), "shape=parallelmarker;selectable=0");
            parallelMarker.vertex = true;
            parallelMarker.geometry.relative = true;
            parallelMarker.geometry.offset = MxFactory.point(-markerWidth / 2, -markerHeight);
            dataObject.insert(parallelMarker);
        }
        return dataObject;
    };

    private dataStore = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, "datastore");
        const dataStore = super.createDefaultVertex(shape, style);
        return dataStore;
    };

    private gateway = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, "gateway");
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_VERTICAL_LABEL_POSITION] = mxConstants.ALIGN_BOTTOM;
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;
        const gateway = super.createDefaultVertex(shape, style);
        const gatewayType = ShapeExtensions.getPropertyByName(shape, "GatewayType");

        if (gatewayType !== "ExclusiveData") {
            let markerWidth = shape.width * 0.6;
            let markerHeight = shape.height * 0.6;
            const markerStrokeWidth = Math.max(1, Math.max(shape.width, shape.height) * 0.015);
            const styleBuilder = new Style();
            styleBuilder[Styles.STYLE_SELECTABLE] = 0;
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
            const marker = MxFactory.cell("", MxFactory.geometry(0, 0, markerWidth, markerHeight));
            marker.vertex = true;
            marker.geometry.relative = true;
            marker.geometry.offset = MxFactory.point((shape.width - markerWidth) / 2, (shape.height - markerHeight) / 2);
            gateway.insert(marker);
            marker.setStyle(styleBuilder.convertToString());
        }
        return gateway;
    };

    private annotationShape = (shape: IShape): MxCell => {
        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_ROUNDED] = 0;
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_MIDDLE;
        const annotationShape = super.createDefaultVertex(shape, style);
        return annotationShape;
    };

    private taskOrSubprocess = (shape: IShape): MxCell => {
        const loopType = ShapeExtensions.getPropertyByName(shape, "LoopType");
        const boundaryType = ShapeExtensions.getPropertyByName(shape, "BoundaryType");
        const taskType = ShapeExtensions.getPropertyByName(shape, "TaskType");
        const isAdHoc = ShapeExtensions.getPropertyByName(shape, "IsAdHoc");
        const isCompensation = ShapeExtensions.getPropertyByName(shape, "IsCompensation");
        const isCollapsed = ShapeExtensions.getPropertyByName(shape, "IsCollapsed");

        const markerSize = 13;

        let isCompensationMarker = null;
        const isCompensationStyle = new Style();
        let isCompensationMarkerHorizontalOffset = 0;
        const isCompensationMarkerVerticalOffset = shape.height - 25;
        let isCompensationMarkerHorizontalLeftShift = 0;

        let isAdhocMarker = null;
        const isAdhocStyle = new Style();
        let isAdhocMarkerHorizontalOffset = 0;
        const isAdhocMarkerVerticalOffset = shape.height - 25;
        let isAdHocMarkerHorizontalLeftShift = 0;

        let isCollapsedMarker = null;
        const isCollapsedStyle = new Style();
        let isCollapsedMarkerHorizontalOffset = 0;
        const isCollapsedMarkerVerticalOffset = shape.height - 25;
        let isCollapsedMarkerHorizontalLeftShift = 0;

        let tasktypeMarker = null;
        const taskTypeStyle = new Style();

        let loopMarker = null;
        const loopStyle = new Style();
        const loopMarkerHorizontalOffset = (shape.width - markerSize / 2) / 2;
        let loopMarkerVerticalOffset = shape.height - 25;
        let loopMarkerHorizontalLeftShift = 0;

        const style = this.styleBuilder.createDefaultShapeStyle(shape, "rectangle");
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_ROUNDED] = 1;
        style[mxConstants.STYLE_ARCSIZE] = 2;

        if (shape.type === "ExpandedSubProcess") {
            style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;
        }

        const taskOrSubprocess = super.createDefaultVertex(shape, style);

        if (boundaryType != null && boundaryType !== "Default") {
            const boundaryWidth = shape.width - 10;
            const boundaryHeight = shape.height - 10;
            const boundaryStyle = new Style();
            boundaryStyle[mxConstants.STYLE_SHAPE] = "taskrect";
            boundaryStyle[Styles.STYLE_SELECTABLE] = 0;
            boundaryStyle[mxConstants.STYLE_STROKECOLOR] = "inherit";
            boundaryStyle[mxConstants.STYLE_FILLCOLOR] = "inherit";
            const boundary = MxFactory.cell("", MxFactory.geometry(0, 0, boundaryWidth, boundaryHeight));
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
            taskTypeStyle[Styles.STYLE_SELECTABLE] = 0;
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
            isCompensationStyle[Styles.STYLE_SELECTABLE] = 0;
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
            isAdhocStyle[Styles.STYLE_SELECTABLE] = 0;

            isAdhocMarker = MxFactory.cell("", MxFactory.geometry(0, 0, markerSize, markerSize));
            isAdhocMarkerHorizontalOffset = (shape.width - markerSize) / 2;
        }


        if (isCollapsed != null && isCollapsed && shape.height > 28) {
            loopMarkerHorizontalLeftShift++;
            isCompensationMarkerHorizontalLeftShift++;
            isAdHocMarkerHorizontalLeftShift--;

            isCollapsedStyle[mxConstants.STYLE_SHAPE] = "taskcollapse";
            isCollapsedStyle[Styles.STYLE_SELECTABLE] = 0;
            isCollapsedStyle[mxConstants.STYLE_FILLCOLOR] = "black";

            isCollapsedMarker = MxFactory.cell("", MxFactory.geometry(0, 0, markerSize, markerSize));
            isCollapsedMarkerHorizontalOffset = (shape.width - markerSize) / 2;
        }

        if (loopType != null && loopType !== "None" && shape.height > 28) {
            isCompensationMarkerHorizontalLeftShift--;
            isAdHocMarkerHorizontalLeftShift--;
            isCollapsedMarkerHorizontalLeftShift--;

            loopStyle[Styles.STYLE_SELECTABLE] = 0;
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


        this.setMarkerStyleAndInsert(isCompensationMarker,
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
        const eventType = ShapeExtensions.getPropertyByName(shape, "EventType");
        const eventTrigger = ShapeExtensions.getPropertyByName(shape, "EventTrigger");

        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_ELLIPSE);
        style[mxConstants.STYLE_PERIMETER] = mxConstants.PERIMETER_RECTANGLE;
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_VERTICAL_LABEL_POSITION] = mxConstants.ALIGN_BOTTOM;
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;
        const event = super.createDefaultVertex(shape, style);

        if (eventType != null
            && (eventType === "IntermediateThrowing"
            || eventType === "IntermediateCatching" || eventType === "IntermediateNonInterrupting")) {
            const innerEllipseWidth = (shape.width - 5) < 0 ? 0 : shape.width - 5;
            let innerEllipseHeight = (shape.height - 5) < 0 ? 0 : shape.height - 5;
            if (innerEllipseHeight % 2 === 1) {
                innerEllipseHeight += 1;
            }
            const innerEllipse = MxFactory.cell("", MxFactory.geometry(0, 0, innerEllipseWidth, innerEllipseHeight));
            innerEllipse.geometry.offset = MxFactory.point((shape.width - innerEllipseWidth) / 2, (shape.height - innerEllipseHeight) / 2);

            const innerEllipseStyle = new Style();
            innerEllipseStyle[mxConstants.STYLE_SHAPE] = "eventellipse";
            innerEllipseStyle[mxConstants.STYLE_STROKECOLOR] = "inherit";
            innerEllipseStyle[mxConstants.STYLE_FILLCOLOR] = "inherit";
            innerEllipseStyle[Styles.STYLE_SELECTABLE] = 0;
            innerEllipse.setStyle(innerEllipseStyle.convertToString());

            innerEllipse.vertex = true;
            innerEllipse.geometry.relative = true;
            event.insert(innerEllipse);
        }

        if (eventTrigger != null && eventTrigger !== "None") {
            let markerWidth = shape.width * 0.5;
            let markerHeight = shape.height * 0.5;
            let triggerMarker = null;
            const triggerStyle = new Style();
            triggerStyle[Styles.STYLE_SELECTABLE] = 0;

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
        const triggerMarker = MxFactory.cell("", MxFactory.geometry(0, 0, markerWidth, markerHeight));
        triggerMarker.geometry.offset = MxFactory.point((shape.width - markerWidth) / 2, (shape.height - markerHeight) / 2);
        return triggerMarker;
    }

    private groupShape = (shape: IShape): MxCell => {
        shape.strokeDashPattern = "6 1 2 1";
        const style = this.styleBuilder.createDefaultShapeStyle(shape, mxConstants.SHAPE_RECTANGLE);
        style[mxConstants.STYLE_FOLDABLE] = 0;
        style[mxConstants.STYLE_ROUNDED] = 1;
        style[mxConstants.STYLE_ARCSIZE] = 2;
        style[mxConstants.STYLE_VERTICAL_ALIGN] = mxConstants.ALIGN_TOP;
        style[mxConstants.STYLE_FILLCOLOR] = "transparent";
        style.removeProperty(mxConstants.STYLE_GRADIENTCOLOR);
        return super.createDefaultVertex(shape, style);
    };
}
