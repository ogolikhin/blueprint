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
import {ProcessViewModel} from "../diagram/viewmodel/process-viewmodel";
import {UserTask, SystemTask, UserDecision} from "../diagram/presentation/graph/shapes";
import {ICondition} from "../diagram/presentation/graph/models";
import {SubArtifactEditorModalOpener} from "./sub-artifact-editor-modal-opener";
import {UserStoryPreviewController} from "./user-story-preview/user-story-preview";
import {UserTaskDialogModel, SystemTaskDialogModel} from "./task-editor/sub-artifact-dialog-model";
import {DecisionEditorModel} from "./decision-editor/decision-editor-model";
import {UserStoryDialogModel} from "./models/user-story-dialog-model";
import * as TestModels from "../../models/test-model-factory";
import * as ProcessModels from "../../models/process-models";

class ExecutionEnvironmentDetectorMock {
    private browserInfo: any;

    constructor() {
        this.browserInfo = { msie: false, firefox: false, version: 0 };
    }

    public getBrowserInfo(): any {
        return this.browserInfo;
    }
}

describe("SubArtifactEditorModalOpener test", () => {
    let dialogManager: IModalDialogCommunication;
    let localScope, localization;
    let dialogService: DialogServiceMock;
    let modalOpener: SubArtifactEditorModalOpener;
    let communicationManager: CommunicationManager;
    let $uibModal: ModalServiceMock;
    let rootScope: any;
    let graph: ProcessGraph;
    let wrapper, container;

    let w: any = window;
    w.executionEnvironmentDetector = ExecutionEnvironmentDetectorMock;

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

        localScope = {
            graphContainer: container, 
            graphWrapper: wrapper, 
            isSpa: false,
            vm: {
                "$rootScope": rootScope
            }
        };
    }));

    beforeEach(() => {
        dialogManager = new ModalDialogCommunication();
        modalOpener = new SubArtifactEditorModalOpener(
            $uibModal,
            communicationManager.modalDialogManager,
            localization
        );
    });

    afterEach(() => {
        dialogManager.onDestroy();
        dialogManager = null;
        modalOpener.destroy();
        modalOpener = null;
    });

    it("registers listener for graph changes when created", () => {
        // arrange
        const setGraphSpy = spyOn(communicationManager.modalDialogManager, "registerSetGraphObserver");

        // act
        new SubArtifactEditorModalOpener($uibModal, communicationManager.modalDialogManager, localization);

        // assert
        expect(setGraphSpy).toHaveBeenCalled();
    });

    it("unregisters listener for graph changes when destroyed", () => {
        // arrange
        const modalOpener = new SubArtifactEditorModalOpener($uibModal, communicationManager.modalDialogManager, localization);
        const setGraphSpy = spyOn(communicationManager.modalDialogManager, "removeSetGraphObserver");

        // act
        modalOpener.destroy();

        // assert
        expect(setGraphSpy).toHaveBeenCalled();
    });

    it("registers listener for open dialog invocations", () => {
        // arrange
        const openDialogSpy = spyOn(communicationManager.modalDialogManager, "registerOpenDialogObserver");

        // act
        new SubArtifactEditorModalOpener($uibModal, communicationManager.modalDialogManager, localization);

        // assert
        expect(openDialogSpy).toHaveBeenCalled();
    });

    it("unregisters listener for open dialog invocations when destroyed", () => {
        // arrange
        const modalOpener = new SubArtifactEditorModalOpener($uibModal, communicationManager.modalDialogManager, localization);
        const setGraphSpy = spyOn(communicationManager.modalDialogManager, "removeOpenDialogObserver");

        // act
        modalOpener.destroy();

        // assert
        expect(setGraphSpy).toHaveBeenCalled();
    });

    it("sets graph when set graph event is raised", () => {
        // arrange
        const process = TestModels.createSystemDecisionForAddBranchTestModel();
        graph = createGraph(process);
        const getGraph = () => graph;

        // act
        communicationManager.modalDialogManager.setGraph(getGraph);

        // assert
        expect(modalOpener["graph"]).toEqual(graph);
        expect(modalOpener["graph"].viewModel).not.toBeNull();
    });

    describe("openDialog", () => {
        it("doesn't open dialog for an unknown dialog type", () => {
            // arrange
            const shapeId: number = 1;
            const dialogType: ModalDialogType = ModalDialogType.None;
            const openSpy = spyOn($uibModal, "open");

            // act
            communicationManager.modalDialogManager.openDialog(shapeId, dialogType);

            // assert
            expect(openSpy).not.toHaveBeenCalled();
        });

        it("handles exception raised while getting modal settings", () => {
            // arrange
            const shapeId: number = 1;
            const dialogType: ModalDialogType = ModalDialogType.UserTaskDetailsDialogType;
            const errorMessage: string = "Test Error";
            const getSettingsSpy = spyOn(modalOpener, "getModalSettings").and.throwError(errorMessage);
            const handlerSpy = spyOn(window.console, "log");

            // act
            communicationManager.modalDialogManager.openDialog(shapeId, dialogType);

            // assert
            expect(handlerSpy).toHaveBeenCalledWith(new Error(errorMessage));
        });

        it("handles exception raised while opening dialog", () => {
            // arrange
            const shapeId: number = 1;
            const dialogType: ModalDialogType = ModalDialogType.UserTaskDetailsDialogType;
            const errorMessage: string = "Test Error";
            const openSpy = spyOn($uibModal, "open").and.throwError(errorMessage);
            const handlerSpy = spyOn(window.console, "log");

            // act
            communicationManager.modalDialogManager.openDialog(shapeId, dialogType);

            // assert
            expect(handlerSpy).toHaveBeenCalledWith(new Error(errorMessage));
        });

        it("opens User Task dialog when User Task dialog is invoked", () => {
            // arrange
            const process = TestModels.createDefaultProcessModel();
            const userTaskShape = <ProcessModels.IUserTaskShape>process.shapes[2];
            const shapeId: number = userTaskShape.id; 
            
            graph = createGraph(process);
            modalOpener["graph"] = graph;

            const userTask = new UserTask(userTaskShape, rootScope, null, null);
            const settings = {
                animation: true,
                component: "userTaskEditor",
                resolve: {
                    dialogModel: () => <UserTaskDialogModel>{
                        subArtifactId: shapeId,
                        isHistoricalVersion: false,
                        isReadonly: false,
                        originalItem: userTask,
                        persona: userTask.persona,
                        action: userTask.action,
                        associatedArtifact: userTask.associatedArtifact,
                        label: userTask.label,
                        objective: userTask.objective
                    }
                },
                windowClass: "storyteller-modal"
            };
            
            const settingsSpy = spyOn(modalOpener, "getUserTaskEditorDialogSettings").and.returnValue(settings);
            const openSpy = spyOn($uibModal, "open");
            
            // act
            communicationManager.modalDialogManager.openDialog(shapeId, ModalDialogType.UserTaskDetailsDialogType);

            // assert
            expect(settingsSpy).toHaveBeenCalledWith(shapeId, graph);
            expect(openSpy).toHaveBeenCalled();
        });

        it("opens System Task dialog when System Task dialog is invoked", () => {
            // arrange
            const process = TestModels.createDefaultProcessModel();
            const systemTaskShape = <ProcessModels.ISystemTaskShape>process.shapes[3];
            const shapeId: number = systemTaskShape.id; 
            
            graph = createGraph(process);
            modalOpener["graph"] = graph;

            const systemTask = new SystemTask(systemTaskShape, rootScope, "", null, null);
            const settings = {
                animation: true,
                component: "systemTaskEditor",
                resolve: {
                    dialogModel: () => <SystemTaskDialogModel>{
                        subArtifactId: shapeId,
                        isHistoricalVersion: false,
                        isReadonly: false,
                        originalItem: systemTask,
                        persona: systemTask.persona,
                        action: systemTask.action,
                        associatedArtifact: systemTask.associatedArtifact,
                        label: systemTask.label,
                        imageId: systemTask.imageId,
                        associatedImageUrl: systemTask.associatedImageUrl
                    }
                },
                windowClass: "storyteller-modal"
            };

            const settingsSpy = spyOn(modalOpener, "getSystemTaskEditorDialogSettings").and.returnValue(settings);
            const openSpy = spyOn($uibModal, "open");
            
            // act
            communicationManager.modalDialogManager.openDialog(shapeId, ModalDialogType.SystemTaskDetailsDialogType);

            // assert
            expect(settingsSpy).toHaveBeenCalledWith(shapeId, graph);
            expect(openSpy).toHaveBeenCalledWith(settings);
        });

        it("opens Decision dialog when Decision dialog is invoked", () => {
            // arrange
            const process = TestModels.createUserDecisionForAddBranchTestModel();
            const decisionShape = process.shapes[3];
            const shapeId: number = decisionShape.id; 
            
            graph = createGraph(process);
            modalOpener["graph"] = graph;

            const decision = new UserDecision(decisionShape, rootScope);
            const settings = {
                animation: true,
                component: "decisionEditor",
                resolve: {
                    dialogModel: () => <DecisionEditorModel>{
                        subArtifactId: shapeId,
                        label: decision.label,
                        conditions: [
                            <ICondition>{
                                sourceId: shapeId,
                                destinationId: 30,
                                orderindex: 0,
                                label: ""
                            },
                            <ICondition>{
                                sourceId: shapeId,
                                destinationId: 40,
                                orderindex: 1,
                                label: ""
                            }
                        ],
                        graph: graph,
                        originalDecision: decision,
                        isReadonly: false,
                        isHistoricalVersion: false
                    }
                },
                windowClass: "storyteller-modal"
            };

            const settingsSpy = spyOn(modalOpener, "getDecisionEditorDialogSettings").and.returnValue(settings);
            const openSpy = spyOn($uibModal, "open");
            
            // act
            communicationManager.modalDialogManager.openDialog(shapeId, ModalDialogType.UserSystemDecisionDetailsDialogType);

            // assert
            expect(settingsSpy).toHaveBeenCalledWith(shapeId, graph);
            expect(openSpy).toHaveBeenCalledWith(settings);
        });

        it("opens User Task Preview Dialog when User Task Preview Dialog is invoked", () => {
            // arrange
            const process = TestModels.createDefaultProcessModel();
            const userTaskShape = <ProcessModels.IUserTaskShape>process.shapes[2];
            const shapeId: number = userTaskShape.id; 
            
            graph = createGraph(process);
            modalOpener["graph"] = graph;

            const userTask = new UserTask(userTaskShape, rootScope, null, null);
            const settings = {
                okButton: localization.get("App_Button_Ok"),
                animation: true,
                template: require("./user-story-preview/user-story-preview.html"),
                controller: UserStoryPreviewController,
                controllerAs: "vm",
                windowClass: "preview-modal",
                size: "",
                resolve: {
                    dialogModel: () => <UserStoryDialogModel>{
                        clonedUserTask: userTask,
                        originalUserTask: userTask,
                        previousSytemTasks: [],
                        nextSystemTasks: [],
                        subArtifactId: shapeId,
                        isUserSystemProcess: false,
                        propertiesMw: null,
                        isReadonly: false,
                        isHistoricalVersion: false
                    }
                }
            };

            const settingsSpy = spyOn(modalOpener, "getPreviewModalDialogSettings").and.returnValue(settings);
            spyOn(graph, "getNodeById").and.returnValue(userTask);
            const openSpy = spyOn($uibModal, "open");

            // act
            communicationManager.modalDialogManager.openDialog(shapeId, ModalDialogType.PreviewDialogType);

            // assert
            expect(settingsSpy).toHaveBeenCalledWith(shapeId, graph);
            expect(openSpy).toHaveBeenCalledWith(settings);
        });

        describe("getUserTaskEditorDialogSettings", () => {
            it("throws error is graph is null", () => {
                // arrange
                const process = TestModels.createDefaultProcessModel();
                const userTaskShape = <ProcessModels.IUserTaskShape>process.shapes[2];
                const shapeId: number = userTaskShape.id;
                
                let error: Error;
            
                // act
                try {
                    modalOpener["getUserTaskDialogModel"](shapeId, null);
                } catch (ex) {
                    error = ex;
                }

                // assert
                expect(error).not.toBeNull();
            });

            it("throws error is graph.viewModel is null", () => {
                // arrange
                const process = TestModels.createDefaultProcessModel();
                const userTaskShape = <ProcessModels.IUserTaskShape>process.shapes[2];
                const shapeId: number = userTaskShape.id;

                graph = createGraph(process);
                graph.viewModel = null;
                
                let error: Error;
            
                // act
                try {
                    modalOpener["getUserTaskDialogModel"](shapeId, graph);
                } catch (ex) {
                    error = ex;
                }

                // assert
                expect(error).not.toBeNull();
            });

            it("returns null for non-User Task node", () => {
                // arrange
                const process = TestModels.createDefaultProcessModel();
                const systemTaskShape = <ProcessModels.ISystemTaskShape>process.shapes[3];
                const shapeId: number = systemTaskShape.id;
                const systemTask = new SystemTask(systemTaskShape, rootScope, "", null, null);

                graph = createGraph(process);
                spyOn(graph, "getNodeById").and.returnValue(systemTask);
                modalOpener["graph"] = graph;
                
                // act
                const model = modalOpener["getUserTaskDialogModel"](shapeId, graph);

                // assert
                expect(model).toBeNull();
            });

            it("returns model as read-only if process is read-only", () => {
                // arrange
                const process = TestModels.createDefaultProcessModel();
                process["artifactState"] = {readonly: true};
                const userTaskShape = <ProcessModels.IUserTaskShape>process.shapes[2];
                const shapeId: number = userTaskShape.id;
                const userTask = new UserTask(userTaskShape, rootScope, null, null);

                graph = createGraph(process);
                spyOn(graph, "getNodeById").and.returnValue(userTask);
                modalOpener["graph"] = graph;
                
                // act
                const model = modalOpener["getUserTaskDialogModel"](shapeId, graph);

                // assert
                expect(model.isReadonly).toBe(true);
            });

            it("returns model as historical if process is historical", () => {
                // arrange
                const process = TestModels.createDefaultProcessModel();
                process["historical"] = true;
                const userTaskShape = <ProcessModels.IUserTaskShape>process.shapes[2];
                const shapeId: number = userTaskShape.id;
                const userTask = new UserTask(userTaskShape, rootScope, null, null);

                graph = createGraph(process);
                spyOn(graph, "getNodeById").and.returnValue(userTask);
                modalOpener["graph"] = graph;
                
                // act
                const model = modalOpener["getUserTaskDialogModel"](shapeId, graph);

                // assert
                expect(model.isHistoricalVersion).toBe(true);
            });

            it("returns correct properties", () => {
                // arrange
                const process = TestModels.createDefaultProcessModel();
                const userTaskShape = <ProcessModels.IUserTaskShape>process.shapes[2];
                const shapeId: number = userTaskShape.id;
                const userTask = new UserTask(userTaskShape, rootScope, null, null);

                graph = createGraph(process);
                spyOn(graph, "getNodeById").and.returnValue(userTask);
                modalOpener["graph"] = graph;
                
                // act
                const model = modalOpener["getUserTaskDialogModel"](shapeId, graph);

                // assert
                expect(model.subArtifactId).toBe(shapeId);
                expect(model.originalItem).toBe(userTask);
                expect(model.isReadonly).toBe(graph.viewModel.isReadonly);
                expect(model.isHistoricalVersion).toBe(graph.viewModel.isHistorical);
                expect(model.persona).toBe(userTask.persona);
                expect(model.action).toBe(userTask.action);
                expect(model.associatedArtifact).toBe(userTask.associatedArtifact);
                expect(model.label).toBe(userTask.label);
                expect(model.objective).toBe(userTask.objective);
            });
        });

        describe("getSystemTaskDialogModel", () => {
            it("throws error is graph is null", () => {
                // arrange
                const process = TestModels.createDefaultProcessModel();
                const systemTaskShape = <ProcessModels.ISystemTaskShape>process.shapes[3];
                const shapeId: number = systemTaskShape.id;
                
                let error: Error;
            
                // act
                try {
                    modalOpener["getSystemTaskDialogModel"](shapeId, null);
                } catch (ex) {
                    error = ex;
                }

                // assert
                expect(error).not.toBeNull();
            });

            it("throws error is graph.viewModel is null", () => {
                // arrange
                const process = TestModels.createDefaultProcessModel();
                const systemTaskShape = <ProcessModels.ISystemTaskShape>process.shapes[3];
                const shapeId: number = systemTaskShape.id;

                graph = createGraph(process);
                graph.viewModel = null;
                
                let error: Error;
            
                // act
                try {
                    modalOpener["getSystemTaskDialogModel"](shapeId, graph);
                } catch (ex) {
                    error = ex;
                }

                // assert
                expect(error).not.toBeNull();
            });

            it("returns null for non-System Task node", () => {
                // arrange
                const process = TestModels.createDefaultProcessModel();
                const userTaskShape = <ProcessModels.IUserTaskShape>process.shapes[2];
                const shapeId: number = userTaskShape.id;
                const userTask = new UserTask(userTaskShape, rootScope, null, null);

                graph = createGraph(process);
                spyOn(graph, "getNodeById").and.returnValue(userTask);
                modalOpener["graph"] = graph;
                
                // act
                const model = modalOpener["getSystemTaskDialogModel"](shapeId, graph);

                // assert
                expect(model).toBeNull();
            });

            it("returns model as read-only if process is read-only", () => {
                // arrange
                const process = TestModels.createDefaultProcessModel();
                process["artifactState"] = {readonly: true};
                const systemTaskShape = <ProcessModels.ISystemTaskShape>process.shapes[3];
                const shapeId: number = systemTaskShape.id;
                const systemTask = new SystemTask(systemTaskShape, rootScope, "", null, null);

                graph = createGraph(process);
                spyOn(graph, "getNodeById").and.returnValue(systemTask);
                modalOpener["graph"] = graph;
                
                // act
                const model = modalOpener["getSystemTaskDialogModel"](shapeId, graph);

                // assert
                expect(model.isReadonly).toBe(true);
            });

            it("returns model as historical if process is historical", () => {
                // arrange
                const process = TestModels.createDefaultProcessModel();
                process["historical"] = true;
                const systemTaskShape = <ProcessModels.ISystemTaskShape>process.shapes[3];
                const shapeId: number = systemTaskShape.id;
                const systemTask = new SystemTask(systemTaskShape, rootScope, "", null, null);

                graph = createGraph(process);
                spyOn(graph, "getNodeById").and.returnValue(systemTask);
                modalOpener["graph"] = graph;
                
                // act
                const model = modalOpener["getSystemTaskDialogModel"](shapeId, graph);

                // assert
                expect(model.isHistoricalVersion).toBe(true);
            });

            it("returns correct properties", () => {
                // arrange
                const process = TestModels.createDefaultProcessModel();
                const systemTaskShape = <ProcessModels.ISystemTaskShape>process.shapes[3];
                const shapeId: number = systemTaskShape.id;
                const systemTask = new SystemTask(systemTaskShape, rootScope, "", null, null);

                graph = createGraph(process);
                spyOn(graph, "getNodeById").and.returnValue(systemTask);
                modalOpener["graph"] = graph;
                
                // act
                const model = modalOpener["getSystemTaskDialogModel"](shapeId, graph);

                // assert
                expect(model.subArtifactId).toBe(shapeId);
                expect(model.originalItem).toBe(systemTask);
                expect(model.isReadonly).toBe(graph.viewModel.isReadonly);
                expect(model.isHistoricalVersion).toBe(graph.viewModel.isHistorical);
                expect(model.persona).toBe(systemTask.persona);
                expect(model.action).toBe(systemTask.action);
                expect(model.associatedArtifact).toBe(systemTask.associatedArtifact);
                expect(model.label).toBe(systemTask.label);
                expect(model.imageId).toBe(systemTask.imageId);
                expect(model.associatedImageUrl).toBe(systemTask.associatedImageUrl);
            });
        });

        describe("getDecisionEditorModel", () => {
            it("throws error is graph is null", () => {
                // arrange
                const process = TestModels.createUserDecisionForAddBranchTestModel();
                const userDecisionShape = process.shapes[3];
                const shapeId: number = userDecisionShape.id;
                
                let error: Error;
            
                // act
                try {
                    modalOpener["getDecisionEditorModel"](shapeId, null);
                } catch (ex) {
                    error = ex;
                }

                // assert
                expect(error).not.toBeNull();
            });

            it("throws error is graph.viewModel is null", () => {
                // arrange
                const process = TestModels.createUserDecisionForAddBranchTestModel();
                const userDecisionShape = process.shapes[3];
                const shapeId: number = userDecisionShape.id;

                graph = createGraph(process);
                graph.viewModel = null;
                
                let error: Error;
            
                // act
                try {
                    modalOpener["getDecisionEditorModel"](shapeId, graph);
                } catch (ex) {
                    error = ex;
                }

                // assert
                expect(error).not.toBeNull();
            });

            it("returns null for non-System Task node", () => {
                // arrange
                const process = TestModels.createUserDecisionForAddBranchTestModel();
                const systemTaskShape = <ProcessModels.ISystemTaskShape>process.shapes[1];
                const shapeId: number = systemTaskShape.id;
                const userTask = new SystemTask(systemTaskShape, rootScope, "", null, null);

                graph = createGraph(process);
                spyOn(graph, "getNodeById").and.returnValue(userTask);
                modalOpener["graph"] = graph;
                
                // act
                const model = modalOpener["getDecisionEditorModel"](shapeId, graph);

                // assert
                expect(model).toBeNull();
            });

            it("returns model as read-only if process is read-only", () => {
                // arrange
                const process = TestModels.createUserDecisionForAddBranchTestModel();
                process["artifactState"] = {readonly: true};
                const userDecisionShape = process.shapes[3];
                const shapeId: number = userDecisionShape.id;
                const userDecision = new UserDecision(userDecisionShape, rootScope);

                graph = createGraph(process);
                spyOn(graph, "getNodeById").and.returnValue(userDecision);
                modalOpener["graph"] = graph;
                
                // act
                const model = modalOpener["getDecisionEditorModel"](shapeId, graph);

                // assert
                expect(model.isReadonly).toBe(true);
            });

            it("returns model as historical if process is historical", () => {
                // arrange
                const process = TestModels.createUserDecisionForAddBranchTestModel();
                process["historical"] = true;
                const userDecisionShape = process.shapes[3];
                const shapeId: number = userDecisionShape.id;
                const userDecision = new UserDecision(userDecisionShape, rootScope);

                graph = createGraph(process);
                spyOn(graph, "getNodeById").and.returnValue(userDecision);
                modalOpener["graph"] = graph;
                
                // act
                const model = modalOpener["getDecisionEditorModel"](shapeId, graph);

                // assert
                expect(model.isHistoricalVersion).toBe(true);
            });

            it("returns correct properties", () => {
                // arrange
                const process = TestModels.createUserDecisionForAddBranchTestModel();
                const userDecisionShape = process.shapes[3];
                const shapeId: number = userDecisionShape.id;
                const userDecision = new UserDecision(userDecisionShape, rootScope);

                graph = createGraph(process);
                spyOn(graph, "getNodeById").and.returnValue(userDecision);
                modalOpener["graph"] = graph;
                
                // act
                const model = modalOpener["getDecisionEditorModel"](shapeId, graph);

                // assert
                expect(model.subArtifactId).toBe(shapeId);
                expect(model.originalDecision).toBe(userDecision);
                expect(model.graph).toBe(graph);
                expect(model.isReadonly).toBe(graph.viewModel.isReadonly);
                expect(model.isHistoricalVersion).toBe(graph.viewModel.isHistorical);
                expect(model.label).toEqual(userDecision.label);
                expect(model.conditions).not.toBeNull();
                expect(model.conditions.length).toBe(2);
            });
        });

        describe("getUserStoryDialogModel", () => {
            it("throws error is graph is null", () => {
                // arrange
                const process = TestModels.createDefaultProcessModel();
                const userTaskShape = <ProcessModels.IUserTaskShape>process.shapes[2];
                const shapeId: number = userTaskShape.id;
                
                let error: Error;
            
                // act
                try {
                    modalOpener["getUserStoryDialogModel"](shapeId, null);
                } catch (ex) {
                    error = ex;
                }

                // assert
                expect(error).not.toBeNull();
            });

            it("throws error is graph.viewModel is null", () => {
                // arrange
                const process = TestModels.createDefaultProcessModel();
                const userTaskShape = <ProcessModels.IUserTaskShape>process.shapes[2];
                const shapeId: number = userTaskShape.id;

                graph = createGraph(process);
                graph.viewModel = null;
                
                let error: Error;
            
                // act
                try {
                    modalOpener["getUserStoryDialogModel"](shapeId, graph);
                } catch (ex) {
                    error = ex;
                }

                // assert
                expect(error).not.toBeNull();
            });

            it("returns null for non-User Task node", () => {
                // arrange
                const process = TestModels.createDefaultProcessModel();
                const systemTaskShape = <ProcessModels.ISystemTaskShape>process.shapes[3];
                const shapeId: number = systemTaskShape.id;
                const systemTask = new SystemTask(systemTaskShape, rootScope, "", null, null);

                graph = createGraph(process);
                spyOn(graph, "getNodeById").and.returnValue(systemTask);
                modalOpener["graph"] = graph;
                
                // act
                const model = modalOpener["getUserStoryDialogModel"](shapeId, graph);

                // assert
                expect(model).toBeNull();
            });

            xit("returns model as read-only if process is read-only", () => {
                // arrange
                const process = TestModels.createDefaultProcessModel();
                process["artifactState"] = {readonly: true};
                const userTaskShape = <ProcessModels.IUserTaskShape>process.shapes[2];
                const shapeId: number = userTaskShape.id;
                const userTask = new UserTask(userTaskShape, rootScope, null, null);

                graph = createGraph(process);
                spyOn(graph, "getNodeById").and.returnValue(userTask);
                modalOpener["graph"] = graph;
                
                // act
                const model = modalOpener["getUserStoryDialogModel"](shapeId, graph);

                // assert
                expect(model.isReadonly).toBe(true);
            });

            xit("returns model as historical if process is historical", () => {
                // arrange
                const process = TestModels.createDefaultProcessModel();
                process["historical"] = true;
                const userTaskShape = <ProcessModels.IUserTaskShape>process.shapes[2];
                const shapeId: number = userTaskShape.id;
                const userTask = new UserTask(userTaskShape, rootScope, null, null);

                graph = createGraph(process);
                spyOn(graph, "getNodeById").and.returnValue(userTask);
                modalOpener["graph"] = graph;
                
                // act
                const model = modalOpener["getUserStoryDialogModel"](shapeId, graph);

                // assert
                expect(model.isHistoricalVersion).toBe(true);
            });

            xit("returns correct properties", () => {
                // arrange
                const process = TestModels.createDefaultProcessModel();
                const userTaskShape = <ProcessModels.IUserTaskShape>process.shapes[2];
                const shapeId: number = userTaskShape.id;
                const userTask = new UserTask(userTaskShape, rootScope, null, null);

                graph = createGraph(process);
                spyOn(graph, "getNodeById").and.returnValue(userTask);
                modalOpener["graph"] = graph;
                
                // act
                const model = modalOpener["getUserStoryDialogModel"](shapeId, graph);

                // assert
                expect(model.subArtifactId).toBe(shapeId);
                expect(model.originalUserTask).toBe(userTask);
                expect(model.clonedUserTask).toEqual(userTask);
                expect(model.isReadonly).toBe(graph.viewModel.isReadonly);
                expect(model.isHistoricalVersion).toBe(graph.viewModel.isHistorical);
                expect(model.previousSytemTasks).not.toBeNull();
                expect(model.previousSytemTasks.length).toBe(0);
                expect(model.nextSystemTasks).not.toBeNull();
                expect(model.nextSystemTasks.length).toBe(0);
                expect(model.isUserSystemProcess).toBe(false);
                expect(model.propertiesMw).not.toBeDefined();
            });
        });
    });

    function createGraph(process: ProcessModels.IProcess): ProcessGraph {
        let viewModel = new ProcessViewModel(process, communicationManager);
        return new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization);
    }
});
