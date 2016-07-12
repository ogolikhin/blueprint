import {AlternateFlow, Flow, Node} from "./flow-graph-objects";

/**
 * Container class that holds all nodes and flows of the diagram.
 *
 * Provides also factory methods to create "Node" and "AlternateFlow" instances.
 */
export class FlowGraph {
    private alternateFlows: Array<AlternateFlow>;
    private nodes: Array<Node>;
    private mainFlow: Flow;

    constructor() {
        this.alternateFlows = [];
        this.nodes = [];
        this.mainFlow = new Flow();
    }

    /**
     * Gets the main flow that is never null and is created by the flow graph.
     *
     * The main flow.
     */
    public getMainFlow(): Flow {
        return this.mainFlow;
    }

    /**
     * Gets all flows of the current graph which contains all "AlternateFlow" instances
     *
     * The flows of the current graph. Is never null.
     */
    public getFlows() {
        const flows = <Array<Flow>>this.alternateFlows.map(n => n);
        flows.push(this.mainFlow);
        return flows;
    }

    private clonedFlows: Array<AlternateFlow> = null;

    /**
     * Gets a readonly collection of all "AlternateFlow" objects of the graph.
     *
     * The "AlternateFlow" objects of the graph.
     */
    public getAlternateFlows(): Array<AlternateFlow> {
        if (this.clonedFlows == null) {
            //Creates clone of the source alternateFlows array
            this.clonedFlows = this.alternateFlows.map(f => f);
        }
        return this.clonedFlows;
    }

    private clonedNodes: Array<Node> = null;

    /**
     * Gets a readonly collection of all "Node" objects of the graph.
     *
     * The "Node" objects of the graph
     */
    public getNodes(): Array<Node> {
        if (this.clonedNodes == null) {
            //Creates clone of the source nodes array
            this.clonedNodes = this.nodes.map(n => n);
        }
        return this.clonedNodes;
    }

    /**
     * Creates a new "Node" object that is added to the graph and is returned.
     *
     * The instantiated "Node" object. Is never null.
     */
    public createNode(): Node {
        const node = new Node();
        this.nodes.push(node);
        return node;
    }

    /**
     * Creates a new "AlternateFlow" object that is added to the graph and is returned.
     *
     * The instantiated "AlternateFlow" object. Is never null.
     */
    public createAlternateFlow(): AlternateFlow {
        const alternateFlow = new AlternateFlow();
        this.alternateFlows.push(alternateFlow);
        return alternateFlow;
    }
}
