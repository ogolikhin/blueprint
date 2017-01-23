import * as angular from "angular";
import "angular-mocks";
import "script!mxClient";
import {ShapesFactory} from "./shapes/shapes-factory";
import {LocalizationServiceMock} from "../../../../../../commonModule/localization/localization.service.mock";
import {DialogServiceMock} from "../../../../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {IProcessGraph} from "./models/process-graph-interfaces";
import {ProcessType} from "../../../../models/enums";
import {IProcess} from "../../../../models/process-models";
import {CommunicationManager} from "../../../../services/communication-manager";
import {ProcessViewModel} from "../../viewmodel/process-viewmodel";
import {ProcessGraph} from "./process-graph";
import {ProcessGraphSelectionHelper} from "./process-graph-selection";
import {StatefulArtifactFactoryMock} from "../../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ExecutionEnvironmentDetectorMock} from "../../../../../../commonModule/services/executionEnvironmentDetector.mock";
import * as TestModels from "../../../../models/test-model-factory";
import {MessageServiceMock} from "../../../../../../main/components/messages/message.mock";

describe("ProcessGraphSelectionHelper", () => {
    let $rootScope: ng.IRootScopeService;
    let $scope: ng.IScope;
    let communicationManager: CommunicationManager;
    let messageService: MessageServiceMock;
    let dialogService: DialogServiceMock;
    let localization: LocalizationServiceMock;
    let shapesFactory: ShapesFactory;

    let _window: any = window;
    _window.executionEnvironmentDetector = ExecutionEnvironmentDetectorMock;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("shapesFactory", ShapesFactory);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
    }));

    beforeEach(inject((
        _$rootScope_: ng.IRootScopeService,
        _communicationManager_: CommunicationManager,
        _messageService_: MessageServiceMock,
        _dialogService_: DialogServiceMock,
        _localization_: LocalizationServiceMock,
        _shapesFactory_: ShapesFactory
    ) => {
        $rootScope = _$rootScope_;
        $scope = _$rootScope_.$new();
        communicationManager = _communicationManager_;
        messageService = _messageService_;
        dialogService = _dialogService_;
        localization = _localization_;
        shapesFactory = _shapesFactory_;

        $rootScope["config"] = {
            labels: [],
            settings: {
                StorytellerShapeLimit: "100",
                StorytellerIsSMB: "false"
            }
        };
    }));

    it("allows selection when start is selected in User-To-System process", () => {
        // arrange
        const process = TestModels.createDefaultProcessModel(ProcessType.UserToSystemProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(helper, "notifySelectionChanged");

        const start = graph.getNodeById("10");

        // act
        graph.getMxGraph().setSelectionCells([start]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(start.model.id);
    });

    it("highlights edges when start is selected in User-To-System process", () => {
        // arrange
        const process = TestModels.createDefaultProcessModel(ProcessType.UserToSystemProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(graph, "highlightNodeEdges");

        const start = graph.getNodeById("10");

        // act
        graph.getMxGraph().setSelectionCells([start]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(start.model.id);
    });

    it("allows selection when start is selected in Business process", () => {
        // arrange
        const process = TestModels.createDefaultProcessModel(ProcessType.BusinessProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(helper, "notifySelectionChanged");

        const start = graph.getNodeById("10");

        // act
        graph.getMxGraph().setSelectionCells([start]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(start.model.id);
    });

    it("highlights edges when start is selected in Business process", () => {
        // arrange
        const process = TestModels.createDefaultProcessModel(ProcessType.BusinessProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(graph, "highlightNodeEdges");

        const start = graph.getNodeById("10");

        // act
        graph.getMxGraph().setSelectionCells([start]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(start.model.id);
    });

    it("allows selection when pre-condition is selected in User-To-System process", () => {
        // arrange
        const process = TestModels.createDefaultProcessModel(ProcessType.UserToSystemProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(helper, "notifySelectionChanged");

        const preCondition = graph.getNodeById("15");

        // act
        graph.getMxGraph().setSelectionCells([preCondition]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(preCondition.model.id);
    });

    it("highlights edges when pre-condition is selected in User-To-System process", () => {
        // arrange
        const process = TestModels.createDefaultProcessModel(ProcessType.UserToSystemProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(graph, "highlightNodeEdges");

        const preCondition = graph.getNodeById("15");

        // act
        graph.getMxGraph().setSelectionCells([preCondition]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(preCondition.model.id);
    });

    it("disallows selection when pre-condition is selected in Business process", () => {
        // arrange
        const process = TestModels.createDefaultProcessModel(ProcessType.BusinessProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(helper, "notifySelectionChanged");

        const preCondition = graph.getNodeById("15");

        // act
        graph.getMxGraph().setSelectionCells([preCondition]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(0);
    });

    it("doesn't highlight edges when pre-condition is selected in Business process", () => {
        // arrange
        const process = TestModels.createDefaultProcessModel(ProcessType.BusinessProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(graph, "highlightNodeEdges");

        const preCondition = graph.getNodeById("15");

        // act
        graph.getMxGraph().setSelectionCells([preCondition]);

        // assert
        expect(spy).not.toHaveBeenCalled();
    });

    it("allows selection when user task is selected in User-To-System process", () => {
        // arrange
        const process = TestModels.createDefaultProcessModel(ProcessType.UserToSystemProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(helper, "notifySelectionChanged");

        const userTask = graph.getNodeById("20");

        // act
        graph.getMxGraph().setSelectionCells([userTask]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(userTask.model.id);
    });

    it("highlights edges when user task is selected in User-To-System process", () => {
        // arrange
        const process = TestModels.createDefaultProcessModel(ProcessType.UserToSystemProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(graph, "highlightNodeEdges");

        const userTask = graph.getNodeById("20");

        // act
        graph.getMxGraph().setSelectionCells([userTask]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(userTask.model.id);
    });

    it("allows selection when user task is selected in Business process", () => {
        // arrange
        const process = TestModels.createDefaultProcessModel(ProcessType.BusinessProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(helper, "notifySelectionChanged");

        const userTask = graph.getNodeById("20");

        // act
        graph.getMxGraph().setSelectionCells([userTask]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(userTask.model.id);
    });

    it("highlights edges when user task is selected in Business process", () => {
        // arrange
        const process = TestModels.createDefaultProcessModel(ProcessType.BusinessProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(graph, "highlightNodeEdges");

        const userTask = graph.getNodeById("20");

        // act
        graph.getMxGraph().setSelectionCells([userTask]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(userTask.model.id);
    });

    it("allows selection when end is selected in User-To-System process", () => {
        // arrange
        const process = TestModels.createDefaultProcessModel(ProcessType.UserToSystemProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(helper, "notifySelectionChanged");

        const end = graph.getNodeById("30");

        // act
        graph.getMxGraph().setSelectionCells([end]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(end.model.id);
    });

    it("highlights edges when end is selected in User-To-System process", () => {
        // arrange
        const process = TestModels.createDefaultProcessModel(ProcessType.UserToSystemProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(graph, "highlightNodeEdges");

        const end = graph.getNodeById("30");

        // act
        graph.getMxGraph().setSelectionCells([end]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(end.model.id);
    });

    it("allows selection when end is selected in Business process", () => {
        // arrange
        const process = TestModels.createDefaultProcessModel(ProcessType.BusinessProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(helper, "notifySelectionChanged");

        const end = graph.getNodeById("30");

        // act
        graph.getMxGraph().setSelectionCells([end]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(end.model.id);
    });

    it("highlights edges when end is selected in Business process", () => {
        // arrange
        const process = TestModels.createDefaultProcessModel(ProcessType.BusinessProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(graph, "highlightNodeEdges");

        const end = graph.getNodeById("30");

        // act
        graph.getMxGraph().setSelectionCells([end]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(end.model.id);
    });

    it("allows selection when user decision is selected in User-To-System process", () => {
        // arrange
        const userDecisionModel = TestModels.createUserDecision(999);
        const process = TestModels.createUserDecisionTestModel(userDecisionModel, ProcessType.UserToSystemProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(helper, "notifySelectionChanged");

        const userDecision = graph.getNodeById("999");

        // act
        graph.getMxGraph().setSelectionCells([userDecision]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(userDecision.model.id);
    });

    it("highlights edges when user decision is selected in User-To-System process", () => {
        // arrange
        const userDecisionModel = TestModels.createUserDecision(999);
        const process = TestModels.createUserDecisionTestModel(userDecisionModel, ProcessType.UserToSystemProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(graph, "highlightNodeEdges");

        const userDecision = graph.getNodeById("999");

        // act
        graph.getMxGraph().setSelectionCells([userDecision]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(userDecision.model.id);
    });

    it("allows selection when user decision is selected in Business process", () => {
        // arrange
        const userDecisionModel = TestModels.createUserDecision(999);
        const process = TestModels.createUserDecisionTestModel(userDecisionModel, ProcessType.BusinessProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(helper, "notifySelectionChanged");

        const userDecision = graph.getNodeById("999");

        // act
        graph.getMxGraph().setSelectionCells([userDecision]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(userDecision.model.id);
    });

    it("highlights edges when user decision is selected in Business process", () => {
        // arrange
        const userDecisionModel = TestModels.createUserDecision(999);
        const process = TestModels.createUserDecisionTestModel(userDecisionModel, ProcessType.BusinessProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(graph, "highlightNodeEdges");

        const userDecision = graph.getNodeById("999");

        // act
        graph.getMxGraph().setSelectionCells([userDecision]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(userDecision.model.id);
    });

    it("allows selection when system decision is selected in User-To-System process", () => {
        // arrange
        const systemDecisionModel = TestModels.createSystemDecision(888);
        const process = TestModels.createSystemDecisionTestModel(systemDecisionModel, ProcessType.UserToSystemProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(helper, "notifySelectionChanged");

        const systemDecision = graph.getNodeById("888");

        // act
        graph.getMxGraph().setSelectionCells([systemDecision]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(systemDecision.model.id);
    });

    it("highlights edges when system decision is selected in User-To-System process", () => {
        // arrange
        const systemDecisionModel = TestModels.createSystemDecision(888);
        const process = TestModels.createSystemDecisionTestModel(systemDecisionModel, ProcessType.UserToSystemProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(graph, "highlightNodeEdges");

        const systemDecision = graph.getNodeById("888");

        // act
        graph.getMxGraph().setSelectionCells([systemDecision]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(systemDecision.model.id);
    });

    it("allows selection when system decision is selected in Business process", () => {
        // arrange
        const systemDecisionModel = TestModels.createSystemDecision(888);
        const process = TestModels.createSystemDecisionTestModel(systemDecisionModel, ProcessType.BusinessProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(helper, "notifySelectionChanged");

        const systemDecision = graph.getNodeById("888");

        // act
        graph.getMxGraph().setSelectionCells([systemDecision]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(systemDecision.model.id);
    });

    it("highlights edges when system decision is selected in Business process", () => {
        // arrange
        const systemDecisionModel = TestModels.createSystemDecision(888);
        const process = TestModels.createSystemDecisionTestModel(systemDecisionModel, ProcessType.BusinessProcess);
        const graph = createGraph(process);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(graph, "highlightNodeEdges");

        const systemDecision = graph.getNodeById("888");

        // act
        graph.getMxGraph().setSelectionCells([systemDecision]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(systemDecision.model.id);
    });

    it("doesn't allows selection when merging point is selected in User-To-System process", () => {
        // arrange
        const systemDecisionModel = TestModels.createSystemDecision(888);
        const process = TestModels.createSystemDecisionTestModel(systemDecisionModel, ProcessType.UserToSystemProcess);
        const graph = createGraph(process);
        graph.layout.setTempShapeId(0);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(helper, "notifySelectionChanged");

        const mergingPoint = graph.getNodeById("-1");

        // act
        graph.getMxGraph().setSelectionCells([mergingPoint]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(0);
    });

    it("doesn't highlights edges when merging point is selected in User-To-System process", () => {
        // arrange
        const systemDecisionModel = TestModels.createSystemDecision(888);
        const process = TestModels.createSystemDecisionTestModel(systemDecisionModel, ProcessType.UserToSystemProcess);
        const graph = createGraph(process);
        graph.layout.setTempShapeId(0);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(graph, "highlightNodeEdges");

        const mergingPoint = graph.getNodeById("-1");

        // act
        graph.getMxGraph().setSelectionCells([mergingPoint]);

        // assert
        expect(spy).not.toHaveBeenCalled();
    });

    it("doesn't allow selection when merging point is selected in Business process", () => {
        // arrange
        const systemDecisionModel = TestModels.createSystemDecision(888);
        const process = TestModels.createSystemDecisionTestModel(systemDecisionModel, ProcessType.BusinessProcess);
        const graph = createGraph(process);
        graph.layout.setTempShapeId(0);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(helper, "notifySelectionChanged");

        const mergingPoint = graph.getNodeById("-1");

        // act
        graph.getMxGraph().setSelectionCells([mergingPoint]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(0);
    });

    it("doesn't highlights edges when merging point is selected in Business process", () => {
        // arrange
        const systemDecisionModel = TestModels.createSystemDecision(888);
        const process = TestModels.createSystemDecisionTestModel(systemDecisionModel, ProcessType.BusinessProcess);
        const graph = createGraph(process);
        graph.layout.setTempShapeId(0);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(graph, "highlightNodeEdges");

        const mergingPoint = graph.getNodeById("-1");

        // act
        graph.getMxGraph().setSelectionCells([mergingPoint]);

        // assert
        expect(spy).not.toHaveBeenCalled();
    });

    it("allow only user tasks shapes when multiple shapes are selected in User-To-System process", () => {
        // arrange
        const systemDecisionModel = TestModels.createSystemDecision(888);
        const process = TestModels.createSystemDecisionTestModel(systemDecisionModel, ProcessType.UserToSystemProcess);
        const graph = createGraph(process);
        graph.layout.setTempShapeId(0);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(helper, "notifySelectionChanged");

        const start = graph.getNodeById("10");
        const preCondition = graph.getNodeById("15");
        const userTask = graph.getNodeById("20");
        const systemDecision = graph.getNodeById("888");
        const systemTask1 = graph.getNodeById("30");
        const systemTask2 = graph.getNodeById("35");
        const mergingPoint = graph.getNodeById("-1");
        const end = graph.getNodeById("40");

        // act
        graph.getMxGraph().setSelectionCells([
            start, preCondition, userTask, systemDecision, systemTask1, systemTask2, mergingPoint, end
        ]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(1);
        expect(callParams[0].model.id).toEqual(userTask.model.id);
    });

    it("allow only user tasks shapes when multiple shapes are selected in Business process", () => {
        // arrange
        const userDecisionModel = TestModels.createUserDecision(999);
        const process = TestModels.createUserDecisionTestModel(userDecisionModel, ProcessType.BusinessProcess);
        const graph = createGraph(process);
        graph.layout.setTempShapeId(0);
        graph.render(true, null);
        const helper = new ProcessGraphSelectionHelper(graph);
        helper.initSelection();
        const spy = spyOn(helper, "notifySelectionChanged");

        const start = graph.getNodeById("10");
        const preCondition = graph.getNodeById("15");
        const userTask1 = graph.getNodeById("20");
        const systemTask2 = graph.getNodeById("25");
        const userDecision = graph.getNodeById("999");
        const userTask2 = graph.getNodeById("35");
        const systemTask3 = graph.getNodeById("40");
        const userTask3 = graph.getNodeById("45");
        const systemTask4 = graph.getNodeById("50");
        const mergingPoint = graph.getNodeById("-1");
        const end = graph.getNodeById("55");

        // act
        graph.getMxGraph().setSelectionCells([
            start, preCondition, userTask1, systemTask2, userDecision, userTask2,
            systemTask3, userTask3, systemTask4, mergingPoint, end
        ]);

        // assert
        const callParams = spy.calls.mostRecent().args[0];
        expect(callParams.length).toEqual(3);
        expect(callParams[0].model.id).toEqual(userTask1.model.id);
        expect(callParams[1].model.id).toEqual(userTask2.model.id);
        expect(callParams[2].model.id).toEqual(userTask3.model.id);
    });

    function createGraph(process: IProcess): IProcessGraph {
        const scope: ng.IScope = $rootScope.$new();
        const container = document.createElement("div");
        const processViewModel = new ProcessViewModel(process, communicationManager, $rootScope, $scope, messageService);
        const graph = new ProcessGraph($rootScope, $scope, container, processViewModel, dialogService, localization, shapesFactory);

        return graph;
    }
});
