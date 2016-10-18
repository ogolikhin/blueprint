import {UsecaseToDiagram, UsecaseFlowGraphBuilder, FlowGraphDiagramBuilder} from "./usecase-to-diagram";
import {BranchingStep, ExitStep, UseCaseShape, PrePostConditionShape} from "./usecase-to-diagram";
import {IStep, IFlow, IUseCase, StepOfType, IUseCaseElement} from "./models";
import {FlowGraph} from "./layout/flow-graph";
import {ConnectionInfo} from "./layout/connection-info";
import {LayoutResult} from "./layout/layout-result";
import {ConnectorTypes, Shapes} from "../utils/constants";

var nextIndexGenerator = () => {
    var orderIndex = 0;
    var generate = () => {
        return orderIndex++;
    };
    return generate;
};

var nextStepOrderIndex = nextIndexGenerator();
var nextFlowOrderIndex = nextIndexGenerator();

var createStep = (id: number, name: string, description?: string): IStep => {
    return {
        id: id,
        name: name,
        orderIndex: nextStepOrderIndex(),
        description: description,
        stepOf: StepOfType.System,
        flows: [],
        condition: false,
        external: null
    };
};
var createFlow = (id: number, name: string, description?: string) => {
    var flow: IFlow = {
        id: id,
        name: name,
        orderIndex: nextFlowOrderIndex(),
        isExternal: false,
        steps: [],
        returnToStepName: null
    };
    return flow;
};

describe("UsecaseToDiagram ", () => {
    var usecase: IUseCase;

    var preCondition: IStep;
    var postCondition: IStep;

    beforeEach(() => {
        preCondition = createStep(1, "Pre condition", "Pre condition");
        postCondition = createStep(2, "Post condition", "Post condition");
        usecase = {
            id: 0,
            preCondition: preCondition,
            steps: [],
            postCondition: postCondition
        };
    });

    it("convert method: calls UsecaseFlowGraphBuilder and FlowGraphDiagramBuilder", () => {
        //arrange

        var usecaseToDiagram = new UsecaseToDiagram();

        var buildGraph = spyOn(UsecaseFlowGraphBuilder.prototype, "buildGraph").and.callThrough();
        var buildDiagram = spyOn(FlowGraphDiagramBuilder.prototype, "buildDiagram").and.callThrough();

        //Act

        var diagram = usecaseToDiagram.convert(usecase);

        //Assert

        expect(diagram).not.toBeNull();

        expect(buildGraph).toHaveBeenCalled();
        expect(buildDiagram).toHaveBeenCalled();
    });
});

