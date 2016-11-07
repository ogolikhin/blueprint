import {ILocalizationService} from "../../../../core";
import {ProcessType} from "../../models/enums";
import {IProcess} from "../../models/process-models";
import {ProcessViewModel, IProcessViewModel} from "./viewmodel/process-viewmodel";
import {IProcessGraph, ISelectionListener, IUserStory} from "./presentation/graph/models/";
import {SystemTask} from "./presentation/graph/shapes";
import {ProcessGraph} from "./presentation/graph/process-graph";
import {ICommunicationManager} from "../../../bp-process";
import {IDialogService} from "../../../../shared";
import {IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {ProcessEvents} from "./process-diagram-communication";
import {ShapesFactory} from "./presentation/graph/shapes/shapes-factory";
import {INavigationService} from "../../../../core/navigation/navigation.svc";
import {IMessageService} from "../../../../core/messages/message.svc";
import {MessageType, Message} from "../../../../core/messages/message";

export class ProcessDiagram {
    public processModel: IProcess;
    public processViewModel: IProcessViewModel = null;
    private graph: IProcessGraph = null;
    private htmlElement: HTMLElement;
    private toggleProcessTypeHandler: string;
    private modelUpdateHandler: string;
    private navigateToAssociatedArtifactHandler: string;
    private userStoriesGeneratedHandler: string;

    private selectionListeners: ISelectionListener[];

    constructor(private $rootScope: ng.IRootScopeService,
                private $scope: ng.IScope,
                private $timeout: ng.ITimeoutService,
                private $q: ng.IQService,
                private $log: ng.ILogService,
                private messageService: IMessageService,
                private communicationManager: ICommunicationManager,
                private dialogService: IDialogService,
                private localization: ILocalizationService,
                private navigationService: INavigationService,
                private statefulArtifactFactory: IStatefulArtifactFactory,
                private shapesFactory: ShapesFactory) {
        this.processModel = null;
        this.selectionListeners = [];
    }

    public createDiagram(process: any, htmlElement: HTMLElement) {
        this.checkParams(process.id, htmlElement);
        this.htmlElement = htmlElement;

        this.processModel = <IProcess>process;

        this.onLoad(this.processModel);
    }

    private checkParams(processId: number, htmlElement: HTMLElement): void {
        if (!this.validProcessId(processId)) {
            throw new Error("Process id '" + processId + "' is invalid.");
        }

        if (htmlElement) {
            // okay
        } else {
            throw new Error("There is no html element for the diagram");
        }
    }

    private validProcessId(processId: number): boolean {
        return processId != null && processId > 0;
    }

    private onLoad(process: IProcess, useAutolayout: boolean = false, selectedNodeId: number = undefined) {
        this.resetBeforeLoad();

        let processViewModel = this.createProcessViewModel(process);
        // set isSpa flag to true. Note: this flag may no longer be needed.
        processViewModel.isSpa = true;

        //if (processViewModel.isReadonly) this.disableProcessToolbar();
        this.createProcessGraph(processViewModel, useAutolayout, selectedNodeId);
    }

    private createProcessViewModel(process: IProcess): IProcessViewModel {
        if (this.processViewModel == null) {
            this.processViewModel = new ProcessViewModel(process, this.communicationManager, this.$rootScope, this.$scope, this.messageService);
        } else {
            this.processViewModel.updateProcessGraphModel(process);
            this.processViewModel.communicationManager.toolbarCommunicationManager
                .removeToggleProcessTypeObserver(this.toggleProcessTypeHandler);
            this.processViewModel.communicationManager.processDiagramCommunication
                .removeModelUpdateObserver(this.modelUpdateHandler);
            this.processViewModel.communicationManager.processDiagramCommunication
                .unregister(ProcessEvents.NavigateToAssociatedArtifact, this.navigateToAssociatedArtifactHandler);
            this.processViewModel.communicationManager.processDiagramCommunication
                .unregister(ProcessEvents.UserStoriesGenerated, this.userStoriesGeneratedHandler);
        }

        this.toggleProcessTypeHandler = this.processViewModel.communicationManager.toolbarCommunicationManager
            .registerToggleProcessTypeObserver(this.processTypeChanged);
        this.modelUpdateHandler = this.processViewModel.communicationManager.processDiagramCommunication
            .registerModelUpdateObserver(this.modelUpdate);
        this.navigateToAssociatedArtifactHandler = this.processViewModel.communicationManager.processDiagramCommunication
            .register(ProcessEvents.NavigateToAssociatedArtifact, this.navigateToAssociatedArtifact);
        this.userStoriesGeneratedHandler = this.processViewModel.communicationManager.processDiagramCommunication
            .register(ProcessEvents.UserStoriesGenerated, this.userStoriesGenerated);

        return this.processViewModel;
    }

    private processTypeChanged = (processType: number) => {
        const isSystemTaskVisible: boolean = processType === ProcessType.UserToSystemProcess;
        this.graph.setSystemTasksVisible(isSystemTaskVisible);
        this.processViewModel.processType = <ProcessType>processType;

        if (!isSystemTaskVisible) {
            const hasSelectedSystemTask: boolean = this.graph.getMxGraph().getSelectionCells().filter(cell => cell instanceof SystemTask).length > 0;
            if (hasSelectedSystemTask) {
                this.graph.clearSelection();
            }
        }
    }

    private modelUpdate = (selectedNodeId: number) => {
        this.recreateProcessGraph(selectedNodeId);
    }

    private navigateToAssociatedArtifact = (info: any) => {
        const options = {
            id: info.id,
            enableTracking: info.enableTracking
        };
        this.navigationService.navigateTo(options);
    }

    private recreateProcessGraph = (selectedNodeId: number = undefined) => {
        this.graph.destroy();
        this.createProcessGraph(this.processViewModel, true, selectedNodeId);
    }

    private userStoriesGenerated = (userStories: IUserStory[]) => {
        this.graph.onUserStoriesGenerated(userStories);
    }

    private createProcessGraph(processViewModel: IProcessViewModel,
                               useAutolayout: boolean = false,
                               selectedNodeId: number = undefined) {
        try {

            this.graph = new ProcessGraph(
                this.$rootScope,
                this.$scope,
                this.htmlElement,
                this.processViewModel,
                this.dialogService,
                this.localization,
                this.shapesFactory,
                this.messageService,
                this.$log,
                this.statefulArtifactFactory,
            );

            this.registerSelectionListeners();
        } catch (err) {
            this.handleInitProcessGraphFailed(processViewModel.id, err);
        }

        try {
            this.graph.render(useAutolayout, selectedNodeId);

        } catch (err) {
            this.handleRenderProcessGraphFailed(processViewModel.id, err);
        }
    }

    private registerSelectionListeners() {
        for (let listener of this.selectionListeners) {
            this.graph.addSelectionListener(listener);
        }
    }

    public addSelectionListener(listener: ISelectionListener) {
        this.selectionListeners.push(listener);
    }

    public clearSelection() {
        this.graph.clearSelection();
    }

    private resetBeforeLoad() {
        if (this.graph != null) {
            this.graph.destroy();
            this.graph = null;
        }
    }

    public destroy() {
        if (this.communicationManager) {
            if (this.communicationManager.toolbarCommunicationManager) {
                this.communicationManager.toolbarCommunicationManager
                    .removeToggleProcessTypeObserver(this.toggleProcessTypeHandler);
            }

            if (this.communicationManager.processDiagramCommunication) {
                this.communicationManager.processDiagramCommunication
                    .removeModelUpdateObserver(this.modelUpdateHandler);
                this.communicationManager.processDiagramCommunication
                    .unregister(ProcessEvents.NavigateToAssociatedArtifact, this.navigateToAssociatedArtifactHandler);
                this.communicationManager.processDiagramCommunication
                    .unregister(ProcessEvents.UserStoriesGenerated, this.userStoriesGeneratedHandler);
            }
        }

        // tear down persistent objects and event handlers
        if (this.graph != null) {
            this.graph.destroy();
            this.graph = null;
        }

        if (this.processViewModel != null) {
            this.processViewModel.destroy();
            this.processViewModel = null;
        }

        this.selectionListeners = null;
    }

    private handleInitProcessGraphFailed(processId: number, err: any) {
        this.messageService.addMessage(new Message(
            MessageType.Error, "There was an error initializing the process graph."));
        this.$log.error("Fatal: cannot initialize process graph for process " + processId);
        this.$log.error("Error: " + err.message);
    }

    private handleRenderProcessGraphFailed(processId: number, err: any) {
        this.messageService.addMessage(new Message(
            MessageType.Error, "There was an error displaying the process graph."));
        this.$log.error("Fatal: cannot render process graph for process " + processId);
        this.$log.error("Error: " + err.message);
    }

    public resize = (width: number, height: number) => {
        if (!!this.graph) {
            this.graph.updateSizeChanges(width, height);
        }
    }
}
