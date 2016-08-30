import {IModalDialogManager} from "../modal-dialogs/modal-dialog-manager";
import {Condition} from "../diagram/presentation/graph/shapes/condition";
import {ModalDialogType} from "./base-modal-dialog-controller";
import {SubArtifactDialogModel} from "./sub-artifact-dialog-model";
import {UserStoryDialogModel} from "./user-story-dialog-model";
import {SubArtifactDecisionDialogModel} from "./sub-artifact-decision-dialog-model";

import ModalSettings = angular.ui.bootstrap.IModalSettings;

export class SubArtifactEditorModalOpener {

    private animationsEnabled: boolean = true;

    private isReadonly: boolean; 
    private isHistorical: boolean; 
    public getGraph: () => any;

    constructor(private $scope: ng.IScope,
        private $uibModal: angular.ui.bootstrap.IModalService,
        private $rootScope: ng.IRootScopeService,
        private dialogManager: IModalDialogManager
    ) {
        dialogManager.registerSetGraphObserver(this.setGraph);
        dialogManager.registerOpenDialogObserver(this.openDialogCaller);
    }

    private setGraph = (graph) => {
        this.getGraph = graph;
    }

    private openDialogCaller = (args: any[]) => {
        this.openDialog.apply(this, args);
    }

    private openDialog = (id: number, dialogType: ModalDialogType) => {

        window.console.log(`Open dialog with parameters ${id}, ${dialogType}`);

        try {
            let graph = this.getGraph(); 

            // get read-only status from viewmodel's isReadonly property 

            let viewModel = graph.viewModel; //<IProcessViewModel>($scope.$parent["vm"].processDiagram.processViewModel);
            if (viewModel) {
                this.isReadonly = viewModel.isReadonly;
                this.isHistorical = viewModel.isHistorical;
            } else {
                throw new Error("ProcessViewModel is null in SubArtifactEditorModalOpener");
            }

            if (dialogType === ModalDialogType.UserSystemTaskDetailsDialogType) {
                this.openUserSystemTaskDetailsModalDialog(this.$scope, id, graph);

            } else if (dialogType === ModalDialogType.UserSystemDecisionDetailsDialogType) {
                this.openUserSystemDecisionDetailsModalDialog(this.$scope, id, graph);

            } else if (dialogType === ModalDialogType.PreviewDialogType) {
                this.openPreviewModalDialog(this.$scope, id, graph);
            }

        } catch (err) {
            // log error
        }
    };
    //TODO: replace signature
    //public getSubArtifactDialogModel(shapeId: number, graph: ProcessGraph, propertiesMw: Shell.IPropertiesMw): SubArtifactDialogModel {
    public getSubArtifactDialogModel(shapeId: number, graph: any): SubArtifactDialogModel {
        var userTaskDialogModel = new SubArtifactDialogModel();
        userTaskDialogModel.subArtifactId = shapeId;
        var node = graph.getNodeById(userTaskDialogModel.subArtifactId.toString());
        // TODO: replace definitions:
        //var userTaskNode: UserTask;
        //var systemTaskNode: SystemTask;
        var userTaskNode: any;
        var systemTaskNode: any;
        // TODO: replace code:
        // if (node.getNodeType() === NodeType.UserTask) {
        if (node.getNodeType() === 7) {
            // TODO: replace code:
            // userTaskNode = <UserTask>node;
            userTaskNode = node;
            let nextNodes = userTaskNode.getNextSystemTasks(graph);
            if (nextNodes) {
                systemTaskNode = nextNodes[0];
            }
            userTaskDialogModel.isUserTask = true;
        // TODO: replace code:
        // } else if (node.getNodeType() === NodeType.SystemTask) {
        } else if (node.getNodeType() === 5) {
            // TODO: replace code:
            // systemTaskNode = <SystemTask>node;
            // userTaskNode = <UserTask>systemTaskNode.getUserTask(graph);
            systemTaskNode = node;
            userTaskNode = systemTaskNode.getUserTask(graph);
            userTaskDialogModel.isUserTask = false;
        } else {
            return;
        }
        userTaskDialogModel.isSystemTask = !userTaskDialogModel.isUserTask;
        userTaskDialogModel.originalUserTask = userTaskNode;
        userTaskDialogModel.clonedUserTask = this.cloneNode(userTaskDialogModel.originalUserTask);
        userTaskDialogModel.originalSystemTask = systemTaskNode;
        userTaskDialogModel.clonedSystemTask = this.cloneNode(userTaskDialogModel.originalSystemTask);

        // set dialog model isReadonly property to enable/disable input controls
        userTaskDialogModel.isReadonly = this.isReadonly;
        userTaskDialogModel.isHistoricalVersion = this.isHistorical;

        return userTaskDialogModel;
    }

