import {IUseCase, IStep, IFlow, IUseCaseElement} from "./models";
import {IDiagram, IShape, IConnection, IPoint, IProp, ILabelStyle} from "./../models";
import {FlowGraph} from "./layout/flow-graph";
import {ISize} from "./layout/rect";
import {AlternateFlow, Flow, Node} from "./layout/flow-graph-objects";
import {LayoutResult} from "./layout/layout-result";
import {ConnectionInfo} from "./layout/connection-info";
import {Diagrams, Shapes, ConnectorTypes} from "./../utils/constants";
import {LayoutCalculator} from "./layout/layout-calculator";

export interface IUseCaseShape extends IShape {
    element: IUseCaseElement;
}

/**
 * Default implementation of the "ILayoutCalculator" interface.
 */
export class UsecaseToDiagram {

    public static get FULL_WIDTH(): number {
        return 300;
    }

    public static get WIDTH(): number {
        return 150;
    }

    public static get HEIGHT(): number {
        return 60;
    }

    public static get TRUNCATED_WIDTH(): number {
        return 150;
    }

    public static get TRUNCATED_HEIGHT(): number {
        return 55;
    }

    public static get PACKAGE_HEIGHT(): number {
        return 14;
    }

    public static get CELL_SPACING(): number {
        return 15;
    }

    public static get FLOW_SPACING(): number {
        return 5;
    }

    public static get BRANCHING_NODE_SIZE(): number {
        return 30;
    }

    public static get EXIT_LABEL(): string {
        return "Exit";
    }

    /**
     * Converts use case artifact to IDiagram object
     */
    public convert(usecase: IUseCase): IDiagram {
        const calculator = new LayoutCalculator();
        calculator.nodeDefaultHeight = UsecaseToDiagram.TRUNCATED_HEIGHT;
        calculator.nodeDefaultWidth = UsecaseToDiagram.TRUNCATED_WIDTH;
        calculator.verticalCellSpacing = UsecaseToDiagram.CELL_SPACING;
        calculator.horizontalCellSpacing = 2 * UsecaseToDiagram.CELL_SPACING;
        calculator.flowSpacing = UsecaseToDiagram.FLOW_SPACING;
        calculator.disableVerticalDisplacement = false;
        const flowGraph = new UsecaseFlowGraphBuilder().buildGraph(usecase);
        const result = calculator.arrangeGraph(flowGraph);
        const diagram = new FlowGraphDiagramBuilder().buildDiagram(result, usecase);

        return diagram;
    }
}

interface IStepNameToNodeMap {
    [stepName: string]: Node;
}

export class FlowGraphDiagramBuilder {

    private diagramOffset = 30;

    /**
     * Bulds IDiagram object based on LayoutResult
     */
    public buildDiagram(layoutResult: LayoutResult, usecase: IUseCase): IDiagram {
        const diagram: IDiagram = {
            id: usecase.id,
            diagramType: Diagrams.USECASE,
            shapes: [],
            connections: [],
            width: 0,
            height: 0,
            libraryVersion: 1
        };

        const graph = layoutResult.getGraph();

        graph.getNodes().forEach((n: Node) => {
            let shape: IUseCaseShape;
            const shapeType = this.getShapeType(usecase, n);
            if (shapeType === Shapes.PRE_POST_CONDITION) {
                shape = new PrePostConditionShape();
            } else {
                shape = new UseCaseShape();
            }
            shape.element = n.tag;
            shape.x = n.position.x;
            shape.y = n.position.y;
            shape.width = n.size.width;
            shape.height = n.size.height;
            shape.type = shapeType;
            diagram.shapes.push(shape);
            diagram.width = Math.max(diagram.width, shape.x + shape.width + this.diagramOffset);
            diagram.height = Math.max(diagram.height, shape.y + shape.height + this.diagramOffset);
        });

        layoutResult.getConnections().forEach((c: ConnectionInfo) => {
            if (c.endNode != null && c.startNode != null && c.endNode.tag != null && c.startNode.tag != null) {
                const connection = new UseCaseConnector();
                connection.sourceId = c.startNode.tag.id;
                connection.targetId = c.endNode.tag.id;
                const points = c.getPoints();
                if (points != null && points.length > 2) {
                    connection.type = ConnectorTypes.RIGHT_ANGLED;
                    connection.points = points;
                } else {
                    connection.type = ConnectorTypes.STRAIGHT;
                }
                diagram.connections.push(connection);
            }
        });
        return diagram;
    }

