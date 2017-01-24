import {ConnectionInfo} from "./connection-info";
import {FlowGraph} from "./flow-graph";

/**
 * Stores the results of the layout process.
 */
export class LayoutResult {
    private connections: Array<ConnectionInfo>;

    /**
     * Initializes a new instance of the "LayoutResult" class with the
     * graph that  was assigned to the "ILayoutCalculator".
     *
     * @param graph. The graph for the layout process. Cannot be null.
     */
    constructor(private graph: FlowGraph) {
        this.connections = [];
    }

    /**
     * Gets the information about all connections that was created during the layout process.
     *
     * The connections that was created during the layout process.
     */
    public getConnections(): Array<ConnectionInfo> {
        return this.connections;
    }

    /**
     * Gets the graph that was assigned to the "ILayoutCalculator" to create
     * the layout for. Contains all positions and sizes of the nodes and flows.
     *
     * The graph that was assigned to the "ILayoutCalculator".
     */
    public getGraph(): FlowGraph {
        return this.graph;
    }
}
