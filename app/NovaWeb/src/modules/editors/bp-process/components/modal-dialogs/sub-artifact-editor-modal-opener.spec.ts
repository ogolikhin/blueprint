import * as angular from "angular";
import "angular-mocks";
require("script!mxClient");
import {IModalDialogCommunication, ModalDialogCommunication} from "./modal-dialog-communication";
import {ModalDialogType} from "./modal-dialog-constants";
import {CommunicationManager} from "../../../bp-process/services/communication-manager";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {ModalServiceMock} from "../../../../shell/login/mocks.spec";
import {DialogServiceMock} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {ProcessGraph} from "../diagram/presentation/graph/process-graph";
import {ProcessGraphModel} from "../diagram/viewmodel/process-graph-model";
import {ProcessViewModel} from "../diagram/viewmodel/process-viewmodel";
import {UserTask, SystemDecision} from "../diagram/presentation/graph/shapes";
import {SubArtifactEditorModalOpener} from "./sub-artifact-editor-modal-opener";
import {UserTaskModalController} from "./task-editor/user-task-modal-controller";
import {SystemTaskModalController} from "./task-editor/system-task-modal-controller";
import * as TestModels from "../../models/test-model-factory";
import * as ProcessModels from "../../models/process-models";
import {ProcessShapeType} from "../../models/enums";

class ObservableHelper {
    public getGraph: () => any;
    public setGraph = (graph) => {
        this.getGraph = graph;
    }

    public openDialog = (id: number, dialogType: ModalDialogType) => {
        this.somePrivateFunc1();
    }

