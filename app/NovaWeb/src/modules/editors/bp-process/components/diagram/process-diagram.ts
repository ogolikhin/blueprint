import {ILocalizationService, IMessageService, Message, MessageType, INavigationService} from "../../../../core";
import {ProcessType} from "../../models/enums";
import {IProcess} from "../../models/process-models";
import {IProcessService} from "../../services/process.svc";
import {ProcessViewModel, IProcessViewModel} from "./viewmodel/process-viewmodel";
import {IProcessGraph} from "./presentation/graph/models/";
import {ProcessGraph} from "./presentation/graph/process-graph";
import {ICommunicationManager} from "../../../bp-process";
import {IDialogService} from "../../../../shared";
import {ShapesFactory} from "./presentation/graph/shapes/shapes-factory";

export class ProcessDiagram {
    public processModel: IProcess;
    public processViewModel: IProcessViewModel = null;
    private graph: IProcessGraph = null;
    private htmlElement: HTMLElement;
    private toggleProcessTypeHandler: string;
    private modelUpdateHandler: string;
    private navigateToAssociatedArtifactHandler: string;
    private shapesFactory: ShapesFactory;

    constructor(
        private $rootScope: ng.IRootScopeService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private $log: ng.ILogService,
        private processService: IProcessService,
        private messageService: IMessageService,
        private communicationManager: ICommunicationManager,
        private dialogService: IDialogService,
        private localization: ILocalizationService,
        private navigationService: INavigationService) {

        this.processModel = null;
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

        this.shapesFactory = new ShapesFactory(this.$rootScope);

        //if (processViewModel.isReadonly) this.disableProcessToolbar();
        this.createProcessGraph(processViewModel, useAutolayout, selectedNodeId);
    }

    private createProcessViewModel(process: IProcess): IProcessViewModel {
        if (this.processViewModel == null) {
            this.processViewModel = new ProcessViewModel(process, this.$rootScope, this.$scope, this.messageService);
            this.processViewModel.communicationManager = this.communicationManager;
        } else {
            this.processViewModel.updateProcessGraphModel(process);
            this.processViewModel.communicationManager.toolbarCommunicationManager
                .removeToggleProcessTypeObserver(this.toggleProcessTypeHandler);
            this.processViewModel.communicationManager.processDiagramCommunication
                .removeModelUpdateObserver(this.modelUpdateHandler);
            this.processViewModel.communicationManager.processDiagramCommunication
                .removeNavigateToAssociatedArtifactObserver(this.navigateToAssociatedArtifactHandler);
        }

        let isProcessTypeToggleEnabled = !this.processViewModel.isReadonly && !this.processViewModel.isHistorical;
        this.processViewModel
            .communicationManager
            .toolbarCommunicationManager
            .enableProcessTypeToggle(isProcessTypeToggleEnabled, this.processViewModel.processType);
        
        this.toggleProcessTypeHandler = this.processViewModel.communicationManager.toolbarCommunicationManager
            .registerToggleProcessTypeObserver(this.processTypeChanged);
        this.modelUpdateHandler = this.processViewModel.communicationManager.processDiagramCommunication
            .registerModelUpdateObserver(this.modelUpdate);
        this.navigateToAssociatedArtifactHandler = this.processViewModel.communicationManager.processDiagramCommunication
            .registerNavigateToAssociatedArtifactObserver(this.navigateToAssociatedArtifact);
        
        return this.processViewModel;
    }

    private processTypeChanged = (processType: number) => {
        this.processViewModel.processType = <ProcessType>processType;
        this.recreateProcessGraph();
    }

    private modelUpdate = (selectedNodeId: number) => {
        this.recreateProcessGraph(selectedNodeId);
    }

    private navigateToAssociatedArtifact = (info: any) => {
        this.navigationService.navigateToArtifact(info.id, info.options);
    }

    private recreateProcessGraph = (selectedNodeId: number = undefined) => {
        this.graph.destroy();
        this.createProcessGraph(this.processViewModel, true, selectedNodeId);
    }

    private createProcessGraph(processViewModel: IProcessViewModel, 
                               useAutolayout: boolean = false, 
                               selectedNodeId: number = undefined) {

        try {
            this.graph = new ProcessGraph(
                            this.$rootScope,
                            this.$scope,
                            this.htmlElement,
                            this.processService,
                            this.processViewModel,
                            this.dialogService,
                            this.localization,
                            this.messageService,
                            this.$log,
                            this.shapesFactory);
        } catch (err) {
            this.handleInitProcessGraphFailed(processViewModel.id, err);
        }

        try {
            this.graph.render(useAutolayout, selectedNodeId);

        } catch (err) {
            this.handleRenderProcessGraphFailed(processViewModel.id, err);
        }
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
                    .removeNavigateToAssociatedArtifactObserver(this.navigateToAssociatedArtifactHandler);
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