describe("FlowGraphDiagramBuilder ", () => {
    var usecase: IUseCase;

    var preCondition: IStep;
    var postCondition: IStep;

    var defaultPosition = {x: 0, y: 0};
    var defaultSize = {width: 100, height: 100};

    beforeEach(() => {
        preCondition = createStep(1, "Pre condition", "Pre condition");
        postCondition = createStep(2, "Post condition", "Post condition");
        usecase = {
            id: 0,
            preCondition: preCondition,
            steps: [],
            postCondition: postCondition
        };
    });

    it("buildDiagram method: 'straight connector'", () => {
        //arrange

        var step = createStep(3, "Step 1", "First step");
        usecase.steps.push(step);

        var graph = new FlowGraph();

        var node = graph.createNode();
        node.position = defaultPosition;
        node.size = defaultSize;
        node.tag = usecase.preCondition;
        graph.getMainFlow().addNode(node);

        node = graph.createNode();
        node.position = defaultPosition;
        node.size = defaultSize;
        node.tag = usecase.steps[0];
        graph.getMainFlow().addNode(node);

        node = graph.createNode();
        node.position = defaultPosition;
        node.size = defaultSize;
        node.tag = usecase.postCondition;
        graph.getMainFlow().addNode(node);

        var layoutResult = new LayoutResult(graph);

        var connection = new ConnectionInfo();
        connection.startNode = graph.getNodes()[0];
        connection.endNode = graph.getNodes()[1];
        layoutResult.getConnections().push(connection);

        //Act

        var diagramBuilder = new FlowGraphDiagramBuilder();
        var diagram = diagramBuilder.buildDiagram(layoutResult, usecase);

        //Assert

        expect(diagram.connections.length).toEqual(1);
        expect(diagram.connections[0].type).toEqual(ConnectorTypes.STRAIGHT);
        expect(diagram.connections[0].sourceId).toEqual(graph.getNodes()[0].tag.id);
        expect(diagram.connections[0].targetId).toEqual(graph.getNodes()[1].tag.id);
    });

    it("buildDiagram method: 'right-angled connector'", () => {
        //arrange

        var step = createStep(3, "Step 1", "First step");

        var ucflow = createFlow(4, "AlternateFlow");
        var condition = createStep(5, "Step 1", "First step");
        ucflow.steps.push(condition);
        step.flows.push(ucflow);
        usecase.steps.push(step);

        var graph = new FlowGraph();

        var node = graph.createNode();
        node.position = defaultPosition;
        node.size = defaultSize;
        node.tag = usecase.preCondition;
        graph.getMainFlow().addNode(node);

        node = graph.createNode();
        node.position = defaultPosition;
        node.size = defaultSize;
        node.tag = new BranchingStep(-usecase.steps[0].id);
        graph.getMainFlow().addNode(node);

        node = graph.createNode();
        node.position = defaultPosition;
        node.size = defaultSize;
        node.tag = usecase.steps[0];
        graph.getMainFlow().addNode(node);

        var flow = graph.createAlternateFlow();
        var conditionNode = graph.createNode();
        conditionNode.position = {x: 100, y: 100};
        conditionNode.size = defaultSize;
        conditionNode.tag = usecase.steps[0].flows[0].steps[0];
        flow.addNode(conditionNode);

        node = graph.createNode();
        node.position = defaultPosition;
        node.size = defaultSize;
        node.tag = usecase.postCondition;
        graph.getMainFlow().addNode(node);

        var layoutResult = new LayoutResult(graph);

        var connection = new ConnectionInfo();
        connection.startNode = graph.getNodes()[1];
        connection.endNode = conditionNode;
        connection.addPointToXy(0, 0);
        connection.addPointToXy(0, 0);
        connection.addPointToXy(0, 0);

        layoutResult.getConnections().push(connection);

        //Act

        var diagramBuilder = new FlowGraphDiagramBuilder();
        var diagram = diagramBuilder.buildDiagram(layoutResult, usecase);

        //Assert

        expect(diagram.connections.length).toEqual(1);
        expect(diagram.connections[0].type).toEqual(ConnectorTypes.RIGHT_ANGLED);
        expect(diagram.connections[0].sourceId).toEqual(graph.getNodes()[1].tag.id);
        expect(diagram.connections[0].targetId).toEqual(conditionNode.tag.id);

        expect(diagram.connections[0].points).toEqual(connection.getPoints());
    });

    it("buildDiagram method: '3 shapes - checks position and size'", () => {
        //arrange

        var step = createStep(3, "Step 1", "First step");
        usecase.steps.push(step);

        var graph = new FlowGraph();

        var node = graph.createNode();
        node.position = {x: 0, y: 0};
        node.size = {width: 100, height: 100};
        node.tag = usecase.preCondition;
        graph.getMainFlow().addNode(node);

        node = graph.createNode();
        node.position = {x: 100, y: 100};
        node.size = {width: 100, height: 100};
        node.tag = usecase.steps[0];
        graph.getMainFlow().addNode(node);

        node = graph.createNode();
        node.position = {x: 200, y: 200};
        node.size = {width: 100, height: 100};
        node.tag = usecase.postCondition;
        graph.getMainFlow().addNode(node);

        var layoutResult = new LayoutResult(graph);

        //Act

        var diagramBuilder = new FlowGraphDiagramBuilder();
        var diagram = diagramBuilder.buildDiagram(layoutResult, usecase);

        //Assert

        expect(diagram.shapes.length).toEqual(3);
        expect(diagram.shapes[0].x).toEqual(0);
        expect(diagram.shapes[0].y).toEqual(0);
        expect(diagram.shapes[0].width).toEqual(100);
        expect(diagram.shapes[0].height).toEqual(100);

        expect(diagram.shapes[1].x).toEqual(100);
        expect(diagram.shapes[1].y).toEqual(100);
        expect(diagram.shapes[1].width).toEqual(100);
        expect(diagram.shapes[1].height).toEqual(100);

        expect(diagram.shapes[2].x).toEqual(200);
        expect(diagram.shapes[2].y).toEqual(200);
        expect(diagram.shapes[2].width).toEqual(100);
        expect(diagram.shapes[2].height).toEqual(100);
    });

    it("buildDiagram method: '5 shapes (Pre, post, branching, step, flow condition) - checks shape types'", () => {
        //arrange

        var step = createStep(3, "Step 1", "First step");

        var ucflow = createFlow(4, "AlternateFlow");
        var condition = createStep(5, "Step 1", "First step");
        ucflow.steps.push(condition);
        step.flows.push(ucflow);
        usecase.steps.push(step);

        var graph = new FlowGraph();

        var node = graph.createNode();
        node.position = defaultPosition;
        node.size = defaultSize;
        node.tag = usecase.preCondition;
        graph.getMainFlow().addNode(node);

        node = graph.createNode();
        node.position = defaultPosition;
        node.size = defaultSize;
        node.tag = new BranchingStep(-usecase.steps[0].id);
        graph.getMainFlow().addNode(node);

        node = graph.createNode();
        node.position = defaultPosition;
        node.size = defaultSize;
        node.tag = usecase.steps[0];
        graph.getMainFlow().addNode(node);

        var flow = graph.createAlternateFlow();
        var conditionNode = graph.createNode();
        conditionNode.position = {x: 100, y: 100};
        conditionNode.size = defaultSize;
        conditionNode.tag = usecase.steps[0].flows[0].steps[0];
        flow.addNode(conditionNode);

        node = graph.createNode();
        node.position = defaultPosition;
        node.size = defaultSize;
        node.tag = usecase.postCondition;
        graph.getMainFlow().addNode(node);

        var layoutResult = new LayoutResult(graph);

        //Act

        var diagramBuilder = new FlowGraphDiagramBuilder();
        var diagram = diagramBuilder.buildDiagram(layoutResult, usecase);

        //Assert

        expect(diagram.shapes.length).toEqual(5);
        expect(diagram.shapes[0].type).toEqual(Shapes.PRE_POST_CONDITION);
        expect(diagram.shapes[1].type).toEqual(Shapes.BRANCHING);
        expect(diagram.shapes[2].type).toEqual(Shapes.STEP);
        expect(diagram.shapes[3].type).toEqual(Shapes.STEP);
        expect(diagram.shapes[4].type).toEqual(Shapes.PRE_POST_CONDITION);
    });

    it("buildDiagram method: '6 shapes (Pre, post, branching, step, flow condition, exit)' - checks shape types", () => {
        //arrange

        var step = createStep(3, "Step 1", "First step");

        var ucflow = createFlow(4, "AlternateFlow");
        var condition = createStep(5, "Step 1", "First step");
        ucflow.steps.push(condition);
        step.flows.push(ucflow);
        usecase.steps.push(step);

        var graph = new FlowGraph();

        var node = graph.createNode();
        node.position = defaultPosition;
        node.size = defaultSize;
        node.tag = usecase.preCondition;
        graph.getMainFlow().addNode(node);

        node = graph.createNode();
        node.position = defaultPosition;
        node.size = defaultSize;
        node.tag = new BranchingStep(-usecase.steps[0].id);
        graph.getMainFlow().addNode(node);

        node = graph.createNode();
        node.position = defaultPosition;
        node.size = defaultSize;
        node.tag = usecase.steps[0];
        graph.getMainFlow().addNode(node);

        var flow = graph.createAlternateFlow();
        var conditionNode = graph.createNode();
        conditionNode.position = {x: 100, y: 100};
        conditionNode.size = defaultSize;
        conditionNode.tag = usecase.steps[0].flows[0].steps[0];
        flow.addNode(conditionNode);

        node = graph.createNode();
        node.position = defaultPosition;
        node.size = defaultSize;
        node.tag = new ExitStep(-usecase.steps[0].flows[0].id);
        flow.addNode(node);

        node = graph.createNode();
        node.position = defaultPosition;
        node.size = defaultSize;
        node.tag = usecase.postCondition;
        graph.getMainFlow().addNode(node);

        var layoutResult = new LayoutResult(graph);

        //Act

        var diagramBuilder = new FlowGraphDiagramBuilder();
        var diagram = diagramBuilder.buildDiagram(layoutResult, usecase);

        //Assert

        expect(diagram.shapes.length).toEqual(6);
        expect(diagram.shapes[0].type).toEqual(Shapes.PRE_POST_CONDITION);
        expect(diagram.shapes[1].type).toEqual(Shapes.BRANCHING);
        expect(diagram.shapes[2].type).toEqual(Shapes.STEP);
        expect(diagram.shapes[3].type).toEqual(Shapes.STEP);
        expect(diagram.shapes[4].type).toEqual(Shapes.EXIT);
        expect(diagram.shapes[5].type).toEqual(Shapes.PRE_POST_CONDITION);
    });
});

