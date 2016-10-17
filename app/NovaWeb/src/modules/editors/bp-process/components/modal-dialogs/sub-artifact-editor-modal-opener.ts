import {IModalDialogCommunication} from "../modal-dialogs/modal-dialog-communication";
import {Condition} from "../diagram/presentation/graph/shapes/condition";
import {ModalDialogType} from "./base-modal-dialog-controller";
import {SubArtifactDialogModel} from "./models/sub-artifact-dialog-model";
import {UserStoryDialogModel} from "./models/user-story-dialog-model";
import {SubArtifactDecisionDialogModel} from "./models/sub-artifact-decision-dialog-model";
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
import {SubArtifactEditorModalController} from "./sub-artifact-editor-modal-controller";
import {UserStoryPreviewController} from "./user-story-preview/user-story-preview";
import {ModalProcessViewModel} from "./models/modal-process-view-model";
import ModalSettings = angular.ui.bootstrap.IModalSettings;
import {ILocalizationService} from "../../../../core";

export class SubArtifactEditorModalOpener {
    private animationsEnabled: boolean = true;
    private isReadonly: boolean;
    private isHistorical: boolean;
    private setGraphHandler: string;
    private openDialogCallerHandler: string;
    private setModalProcessViewModelHandler: string;
    private modalProcessViewModel: ModalProcessViewModel;

    public getGraph: () => any;

    constructor(private $scope: ng.IScope,
                private $uibModal: angular.ui.bootstrap.IModalService,
                private $rootScope: ng.IRootScopeService,
                private dialogCommunication: IModalDialogCommunication,
                private localization: ILocalizationService) {
        this.setGraphHandler = dialogCommunication.registerSetGraphObserver(this.setGraph);
        this.openDialogCallerHandler = dialogCommunication.registerOpenDialogObserver(this.openDialogCaller);
        this.setModalProcessViewModelHandler = dialogCommunication.registerModalProcessViewModelObserver(this.setModalProcessViewModel);
    }

    private setGraph = (graph) => {
        this.getGraph = graph;
        this.modalProcessViewModel = new ModalProcessViewModel(this.getGraph().viewModel);
    }

    private setModalProcessViewModel = (setModalProcessViewModel) => {
        setModalProcessViewModel(this.modalProcessViewModel);
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

            if (dialogType === ModalDialogType.UserSystemTaskDetailsDialogType) {
                this.openUserSystemTaskDetailsModalDialog(this.$scope, id, graph);
            } else if (dialogType === ModalDialogType.UserSystemDecisionDetailsDialogType) {
                this.openDecisionEditorDialog(this.$scope, id, graph);
            } else if (dialogType === ModalDialogType.PreviewDialogType) {
                this.openPreviewModalDialog(this.$scope, id, graph);
            }
        } catch (err) {
            window.console.log(err);
        }
    };

    public getSubArtifactDialogModel(shapeId: number, graph: IProcessGraph): SubArtifactDialogModel {
        const userTaskDialogModel = new SubArtifactDialogModel();
        userTaskDialogModel.subArtifactId = shapeId;

        const node = graph.getNodeById(userTaskDialogModel.subArtifactId.toString());
        let userTaskNode: UserTask;
        let systemTaskNode: SystemTask;
        
        if (node.getNodeType() === NodeType.UserTask) {
            userTaskNode = <UserTask>node;

            let nextNodes = userTaskNode.getNextSystemTasks(graph);
            if (nextNodes) {
                systemTaskNode = nextNodes[0] as SystemTask;
            }

            userTaskDialogModel.isUserTask = true;
        } else if (node.getNodeType() === NodeType.SystemTask) {
            systemTaskNode = <SystemTask>node;
            userTaskNode = <UserTask>systemTaskNode.getUserTask(graph);
            userTaskDialogModel.isUserTask = false;
        } else {
            return;
        }

        userTaskDialogModel.isSystemTask = !userTaskDialogModel.isUserTask;

        if (userTaskNode) { // When the node selected is the "pre-condition",the user taskNode is null, since it is the start node
            userTaskDialogModel.originalUserTask = userTaskNode;
            userTaskDialogModel.clonedUserTask = userTaskDialogModel.originalUserTask.cloneUserTask();
        }

        userTaskDialogModel.originalSystemTask = systemTaskNode;
        userTaskDialogModel.clonedSystemTask = userTaskDialogModel.originalSystemTask.cloneSystemTask();

        // set dialog model isReadonly property to enable/disable input controls
        userTaskDialogModel.isReadonly = this.isReadonly;
        userTaskDialogModel.isHistoricalVersion = this.isHistorical;

        return userTaskDialogModel;
    }

    public getDecisionEditorModel(shapeId: number, graph: IProcessGraph): SubArtifactDecisionDialogModel {
        const decision = <IDecision>graph.getNodeById(shapeId.toString());

        if (!decision) {
            return null;
        }

        const dialogModel: SubArtifactDecisionDialogModel = new SubArtifactDecisionDialogModel();

        dialogModel.graph = graph;
        dialogModel.conditions = [];

        // cloning decision
        dialogModel.originalDecision = decision;
        dialogModel.clonedDecision = decision.cloneDecision();
        
        // populate existing conditions
        const outgoingLinks: IProcessLink[] = dialogModel.graph.getNextLinks(decision.model.id);

        for (let index = 0; index < outgoingLinks.length; index++) {
            const outgoingLink: IProcessLink = outgoingLinks[index];
            let mergePoint: IDiagramNode = null;

            // We do not display change merge node option for first branch
            if (index !== 0) {
                const mergeNodeId: string = dialogModel.originalDecision.getMergeNode(graph, outgoingLink.orderindex).id.toString();
                mergePoint = graph.getNodeById(mergeNodeId);
            }

            const validMergeNodes: IDiagramNode[] = dialogModel.graph.getValidMergeNodes(outgoingLink);
            const condition: ICondition = Condition.create(outgoingLink, mergePoint, validMergeNodes);
            dialogModel.conditions.push(condition);
        }

        // set dialog model isReadonly property to enable/disable input controls
        dialogModel.isReadonly = this.isReadonly;
        dialogModel.isHistoricalVersion = this.isHistorical;

        return dialogModel;
    }

    private openUserSystemTaskDetailsModalDialog($scope: ng.IScope, shapeId: number, graph: IProcessGraph) {
        this.open("",
            require("./sub-artifact-editor-modal-template.html"),
            SubArtifactEditorModalController,
            this.getSubArtifactDialogModel(shapeId, graph),
            "storyteller-modal");
    }

    private openDecisionEditorDialog = ($scope: ng.IScope, shapeId: number, graph: IProcessGraph): void => {
        const settings = <ModalSettings>{
            animation: this.animationsEnabled,
            component: "decisionEditor",
            resolve: {
                dialogModel: () => this.getDecisionEditorModel(shapeId, graph),
                modalProcessViewModel: () => this.modalProcessViewModel
            },
            windowClass: "storyteller-modal"
        };

        this.$uibModal.open(settings);
    };

    public getUserStoryDialogModel(shapeId: number, graph: IProcessGraph): UserStoryDialogModel {
        const userStoryDialogModel = new UserStoryDialogModel();
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
        //return angular.extend({}, model);
        //return angular.copy(node);
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
        this.dialogCommunication.removeModalProcessViewModelObserver(this.setModalProcessViewModelHandler);
    }
}
