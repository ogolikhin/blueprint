import {SystemTaskDialogModel} from "./task-editor/systemTaskDialogModel";
import {UserTaskDialogModel} from "./task-editor/userTaskDialogModel";
import {IModalDialogCommunication} from "../modal-dialogs/modal-dialog-communication";
import {ICondition, Condition} from "../diagram/presentation/graph/shapes/condition";
import {ModalDialogType} from "./modal-dialog-constants";
import {UserStoryDialogModel} from "./models/user-story-dialog-model";
import {DecisionEditorModel} from "./decisionEditor/decisionEditor.model";
import {
    IProcessGraph,
    IDiagramNode,
    IDecision
} from "../diagram/presentation/graph/models/process-graph-interfaces";
import {UserTask} from "../diagram/presentation/graph/shapes/user-task";
import {SystemTask} from "../diagram/presentation/graph/shapes/system-task";
import {NodeType, IProcessLink} from "../diagram/presentation/graph/models/";
import {UserStoryPreviewController} from "./user-story-preview/user-story-preview";
import {IPersonaOption} from "./task-editor/taskDialogModel";
import ModalSettings = angular.ui.bootstrap.IModalSettings;
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {IArtifactReference} from "../../models/process-models";

export class SubArtifactEditorModalOpener {
    private graph: IProcessGraph;
    private setGraphHandler: string;
    private openDialogCallerHandler: string;

    constructor(private $uibModal: angular.ui.bootstrap.IModalService,
        private dialogCommunication: IModalDialogCommunication,
                private localization: ILocalizationService) {
        this.setGraphHandler = dialogCommunication.registerSetGraphObserver(this.setGraph);
        this.openDialogCallerHandler = dialogCommunication.registerOpenDialogObserver(this.openDialogCaller);
    }

    private setGraph = (getGraph: () => IProcessGraph) => {
        this.graph = getGraph();
    };

    private openDialogCaller = (args: any[]) => {
        this.openDialog.apply(this, args);
    };

    private openDialog = (id: number, dialogType: ModalDialogType) => {
        try {
            const settings: ModalSettings = this.getModalSettings(id, dialogType, this.graph);

            if (settings) {
                this.$uibModal.open(settings);
            }
        } catch (err) {
            // TODO: NEED TO REMOVE WINDOW.CONSOLE.LOG, TEMP LOW-RISK FIX
            if (window && window.console) {
                window.console.log(err);
            }
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
    };

    private getSystemTaskEditorDialogSettings = (shapeId: number, graph: IProcessGraph): ModalSettings => {
        return <ModalSettings>{
            animation: true,
            component: "systemTaskEditor",
            resolve: {
                dialogModel: () => this.getSystemTaskDialogModel(shapeId, graph)
            },
            windowClass: "storyteller-modal"
        };
    };

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
    };

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
        model.itemTypeId = graph.viewModel.itemTypeId;
        model.subArtifactId = shapeId;
        model.isReadonly = graph.viewModel.isReadonly;
        model.isHistoricalVersion = graph.viewModel.isHistorical;
        model.originalItem = userTask;
        model.action = model.originalItem.action;
        model.associatedArtifact = model.originalItem.associatedArtifact;
        model.objective = model.originalItem.objective;
        model.label = model.originalItem.label;
        model.personaReference = model.originalItem.personaReference;

        model.userTaskPersonaReferenceOptions = this.populatePersonaReferenceOptions(graph.viewModel.userTaskPersonaReferenceList);

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
        model.itemTypeId = graph.viewModel.itemTypeId;
        model.subArtifactId = shapeId;
        model.isReadonly = graph.viewModel.isReadonly;
        model.isHistoricalVersion = graph.viewModel.isHistorical;
        model.originalItem = systemTask;
        model.action = model.originalItem.action;
        model.associatedArtifact = model.originalItem.associatedArtifact;
        model.imageId = model.originalItem.imageId;
        model.label = model.originalItem.label;
        model.associatedImageUrl = model.originalItem.associatedImageUrl;
        model.personaReference = model.originalItem.personaReference;

        model.systemTaskPersonaReferenceOptions = this.populatePersonaReferenceOptions(graph.viewModel.systemTaskPersonaReferenceList);

        return model;
    }

    private populatePersonaReferenceOptions(personaReferenceList: IArtifactReference[]): IPersonaOption[] {
        const personaOptions: IPersonaOption[] = [];
        for (let i = 0; i < personaReferenceList.length; i++) {
            let label: string = personaReferenceList[i].name;

            if (personaReferenceList[i].id > 0) {
                label = personaReferenceList[i].typePrefix +
                       personaReferenceList[i].id +
                      ": " +
                       personaReferenceList[i].name;
            }

            personaOptions.push({
                value: personaReferenceList[i],
                label: label
            });
        }

        return personaOptions;
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
        model.conditionHeader = node.getNodeType() === NodeType.SystemDecision ?
            this.localization.get("ST_Condition_Label") :
            this.localization.get("ST_Choice_Label");
        model.defaultDestinationId = graph.layout.getConditionDestination(decision.model.id).id;

        return model;
    }

    private getConditions(decision: IDecision, graph: IProcessGraph): ICondition[] {
        const outgoingLinks: IProcessLink[] = graph.getNextLinks(decision.model.id);

        return _.map(
            outgoingLinks,
            outgoingLink => {
                const branchStartLink: IProcessLink = graph.getBranchStartingLink(outgoingLink);
                const decisionId: number = branchStartLink.sourceId;
                const branchEndLink = graph.getBranchEndingLink(branchStartLink);
                const branchDestinationLink = graph.getDecisionBranchDestLinkForIndex(decisionId, branchStartLink.orderindex);
                const validMergeNodes = graph.getValidMergeNodes(branchStartLink);

                return new Condition(outgoingLink, branchEndLink, branchDestinationLink, validMergeNodes);
            });
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
