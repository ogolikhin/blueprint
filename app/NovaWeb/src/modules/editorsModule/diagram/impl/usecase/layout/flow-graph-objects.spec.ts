import {AlternateFlow, Node} from "./flow-graph-objects";
import {Rect} from "./rect";
import {FlowGraph} from "./flow-graph";
import {LayoutResult} from "./layout-result";
import {ConnectionInfo} from "./connection-info";

describe("FlowGraphObjects", () => {
    it("Flow.addNode method throws error 'Node is already part of another flow'", () => {
        //arrange
        const flow = new AlternateFlow();
        const node = new Node();
        node.flow = new AlternateFlow();
        //act
        const act = () => {
            flow.addNode(node);
        };
        //assert
        expect(act).toThrow(Error("Node is already part of another flow"));
    });
    it("Node.addAlternateFlow method throws error 'Alternate flow is already assigned'", () => {
        //arrange
        const flow = new AlternateFlow();
        flow.startNode = new Node();
        const node = new Node();
        //act
        const act = () => {
            node.addAlternateFlow(flow);
        };
        //assert
        expect(act).toThrow(Error("Alternate flow is already assigned"));
    });
    it("Rect.union method does not change rectangle dimensions when null passed as a parameter", () => {
        //arrange
        const rect = new Rect(100, 100, 100, 100);
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
        const graph = new FlowGraph();
        const result = new LayoutResult(graph);
        //act
        const actualGraph = result.getGraph();
        //assert
        expect(actualGraph).toEqual(graph);
    });
    it("ConnectionInfo.getLastPoint() returns 'undefined' when no points", () => {
        //arrange
        const connectionInfo = new ConnectionInfo();
        //act
        const point = connectionInfo.getLastPoint();
        //assert
        expect(point).toBeUndefined();
    });
});
