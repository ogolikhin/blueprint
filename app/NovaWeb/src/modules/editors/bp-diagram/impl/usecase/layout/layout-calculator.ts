import {FlowGraph} from "./flow-graph";
import {FlowGraphObject, AlternateFlow, Flow, Node} from "./flow-graph-objects";
import {MinMax} from "./min-max";
import {IRect, Rect} from "./rect";
import {LayoutResult} from "./layout-result";
import {ConnectionInfo} from "./connection-info";
import {ConnectionSide} from "./connection-side";

/**
 * Declares a map where key and value are numbers
 */
interface INumberToNumberMap {
    [key: number]: number;
}

/**
 * Declares a map where key is string and value is Node
 */
interface IStringToNodeMap {
    [key: string]: Node;
}

/**
 * Interface for all layout calculators.
 */
export interface ILayoutCalculator {
    /**
     * Arranges the graph by calculating the size and positiion for all nodes and
     * the connections between the nodes.
     *
     * @param "graph". The graph that must be arranged by the layout calculator. Cannot be null.
     */
    arrangeGraph(graph: FlowGraph);
}

/**
 * Default implementation of the "ILayoutCalculator" interface.
 */
export class LayoutCalculator implements ILayoutCalculator {
    /**
     * Gets or sets the default width of the nodes.
     *
     * The default width of the nodes.
     */
    public nodeDefaultWidth: number;

    /**
     * Gets or sets the default height of the nodes.
     *
     * The default height of the nodes.
     */
    public nodeDefaultHeight: number;

    /**
     * Gets or sets the cell spacing that is half of the distance between two nodes.
     *
     * The cell spacing.
     */
    public verticalCellSpacing: number;

    /**
     * Gets or sets the cell spacing that is half of the distance between two nodes.
     *
     * The cell spacing.
     */
    public horizontalCellSpacing: number;

    /**
     * Gets or sets the flow spacing that is the distance between the node and the border of the flow.
     *
     * The flow spacing.
     */
    public flowSpacing: number;

    /**
     * Gets or sets a value indicating whether or not vertical displacement should be disable.
     */
    public disableVerticalDisplacement: boolean;


    private isRightColumn: boolean = true;

    private rowSizes: INumberToNumberMap;

    private rowOffsets: INumberToNumberMap;

    private colSizes: INumberToNumberMap;

    private colOffsets: INumberToNumberMap;

    private connectorEndPointSpacing: number = 15;

    private flowGraphNodes: IStringToNodeMap;

    /**
     * Arranges the graph by calculating the size and positiion for all nodes and the connections between the nodes.
     *
     * @param graph. The graph that must be arranged by the layout calculator. Cannot be null.
     *
     * @throws Exception when
     * -graph contains at least one flow that has no node.
     * -graph contains at least one alternate row has no start node.
     * -graph contains at least one alternate row has no end node.
     * -graph contains at least one node is not part of any flow.
     */
    public arrangeGraph(graph: FlowGraph): LayoutResult {
        const result = new LayoutResult(graph);
        // Clear the virtual table.
        this.clearVirtualTable();

        this.verifyGraph(graph);

        this.calculateCollapsing(graph);
        this.calculateRows(graph);
        this.calculateColumns(graph);
        this.calculateObjectSizes(graph);
        this.calculateTableSizes(graph);
        this.calculateTableOffsets();
        this.calculateObjectPositions(graph);
        this.calculateFlowSizesAndPositions(graph);
        this.populateFlowGraphNodes(graph);
        this.calculateFlowConnections(graph, result);
        this.calculateBranchingConnections(graph, result);
        this.calculateEndConnections(graph, result);
        this.clearFlowGraphNodes();

        return result;
    }

    private clearVirtualTable() {
        this.rowOffsets = {};
        this.rowSizes = {};

        this.colOffsets = {};
        this.colSizes = {};
    }

