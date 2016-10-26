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
import {UserStoryPreviewController} from "./user-story-preview/user-story-preview";
import {UserTaskDialogModel, SystemTaskDialogModel} from "./task-editor/sub-artifact-dialog-model";
import ModalSettings = angular.ui.bootstrap.IModalSettings;
import {ILocalizationService} from "../../../../core";

export class SubArtifactEditorModalOpener {
    private graph: IProcessGraph;
    private setGraphHandler: string;
    private openDialogCallerHandler: string;

    constructor(
        private $uibModal: angular.ui.bootstrap.IModalService,
        private dialogCommunication: IModalDialogCommunication,
        private localization: ILocalizationService
    ) {
        this.setGraphHandler = dialogCommunication.registerSetGraphObserver(this.setGraph);
        this.openDialogCallerHandler = dialogCommunication.registerOpenDialogObserver(this.openDialogCaller);
    }

    private setGraph = (getGraph: () => IProcessGraph) => {
        this.graph = getGraph();
    }

    private openDialogCaller = (args: any[]) => {
        this.openDialog.apply(this, args);
    }

    private openDialog = (id: number, dialogType: ModalDialogType) => {
        try {
            const settings: ModalSettings = this.getModalSettings(id, dialogType, this.graph);

            if (settings) {
                this.$uibModal.open(settings);
            }
        } catch (err) {
            window.console.log(err);
        }
    };

    private getModalSettings(id: number, dialogType: ModalDialogType, graph: IProcessGraph): ModalSettings {
        switch (dialogType) {
            case ModalDialogType.UserTaskDetailsDialogType:
                return this.getUserTaskEditorDialogSettings(id, graph);

            case ModalDialogType.SystemTaskDetailsDialogType:
                return this.getSystemTaskEditorDialogSettings(id, graph);

            case ModalDialogType.UserSystemDecisionDetailsDialogType:
                return this.getDecisionEditorDialogSettings(id, graph);

            case ModalDialogType.PreviewDialogType:
                return this.getPreviewModalDialogSettings(id, graph);

            default:
                return null;
        }
    }

    private getUserTaskEditorDialogSettings = (shapeId: number, graph: IProcessGraph): ModalSettings => {
        return <ModalSettings>{
            animation: true,
            component: "userTaskEditor",
            resolve: {
                dialogModel: () => this.getUserTaskDialogModel(shapeId, graph)
            },
            windowClass: "storyteller-modal"
        };
    }

    private getSystemTaskEditorDialogSettings = (shapeId: number, graph: IProcessGraph): ModalSettings => {
        return <ModalSettings>{
            animation: true,
            component: "systemTaskEditor",
            resolve: {
                dialogModel: () => this.getSystemTaskDialogModel(shapeId, graph)
            },
            windowClass: "storyteller-modal"
        };
    }

    private getDecisionEditorDialogSettings = (shapeId: number, graph: IProcessGraph): ModalSettings => {
        return <ModalSettings>{
            animation: true,
            component: "decisionEditor",
            resolve: {
                dialogModel: () => this.getDecisionEditorModel(shapeId, graph)
            },
            windowClass: "storyteller-modal"
        };
    };

    private getPreviewModalDialogSettings = (shapeId: number, graph: IProcessGraph): ModalSettings => {
        return <ModalSettings>{
            okButton: this.localization.get("App_Button_Ok"),
            animation: true,
            template: require("./user-story-preview/user-story-preview.html"),
            controller: UserStoryPreviewController,
            controllerAs: "vm",
            windowClass: "preview-modal",
            size: "",
            resolve: {
                dialogModel: () => this.getUserStoryDialogModel(shapeId, graph)
            }
        };
    }

    private getUserTaskDialogModel(shapeId: number, graph: IProcessGraph): UserTaskDialogModel {
        if (!graph || !graph.viewModel) {
            throw new Error("graph is null or invalid in SubArtifactEditorModalOpener");
        }

        const node = graph.getNodeById(shapeId.toString());

        if (!node || node.getNodeType() !== NodeType.UserTask) {
            return null;
        }

        const userTask: UserTask = <UserTask>node;
        const model = new UserTaskDialogModel();

        model.artifactId = graph.viewModel.id;
        model.subArtifactId = shapeId;
        model.isReadonly = graph.viewModel.isReadonly;
        model.isHistoricalVersion = graph.viewModel.isHistorical;
        model.originalItem = userTask;
        model.action = model.originalItem.action;
        model.associatedArtifact = model.originalItem.associatedArtifact;
        model.objective = model.originalItem.objective;
        model.label = model.originalItem.label;
        model.persona = model.originalItem.persona;

        return model;
    }

