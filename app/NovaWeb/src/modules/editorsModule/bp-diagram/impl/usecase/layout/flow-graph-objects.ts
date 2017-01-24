import {ISize, IPosition} from "./rect";
import {ProcessData} from "./process-data";
/**
 * Abstract base class for all objects of the graph.
 *
 */
export class FlowGraphObject {
    /**
     * Gets or sets a value indicating whether this FlowGraphObject is hidden.
     *
     * @return {boolean}
     */
    public hidden: boolean = false;

    /**
     * Gets or sets the size of the FlowGraphObject.
     *
     * @return {ISize} The size of the FlowGraphObject.
     */
    public size: ISize = {height: 0, width: 0};

    /**
     * Gets or sets the position of the FlowGraphObject.
     *
     * @return {IPosition} The position of the FlowGraphObject.
     */
    public position: IPosition;

    /**
     * Gets or sets the tag property that can be used to assigne additional properties.
     */
    public tag;


    private processData: ProcessData;

    /**
     * Gets the ProcessData object assigned to the FlowGraphObject.
     *
     * @return {ProcessData} The ProcessData of the FlowGraphObject.
     */
    public getProcessData(): ProcessData {
        if (this.processData == null) {
            this.processData = new ProcessData();
        }
        return this.processData;
    }

    /**
     * Clears the ProcessData object assigned to the FlowGraphObject.
     */
    public clearProcessData() {
        this.processData = null;
    }

    /**
     * Gets the column index of the FlowGraphObject.
     *
     * @return {number} The column index of the FlowGraphObject.
     */
    public getCol(): number {
        return this.getProcessData().col;
    }

    /**
     * Sets the column index of the FlowGraphObject.
     *
     * @param {number} A column index.
     */
    public setCol(value: number) {
        this.getProcessData().col = value;
    }

    /**
     * Gets the ProcessData object assigned to the FlowGraphObject.
     *
     * @return {number} The row of the FlowGraphObject.
     */
    public getRow(): number {
        return this.getProcessData().row;
    }

    /**
     * Sets the row index of the FlowGraphObject.
     *
     * @param {number} A row index.
     */
    public setRow(value: number) {
        this.getProcessData().row = value;
    }

    /**
     * Gets the replacement object of the FlowGraphObject.
     *
     * @return {FlowGraphObject} The replacement object of the FlowGraphObject.
     */
    public getReplacement(): FlowGraphObject {
        return this.getProcessData().replacement;
    }

    /**
     * Sets the FlowGraphObject as a replacement object of the FlowGraphObject.
     *
     * @param {FlowGraphObject} A FlowGraphObject to be replacement.
     */
    public setReplacement(replacement: FlowGraphObject) {
        this.getProcessData().replacement = replacement;
    }
}

/**
 * Represents one flow of the flow chart diagram.
 *
 * This class cannot be instantiated manually. Use the "FlowGraph.createAlternateFlow()"
 * method to create alternate flows or use the "FlowGraph.mainFlow" property.
 */
export class Flow extends FlowGraphObject {
    private nodes: Array<Node>;

    constructor() {
        super();
        this.nodes = [];
    }

    private clonedNodes: Array<Node> = null;

    /**
     * Gets a collection of all nodes of the flow. The returned flow collection
     * is readonly and will never be null.
     *
     * @return {Array<Node>} The nodes of the current flow.
     */
    public getNodes(): Array<Node> {
        if (this.clonedNodes == null) {
            //Creates clone of the source nodes array
            this.clonedNodes = this.nodes.map(n => n);
        }
        return this.clonedNodes;
    }

    /**
     * Gets or sets a value indicating whether this Flow is collapsed.
     */
    public collapsed: boolean;

    /**
     * Gets a first node of the current flow.
     */
    public getFirstNode() {
        return this.nodes[0];
    }

    /**
     * Gets a last node of the current flow.
     */
    public getLastNode() {
        return this.nodes[this.nodes.length - 1];
    }