    private calculateFlowSizesAndPositions(graph: FlowGraph) {
        graph.getFlows().forEach(flow => {
            // Calculate the positions and flows for all sizes.
            let bounds: IRect;
            if (flow != null && !flow.hidden) {
                if (flow.collapsed) {
                    const firstNode = flow.getFirstNode();
                    bounds = Rect.createRect(firstNode.position, firstNode.size);
                } else {
                    bounds = Rect.unionAll(flow.getNodes().map(n => Rect.createRect(n.position, n.size)));
                }
                // Calculate the bounding box of all nodes of the flow and inflate the
                // bounding box by the flow spacing.
                bounds.inflate(this.flowSpacing);

                // Set the position to the position of the bounding box.
                flow.position = {x: bounds.x, y: bounds.y};
                flow.size = {width: bounds.width, height: bounds.height};
            }
        });
    }

    private calculateCollapsing(graph: FlowGraph) {
        graph.getAlternateFlows().forEach(alternateFlow => {
            let children: Array<FlowGraphObject>;
            let endNode: FlowGraphObject;
            if (alternateFlow.collapsed && !alternateFlow.hidden) {
                const startNode = alternateFlow.getFirstNode();

                // Reeturn all alternate flows and nodes in the graph of the current flow.
                children = alternateFlow.getChildObjects();

                endNode = alternateFlow.getLastNode();
                if (endNode != null) {
                    endNode.setReplacement(startNode); //alternateFlow);
                }

                children.forEach(child => {
                    if (child != null) {
                        // hide all nodes except the first one
                        if (child !== startNode) {
                            child.hidden = true;
                        }
                        if (child instanceof AlternateFlow) {
                            // Is the flow chart object an alternate flow render the
                            // connection from the current flow instead of render it from the original node.
                            endNode = child.getLastNode();

                            if (endNode != null) {
                                endNode.setReplacement(startNode);
                            }
                        }
                    }
                });
            } else if (!alternateFlow.collapsed && !alternateFlow.hidden) { //this part is not needed, when graph is created from scratch
                children = alternateFlow.getChildObjects();

                endNode = alternateFlow.getLastNode();
                if (endNode != null) {
                    endNode.setReplacement(null);
                }
                children.forEach(child => {
                    if (child != null) {
                        child.hidden = false;
                    }
                    if (child instanceof AlternateFlow) {
                        // Is the flow chart object an alternate flow render the
                        // connection from the current flow instead of render it from the original node.
                        endNode = child.getLastNode();

                        if (endNode != null) {
                            endNode.setReplacement(null);
                        }
                    }
                });
            }
        });
    }

    private calculateEndConnections(graph: FlowGraph, result: LayoutResult) {
        graph.getAlternateFlows().forEach(alternateFlow => {
            if (this.canCalculateEndConnections(alternateFlow)) {
                let startNode: FlowGraphObject = alternateFlow.getLastNode();

                if (startNode != null) {
                    startNode = this.getFinalObject(startNode);

                    const endNode = this.getFinalObject(alternateFlow.endNode);

                    let connectionInfo = new ConnectionInfo();
                    connectionInfo.isReturnConnector = true;
                    connectionInfo.startNode = startNode;
                    connectionInfo.startSide = ConnectionSide.Bottom;
                    connectionInfo.endNode = endNode;
                    connectionInfo.endSide = ConnectionSide.Top;

                    this.addEndConnectionPoints(connectionInfo);
                    this.addConnection(result, connectionInfo);
                }
            }
        });
    }

