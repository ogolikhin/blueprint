import {IPosition} from "./rect";
import {ConnectionSide} from "./connection-side";
import {FlowGraphObject} from "./flow-graph-objects";

/**
 * Stores information about a connection that was calculated during the layout process..
 */
export class ConnectionInfo {
    /**
     * Gets or sets the "Node" where the connection starts.
     */
    public startNode: FlowGraphObject;

    /**
     * Gets or sets the side of the of the start node.
     */
    public startSide: ConnectionSide;

    /**
     * Gets or sets the <see cref="Node"/> where the connection ends.
     */
    public endNode: FlowGraphObject;

    /**
     * Gets or sets the side of the of the end node.
     */
    public endSide: ConnectionSide;

    /**
     * Gets or sets a value indicating whether this connection is visible.
     */
    public isVisible: boolean;

    /**
     * Gets or sets a value indicating whether this ConnectionInfo is return connector.
     */
    public isReturnConnector: boolean;

    private points: Array<IPosition> = [];

    /**
     * Gets a clonned array of points.
     */
    public getPoints() {
        return this.points.map(p => p);
    }

    private lastPoint: IPosition;

    /**
     * Gets last point of the points array
     */
    public getLastPoint(): IPosition {
        if (this.lastPoint != null) {
            return this.lastPoint;
        }
        return this.points[this.points.length - 1];
    }

    /**
     * Adds point to the points array
     */
    public addPointToXy(x: number, y: number) {
        this.addPoint({x: x, y: y});
    }

    /**
     * Adds point to the points array and keep last point Y coordinate
     */
    public addPointToX(x: number) {
        const y = this.getLastPoint() ? this.getLastPoint().y : 0;
        this.addPoint({x: x, y: y});
    }

    /**
     * Adds point to the points array and keep last point X coordinate
     */
    public addPointToY(y: number) {
        const x = this.getLastPoint() ? this.getLastPoint().x : 0;
        this.addPoint({x: x, y: y});
    }

    /**
     * Adds point to the points array
     */
    public addPoint(point: IPosition) {
        this.lastPoint = point;
        this.points.push(this.lastPoint);
    }
}
