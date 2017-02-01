import {GRAPH_COLUMN_WIDTH, GRAPH_ROW_HEIGHT} from "../models/";


export var CS_VERTICAL: string = "Vertical";
export var CS_RIGHT: string = "Right";
export var CS_LEFT: string = "Left";

export class ConnectorStyles extends mxEdgeStyle {

    public static createStyles() {
        mxStyleRegistry.putValue(CS_VERTICAL, this.vertical);
        mxStyleRegistry.putValue(CS_RIGHT, mxEdgeStyle.OrthConnector);
        mxStyleRegistry.putValue(CS_LEFT, this.left);
    }

    private static vertical(state, source, target, points, result) {
        if (source != null && target != null) {
            let point = new mxPoint(source.getCenterX(), target.getCenterY());
            result.push(point);
        }
    };

    private static left(state, source, target, points, result) {
        if (source != null && target != null) {
            let offsetX = GRAPH_COLUMN_WIDTH * 0.8;
            let offsetY = GRAPH_ROW_HEIGHT / 2 - 15;

            let pointRight = new mxPoint(source.getCenterX() + offsetX, source.getCenterY());
            let pointDown = new mxPoint(source.getCenterX() + offsetX, source.getCenterY() + offsetY);
            let pointLeft = new mxPoint(target.getCenterX(), source.getCenterY() + offsetY);

            result.push(pointRight);
            result.push(pointDown);
            result.push(pointLeft);
        }
    };
}
