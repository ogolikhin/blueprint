import "script!mxClient";
import * as angular from "angular";
import "angular-mocks";
import {ProcessDeleteHelper} from "./process-delete-helper";
import {IStatefulArtifactFactory} from "../../../../../../managers/artifact-manager/artifact";
import {ShapesFactory} from "./shapes/shapes-factory";
import {ILocalizationService} from "../../../../../../commonModule/localization";
import {DialogService, IDialogService} from "../../../../../../shared/widgets/bp-dialog/bp-dialog";
import {IMessageService} from "../../../../../../main/components/messages/message.svc";
import {CommunicationManager, ICommunicationManager} from "../../../../services/communication-manager";
import {IProcess} from "./models";
import {ProcessGraph} from "./process-graph";
import {ProcessGraphModel} from "../../viewmodel/process-graph-model";
import {ProcessViewModel} from "../../viewmodel/process-viewmodel";
import {createXUserTasksGraphModel} from "../../../../models/test-model-factory";
import {MessageServiceMock} from "../../../../../../main/components/messages/message.mock";
import {StatefulArtifactFactoryMock} from "../../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {LocalizationServiceMock} from "../../../../../../commonModule/localization/localization.service.mock";
import {ModalServiceMock} from "../../../../../../shell/login/mocks.spec";
import {ExecutionEnvironmentDetectorMock} from "../../../../../../commonModule/services/executionEnvironmentDetector.mock";


describe("ProcessDeleteHelper tests", () => {
    let communicationManager: ICommunicationManager;
    let rootScope;
    let localScope;
    let messageService: IMessageService;
    let container;
    let dialogService: IDialogService;
    let localization: ILocalizationService;
    let shapesFactory: ShapesFactory;
    let statefulArtifactFactory: IStatefulArtifactFactory;

    const _window: any = window;
    _window.executionEnvironmentDetector = ExecutionEnvironmentDetectorMock;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("dialogService", DialogService);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("shapesFactory", ShapesFactory);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("$uibModal", ModalServiceMock);
    }));

    beforeEach(inject((_$window_: ng.IWindowService,
                       $rootScope: ng.IRootScopeService,
                       $timeout: ng.ITimeoutService,
                       _communicationManager_: ICommunicationManager,
                       _dialogService_: DialogService,
                       _localization_: LocalizationServiceMock,
                       _statefulArtifactFactory_: IStatefulArtifactFactory,
                       _shapesFactory_: ShapesFactory,
                       _messageService_: IMessageService) => {
        rootScope = $rootScope;
        communicationManager = _communicationManager_;
        dialogService = _dialogService_;
        localization = _localization_;
        const wrapper = document.createElement("DIV");
        container = document.createElement("DIV");
        wrapper.appendChild(container);
        document.body.appendChild(wrapper);
        statefulArtifactFactory = _statefulArtifactFactory_;
        shapesFactory = _shapesFactory_;
        messageService = _messageService_;

        $rootScope["config"] = {};
        $rootScope["config"].labels = {
        };
        $rootScope["config"].settings = {
            StorytellerShapeLimit: "100",
            StorytellerIsSMB: "false"
        };
        localScope = {graphContainer: container, graphWrapper: wrapper, isSpa: false};
        shapesFactory = new ShapesFactory(rootScope, _statefulArtifactFactory_);
    }));

    describe("deleteUserTasks", () => {
        it("no changes when failure to delete one user task", () => {
            const model = createXUserTasksGraphModel(2);
            const graph = createGraph(model);
            const numShapesInModel = model.shapes.length;
            const numLinksInModel = model.links.length;
            const deleteUserTaskIds = [20, 30];
            const deleteSpy = spyOn(ProcessDeleteHelper, "deleteShapesAndLinksByIds").and.callThrough();
            const updateSpy = spyOn(graph, "notifyUpdateInModel").and.callThrough();

            ProcessDeleteHelper.deleteUserTasks(deleteUserTaskIds, graph);

            expect(deleteSpy).toHaveBeenCalledTimes(1);
            expect(updateSpy).not.toHaveBeenCalled();
            expect(graph.viewModel.shapes.length).toBe(numShapesInModel);
            expect(graph.viewModel.links.length).toBe(numLinksInModel);
        });

        it("successfully deletes task and model is updated", () => {
            const model = createXUserTasksGraphModel(3);
            const graph = createGraph(model);
            const numShapesInModel = model.shapes.length;
            const numLinksInModel = model.links.length;
            const deleteUserTaskIds = [20, 30];
            const deleteSpy = spyOn(ProcessDeleteHelper, "deleteShapesAndLinksByIds").and.callThrough();
            const updateSpy = spyOn(graph, "notifyUpdateInModel").and.callThrough();

            ProcessDeleteHelper.deleteUserTasks(deleteUserTaskIds, graph);

            expect(deleteSpy).toHaveBeenCalledTimes(2);
            expect(updateSpy).toHaveBeenCalled();
            expect(graph.viewModel.shapes.length).not.toBe(numShapesInModel);
            expect(graph.viewModel.links.length).not.toBe(numLinksInModel);
        });
    });

    function createGraph(process: IProcess): ProcessGraph {
        const clientModel = new ProcessGraphModel(process);
        const viewModel = new ProcessViewModel(clientModel, communicationManager, rootScope, localScope, messageService);

        //bypass testing stateful shapes logic here
        spyOn(viewModel, "removeStatefulShape").and.returnValue(null);
        spyOn(viewModel, "addToSubArtifactCollection").and.returnValue(null);
        return new ProcessGraph(
            rootScope, localScope, container, viewModel, dialogService, localization,
            shapesFactory, messageService, null, statefulArtifactFactory
        );
    }
});