    private somePrivateFunc1() {
        // do nothing
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
        // arrange
        const process = TestModels.createSystemDecisionForAddBranchTestModel();
        graph = createGraph(process);
        const getGraph = () => graph;

        // act
        communicationManager.modalDialogManager.setGraph(getGraph);

        // assert
        expect(subArtifactEditorModalOpener["graph"]).toEqual(graph);
        expect(subArtifactEditorModalOpener["graph"].viewModel).toBeDefined();
    });

    describe("open", () => {
        it("calls openUserTaskDetailsModalDialog when open dialog for user task is invoked", () => {
            // arrange
            const shapeId: number = 40;
            const process = TestModels.createUserDecisionInfiniteLoopModel();
            const openDialogSpy = spyOn(subArtifactEditorModalOpener, "openUserTaskDetailsModalDialog").and.callFake(() => {/* no op */});
            graph = createGraph(process);
            subArtifactEditorModalOpener["graph"] = graph;
            
            // act
            communicationManager.modalDialogManager.openDialog(1, ModalDialogType.UserTaskDetailsDialogType);

            // assert
            expect(openDialogSpy).toHaveBeenCalled();
        });

        it("calls openSystemTaskDetailsModalDialog when open dialog for system task is invoked", () => {
            // arrange
            const shapeId: number = 70;
            const process = TestModels.createUserDecisionInfiniteLoopModel();
            const openDialogSpy = spyOn(subArtifactEditorModalOpener, "openSystemTaskDetailsModalDialog").and.callFake(() => {/* no op */});
            graph = createGraph(process);
            subArtifactEditorModalOpener["graph"] = graph;
            
            // act
            communicationManager.modalDialogManager.openDialog(shapeId, ModalDialogType.SystemTaskDetailsDialogType);

            // assert
            expect(openDialogSpy).toHaveBeenCalled();
        });

        it("calls openDecisionEditorDialog when open dialog for decision is invoked", () => {
            // arrange
            const shapeId: number = 30;
            const process = TestModels.createUserDecisionInfiniteLoopModel();
            const openDialogSpy = spyOn(subArtifactEditorModalOpener, "openDecisionEditorDialog").and.callFake(() => {/* no op */});
            graph = createGraph(process);
            subArtifactEditorModalOpener["graph"] = graph;

            // act
            communicationManager.modalDialogManager.openDialog(shapeId, ModalDialogType.UserSystemDecisionDetailsDialogType);

            // assert
            expect(openDialogSpy).toHaveBeenCalled();
        });

        it("calls openPreviewModalDialog when preview dialog is invoked", () => {
            // arrange
            const shapeId: number = 30;
            const process = TestModels.createUserDecisionInfiniteLoopModel();
            const openDialogSpy = spyOn(subArtifactEditorModalOpener, "openPreviewModalDialog").and.callFake(() => {/* no op */});
            graph = createGraph(process);
            subArtifactEditorModalOpener["graph"] = graph;

            // act
            communicationManager.modalDialogManager.openDialog(shapeId, ModalDialogType.PreviewDialogType);

            // assert
            expect(openDialogSpy).toHaveBeenCalled();
        });
    });

    describe("getDecisionEditorModel", () => {
        it("returns null for non-existing shape", () => {
            // arrange
            const process: ProcessModels.IProcess = TestModels.createUserDecisionForAddBranchTestModel();
            graph = createGraph(process);
            spyOn(graph, "getNodeById").and.returnValue(null);

            // act
            const model = subArtifactEditorModalOpener.getDecisionEditorModel(1, graph);

            // assert
            expect(model).toBeNull();
        });

        it("returns null for non-decision shape", () => {
            // arrange
            const process: ProcessModels.IProcess = TestModels.createDefaultProcessModel();
            const userTask: ProcessModels.IUserTaskShape = <ProcessModels.IUserTaskShape>process.shapes[2];
            const userTaskNode = new UserTask(userTask, rootScope, null, null);
            
            graph = createGraph(process);
            spyOn(graph, "getNodeById").and.returnValue(userTaskNode);

            // act
            const model = subArtifactEditorModalOpener.getDecisionEditorModel(userTask.id, graph);

            // assert
            expect(model).toBeNull();
        });

        it("returns correct decision model graph for decision shape", () => {
            // arrange
            const process: ProcessModels.IProcess = TestModels.createSystemDecisionForAddBranchTestModel();
            const decisionShape: ProcessModels.IProcessShape = process.shapes[3];
            const systemDecision = new SystemDecision(decisionShape, rootScope);
            
            graph = createGraph(process);
            spyOn(graph, "getNodeById").and.returnValue(systemDecision);

            // act
            const model = subArtifactEditorModalOpener.getDecisionEditorModel(decisionShape.id, graph);

            // assert
            expect(model.graph).toBe(graph);
        });

        it("returns correct decision model conditions for decision shape", () => {
            // arrange
            const process: ProcessModels.IProcess = TestModels.createSystemDecisionForAddBranchTestModel();
            const decisionShape: ProcessModels.IProcessShape = process.shapes[3];
            const systemDecision = new SystemDecision(decisionShape, rootScope);
            
            graph = createGraph(process);
            spyOn(graph, "getNodeById").and.returnValue(systemDecision);

            // act
            const model = subArtifactEditorModalOpener.getDecisionEditorModel(decisionShape.id, graph);

            // assert
            expect(model.conditions.length).toEqual(2);
        });

        it("returns correct decision model original for decision shape", () => {
            // arrange
            const process: ProcessModels.IProcess = TestModels.createSystemDecisionForAddBranchTestModel();
            const decisionShape: ProcessModels.IProcessShape = process.shapes[3];
            const systemDecision = new SystemDecision(decisionShape, rootScope);
            
            graph = createGraph(process);
            spyOn(graph, "getNodeById").and.returnValue(systemDecision);

            // act
            const model = subArtifactEditorModalOpener.getDecisionEditorModel(decisionShape.id, graph);

            // assert
            expect(model.originalDecision).toBe(systemDecision);
        });

        it("returns correct decision model label for decision shape", () => {
            // arrange
            const process: ProcessModels.IProcess = TestModels.createSystemDecisionForAddBranchTestModel();
            const decisionShape: ProcessModels.IProcessShape = process.shapes[3];
            decisionShape.name = "SD1";
            const systemDecision = new SystemDecision(decisionShape, rootScope);
            
            graph = createGraph(process);
            spyOn(graph, "getNodeById").and.returnValue(systemDecision);

            // act
            const model = subArtifactEditorModalOpener.getDecisionEditorModel(decisionShape.id, graph);

            // assert
            expect(model.label).toEqual(systemDecision.label);
        });

        it("returns decision model is read-only if read-only", () => {
            // arrange
            const process: ProcessModels.IProcess = TestModels.createSystemDecisionForAddBranchTestModel();
            process["artifactState"] = { readonly: true };
            const decisionShape: ProcessModels.IProcessShape = process.shapes[3];
            const systemDecision = new SystemDecision(decisionShape, rootScope);
            
            graph = createGraph(process);
            spyOn(graph, "getNodeById").and.returnValue(systemDecision);

            // act
            const model = subArtifactEditorModalOpener.getDecisionEditorModel(decisionShape.id, graph);

            // assert
            expect(model.isReadonly).toEqual(true);
        });

        it("returns decision model is not read-only if not read-only", () => {
            // arrange
            const process: ProcessModels.IProcess = TestModels.createSystemDecisionForAddBranchTestModel();
            process["artifactState"] = { readonly: false };
            const decisionShape: ProcessModels.IProcessShape = process.shapes[3];
            const systemDecision = new SystemDecision(decisionShape, rootScope);
            
            graph = createGraph(process);
            spyOn(graph, "getNodeById").and.returnValue(systemDecision);

            // act
            const model = subArtifactEditorModalOpener.getDecisionEditorModel(decisionShape.id, graph);

            // assert
            expect(model.isReadonly).toEqual(false);
        });

        it("returns decision model is historical if historical", () => {
            // arrange
            const process: ProcessModels.IProcess = TestModels.createSystemDecisionForAddBranchTestModel();
            process["historical"] = true;
            const decisionShape: ProcessModels.IProcessShape = process.shapes[3];
            const systemDecision = new SystemDecision(decisionShape, rootScope);
            
            graph = createGraph(process);
            spyOn(graph, "getNodeById").and.returnValue(systemDecision);

            // act
            const model = subArtifactEditorModalOpener.getDecisionEditorModel(decisionShape.id, graph);

            // assert
            expect(model.isHistoricalVersion).toEqual(true);
        });

        it("returns decision model is not historical if not historical", () => {
            // arrange
            const process: ProcessModels.IProcess = TestModels.createSystemDecisionForAddBranchTestModel();
            process["historical"] = false;
            const decisionShape: ProcessModels.IProcessShape = process.shapes[3];
            const systemDecision = new SystemDecision(decisionShape, rootScope);
            
            graph = createGraph(process);
            spyOn(graph, "getNodeById").and.returnValue(systemDecision);

            // act
            const model = subArtifactEditorModalOpener.getDecisionEditorModel(decisionShape.id, graph);

            // assert
            expect(model.isHistoricalVersion).toEqual(false);
        });

        it("throw an error if graph doesn't have viewModel", () => {
            // arrange
            const process: ProcessModels.IProcess = TestModels.createSystemDecisionForAddBranchTestModel();
            const decisionShape: ProcessModels.IProcessShape = process.shapes[3];
            const systemDecision = new SystemDecision(decisionShape, rootScope);
            
            graph = createGraph(process);
            graph.viewModel = null;
            spyOn(graph, "getNodeById").and.returnValue(systemDecision);
            let error: Error;

            // act
            try {
                subArtifactEditorModalOpener.getDecisionEditorModel(decisionShape.id, graph);
            } catch (ex) {
                error = ex;
            }
            
            // assert
            expect(error).not.toBeNull();
        });
    });

    function createGraph(process: ProcessModels.IProcess): ProcessGraph {
        let viewModel = new ProcessViewModel(process, communicationManager);
        return new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization);
    }
});
