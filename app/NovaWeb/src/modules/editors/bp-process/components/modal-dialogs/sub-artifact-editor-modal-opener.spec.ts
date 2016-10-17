import * as angular from "angular";
import {IModalDialogCommunication, ModalDialogCommunication} from "./modal-dialog-communication";
import {ModalDialogType} from "./modal-dialog-constants";
import {CommunicationManager} from "../../../bp-process/services/communication-manager";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {ModalServiceMock} from "../../../../shell/login/mocks.spec";
import {DialogServiceMock} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {ProcessGraph} from "../diagram/presentation/graph/process-graph";
import {ProcessGraphModel} from "../diagram/viewmodel/process-graph-model";
import {ProcessViewModel} from "../diagram/viewmodel/process-viewmodel";
import {SubArtifactEditorModalOpener} from "./sub-artifact-editor-modal-opener";
import {SubArtifactEditorUserTaskModalController} from "./sub-artifact-editor-user-task-modal-controller";
import {SubArtifactEditorSystemTaskModalController} from "./sub-artifact-editor-system-task-modal-controller";
import * as TestModels from "../../models/test-model-factory";
import * as ProcessModels from "../../models/process-models";

class ObservableHelper {
    public getGraph: () => any;
    public setGraph = (graph) => {
        this.getGraph = graph;
    }

    public openDialog = (id: number, dialogType: ModalDialogType) => {
        this.somePrivateFunc1();
    }

    private somePrivateFunc1() {

    }
}

describe("SubArtifactEditorModalOpener test", () => {
    let dm: IModalDialogCommunication;
    let localScope, localization;
    let dialogService: DialogServiceMock;
    let subArtifactEditorModalOpener: SubArtifactEditorModalOpener;
    let communicationManager: CommunicationManager;
    let $uibModal: ModalServiceMock;
    let rootScope: any;
    let graph: ProcessGraph;
    let wrapper, container;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("dialogService", DialogServiceMock);
    }));

    beforeEach(inject(($rootScope: ng.IRootScopeService,
                       _localization_: LocalizationServiceMock,
                       _communicationManager_: CommunicationManager,
                       _$uibModal_: ModalServiceMock,
                       _dialogService_: DialogServiceMock) => {
        rootScope = $rootScope;
        localization = _localization_;
        $uibModal = _$uibModal_;
        dialogService = _dialogService_;
        communicationManager = _communicationManager_;

        wrapper = document.createElement("DIV");
        container = document.createElement("DIV");
        wrapper.appendChild(container);
        document.body.appendChild(wrapper);

        $rootScope["config"] = {
            labels: {
                "ST_Persona_Label": "Persona",
                "ST_Colors_Label": "Color",
                "ST_Comments_Label": "Comments",
                "ST_New_User_Task_Label": "New User Task",
                "ST_New_User_Task_Persona": "User",
                "ST_New_User_Decision_Label": "New Decision",
                "ST_New_System_Task_Label": "New System Task",
                "ST_New_System_Task_Persona": "System",
                "ST_Eighty_Percent_of_Shape_Limit_Reached": "The Process now has {0} of the maximum {1} shapes",
                "ST_Shape_Limit_Exceeded": "The Process will exceed the maximum {0} shapes",
                "ST_Shape_Limit_Exceeded_Initial_Load": "The Process will exceed the maximum {0} shapes"
            },
            settings: {
                "StorytellerShapeLimit": 100,
                "StorytellerIsSMB": "false"
            }
        };

        localScope = {graphContainer: container, graphWrapper: wrapper, isSpa: false};

        localScope["vm"] = {
            "$rootScope": rootScope
        };

    }));

    beforeEach(() => {
        dm = new ModalDialogCommunication();
        subArtifactEditorModalOpener = new SubArtifactEditorModalOpener(
            localScope,
            $uibModal,
            rootScope,
            communicationManager.modalDialogManager,
            localization);
    });

    afterEach(() => {
        dm.onDestroy();
        dm = null;
        subArtifactEditorModalOpener.onDestroy();
        subArtifactEditorModalOpener = null;
    });

    it("graph has to be injected into subArtifactEditorModalOpener", () => {
        let process = TestModels.createSystemDecisionForAddBranchTestModel();

        // Act
        graph = createGraph(process);
        graph.render(true, null);

        // Assert
        expect(subArtifactEditorModalOpener.getGraph()).toEqual(graph);
        expect(subArtifactEditorModalOpener.getGraph().viewModel).toBeDefined();
    });

    it("subArtifactEditorModalOpener.openDialog should be called on modalDialogManager.openDialog", () => {
        let process = TestModels.createSystemDecisionForAddBranchTestModel();
        let spy = spyOn(subArtifactEditorModalOpener, "openDialog");

        // Act
        graph = createGraph(process);
        graph.render(true, null);
        communicationManager.modalDialogManager.openDialog(1, ModalDialogType.UserTaskDetailsDialogType);

        // Assert
        expect(spy).toHaveBeenCalled();
    });

    it("openUserSystemTaskDetailsModalDialog.open called with parameters", () => {
        let process = TestModels.createUserDecisionInfiniteLoopModel();
        spyOn(subArtifactEditorModalOpener, "open");

        // Act
        graph = createGraph(process);
        graph.render(true, null);
        communicationManager.modalDialogManager.openDialog(80, ModalDialogType.UserTaskDetailsDialogType);

        // Assert
        expect(subArtifactEditorModalOpener.open).toHaveBeenCalledWith(
            "",
            require("./sub-artifact-user-task-editor-modal-template.html"),
            SubArtifactEditorUserTaskModalController,
            subArtifactEditorModalOpener.getSubArtifactDialogModel(80, graph),
            "storyteller-modal");
    });

    function createGraph(process: ProcessModels.IProcess): ProcessGraph {
        let clientModel = new ProcessGraphModel(process);
        let viewModel = new ProcessViewModel(clientModel);
        viewModel.communicationManager = communicationManager;
        return new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization);
    }

});

