import {ShapesFactory} from "./shapes-factory";
import {ProcessGraph} from "../process-graph";
import {ArtifactReferenceLinkMock} from "./shape-model.mock";
import {ProcessViewModel} from "../../../viewmodel/process-viewmodel";
import {ProcessServiceMock} from "../../../../../services/process/process.svc.mock";
import {IProcessService} from "../../../../../services/process/process.svc";
import * as ProcessModels from "../../../../../models/processModels";
import {UserDecision} from "./";
import {NodeChange} from "../process-graph-constants";

describe("UserDecision", () => {

    var shapesFactory: ShapesFactory;
    var graph: ProcessGraph;
    var localScope, rootScope, processModelService, artifactVersionControlService, wrapper, container;

    var testArtifactReferenceLink2 = new ArtifactReferenceLinkMock(2);

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("processModelService", ProcessServiceMock);
    }));

    beforeEach(inject((_$window_: ng.IWindowService, $rootScope: ng.IRootScopeService, processModelService: IProcessService) => {
        rootScope = $rootScope;
        processModelService = processModelService;
        wrapper = document.createElement('DIV');
        container = document.createElement('DIV');
        wrapper.appendChild(container);
        document.body.appendChild(wrapper);

        $rootScope["config"] = {};
        $rootScope["config"].labels = {};
        shapesFactory = new ShapesFactory($rootScope);
        localScope = { graphContainer: container, graphWrapper: wrapper, isSpa: false };
    }));

    it("initializes details button", () => {
        // Arrange

        var id = 3;

        var testModel = new ProcessModels.ProcessModel();
        var model = shapesFactory.createModelUserDecisionShape(2, 1, id, 0, 0);
        testModel.shapes.push(model);
        var processViewModel = new ProcessViewModel(testModel);
        var graph = new ProcessGraph(rootScope, localScope, container, processModelService, processViewModel);

        // Act
        graph.render(false, null);

        // Assert
        expect(graph.getMxGraphModel().getCell(`DB${id}`)).not.toBeNull();
    });

    it("doesn't call notify when label doesn't change", () => {
        // Arrange
        var model = shapesFactory.createModelUserDecisionShape(2, 1, 3, 0, 0);
        model.propertyValues["label"].value = "Test";
        var userDecision = new UserDecision(model, rootScope);

        var notifySpy = spyOn(userDecision, "notify");

        // Act
        userDecision.action = "Test";

        // Assert
        expect(notifySpy).not.toHaveBeenCalled();
    });

    it("notifies of changes when label changes", () => {
        // Arrange
        var model = shapesFactory.createModelUserDecisionShape(2, 1, 3, 0, 0);
        var userDecision = new UserDecision(model, rootScope);

        var notifySpy = spyOn(userDecision, "notify");

        // Act
        userDecision.label = "Test";

        // Assert
        expect(notifySpy).toHaveBeenCalledWith(NodeChange.Update);
    });

    it("notifies of changes when label changes and UI redraw", () => {
        // Arrange
        var model = shapesFactory.createModelUserDecisionShape(2, 1, 3, 0, 0);
        var userDecision = new UserDecision(model, rootScope);
        userDecision.textLabel = {
            render: () => { },
            text: "",
            setVisible: (value: boolean) => { },
            onDispose: () => { }
        };

        var notifySpy = spyOn(userDecision, "notify");

        // Act
        userDecision.setLabelWithRedrawUi("Test");

        // Assert
        expect(notifySpy).toHaveBeenCalledWith(NodeChange.Update, true);
    });
});