    private addEndConnectionPoints(connectionInfo: ConnectionInfo) {
        const startNodePosition = connectionInfo.startNode.position;
        const startNodeSize = connectionInfo.startNode.size;

        const endNodePosition = connectionInfo.endNode.position;
        const endNodeSize = connectionInfo.endNode.size;

        connectionInfo.addPointToXy(startNodePosition.x + startNodeSize.width / 2, startNodePosition.y + startNodeSize.height);
        const endNodeCol = connectionInfo.endNode.getCol();
        if (connectionInfo.startNode.getRow() > connectionInfo.endNode.getRow()) {
            connectionInfo.addPointToY(startNodePosition.y + startNodeSize.height + this.connectorEndPointSpacing);
            const x = this.getColumnContentX(endNodeCol);
            if (connectionInfo.startNode.getCol() < endNodeCol) {
                connectionInfo.addPointToX(x - this.connectorEndPointSpacing);
            } else {
                connectionInfo.addPointToX(x + this.getColumnContentWidth(endNodeCol) + this.connectorEndPointSpacing);
            }
        } else {
            const collideableNode = this.getFirstCollideableNode(connectionInfo);
            if (collideableNode != null) {
                connectionInfo.addPointToXy(
                    collideableNode.position.x + collideableNode.size.width / 2,
                    collideableNode.position.y - this.connectorEndPointSpacing
                );
                if (connectionInfo.startNode.getCol() > endNodeCol) {
                    connectionInfo.addPointToXy(
                        endNodePosition.x + endNodeSize.width + this.connectorEndPointSpacing,
                        collideableNode.position.y - this.connectorEndPointSpacing
                    );
                } else {
                    connectionInfo.addPointToXy(
                        endNodePosition.x - this.connectorEndPointSpacing,
                        collideableNode.position.y - this.connectorEndPointSpacing
                    );
                }
            }
        }
        connectionInfo.addPointToY(endNodePosition.y - this.connectorEndPointSpacing);
        connectionInfo.addPointToX(endNodePosition.x + endNodeSize.width / 2);
        connectionInfo.addPointToY(endNodePosition.y);
    }

    private getColumnContentX(col: number) {
        return this.colOffsets[col] + this.horizontalCellSpacing;
    }

    private getColumnContentWidth(col: number) {
        return this.colSizes[col] - 2 * this.horizontalCellSpacing;
    }

    private getFirstCollideableNode(connectionInfo: ConnectionInfo) {
        const startNodeColumn = connectionInfo.startNode.getCol();
        const startNodeRow = connectionInfo.startNode.getRow();
        const endNodeRow = connectionInfo.endNode.getRow();

        let i = startNodeRow + 1;
        for (i; i < endNodeRow; i++) {
            const node = this.flowGraphNodes[startNodeColumn + "-" + i];
            if (node != null) {
                return node;
            }
        }

        return null;
    }

    private canCalculateEndConnections(alternateFlow: AlternateFlow): boolean {
        if (alternateFlow != null && alternateFlow.endNode != null) {
            // If alternate flow is hidden and EndNode is in collapsed flow,
            // than we don't need to draw connection.
            if (alternateFlow.hidden &&
                (alternateFlow.endNode.flow.collapsed || alternateFlow.endNode.flow.hidden)) {
                return false;
            }

            // If EndNode is Exit (equals to the last node in altFlow)
            if (alternateFlow.getNodes()[alternateFlow.getNodes().length - 1] === alternateFlow.endNode) {
                return false;
            }
            return true;
        }
        return false;
    }

    private calculateBranchingConnections(graph: FlowGraph, result: LayoutResult) {
        graph.getAlternateFlows().forEach(alternateFlow => {
            if (alternateFlow != null && alternateFlow.startNode != null && !alternateFlow.startNode.hidden) {
                let endNode: FlowGraphObject = alternateFlow.getFirstNode();

                if (endNode != null) {
                    endNode = this.getFinalObject(endNode);

                    const connectionInfo = new ConnectionInfo();
                    connectionInfo.isReturnConnector = false;
                    connectionInfo.startNode = this.getFinalObject(alternateFlow.startNode);
                    connectionInfo.startSide = endNode.getCol() > 0 ? ConnectionSide.Right : ConnectionSide.Left;
                    connectionInfo.endNode = endNode;
                    connectionInfo.endSide = ConnectionSide.Top;

                    this.addBranchingConnectionPoints(connectionInfo);
                    this.addConnection(result, connectionInfo);
                }
            }
        });
    }

    private addBranchingConnectionPoints(connectionInfo: ConnectionInfo) {
        const startNodePosition = connectionInfo.startNode.position;
        const startNodeSize = connectionInfo.startNode.size;
        const endNodePosition = connectionInfo.endNode.position;
        const endNodeSize = connectionInfo.endNode.size;

        if (connectionInfo.startSide === ConnectionSide.Right) {
            connectionInfo.addPointToXy(startNodePosition.x + startNodeSize.width, startNodePosition.y + startNodeSize.height / 2);
        } else {
            connectionInfo.addPointToXy(startNodePosition.x, startNodePosition.y + startNodeSize.height / 2);
        }
        connectionInfo.addPointToX(endNodePosition.x + endNodeSize.width / 2);
        connectionInfo.addPointToY(endNodePosition.y);
    }