describe("UsecaseFlowGraphBuilder ", () => {
    var usecase: IUseCase;

    var preCondition: IStep;
    var postCondition: IStep;

    beforeEach(() => {
        preCondition = createStep(1, "Pre condition", "Pre condition");
        postCondition = createStep(2, "Post condition", "Post condition");
        usecase = {
            id: 0,
            preCondition: preCondition,
            steps: [],
            postCondition: postCondition
        };
    });

    it("buildGraph method: 'use case (pre condition, post condition and 1 step) '", () => {
        //arrange
        var graphBuilder = new UsecaseFlowGraphBuilder();
        var step = createStep(3, "Step 1", "First step");
        usecase.steps.push(step);
        //act
        var graph = graphBuilder.buildGraph(usecase);

        //assert
        expect(graph).not.toBeNull();
        expect(graph.getAlternateFlows().length).toEqual(0);

        expect(graph.getNodes().length).toEqual(3);
        expect(graph.getNodes()[0].tag).toEqual(usecase.preCondition);
        expect(graph.getNodes()[1].tag).toEqual(step);
        expect(graph.getNodes()[2].tag).toEqual(usecase.postCondition);
    });

    it("buildGraph method: 'use case (Alternate flow: return step = Post Condition) '", () => {
        //arrange
        var graphBuilder = new UsecaseFlowGraphBuilder();
        var step = createStep(3, "Step 1", "First step");
        var alternateFlow = createFlow(4, "Alternate Flow");
        alternateFlow.returnToStepName = usecase.postCondition.name;
        var flowCondition = createStep(5, "Step 1a", "Alternate flow condition");
        alternateFlow.steps.push(flowCondition);
        step.flows.push(alternateFlow);
        usecase.steps.push(step);

        //act
        var graph = graphBuilder.buildGraph(usecase);

        //assert
        expect(graph).not.toBeNull();
        expect(graph.getAlternateFlows().length).toEqual(1);

        expect(graph.getNodes().length).toEqual(5);
        expect(graph.getNodes()[0].tag).toEqual(usecase.preCondition);
        expect(graph.getNodes()[1].tag.id).toEqual(-step.id);
        expect(graph.getNodes()[2].tag).toEqual(step);
        expect(graph.getNodes()[3].tag).toEqual(flowCondition);
        expect(graph.getNodes()[4].tag).toEqual(usecase.postCondition);

        expect(graph.getAlternateFlows()[0].endNode).toEqual(graph.getNodes()[4]);
    });

    it("buildGraph method: 'use case (Alternate flow: return step = Exit) '", () => {
        //arrange
        var graphBuilder = new UsecaseFlowGraphBuilder();
        var step = createStep(3, "Step 1", "First step");
        var alternateFlow = createFlow(4, "Alternate Flow");
        alternateFlow.returnToStepName = "Exit";
        var flowCondition = createStep(5, "Step 1a", "Alternate flow condition");
        alternateFlow.steps.push(flowCondition);
        step.flows.push(alternateFlow);
        usecase.steps.push(step);

        //act
        var graph = graphBuilder.buildGraph(usecase);

        //assert
        expect(graph).not.toBeNull();
        expect(graph.getAlternateFlows().length).toEqual(1);

        expect(graph.getNodes().length).toEqual(6);
        expect(graph.getNodes()[0].tag).toEqual(usecase.preCondition);
        expect(graph.getNodes()[1].tag.id).toEqual(-step.id);
        expect(graph.getNodes()[2].tag).toEqual(step);
        expect(graph.getNodes()[3].tag).toEqual(flowCondition);
        expect(graph.getNodes()[4].tag.id).toEqual(-alternateFlow.id); //Fake exit node
        expect(graph.getNodes()[5].tag).toEqual(usecase.postCondition);
    });

    it("buildGraph method: 'use case (Alternate flow: return step = Step with alternate flow) '", () => {
        //arrange
        var graphBuilder = new UsecaseFlowGraphBuilder();
        var step = createStep(3, "Step 1", "First step");
        var alternateFlow = createFlow(4, "Alternate Flow");
        alternateFlow.returnToStepName = step.name;
        var flowCondition = createStep(5, "Step 1a", "Alternate flow condition");
        alternateFlow.steps.push(flowCondition);
        step.flows.push(alternateFlow);
        usecase.steps.push(step);

        //act
        var graph = graphBuilder.buildGraph(usecase);

        //assert
        expect(graph).not.toBeNull();
        expect(graph.getAlternateFlows().length).toEqual(1);

        expect(graph.getNodes().length).toEqual(5);
        expect(graph.getNodes()[0].tag).toEqual(usecase.preCondition);
        expect(graph.getNodes()[1].tag.id).toEqual(-step.id); //Branching node
        expect(graph.getNodes()[2].tag).toEqual(step);
        expect(graph.getNodes()[3].tag).toEqual(flowCondition);
        expect(graph.getNodes()[4].tag).toEqual(usecase.postCondition);

        expect(graph.getAlternateFlows()[0].endNode).toEqual(graph.getNodes()[1]);
    });
});