    private getShapeType(usecase: IUseCase, node: Node): string {
        if (node.tag instanceof BranchingStep) {
            return Shapes.BRANCHING;
        }
        if (node.tag instanceof ExitStep) {
            return Shapes.EXIT;
        }
        if (node.tag === usecase.preCondition || node.tag === usecase.postCondition) {
            return Shapes.PRE_POST_CONDITION;
        }
        return Shapes.STEP;
    }
}

export class UsecaseFlowGraphBuilder {
    private graph: FlowGraph;
    private stepNameToNodeMap: IStepNameToNodeMap;

    /**
     * Builds FlowGraph
     */
    public buildGraph(usecase: IUseCase) {
        this.graph = new FlowGraph();

        this.stepNameToNodeMap = {};

        let node;
        const mainFlow = this.graph.getMainFlow();
        if (usecase.preCondition != null) {
            node = this.addStepNode(usecase.preCondition, mainFlow);
            this.stepNameToNodeMap[usecase.preCondition.name] = node;
        }

        this.processSteps(usecase.steps, mainFlow);

        if (usecase.postCondition != null) {
            node = this.addStepNode(usecase.postCondition, mainFlow);
            this.stepNameToNodeMap[usecase.postCondition.name] = node;
        }
        this.resolveReturnSteps();

        return this.graph;
    }

    /**
     * Creates FlowGraph objects by iterating through steps
     */
    private processSteps(steps: Array<IStep>, flow: Flow) {
        if (steps != null) {
            let node: Node;
            steps.forEach((step: IStep) => {
                if (step.flows != null && step.flows.length > 0) {
                    const branchingNode = this.addBranchingNode(new BranchingStep(-step.id), flow);
                    node = this.addStepNode(step, flow);
                    step.condition = true;
                    this.stepNameToNodeMap[step.name] = branchingNode;
                    step.flows.forEach((f: IFlow) => {
                        const alternateFlow = this.addAlternateFlow(f);
                        branchingNode.addAlternateFlow(alternateFlow);
                        this.markExternalStep(f);
                        this.processSteps(f.steps, alternateFlow);
                        if (f.returnToStepName === UsecaseToDiagram.EXIT_LABEL) {
                            node = this.addStepNode(new ExitStep(-f.id), alternateFlow);
                            this.stepNameToNodeMap[this.getEffectiveStepName(f.returnToStepName, f)] = node;
                        }
                        const conditionNode = alternateFlow.getFirstNode();
                        if (conditionNode != null && conditionNode.tag != null) {
                            (<IStep>conditionNode.tag).condition = true;
                        }
                    });
                } else {
                    node = this.addStepNode(step, flow);
                    this.stepNameToNodeMap[step.name] = node;
                }
            });
        }
    }

    private markExternalStep(flow: IFlow) {
        if (flow.isExternal) {
            if (flow.steps != null) {
                const externalStep = flow.steps[flow.steps.length - 1];
                if (externalStep != null) {
                    externalStep.external = true;
                }
            }
        }
    }

    private addAlternateFlow(flow: IUseCaseElement) {
        const alternateFlow = this.graph.createAlternateFlow();
        alternateFlow.tag = flow;
        return alternateFlow;
    }

