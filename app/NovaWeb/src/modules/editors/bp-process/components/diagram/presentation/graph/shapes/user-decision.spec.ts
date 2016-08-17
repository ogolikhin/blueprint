import {ShapesFactory} from "./shapes-factory";
import {ProcessGraph} from "../process-graph";
import {ProcessViewModel} from "../../../viewmodel/process-viewmodel";
import {ProcessServiceMock} from "../../../../../services/process/process.svc.mock";
import {IProcessService} from "../../../../../services/process/process.svc";
import * as ProcessModels from "../../../../../models/processModels";
import {UserDecision} from "./";
import {NodeChange} from "../models/";

describe("UserDecision", () => {

    let shapesFactory: ShapesFactory;
    let localScope, rootScope, processModelService,  wrapper, container;
    
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("processModelService", ProcessServiceMock);
    }));

    beforeEach(inject((
        _$window_: ng.IWindowService,
        $rootScope: ng.IRootScopeService,
        _processModelService_: IProcessService
    ) => {
        rootScope = $rootScope;
        processModelService = _processModelService_;
        wrapper = document.createElement("DIV");
        container = document.createElement("DIV");
        wrapper.appendChild(container);
        document.body.appendChild(wrapper);

        $rootScope["config"] = {};
        $rootScope["config"].labels = {};
        shapesFactory = new ShapesFactory($rootScope);
        localScope = { graphContainer: container, graphWrapper: wrapper, isSpa: false };
    }));

    it("initializes details button", () => {
        // Arrange

        let id = 3;

        let testModel = new ProcessModels.ProcessModel();
        let model = shapesFactory.createModelUserDecisionShape(2, 1, id, 0, 0);
        testModel.shapes.push(model);
        let processViewModel = new ProcessViewModel(testModel);
        let graph = new ProcessGraph(rootScope, localScope, container, processModelService, processViewModel);

        // Act
        graph.render(false, null);

        // Assert
        expect(graph.getMxGraphModel().getCell(`DB${id}`)).not.toBeNull();
    });

    it("doesn't call notify when label doesn't change", () => {
        // Arrange
        let model = shapesFactory.createModelUserDecisionShape(2, 1, 3, 0, 0);
        model.propertyValues["label"].value = "Test";
        let userDecision = new UserDecision(model, rootScope);

        let notifySpy = spyOn(userDecision, "notify");

        // Act
        userDecision.action = "Test";

        // Assert
        expect(notifySpy).not.toHaveBeenCalled();
    });

    it("notifies of changes when label changes", () => {
        // Arrange
        let model = shapesFactory.createModelUserDecisionShape(2, 1, 3, 0, 0);
        let userDecision = new UserDecision(model, rootScope);

        let notifySpy = spyOn(userDecision, "notify");

        // Act
        userDecision.label = "Test";

        // Assert
        expect(notifySpy).toHaveBeenCalledWith(NodeChange.Update);
    });

    it("notifies of changes when label changes and UI redraw", () => {
        // Arrange
        let model = shapesFactory.createModelUserDecisionShape(2, 1, 3, 0, 0);
        let userDecision = new UserDecision(model, rootScope);
        userDecision.textLabel = {
            render: () => { },
            text: "",
            setVisible: (value: boolean) => { },
            onDispose: () => { }
        };

        let notifySpy = spyOn(userDecision, "notify");

        // Act
        userDecision.setLabelWithRedrawUi("Test");

        // Assert
        expect(notifySpy).toHaveBeenCalledWith(NodeChange.Update, true);
    });
});