    private populateFlowGraphNodes(graph: FlowGraph) {
        this.flowGraphNodes = {};
        graph.getNodes().forEach((n: Node) => {
            this.flowGraphNodes[n.getCol() + "-" + n.getRow()] = n;
        });
    }

    private clearFlowGraphNodes() {
        this.flowGraphNodes = {};
    }

    private calculateFlowConnections(graph: FlowGraph, result: LayoutResult) {
        graph.getFlows().forEach(flow => {
            if (flow != null && !flow.collapsed && !flow.hidden) {
                const nodes = flow.getNodes();
                for (let i = 0; i < nodes.length - 1; i++) {
                    const connectionInfo = new ConnectionInfo();
                    connectionInfo.isReturnConnector = false;
                    connectionInfo.startNode = nodes[i];
                    connectionInfo.startSide = ConnectionSide.Bottom;
                    connectionInfo.endNode = nodes[i + 1];
                    connectionInfo.endSide = ConnectionSide.Top;

                    this.addConnection(result, connectionInfo);
                }
            }
        });
    }

    private getFinalObject(flowChartObject: FlowGraphObject): FlowGraphObject {
        const replacement = flowChartObject.getReplacement();

        // Get or set the final object that is used to render the connection.
        // If an replacement is assigned render the flow chart object to the the replacement.
        return replacement != null ? replacement : flowChartObject;
    }

    private addConnection(result: LayoutResult, connectionInfo: ConnectionInfo) {
        connectionInfo.isVisible = !connectionInfo.startNode.hidden && !connectionInfo.endNode.hidden;

        if (connectionInfo.startNode != null && connectionInfo.endNode != null) {
            // Only add the connection when the start and end node are both not null.
            result.getConnections().push(connectionInfo);
        }
    }

    private calculateTableOffsets() {
        this.calculateOffsets(this.rowSizes, this.rowOffsets);
        this.calculateOffsets(this.colSizes, this.colOffsets);
    }

    private calculateOffsets(sizes: INumberToNumberMap, offsets: INumberToNumberMap) {
        let previousOffset = 0;
        let previousSize = 0;
        Object.keys(sizes).sort(this.orderAsNumbers).forEach(strKey => {
            const key = parseInt(strKey, 10);
            if (!isNaN(key)) {
                // Get the offset value of the previous column or row when such a column or row exists.
                previousOffset = offsets[key - 1] || 0;

                // Get the size value of the previous column or row when such a column or row exists.
                previousSize = sizes[key - 1] || 0;

                // Update the offset for the column or row.
                offsets[key] = previousSize + previousOffset;
            }
        });
    }

    private orderAsNumbers(a, b): number {
        return a - b;
    }

    private calculateObjectPositions(graph: FlowGraph) {
        graph.getNodes().forEach(node => {
            if (node != null) {
                const processData = node.getProcessData();

                // Place the node onthe center of the cell.
                const x = this.colOffsets[processData.col] + 0.5 * (this.colSizes[processData.col] - node.size.width);
                const y = this.rowOffsets[processData.row] + 0.5 * (this.rowSizes[processData.row] - node.size.height);

                node.position = {x: x, y: y};
            }
        });
    }

    private calculateObjectSizes(graph: FlowGraph) {
        // Calculate the width and height for all visible objects,
        // that are the nodes and the collapsed, but visible flows.
        this.getVisibileObjects(graph).forEach(visibleObject => {
            if (visibleObject != null) {
                let width = visibleObject.size.width;

                if (!this.isNumber(width)) {
                    // Use the default width for a node when the width is not a valid number.
                    width = this.nodeDefaultWidth;
                }

                let height = visibleObject.size.height;

                if (!this.isNumber(height)) {
                    // Use the default width for a node when the width is not a valid number.
                    height = this.nodeDefaultHeight;
                }

                visibleObject.size = {width: width, height: height};
            }
        });
    }

    private isNumber(n: any) {
        return !isNaN(parseFloat(n)) && isFinite(n);
    }

