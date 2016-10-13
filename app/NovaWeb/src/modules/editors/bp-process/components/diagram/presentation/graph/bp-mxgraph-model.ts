export class BpMxGraphModel extends mxGraphModel {
    private linkCounter: number;

    constructor() {
        super();

        this.linkCounter = 0;
    }

    // override the add method that is used when graph is constructed and when cells are reordered. 
    // index: 0 - Back most shape
    // index: length-1, Front most shape.
    public add(parent, child, index) {

        // Connector indexes should always be beneath all other shapes when the parent is the whole mxGraph cell.
        if (child.edge && parent.id === 1) {
            // Count the connector shapes in the graph when constructed, and adds connectors to the top most index of connector shapes.
            if (child.source == null || child.target == null) {
                this.linkCounter++;
            }
            if (this.linkCounter < parent.children.length) {
                index = this.linkCounter;
            }
        }
        super.add(parent, child, index);
    }
}
