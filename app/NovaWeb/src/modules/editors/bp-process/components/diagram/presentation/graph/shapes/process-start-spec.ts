import {ProcessStart} from "./";
import {ShapesFactory} from "./shapes-factory";
import {ProcessGraph} from "../process-graph";
import {ProcessModel, ProcessShapeModel} from "../../../../../models/processModels";
import {ProcessShapeType} from "../../../../../models/enums";
import {IProcessService} from "../../../../../services/process/process.svc";
import {ProcessServiceMock} from "../../../../../services/process/process.svc.mock";
import {ProcessViewModel} from "../../../viewmodel/process-viewmodel";
import {NodeType} from "../models/";

describe("ProcessStart test", () => {
    var shapesFactory: ShapesFactory;
    var localScope, rootScope, wrapper, container;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("processModelService", ProcessServiceMock);
    }));

    beforeEach(inject((_$window_: ng.IWindowService, $rootScope: ng.IRootScopeService, processModelService: IProcessService) => {
        rootScope = $rootScope;
        processModelService = processModelService;
        wrapper = document.createElement("DIV");
        container = document.createElement("DIV");
        wrapper.appendChild(container);
        document.body.appendChild(wrapper);

        $rootScope["config"] = {};
        $rootScope["config"].labels = {};
        shapesFactory = new ShapesFactory($rootScope);
        localScope = { graphContainer: container, graphWrapper: wrapper, isSpa: false };
    }));


    it("Test ProcessStart class", () => {
        // Arrange
        var testModel = new ProcessShapeModel(30);
        testModel.propertyValues = shapesFactory.createPropertyValuesForSystemTaskShape();
        testModel.propertyValues["clientType"].value = ProcessShapeType.Start;
        testModel.propertyValues["x"].value = 0;

        let processModel = new ProcessModel();
        let viewModel = new ProcessViewModel(processModel);
        viewModel.isReadonly = false;

        // Act
        var graph = new ProcessGraph(rootScope, localScope, container, this.processModelService, viewModel);

        var node = new ProcessStart(testModel);
        node.render(graph, 30, 30, false);

        //Assert
        expect(graph.getNodeById("30").getNodeType()).toEqual(NodeType.ProcessStart);
    });
});