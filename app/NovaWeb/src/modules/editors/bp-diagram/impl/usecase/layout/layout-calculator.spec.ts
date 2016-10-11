import {LayoutCalculator} from "./layout-calculator";
import {FlowGraph} from "./flow-graph";
import {AlternateFlow, Node} from "./flow-graph-objects";
import {ConnectionInfo} from "./connection-info";
import {UsecaseToDiagram} from "../usecase-to-diagram";


/* tslint:disable:max-line-length */
describe("LayoutCalculator ", () => {
    var calculator: LayoutCalculator;
    beforeEach(() => {
        calculator = new LayoutCalculator();
        calculator.nodeDefaultHeight = UsecaseToDiagram.TRUNCATED_HEIGHT;
        calculator.nodeDefaultWidth = UsecaseToDiagram.TRUNCATED_WIDTH;
        calculator.verticalCellSpacing = UsecaseToDiagram.CELL_SPACING;
        calculator.horizontalCellSpacing = 2 * UsecaseToDiagram.CELL_SPACING;
        calculator.flowSpacing = UsecaseToDiagram.FLOW_SPACING;
        calculator.disableVerticalDisplacement = false;
    });
    var createNode = (graph: FlowGraph): Node => {
        var node = graph.createNode();
        node.size = {width: 150, height: 60};
        return node;
    };
    var createBranchingNode = (graph: FlowGraph): Node => {
        var node = graph.createNode();
        node.size = {width: 30, height: 30};
        return node;
    };

    it("verifyGraph method throws the error 'At least one flow has no nodes'", () => {
        //arrange
        var graph = new FlowGraph();
        graph.createAlternateFlow();
        //act
        var act = () => {
            calculator.arrangeGraph(graph);
        };
        //assert
        expect(act).toThrow(Error("At least one flow has no nodes"));
    });

    it("verifyGraph method throws the error 'At least one alternate row has no start node'", () => {
        //arrange
        var graph = new FlowGraph();
        var alternateFlow = graph.createAlternateFlow();
        alternateFlow.addNode(graph.createNode());
        //act
        var act = () => {
            calculator.arrangeGraph(graph);
        };
        //assert
        expect(act).toThrow(Error("At least one alternate row has no start node"));
    });

    it("verifyGraph method throws the error 'At least one alternate row has no end node'", () => {
        //arrange
        var graph = new FlowGraph();
        var node = graph.createNode();
        graph.getMainFlow().addNode(node);
        var alternateFlow = graph.createAlternateFlow();
        alternateFlow.addNode(graph.createNode());
        node.addAlternateFlow(alternateFlow);
        //act
        var act = () => {
            calculator.arrangeGraph(graph);
        };
        //assert
        expect(act).toThrow(Error("At least one alternate row has no end node"));
    });

    it("verifyGraph method throws the error 'At least one node is not part of any flow", () => {
        //arrange
        var graph = new FlowGraph();
        graph.createNode();
        //act
        var act = () => {
            calculator.arrangeGraph(graph);
        };
        //assert
        expect(act).toThrow(Error("At least one node is not part of any flow"));
    });

    it("arrangeGraph method: correct nodes' position (3 nodes in main flow)", () => {
        //arrange
        var node: Node;
        var graph = new FlowGraph();
        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        //act
        calculator.arrangeGraph(graph);
        //assert
        var allNodes = graph.getMainFlow().getNodes();
        expect(allNodes.length).toEqual(3);
        expect(allNodes[0].position).toEqual({x: 30, y: 22.5});
        expect(allNodes[1].position).toEqual({x: 30, y: 120});
        expect(allNodes[2].position).toEqual({x: 30, y: 210});
    });

    it("arrangeGraph method: correct number of connectors (3 nodes in main flow)", () => {
        //arrange
        var node: Node;
        var graph = new FlowGraph();
        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        //act
        var result = calculator.arrangeGraph(graph);
        //assert
        expect(result.getConnections().length).toEqual(2);
    });

    it("arrangeGraph method: correct nodes' position (2 alternate flows on the same node in main flow)", () => {
        //arrange
        var node: Node;
        var alternateFlow: AlternateFlow;
        var graph = new FlowGraph();
        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        node = createBranchingNode(graph);
        var branchingNode = node;
        graph.getMainFlow().addNode(branchingNode);
        //first alternate flow
        alternateFlow = graph.createAlternateFlow();
        branchingNode.addAlternateFlow(alternateFlow);
        node = createNode(graph);
        alternateFlow.addNode(node);
        //second alternate flow
        alternateFlow = graph.createAlternateFlow();
        branchingNode.addAlternateFlow(alternateFlow);
        //condition
        node = createNode(graph);
        alternateFlow.addNode(node);

        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        //assign end nodes
        graph.getAlternateFlows().forEach((a: AlternateFlow) => {
            a.endNode = node;
        });

        //act
        calculator.arrangeGraph(graph);
        //assert
        var allNodes = graph.getMainFlow().getNodes();
        expect(allNodes.length).toEqual(4);
        expect(allNodes[0].position).toEqual({x: 240, y: 22.5});
        expect(allNodes[1].position).toEqual({x: 300, y: 120});
        expect(allNodes[2].position).toEqual({x: 240, y: 180});
        expect(allNodes[3].position).toEqual({x: 240, y: 270});
        var alternateFlows = graph.getAlternateFlows();
        expect(alternateFlows.length).toEqual(2);
        expect(alternateFlows[0].getFirstNode().position).toEqual({x: 450, y: 180});
        expect(alternateFlows[1].getFirstNode().position).toEqual({x: 30, y: 180});
    });

    it("arrangeGraph method: correct branching node connections' points (2 alternate flows on the same node in main flow)", () => {
        //arrange
        var node: Node;
        var alternateFlow: AlternateFlow;
        var graph = new FlowGraph();
        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        node = createBranchingNode(graph);
        var branchingNode = node;
        graph.getMainFlow().addNode(branchingNode);
        //first alternate flow
        alternateFlow = graph.createAlternateFlow();
        branchingNode.addAlternateFlow(alternateFlow);
        node = createNode(graph);
        alternateFlow.addNode(node);
        //second alternate flow
        alternateFlow = graph.createAlternateFlow();
        branchingNode.addAlternateFlow(alternateFlow);
        //second alternate flow condition
        node = createNode(graph);
        alternateFlow.addNode(node);

        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        //assign end nodes
        graph.getAlternateFlows().forEach((a: AlternateFlow) => {
            a.endNode = node;
        });

        //act
        var result = calculator.arrangeGraph(graph);
        //assert
        var branchingNodeConnectors = result.getConnections().filter((c: ConnectionInfo) => {
            return c.startNode === branchingNode;
        });
        expect(branchingNodeConnectors.length).toEqual(3);
        expect(branchingNodeConnectors[0].getPoints()).toEqual([]);
        expect(branchingNodeConnectors[1].getPoints()).toEqual([{x: 330, y: 135}, {x: 525, y: 135}, {x: 525, y: 180}]);
        expect(branchingNodeConnectors[2].getPoints()).toEqual([{x: 300, y: 135}, {x: 105, y: 135}, {x: 105, y: 180}]);
    });

    it("arrangeGraph method: correct nodes' position (1 alternate flow on a node in main flow and one nested alternate flow)", () => {
        //arrange
        var node: Node;
        var graph = new FlowGraph();
        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        var node2 = createNode(graph);
        graph.getMainFlow().addNode(node2);

        var branchingNode = createBranchingNode(graph);
        graph.getMainFlow().addNode(branchingNode);

        //first alternate flow
        var alternateFlow1 = graph.createAlternateFlow();
        branchingNode.addAlternateFlow(alternateFlow1);
        //condition
        node = createNode(graph);
        alternateFlow1.addNode(node);

        branchingNode = createBranchingNode(graph);
        alternateFlow1.addNode(branchingNode);

        //nested alternate flow
        var nestedAlternateFlow = graph.createAlternateFlow();
        branchingNode.addAlternateFlow(nestedAlternateFlow);
        //nested alternate flow condition
        node = createNode(graph);
        nestedAlternateFlow.addNode(node);
        nestedAlternateFlow.endNode = node2;

        node = createNode(graph);
        alternateFlow1.addNode(node);
        node = createNode(graph);
        alternateFlow1.addNode(node);

        //Main flow nodes
        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        node = createNode(graph);
        graph.getMainFlow().addNode(node);

        alternateFlow1.endNode = node;

        //act
        calculator.arrangeGraph(graph);
        //assert
        var allNodes = graph.getMainFlow().getNodes();
        expect(allNodes.length).toEqual(6);
        expect(allNodes[0].position).toEqual({x: 30, y: 22.5});
        expect(allNodes[1].position).toEqual({x: 30, y: 120});
        expect(allNodes[2].position).toEqual({x: 90, y: 210});
        expect(allNodes[3].position).toEqual({x: 30, y: 270});
        expect(allNodes[4].position).toEqual({x: 30, y: 360});
        expect(allNodes[5].position).toEqual({x: 30, y: 630});

        expect(alternateFlow1.getNodes().length).toEqual(4);
        expect(alternateFlow1.getNodes()[0].position).toEqual({x: 240, y: 270});
        expect(alternateFlow1.getNodes()[1].position).toEqual({x: 300, y: 375});
        expect(alternateFlow1.getNodes()[2].position).toEqual({x: 240, y: 450});
        expect(alternateFlow1.getNodes()[3].position).toEqual({x: 240, y: 540});

        expect(nestedAlternateFlow.getNodes().length).toEqual(1);
        expect(nestedAlternateFlow.getNodes()[0].position).toEqual({x: 450, y: 450});
    });

    it("arrangeGraph method: correct end nodes connections' points (1 alternate flow on a node in main flow and one nested alternate flow)", () => {
        //arrange
        var node: Node;
        var graph = new FlowGraph();
        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        var node2 = createNode(graph);
        graph.getMainFlow().addNode(node2);

        var branchingNode1 = createBranchingNode(graph);
        graph.getMainFlow().addNode(branchingNode1);

        //first alternate flow
        var alternateFlow1 = graph.createAlternateFlow();
        branchingNode1.addAlternateFlow(alternateFlow1);
        //condition
        node = createNode(graph);
        alternateFlow1.addNode(node);

        var branchingNode2 = createBranchingNode(graph);
        alternateFlow1.addNode(branchingNode2);

        //nested alternate flow
        var nestedAlternateFlow = graph.createAlternateFlow();
        branchingNode2.addAlternateFlow(nestedAlternateFlow);
        //nested alternate flow condition
        node = createNode(graph);
        nestedAlternateFlow.addNode(node);
        nestedAlternateFlow.endNode = node2;

        node = createNode(graph);
        alternateFlow1.addNode(node);
        node = createNode(graph);
        alternateFlow1.addNode(node);

        //Main flow nodes
        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        node = createNode(graph);
        graph.getMainFlow().addNode(node);

        alternateFlow1.endNode = node;

        //act
        var result = calculator.arrangeGraph(graph);
        //assert
        var endNodeConnectors = result.getConnections().filter((c: ConnectionInfo) => {
            return c.isReturnConnector;
        });
        expect(endNodeConnectors.length).toEqual(2);
        expect(endNodeConnectors[0].getPoints()).toEqual([{x: 315, y: 600}, {x: 315, y: 615}, {x: 105, y: 615}, {
            x: 105,
            y: 630
        }]);
        expect(endNodeConnectors[1].getPoints()).toEqual([{x: 525, y: 510}, {x: 525, y: 525}, {x: 195, y: 525}, {
            x: 195,
            y: 105
        }, {x: 105, y: 105}, {x: 105, y: 120}]);
    });

    it("arrangeGraph method: correct end nodes connections' points (2 alternate flows on the second node in main flow and 2 alternate flows on the third node in main flow)", () => {
        //arrange
        var node: Node;
        var graph = new FlowGraph();
        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        var node2 = createNode(graph);
        graph.getMainFlow().addNode(node2);

        var branchingNode1 = createBranchingNode(graph);
        graph.getMainFlow().addNode(branchingNode1);

        //first alternate flow
        var alternateFlow1 = graph.createAlternateFlow();
        branchingNode1.addAlternateFlow(alternateFlow1);
        //first alternate flow condition
        node = createNode(graph);
        alternateFlow1.addNode(node);

        //second alternate flow
        var alternateFlow2 = graph.createAlternateFlow();
        branchingNode1.addAlternateFlow(alternateFlow2);
        //second alternate flow condition
        node = createNode(graph);
        alternateFlow2.addNode(node);

        node = createNode(graph);
        graph.getMainFlow().addNode(node);

        var branchingNode2 = createBranchingNode(graph);
        graph.getMainFlow().addNode(branchingNode2);

        //third alternate flow
        var alternateFlow3 = graph.createAlternateFlow();
        branchingNode2.addAlternateFlow(alternateFlow3);
        //first alternate flow condition
        node = createNode(graph);
        alternateFlow3.addNode(node);

        //forth alternate flow
        var alternateFlow4 = graph.createAlternateFlow();
        branchingNode2.addAlternateFlow(alternateFlow4);
        //second alternate flow condition
        node = createNode(graph);
        alternateFlow4.addNode(node);

        node = createNode(graph);
        graph.getMainFlow().addNode(node);

        node = createNode(graph);
        graph.getMainFlow().addNode(node);

        alternateFlow1.endNode = node;
        alternateFlow2.endNode = node;

        alternateFlow3.endNode = node2;
        alternateFlow4.endNode = node2;

        //act
        var result = calculator.arrangeGraph(graph);
        //assert
        var endNodeConnectors = result.getConnections().filter((c: ConnectionInfo) => {
            return c.isReturnConnector;
        });
        expect(endNodeConnectors.length).toEqual(4);
        expect(endNodeConnectors[0].getPoints()).toEqual([{x: 525, y: 330}, {x: 525, y: 405}, {x: 405, y: 405}, {
            x: 405,
            y: 495
        }, {x: 315, y: 495}, {x: 315, y: 510}]);
        expect(endNodeConnectors[1].getPoints()).toEqual([{x: 105, y: 330}, {x: 105, y: 405}, {x: 225, y: 405}, {
            x: 225,
            y: 495
        }, {x: 315, y: 495}, {x: 315, y: 510}]);
        expect(endNodeConnectors[2].getPoints()).toEqual([{x: 105, y: 480}, {x: 105, y: 495}, {x: 225, y: 495}, {
            x: 225,
            y: 105
        }, {x: 315, y: 105}, {x: 315, y: 120}]);
        expect(endNodeConnectors[3].getPoints()).toEqual([{x: 525, y: 480}, {x: 525, y: 495}, {x: 405, y: 495}, {
            x: 405,
            y: 105
        }, {x: 315, y: 105}, {x: 315, y: 120}]);
    });

    it("arrangeGraph method: correct nodes' position (1 alternate flows and 1 collapsed alternate flow on the same node in main flow)", () => {
        //arrange
        var node: Node;
        var alternateFlow: AlternateFlow;
        var graph = new FlowGraph();
        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        node = createBranchingNode(graph);
        var branchingNode = node;
        graph.getMainFlow().addNode(branchingNode);
        //first alternate flow
        alternateFlow = graph.createAlternateFlow();
        branchingNode.addAlternateFlow(alternateFlow);
        //first alternate flow condition
        node = createNode(graph);
        alternateFlow.addNode(node);
        node = createNode(graph);
        alternateFlow.addNode(node);
        node = createNode(graph);
        alternateFlow.addNode(node);
        //second alternate flow
        alternateFlow = graph.createAlternateFlow();
        branchingNode.addAlternateFlow(alternateFlow);
        //second alternate flow condition
        node = createNode(graph);
        alternateFlow.addNode(node);

        node = createNode(graph);
        alternateFlow.addNode(node);
        node = createNode(graph);
        alternateFlow.addNode(node);

        alternateFlow.collapsed = true;

        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        node = createNode(graph);
        graph.getMainFlow().addNode(node);
        //assign end nodes
        graph.getAlternateFlows().forEach((a: AlternateFlow) => {
            a.endNode = node;
        });

        //act
        calculator.arrangeGraph(graph);
        //assert
        var allNodes = graph.getMainFlow().getNodes();
        expect(allNodes.length).toEqual(4);
        expect(allNodes[0].position).toEqual({x: 240, y: 22.5});
        expect(allNodes[1].position).toEqual({x: 300, y: 120});
        expect(allNodes[2].position).toEqual({x: 240, y: 180});
        expect(allNodes[3].position).toEqual({x: 240, y: 450});
        var alternateFlows = graph.getAlternateFlows();
        expect(alternateFlows.length).toEqual(2);
        expect(alternateFlows[0].position).toEqual({x: -5, y: -5});
        expect(alternateFlows[0].size).toEqual({width: 610, height: 430});

        expect(alternateFlows[1].position).toEqual({x: 25, y: 175});
        expect(alternateFlows[1].size).toEqual({width: 160, height: 70});
    });
});