    //TODO: replace signature
    // public getUserSystemDecisionDialogModel(shapeId: number, graph: ProcessGraph): SubArtifactDecisionDialogModel {
    public getUserSystemDecisionDialogModel(shapeId: number, graph: any): SubArtifactDecisionDialogModel {
        let node = graph.getNodeById(shapeId.toString());

        // TODO: replace code:
        //if (node == null || (node.getNodeType() !== NodeType.UserDecision && node.getNodeType() !== NodeType.SystemDecision)) {
        if (node == null || (node.getNodeType() !== 6 && node.getNodeType() !== 4)) {
            return null;
        }

        let dialogModel: SubArtifactDecisionDialogModel = new SubArtifactDecisionDialogModel();
        // TODO: replace definitions:
        // let systemTasks: SystemTask[] = [];
        let systemTasks: any[] = [];

        dialogModel.graph = graph;
        dialogModel.originalExistingNodes = systemTasks;
        // TODO: replace code:
        // dialogModel.clonedExistingNodes = <IDiagramNode[]>this.cloneArray(dialogModel.originalExistingNodes);
        dialogModel.clonedExistingNodes = <any[]>this.cloneArray(dialogModel.originalExistingNodes);

        dialogModel.conditions = [];

        // cloning decision
        // TODO: replace code:
        // dialogModel.originalDecision = <IDecision>node;
        dialogModel.originalDecision = <any>node;
        dialogModel.clonedDecision = this.cloneNode(dialogModel.originalDecision);

        // populate existing conditions
        let decisionId: number = node.model.id;
        // TODO: replace code:
        // let outgoingLinks: IProcessLink[] = dialogModel.graph.getNextLinks(decisionId);
        let outgoingLinks: any[] = dialogModel.graph.getNextLinks(decisionId);

        for (let index = 0; index < outgoingLinks.length; index++) {
            // TODO: replace definitions:
            // let outgoingLink: IProcessLink = outgoingLinks[index];
            // let mergePoint: IDiagramNode = null;
            let outgoingLink: any = outgoingLinks[index];
            let mergePoint: any = null;

            // We do not display change merge node option for first branch
            if (index !== 0) {
                let mergeNodeId: string = dialogModel.originalDecision.getMergeNode(graph, outgoingLink.orderindex).id.toString();
                mergePoint = graph.getNodeById(mergeNodeId);
            }

            // TODO: replace definitions:
            // let validMergeNodes: IDiagramNode[] = dialogModel.graph.getValidMergeNodes(outgoingLink);
            // let condition: ICondition = Condition.create(outgoingLink, mergePoint, validMergeNodes);
            let validMergeNodes: any[] = dialogModel.graph.getValidMergeNodes(outgoingLink);
            let condition: any = Condition.create(outgoingLink, mergePoint, validMergeNodes);
            dialogModel.conditions.push(condition);
        }
        
        // clone edges one by one
        dialogModel.clonedDecision.edges = [];
        for (let edge of dialogModel.originalDecision.edges) {
            dialogModel.clonedDecision.edges.push(this.cloneNode(edge));
        }

        // set dialog model isReadonly property to enable/disable input controls
        dialogModel.isReadonly = this.isReadonly;
        dialogModel.isHistoricalVersion = this.isHistorical;

        return dialogModel;
    }