    private calculateTableSizes(graph: FlowGraph) {
        // Updates the column and row size for all visible objects of the
        // graph that are the nodes and the collapsed, but visible flows.
        this.getVisibileObjects(graph).forEach(visibleObject => {
            if (visibleObject != null) {
                const processData = visibleObject.getProcessData();

                // Calculate the width for the column by the width of the object and the double cell spacing.
                const width = visibleObject.size.width + 2 * this.horizontalCellSpacing;

                // Update the width of the column by calculating the maximum width of all objects of this column.
                if (processData.col in this.colSizes) {
                    this.colSizes[processData.col] = Math.max(width, this.colSizes[processData.col]);
                } else {
                    this.colSizes[processData.col] = width;
                }

                // Calculate the width for the column by the height of the object and the double cell spacing.
                let height = visibleObject.size.height + 2 * this.verticalCellSpacing;
                if (processData.row === 0) {
                    height += 15;
                }

                // Update the height of the row by calculating the maximum height of all objects of this column.
                if (processData.row in this.rowSizes) {
                    this.rowSizes[processData.row] = Math.max(height, this.rowSizes[processData.row]);
                } else {
                    this.rowSizes[processData.row] = height;
                }
            }
        });
    }

    private getVisibileObjects(graph: FlowGraph): Array<FlowGraphObject> {
        const visibleObject: Array<FlowGraphObject> = graph.getNodes().map(node => node);

        graph.getAlternateFlows().forEach(flow => {
            if (!flow.hidden && flow.collapsed) {
                visibleObject.push(flow);
            }
        });

        return visibleObject;
    }

    private calculateRows(graph: FlowGraph) {
        this.calculateRowsInternal(graph, graph.getMainFlow(), 0);
    }

    /**
     * Calculate row indicies based on nodes amount (in this flow and in children alternate flows)
     *
     * @param graph
     * @param flow
     * @param rowIndex
     *
     * @return Max row in this flow and child flows</returns>
     */
    private calculateRowsInternal(graph: FlowGraph, flow: Flow, rowIndex: number): number {
        if (flow.collapsed) {
            // Assign the row index to the flow when it is collapsed.
            flow.setRow(rowIndex);

            //Assign rowIndex of the first node (equals to flow's rowIndex)
            flow.getFirstNode().setRow(rowIndex);

            return rowIndex; // max row
        } else {
            let maxRow = rowIndex;
            let previousNode = null;
            flow.getNodes().forEach(node => {
                if (node != null) {
                    // For branches: get proper row
                    const branchingNode = node.getAlternateFlows().length > 0;
                    if (branchingNode) {
                        rowIndex = rowIndex > (maxRow + 1) ? rowIndex : maxRow + 1;
                    }

                    let maxAltFlowsRow = 0;

                    // Calculates the column index for all alternate flows of the node.
                    node.getAlternateFlows().forEach(subFlow => {
                        const r = this.calculateRowsInternal(graph, subFlow, rowIndex + 1);

                        maxAltFlowsRow = r > maxAltFlowsRow ? r : maxAltFlowsRow;
                    });
                    if (!this.disableVerticalDisplacement) {
                        // For simple steps (not branches): get proper row
                        const maxIndex = this.getMaxRowIndex(graph, flow, node, previousNode, rowIndex);
                        if (maxIndex >= rowIndex) {
                            // Update the row index to the maximum index of the nodes with
                            // an incoming connection plus one to avoid upward connections.
                            rowIndex = maxIndex + 1;
                        }
                    }
                    node.setRow(rowIndex);

                    maxRow = Math.max(Math.max(maxRow, rowIndex), maxAltFlowsRow);
                    rowIndex++;
                    previousNode = node;
                }
            });
            return maxRow;
        }
    }