describe("UseCaseShape", () => {

    var useCaseElement: IUseCaseElement;

    beforeEach(() => {
        useCaseElement = {
            id: 1,
            name: "",
            orderIndex: 0
        };
    });

    it("id method: returns id of element", () => {
        //arrange

        var shape = new UseCaseShape();
        shape.element = useCaseElement;

        //Act

        var id = shape.id;

        //Assert
        expect(id).toEqual(1);
    });

    it("name method: returns name of element", () => {
        //arrange

        var shape = new UseCaseShape();
        useCaseElement.name = "Step 1a";
        shape.element = useCaseElement;

        //Act

        var name = shape.name;

        //Assert
        expect(name).toEqual("Step 1a");
    });

    it("label method: returns name and description of element", () => {
        //arrange

        var shape = new UseCaseShape();
        shape.element = useCaseElement;
        useCaseElement.name = "Step 1a";
        (<IStep>useCaseElement).description = "<p><span>description</span></p>";

        //Act

        var label = shape.label;

        //Assert
        expect(label).toEqual("<p><span style='font-size: 12px; line-height: 1.45000004768372'><b>1a: </b></span><span>description</span></p>");
    });

    it("label method: returns null when description is null", () => {
        //arrange

        var shape = new UseCaseShape();
        shape.element = useCaseElement;
        (<IStep>useCaseElement).description = null;

        //Act
        var label = shape.label;

        //Assert
        expect(label).toBeNull();
    });

    it("label method: returns description of element", () => {
        //arrange

        var shape = new PrePostConditionShape();
        shape.element = useCaseElement;
        (<IStep>useCaseElement).description = "description";

        //Act

        var label = shape.label;

        //Assert
        expect(label).toEqual("description");
    });
});