    //TODO: replace signature
    // private openUserSystemTaskDetailsModalDialog($scope: ng.IScope, shapeId: number, graph: ProcessGraph) {
    private openUserSystemTaskDetailsModalDialog($scope: ng.IScope, shapeId: number, graph: any) {
        this.open("",
            "SubArtifactEditorModalTemplate.html",
            "SubArtifactEditorModalController",
            this.getSubArtifactDialogModel(shapeId, graph),
            "storyteller-modal");
    }

    // @todo: replace with proper controller / template
    //TODO: replace signature
    // private openUserSystemDecisionDetailsModalDialog($scope: ng.IScope, shapeId: number, graph: ProcessGraph) {
    private openUserSystemDecisionDetailsModalDialog($scope: ng.IScope, shapeId: number, graph: any) {
        this.open("",
            "SubArtifactDecisionEditorModalTemplate.html",
            "SubArtifactDecisionEditorModalController",
            this.getUserSystemDecisionDialogModel(shapeId, graph),
            "storyteller-modal");
    }

    //TODO: replace signature
    // public getUserStoryDialogModel(shapeId: number, graph: ProcessGraph): UserStoryDialogModel {
    public getUserStoryDialogModel(shapeId: number, graph: any): UserStoryDialogModel {
        let userStoryDialogModel = new UserStoryDialogModel();
        userStoryDialogModel.subArtifactId = shapeId;
        let node = graph.getNodeById(userStoryDialogModel.subArtifactId.toString());
        // TODO: replace definitions:
        // let userTaskNode = <UserTask>node;
        let userTaskNode = node;
        userStoryDialogModel.previousSytemTasks = userTaskNode.getPreviousSystemTasks(graph);
        userStoryDialogModel.nextSystemTasks = userTaskNode.getNextSystemTasks(graph);
        userStoryDialogModel.originalUserTask = userTaskNode;
        userStoryDialogModel.clonedUserTask = this.cloneNode(userStoryDialogModel.originalUserTask);
        // #TODO: processService should not be used here to get state 
        //if (this.$scope.$parent["vm"].processModelService) {
        //    userStoryDialogModel.isUserSystemProcess = this.$scope.$parent["vm"].processModelService.isUserToSystemProcess();
        //}
        userStoryDialogModel.isUserSystemProcess = graph.IsUserSystemProcess;

        // set dialog model isReadonly property to enable/disable input controls
        userStoryDialogModel.isReadonly = this.isReadonly;
        userStoryDialogModel.isHistoricalVersion = this.isHistorical;

        return userStoryDialogModel;
    }

    //TODO: replace signature
    // private openPreviewModalDialog($scope: ng.IScope, shapeId: number, graph: ProcessGraph) {
    private openPreviewModalDialog($scope: ng.IScope, shapeId: number, graph: any) {

        this.open("",
            "UserStoryPreviewTemplate.html",
            "UserStoryPreviewController",
            this.getUserStoryDialogModel(shapeId, graph),
            "preview-modal");
    }

    public cloneNode = (node): any => {
        //return angular.extend({}, model);
        return jQuery.extend(true, {}, node);
    }

    public cloneArray = (arr: any[]): any[] => {
        return jQuery.extend(true, [], arr);
    }
    // #TODO: templateUrl must be changed
    public open = (size, htmlFileName: string, controllerClassName: string, dialogModel: any, windowClass: string) => {
        this.$uibModal.open(<ModalSettings>{
            animation: this.animationsEnabled,
            templateUrl: `/Areas/Web/App/Components/Storyteller/Dialogs/${htmlFileName}`,
            controller: controllerClassName,
            controllerAs: "vm",
            windowClass: windowClass,
            size: size,
            resolve: {
                dialogModel: () => {
                    return dialogModel;
                }
            }
        });
    }

    public onDestroy = () => {
        this.dialogManager.removeSetGraphObserver(this.setGraph);
        this.dialogManager.removeOpenDialogObserver(this.openDialog);
    }

}