    // Calculate the maximum row index of all nodes that have a connection
    // to the current node and where the start node of the related flow
    // has a less index than the index of the current node.
    private getMaxRowIndex(graph: FlowGraph, currentFlow: Flow, node: Node, previousNode: Node, rowIndex: number): number {
        let maxRowIndex = -1;
        graph.getAlternateFlows().forEach(f => {
            let effectiveEndNode = node;
            const branchingEndNode = f.endNode != null && f.endNode.getAlternateFlows().length > 0;
            if (branchingEndNode) {
                if (previousNode != null) {
                    effectiveEndNode = previousNode;
                }
            }
            if (f !== currentFlow && currentFlow.isChild(f) && f.endNode === effectiveEndNode && f.startNode.getRow() < rowIndex) {
                maxRowIndex = Math.max(maxRowIndex, this.getFinalObject(f.getLastNode()).getRow());
            }
        });
        return maxRowIndex;
    }

    private calculateColumns(graph: FlowGraph) {
        this.calculateColumnsInternal(graph, graph.getMainFlow(), Number.MAX_VALUE, new MinMax(), true);
    }

    private calculateColumnsInternal(graph: FlowGraph, flow: Flow, previousColumn: number, minMax: MinMax, isMainFlow: boolean) {
        // Get the index value for the current flow.
        const column = this.getColumnIndex(graph, flow, previousColumn, minMax);

        // Update the minimum and maximum value of the min max object.
        minMax.update(column);

        if (flow.collapsed) {
            // Assign the column index to the flow when it is collapsed.
            flow.setCol(column);

            //Assign columnIndex of the first node (equals to flow's columnIndex)
            flow.getFirstNode().setCol(column);
        } else {
            flow.getNodes().forEach(node => {
                if (isMainFlow) {
                    // Reset the min max value foreach node when the current flow is
                    // the main flow of the graph.
                    minMax = new MinMax();
                }

                node.setCol(column);

                // Calculates the column index for all alternate flows of the node.
                node.getAlternateFlows().forEach(subFlow => {
                    this.calculateColumnsInternal(graph, subFlow, column, minMax, false);
                });
            });
        }

        return minMax;
    }

    private getColumnIndex(graph: FlowGraph, flow: Flow, previousColumn: number, minMax: MinMax): number {
        let column = 0;

        // The maximum value means that the this is the main flow.
        if (previousColumn !== Number.MAX_VALUE) {
            if (previousColumn === 0) {
                // If the maximum and minimum are both equals to zero this means
                // that there current node has no alternate flows yet.
                if (minMax.max === 0 && minMax.min === 0) {
                    const firstNode = flow.getFirstNode();

                    if (firstNode != null) {
                        // Is there a node just one or two nodes above, place the flow on the other side of the main flow.
                        if (this.flipSide(graph, firstNode)) {
                            this.isRightColumn = !this.isRightColumn;
                        }
                    }
                }

                // Use an alternating algorithm to place the flow on the right side first
                // and then on the left side of the main flow and so on.
                if (this.isRightColumn) {
                    column = minMax.max + 1;
                } else {
                    column = minMax.min - 1;
                }

                this.isRightColumn = !this.isRightColumn;
            } else if (previousColumn > 0) {
                // The previous column is on the right side of the main flow.
                column = minMax.max + 1;
            } else {
                // The previous column is on the left side of the main flow.
                column = minMax.min - 1;
            }
        }

        return column;
    }

    private flipSide(graph: FlowGraph, node: Node): boolean {
        // Get the row index of the current flow by calculating the
        // row index of the first node of the flow.
        const processData = node.getProcessData();

        const prevRow1 = processData.row - 1;
        const prevRow2 = processData.row - 2;

        // Is there a node just one or two nodes above, place the flow on the other side of the main flow.
        return graph.getNodes().map(n => n.getProcessData()).some(x => (x.row === prevRow1 || x.row === prevRow2) && x.col === 1);
    }

    private verifyGraph(graph: FlowGraph) {
        graph.getAlternateFlows().forEach(alternateFlow => {
            if (alternateFlow != null) {
                if (alternateFlow.getNodes().length === 0) {
                    throw new Error("At least one flow has no nodes");
                }

                if (alternateFlow.startNode == null) {
                    throw new Error("At least one alternate row has no start node");
                }

                if (alternateFlow.endNode == null) {
                    throw new Error("At least one alternate row has no end node");
                }
            }
        });

        graph.getNodes().forEach(node => {
            if (node != null && node.flow == null) {
                throw new Error("At least one node is not part of any flow");
            }
        });
    }
}