    /**
     * Adds a new "Node" to the flow.
     *
     * The "Node" to add to the flow. The node cannot be null and cannot be part of another flow.
     *
     * @throws Exceptions
     */
    public addNode(node: Node) {
        if (node.flow != null) {
            throw new Error("Node is already part of another flow");
        }
        node.flow = this;
        this.nodes.push(node);
    }

    /**
     * Determines whether the specified flow is a child of the the current flow.
     *
     * True if the specified flow is a child of the current flow or if the the specified flow is null; otherwise, false.
     */
    public isChild(flow: Flow): boolean {
        let isParent = false;
        while (flow != null) {
            if (flow === this) {
                isParent = true;
                break;
            } else {
                if (flow instanceof AlternateFlow && flow.startNode != null) {
                    flow = (<AlternateFlow>flow).startNode.flow;
                } else {
                    flow = null;
                }
            }
        }

        return isParent;
    }

    /**
     * Gets the child objects of the alternate flow and the all nodes and other
     * alternate flows that are part of the current branch.
     *
     * The collection of all objects of the branch. Is never null.
     */
    public getChildObjects(): Array<FlowGraphObject> {
        const objects = [];
        this.addObjects(this, objects);

        // The objects to add all objects of the graph also returns the current
        // flow. Therefore just remove the first node of the list to remove the current flow.
        objects.shift();

        return objects;
    }

    private addObjects(flow: Flow, objects: Array<FlowGraphObject>) {
        const alternateFlows = [];
        objects.push(flow);

        flow.getNodes().forEach((n: Node) => {
            objects.push(n);
            n.getAlternateFlows().forEach(f => alternateFlows.push(f));
        });
        alternateFlows.forEach(f => this.addObjects(f, objects));
    }

}

/**
 * An alternate flow that also supports collapsing and a start and
 * and a start and end node.
 *
 * The "endNode" must be assigned manually to get a valid flow graph.
 * This class cannot be instantiated manually. Use the FlowGraph.createAlternateFlow() method to create a new instance of the "AlternateFlow" class.
 */
export class AlternateFlow extends Flow {

    /**
     * Gets the start node of the alternate flow. This value is assigned when the
     * alternate flow is added to a node.
     *
     * @return {Node} The start node of the alternate flow.
     */
    public startNode: Node;

    /**
     * Gets the end node of the alternate flow. This node must be assigned manually.
     *
     * @return {Node} The end node of the alternate flow.
     */
    public endNode: Node;
}

/**
 * Represents one node of the flow graph.
 *
 * This class cannot be instantiated manually. Use the FlowGraph.createNode()"
 * method to create a new instance of the "Node" class.
 */
export class Node extends FlowGraphObject {
    private alternateFlows: Array<AlternateFlow>;

    constructor() {
        super();
        this.alternateFlows = [];
    }

    private clonedFlows: Array<Flow> = null;

    /**
     * Gets a reaonly collection of all "AlternateFlow" objects that are assigned
     * that are assigned to this node.
     *
     * The "AlternateFlow" objects of this node.
     */
    public getAlternateFlows() {
        if (this.clonedFlows == null) {
            //Creates clone of the source alternateFlows array
            this.clonedFlows = this.alternateFlows.map(f => f);
        }
        return this.clonedFlows;
    }

    /**
     * Gets the flow where this node is assigned to.
     *
     * The flow where this node is assigned to.
     */
    public flow: Flow;

    /**
     * Assigns a new "AlternateFlow" to the node.
     *
     * @param alternateFlow. The "AlternateFlow" to add to the node. The flow cannot be null and cannot be part
     * of another node.
     *
     * @throws alternateFlow is already assigned to another node.
     */
    public addAlternateFlow(alternateFlow: AlternateFlow) {
        if (alternateFlow.startNode != null) {
            throw new Error("Alternate flow is already assigned");
        }

        alternateFlow.startNode = this;
        this.alternateFlows.push(alternateFlow);
    }
}