    private getSystemTaskDialogModel(shapeId: number, graph: IProcessGraph): SystemTaskDialogModel {
        if (!graph || !graph.viewModel) {
            throw new Error("graph is null or invalid in SubArtifactEditorModalOpener");
        }

        const node = graph.getNodeById(shapeId.toString());

        if (!node || node.getNodeType() !== NodeType.SystemTask) {
            return null;
        }

        const systemTask: SystemTask = <SystemTask>node;
        const model = new SystemTaskDialogModel();

        model.artifactId = graph.viewModel.id;
        model.subArtifactId = shapeId;
        model.isReadonly = graph.viewModel.isReadonly;
        model.isHistoricalVersion = graph.viewModel.isHistorical;
        model.originalItem = systemTask;
        model.action = model.originalItem.action;
        model.associatedArtifact = model.originalItem.associatedArtifact;
        model.imageId = model.originalItem.imageId;
        model.label = model.originalItem.label;
        model.persona = model.originalItem.persona;
        model.associatedImageUrl = model.originalItem.associatedImageUrl;

        return model;
    }

    private getDecisionEditorModel(shapeId: number, graph: IProcessGraph): DecisionEditorModel {
        if (!graph || !graph.viewModel) {
            throw new Error("graph is null or invalid in SubArtifactEditorModalOpener");
        }

        const node = graph.getNodeById(shapeId.toString());
        const allowedNodeTypes: NodeType[] = [NodeType.UserDecision, NodeType.SystemDecision];

        if (!node || allowedNodeTypes.indexOf(node.getNodeType()) < 0) {
            return null;
        }

        const decision: IDecision = <IDecision>node;
        const model: DecisionEditorModel = new DecisionEditorModel();

        model.artifactId = graph.viewModel.id;
        model.subArtifactId = shapeId;
        model.label = decision.label;
        model.conditions = this.getConditions(decision, graph);
        model.isReadonly = graph.viewModel.isReadonly;
        model.isHistoricalVersion = graph.viewModel.isHistorical;
        model.graph = graph;
        model.originalDecision = decision;

        return model;
    }

    private getConditions(decision: IDecision, graph: IProcessGraph): ICondition[] {
        const conditions: ICondition[] = [];
        const outgoingLinks: IProcessLink[] = graph.getNextLinks(decision.model.id);

        for (let index = 0; index < outgoingLinks.length; index++) {
            const outgoingLink: IProcessLink = outgoingLinks[index];
            let mergePoint: IDiagramNode = null;

            // We do not display change merge node option for first branch
            if (index !== 0) {
                const mergeNodeId: string = decision.getMergeNode(graph, outgoingLink.orderindex).id.toString();
                mergePoint = graph.getNodeById(mergeNodeId);
            }

            const validMergeNodes: IDiagramNode[] = graph.getValidMergeNodes(outgoingLink);
            const condition: ICondition = Condition.create(outgoingLink, mergePoint, validMergeNodes);
            conditions.push(condition);
        }

        return conditions;
    }

    private getUserStoryDialogModel(shapeId: number, graph: IProcessGraph): UserStoryDialogModel {
        if (!graph || !graph.viewModel) {
            throw new Error("graph is null or invalid in SubArtifactEditorModalOpener");
        }

        const node = graph.getNodeById(shapeId.toString());

        if (!node || node.getNodeType() !== NodeType.UserTask) {
            return null;
        }

        const userTask = <UserTask>node;
        const model: UserStoryDialogModel = new UserStoryDialogModel();
        
        model.artifactId = graph.viewModel.id;
        model.subArtifactId = shapeId;
        model.previousSystemTasks = userTask.getPreviousSystemTasks(graph) as SystemTask[];
        model.nextSystemTasks = userTask.getNextSystemTasks(graph) as SystemTask[];
        model.originalUserTask = userTask;
        model.isUserSystemProcess = graph.isUserSystemProcess;
        model.isReadonly = graph.viewModel.isReadonly;
        model.isHistoricalVersion = graph.viewModel.isHistorical;

        return model;
    }

    public destroy = () => {
        this.dialogCommunication.removeSetGraphObserver(this.setGraphHandler);
        this.dialogCommunication.removeOpenDialogObserver(this.openDialogCallerHandler);
    }
}