    private addStepNode(step: IUseCaseElement, flow: Flow, size?: ISize): Node {
        const node = this.graph.createNode();
        node.size = size || {width: UsecaseToDiagram.WIDTH, height: UsecaseToDiagram.HEIGHT};
        node.tag = step;
        flow.addNode(node);
        return node;
    }

    private addBranchingNode(tag: IUseCaseElement, flow: Flow): Node {
        return this.addStepNode(tag, flow, {
            width: UsecaseToDiagram.BRANCHING_NODE_SIZE,
            height: UsecaseToDiagram.BRANCHING_NODE_SIZE
        });
    }

    private resolveReturnSteps() {
        let returnStep;
        this.graph.getAlternateFlows().forEach((f: AlternateFlow) => {
            if (f.tag != null) {
                const flow: IFlow = f.tag;
                returnStep = flow.returnToStepName;
                if (returnStep != null) {
                    const node = this.stepNameToNodeMap[this.getEffectiveStepName(returnStep, flow)];
                    if (node != null) {
                        f.endNode = node;
                    }
                }
            }
        });
    }

    private getEffectiveStepName(stepName: string, flow: IFlow) {
        if (stepName === UsecaseToDiagram.EXIT_LABEL) {
            return (-flow.id).toString();
        }
        return stepName;
    }
}

/**
 * Implements branching step
 */
export class BranchingStep implements IUseCaseElement {
    public id: number;
    public name: string;
    public orderIndex: number;

    constructor(id: number) {
        this.id = id;
    }
}

/**
 * Implements exit step
 */
export class ExitStep implements IUseCaseElement {
    public id: number;
    public name: string;
    public orderIndex: number;
    public description: string = UsecaseToDiagram.EXIT_LABEL;

    constructor(id: number) {
        this.id = id;
    }
}

/**
 * Implements use case connector
 */
export class UseCaseConnector implements IConnection {
    public id: number;
    public name: string;
    public parentId: number;
    public sourceId: number;
    public targetId: number;
    public type: string;
    public stroke: string;
    public strokeOpacity: number;
    public strokeWidth: number;
    public strokeDashPattern: string;
    public label: string;
    public sourceLabel: string;
    public targetLabel: string;
    public points: IPoint[];
    public startArrow: string;
    public endArrow: string;
    public zIndex: number = 999;
    public props: IProp[];
    public isShape: boolean;
}

/**
 * Implements use case shape
 */
export class UseCaseShape implements IUseCaseShape {
    public get id(): number {
        return this.element != null ? this.element.id : null;
    }

    public get name(): string {
        return this.element != null ? this.element.name : null;
    }

    public get label(): string {
        return this.element != null ? this.injectNameToDescription((<IStep>this.element).description, this.name) : null;
    }

    private injectNameToDescription(description: string, name: string) {
        //TODO parse description html and inject name
        if (description != null && name != null && name.length > 0) {
            const index = description.indexOf("<span");
            if (index >= 0) {
                const span = "<span style='font-size: 12px; line-height: 1.45000004768372'><b>" + name.replace("Step ", "") + ": </b></span>";
                return description.substring(0, index) + span + description.substring(index, description.length);
            }
        }
        return description;
    }

    public parentId: number;
    public type: string;
    public height: number;
    public width: number;
    public zIndex: number = 0;
    public angle: number;
    public stroke: string;
    public strokeOpacity: number;
    public strokeWidth: number;
    public strokeDashPattern: string;
    public fill: string;
    public fillOpacity: number;
    public gradientFill: string;
    public isGradient: boolean;
    public shadow: boolean;
    public labelTextAlignment: string;
    public description: string;
    public image: string;
    public props: IProp[];
    public labelStyle: ILabelStyle;
    public column: number;
    public row: number;
    public x: number;
    public y: number;
    public isShape: boolean;
    public element: IUseCaseElement;
}

export class PrePostConditionShape extends UseCaseShape {
    public get label(): string {
        return this.element != null ? (<IStep>this.element).description : null;
    }
}
