import {AlternateFlow, Node} from "./flow-graph-objects";
import {Rect} from "./rect";
import {FlowGraph} from "./flow-graph";
import {LayoutResult} from "./layout-result";
import {ConnectionInfo} from "./connection-info";


describe("FlowGraphObjects", () => {
    it("Flow.addNode method throws error 'Node is already part of another flow'", () => {
        //arrange
        var flow = new AlternateFlow();
        var node = new Node();
        node.flow = new AlternateFlow();
        //act
        var act = () => {
            flow.addNode(node);
        }
        //assert
        expect(act).toThrow(Error("Node is already part of another flow"));
    });
    it("Node.addAlternateFlow method throws error 'Alternate flow is already assigned'", () => {
        //arrange
        var flow = new AlternateFlow();
        flow.startNode = new Node();
        var node = new Node();
        //act
        var act = () => {
            node.addAlternateFlow(flow);
        }
        //assert
        expect(act).toThrow(Error("Alternate flow is already assigned"));
    });
    it("Rect.union method does not change rectangle dimensions when null passed as a parameter", () => {
        //arrange
        var rect = new Rect(100, 100, 100, 100);
        //act
        rect.union(null);
        //assert
        expect(rect.x).toEqual(100);
        expect(rect.y).toEqual(100);
        expect(rect.width).toEqual(100);
        expect(rect.height).toEqual(100);
    });
    it("LayoutResult.getGraph() returns graph", () => {
        //arrange
        var graph = new FlowGraph();
        var result = new LayoutResult(graph);
        //act
        var actualGraph = result.getGraph();
        //assert
        expect(actualGraph).toEqual(graph);
    });
    it("ConnectionInfo.getLastPoint() returns 'undefined' when no points", () => {
        //arrange
        var connectionInfo = new ConnectionInfo();
        //act
        var point = connectionInfo.getLastPoint();
        //assert
        expect(point).toBeUndefined();
    });
});