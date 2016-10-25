import {IModalDialogCommunication} from "../modal-dialogs/modal-dialog-communication";
import {Condition} from "../diagram/presentation/graph/shapes/condition";
import {ModalDialogType} from "./modal-dialog-constants";
import {UserStoryDialogModel} from "./models/user-story-dialog-model";
import {DecisionEditorModel} from "./decision-editor/decision-editor-model";
import {
    IProcessGraph,
    IDiagramNode,
    ICondition,
    IDecision
} from "../diagram/presentation/graph/models/process-graph-interfaces";
import {UserTask} from "../diagram/presentation/graph/shapes/user-task";
import {SystemTask} from "../diagram/presentation/graph/shapes/system-task";
import {NodeType} from "../diagram/presentation/graph/models/";
import {IProcessLink} from "../diagram/presentation/graph/models/";
import {UserTaskModalController} from "./task-editor/user-task-modal-controller";
import {SystemTaskModalController} from "./task-editor/system-task-modal-controller";
import {UserStoryPreviewController} from "./user-story-preview/user-story-preview";
import {UserTaskDialogModel, SystemTaskDialogModel} from "./task-editor/sub-artifact-dialog-model";
import ModalSettings = angular.ui.bootstrap.IModalSettings;
import {ILocalizationService} from "../../../../core";

export class SubArtifactEditorModalOpener {
    private animationsEnabled: boolean = true;
    private isReadonly: boolean;
    private isHistorical: boolean;
    private setGraphHandler: string;
    private openDialogCallerHandler: string;

    constructor(
        private $scope: ng.IScope,
        private $uibModal: angular.ui.bootstrap.IModalService,
        private $rootScope: ng.IRootScopeService,
        private dialogCommunication: IModalDialogCommunication,
        private localization: ILocalizationService
    ) {
        this.setGraphHandler = dialogCommunication.registerSetGraphObserver(this.setGraph);
        this.openDialogCallerHandler = dialogCommunication.registerOpenDialogObserver(this.openDialogCaller);
    }

    public getGraph: () => any;

    private setGraph = (graph) => {
        this.getGraph = graph;
    }

    private openDialogCaller = (args: any[]) => {
        this.openDialog.apply(this, args);
    }

    private openDialog = (id: number, dialogType: ModalDialogType) => {
        window.console.log(`Open dialog with parameters ${id}, ${dialogType}`);

        try {
            const graph = this.getGraph();

            // get read-only status from viewmodel's isReadonly property
            const viewModel = graph.viewModel;
            if (viewModel) {
                this.isReadonly = viewModel.isReadonly;
                this.isHistorical = viewModel.isHistorical;
            } else {
                throw new Error("ProcessViewModel is null in SubArtifactEditorModalOpener");
            }

            if (dialogType === ModalDialogType.UserTaskDetailsDialogType) {
                this.openUserTaskDetailsModalDialog(this.$scope, id, graph);
            }

            if (dialogType === ModalDialogType.SystemTaskDetailsDialogType) {
                this.openSystemTaskDetailsModalDialog(this.$scope, id, graph);
            } else if (dialogType === ModalDialogType.UserSystemDecisionDetailsDialogType) {
                this.openDecisionEditorDialog(this.$scope, id, graph);
            } else if (dialogType === ModalDialogType.PreviewDialogType) {
                this.openPreviewModalDialog(this.$scope, id, graph);
            }
        } catch (err) {
            window.console.log(err);
        }
    };

    public getSubArtifactUserTaskDialogModel(shapeId: number, graph: IProcessGraph): UserTaskDialogModel {
        const taskDialogModel = new UserTaskDialogModel();
        taskDialogModel.artifactId = graph.viewModel.id;
        taskDialogModel.subArtifactId = shapeId;

        const node = graph.getNodeById(taskDialogModel.subArtifactId.toString());
        let userTaskNode: UserTask;

        // set dialog model isReadonly property to enable/disable input controls
        taskDialogModel.isReadonly = this.isReadonly;
        taskDialogModel.isHistoricalVersion = this.isHistorical;
        
        if (node.getNodeType() === NodeType.UserTask) {
            userTaskNode = <UserTask>node;
            if (userTaskNode) { // When the node selected is the "pre-condition",the user taskNode is null, since it is the start node
                taskDialogModel.originalItem = userTaskNode;
                taskDialogModel.action = taskDialogModel.originalItem.action;
                taskDialogModel.associatedArtifact = taskDialogModel.originalItem.associatedArtifact;
                taskDialogModel.objective = taskDialogModel.originalItem.objective;
                taskDialogModel.label = taskDialogModel.originalItem.label;
                taskDialogModel.persona = taskDialogModel.originalItem.persona;
            }
        } else {
            return null;
        }

        return taskDialogModel;
    }

    public getSubArtifactSystemTaskDialogModel(shapeId: number, graph: IProcessGraph): SystemTaskDialogModel {
        const taskDialogModel = new SystemTaskDialogModel();
        taskDialogModel.subArtifactId = shapeId;
        taskDialogModel.artifactId = graph.viewModel.id;
        // set dialog model isReadonly property to enable/disable input controls
        taskDialogModel.isReadonly = this.isReadonly;
        taskDialogModel.isHistoricalVersion = this.isHistorical;

        const node = graph.getNodeById(taskDialogModel.subArtifactId.toString());

        let systemTaskNode: SystemTask;

        if (node.getNodeType() === NodeType.SystemTask) {
            systemTaskNode = <SystemTask>node;

            if (systemTaskNode) { // When the node selected is the "pre-condition",the user taskNode is null, since it is the start node
                taskDialogModel.originalItem = systemTaskNode;
                taskDialogModel.action = taskDialogModel.originalItem.action;
                taskDialogModel.associatedArtifact = taskDialogModel.originalItem.associatedArtifact;
                taskDialogModel.imageId = taskDialogModel.originalItem.imageId;
                taskDialogModel.label = taskDialogModel.originalItem.label;
                taskDialogModel.persona = taskDialogModel.originalItem.persona;
                taskDialogModel.associatedImageUrl = taskDialogModel.originalItem.associatedImageUrl;
            }
        } else {
            return null;
        }

        return taskDialogModel;
    }

