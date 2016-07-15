import {UsecaseToDiagram, UsecaseFlowGraphBuilder, FlowGraphDiagramBuilder} from "./usecase-to-diagram";
import {IStep, IFlow, IUseCase, StepOfType} from "./models";

var nextIndexGenerator = () => {
    var orderIndex = 0;
    var generate = () => {
        return orderIndex++;
    }
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

        var buildGraph = spyOn(UsecaseFlowGraphBuilder.prototype, "buildGraph").and.callThrough();;
        var buildDiagram = spyOn(FlowGraphDiagramBuilder.prototype, "buildDiagram").and.callThrough();;

        //Act

        var diagram = usecaseToDiagram.convert(usecase);

        //Assert

        expect(diagram).not.toBeNull();

        expect(buildGraph).toHaveBeenCalled();
        expect(buildDiagram).toHaveBeenCalled();
    });
});