    public getDecisionEditorModel(shapeId: number, graph: IProcessGraph): DecisionEditorModel {
        const decision = <IDecision>graph.getNodeById(shapeId.toString());

        if (!decision) {
            return null;
        }

        const model: DecisionEditorModel = new DecisionEditorModel();

        model.artifactId = graph.viewModel.id;

        model.graph = graph;
        model.conditions = [];

        // cloning decision
        model.originalDecision = decision;
        model.label = decision.label;

        // populate existing conditions
        const outgoingLinks: IProcessLink[] = model.graph.getNextLinks(decision.model.id);

        for (let index = 0; index < outgoingLinks.length; index++) {
            const outgoingLink: IProcessLink = outgoingLinks[index];
            let mergePoint: IDiagramNode = null;

            // We do not display change merge node option for first branch
            if (index !== 0) {
                const mergeNodeId: string = model.originalDecision.getMergeNode(graph, outgoingLink.orderindex).id.toString();
                mergePoint = graph.getNodeById(mergeNodeId);
            }

            const validMergeNodes: IDiagramNode[] = model.graph.getValidMergeNodes(outgoingLink);
            const condition: ICondition = Condition.create(outgoingLink, mergePoint, validMergeNodes);
            model.conditions.push(condition);
        }

        // set dialog model isReadonly property to enable/disable input controls
        model.isReadonly = this.isReadonly;
        model.isHistoricalVersion = this.isHistorical;

        return model;
    }

    private openUserTaskDetailsModalDialog = ($scope: ng.IScope, shapeId: number, graph: IProcessGraph): void => {
        const settings = <ModalSettings>{
            animation: this.animationsEnabled,
            component: "userTaskEditor",
            resolve: {
                dialogModel: () => this.getSubArtifactUserTaskDialogModel(shapeId, graph)
            },
            windowClass: "storyteller-modal"
        };

        this.$uibModal.open(settings);
    }

    private openSystemTaskDetailsModalDialog = ($scope: ng.IScope, shapeId: number, graph: IProcessGraph): void => {
        const settings = <ModalSettings>{
            animation: this.animationsEnabled,
            component: "systemTaskEditor",
            resolve: {
                dialogModel: () => this.getSubArtifactSystemTaskDialogModel(shapeId, graph)
            },
            windowClass: "storyteller-modal"
        };

        this.$uibModal.open(settings);
    }

    private openDecisionEditorDialog = ($scope: ng.IScope, shapeId: number, graph: IProcessGraph): void => {
        const settings = <ModalSettings>{
            animation: this.animationsEnabled,
            component: "decisionEditor",
            resolve: {
                dialogModel: () => this.getDecisionEditorModel(shapeId, graph)
            },
            windowClass: "storyteller-modal"
        };

        this.$uibModal.open(settings);
    };

    public getUserStoryDialogModel(shapeId: number, graph: IProcessGraph): UserStoryDialogModel {
        const userStoryDialogModel = new UserStoryDialogModel();
        userStoryDialogModel.artifactId = graph.viewModel.id;
        userStoryDialogModel.subArtifactId = shapeId;
        const node = graph.getNodeById(userStoryDialogModel.subArtifactId.toString());
        const userTaskNode = <UserTask>node;
        userStoryDialogModel.previousSytemTasks = userTaskNode.getPreviousSystemTasks(graph) as SystemTask[];
        userStoryDialogModel.nextSystemTasks = userTaskNode.getNextSystemTasks(graph) as SystemTask[];
        userStoryDialogModel.originalUserTask = userTaskNode;
        userStoryDialogModel.clonedUserTask = userStoryDialogModel.originalUserTask.cloneUserTask();
        userStoryDialogModel.isUserSystemProcess = graph.isUserSystemProcess;

        // set dialog model isReadonly property to enable/disable input controls
        userStoryDialogModel.isReadonly = this.isReadonly;
        userStoryDialogModel.isHistoricalVersion = this.isHistorical;

        return userStoryDialogModel;
    }

    private openPreviewModalDialog($scope: ng.IScope, shapeId: number, graph: IProcessGraph) {
        this.open("",
            require("./user-story-preview/user-story-preview.html"),
            UserStoryPreviewController,
            this.getUserStoryDialogModel(shapeId, graph),
            "preview-modal");
    }

    public cloneNode = (node): any => {
        return JSON.parse(JSON.stringify(node));
    }

    public cloneArray = (arr: any[]): any[] => {
        const retArr: any[] = [];
        for (let node of arr) {
            retArr.push(this.cloneNode(node));
        }

        return retArr;
    }

    public open = (size, htmlTemplate: string, ctrl: any, dialogModel: any, windowClass: string) => {
        this.$uibModal.open(<ModalSettings>{
            okButton: this.localization.get("App_Button_Ok"),
            animation: this.animationsEnabled,
            template: htmlTemplate,
            controller: ctrl,
            controllerAs: "vm",
            windowClass: windowClass,
            size: size,
            resolve: {
                dialogModel: () => dialogModel
            }
        });
    }

    public onDestroy = () => {
        this.dialogCommunication.removeSetGraphObserver(this.setGraphHandler);
        this.dialogCommunication.removeOpenDialogObserver(this.openDialogCallerHandler);
    }
}
