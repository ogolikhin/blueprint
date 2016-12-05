import {ProcessType} from "../../models/enums";
import {IProcess} from "../../models/process-models";
import {ProcessViewModel, IProcessViewModel} from "./viewmodel/process-viewmodel";
import {IProcessGraph, ISelectionListener, IUserStory} from "./presentation/graph/models/";
import {IArtifactManager} from "./../../../bp-base-editor";
import {IStatefulProcessSubArtifact} from "./../../process-subartifact";
import {IStatefulProcessArtifact} from "../../process-artifact";
import {IStatefulSubArtifact} from "../../../../managers/artifact-manager/sub-artifact/sub-artifact";
import {IDiagramNode} from "./presentation/graph/models/process-graph-interfaces";
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
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {PanelType, IUtilityPanelService} from "../../../../shell/bp-utility-panel/utility-panel.svc";
import {IClipboardService} from "../../services/clipboard.svc";
import {ProcessCopyPasteHelper} from "./presentation/graph/process-copy-paste-helper";

export class ProcessDiagram {
    
    public processModel: IProcess = null;
    public processViewModel: IProcessViewModel = null;
    private processArtifact: IStatefulProcessArtifact = null;
    private graph: IProcessGraph = null;
    private htmlElement: HTMLElement;
    private toggleProcessTypeHandler: string;
    private copySelectionHandler: string; 
    private modelUpdateHandler: string;
    private navigateToAssociatedArtifactHandler: string;
    private userStoriesGeneratedHandler: string;
    private openUtilityPanelHandler: string;
    private selectionChangedHandler: string;
 
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
                private shapesFactory: ShapesFactory,
                private utilityPanelService: IUtilityPanelService,
                private clipboard: IClipboardService,
                private artifactManager: IArtifactManager) {

        this.processModel = null;
       
    }

    public createDiagram(process: any, htmlElement: HTMLElement) {
        this.checkParams(process.id, htmlElement);
        this.htmlElement = htmlElement;

        this.processModel = <IProcess>process;
        this.processArtifact = <IStatefulProcessArtifact>process;
        // #DEBUG
        //this.artifactManager.selection.subArtifactObservable
        //    .subscribeOnNext(this.onSubArtifactChanged, this);

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
   
        this.createProcessGraph(processViewModel, useAutolayout, selectedNodeId);
    }

    private createProcessViewModel(process: IProcess): IProcessViewModel {
        if (this.processViewModel == null) {
            this.processViewModel = new ProcessViewModel(process, this.communicationManager, this.$rootScope, this.$scope, this.messageService);
        } else {
            this.processViewModel.updateProcessGraphModel(process);
            this.processViewModel.communicationManager.toolbarCommunicationManager
                .removeToggleProcessTypeObserver(this.toggleProcessTypeHandler);
            this.processViewModel.communicationManager.toolbarCommunicationManager
                .removeCopySelectionObserver(this.copySelectionHandler);
            this.processViewModel.communicationManager.processDiagramCommunication
                .removeModelUpdateObserver(this.modelUpdateHandler);
            this.processViewModel.communicationManager.processDiagramCommunication
                .unregister(ProcessEvents.NavigateToAssociatedArtifact, this.navigateToAssociatedArtifactHandler);
            this.processViewModel.communicationManager.processDiagramCommunication
                .unregister(ProcessEvents.OpenUtilityPanel, this.openUtilityPanelHandler);
            this.processViewModel.communicationManager.processDiagramCommunication
                .unregister(ProcessEvents.UserStoriesGenerated, this.userStoriesGeneratedHandler);
            this.processViewModel.communicationManager.processDiagramCommunication
                .unregister(ProcessEvents.SelectionChanged, this.selectionChangedHandler);
          
        }

        this.toggleProcessTypeHandler = this.processViewModel.communicationManager.toolbarCommunicationManager
            .registerToggleProcessTypeObserver(this.processTypeChanged);
        this.copySelectionHandler = this.processViewModel.communicationManager.toolbarCommunicationManager
            .registerCopySelectionObserver(this.copySelection);
        this.modelUpdateHandler = this.processViewModel.communicationManager.processDiagramCommunication
            .registerModelUpdateObserver(this.modelUpdate);
        this.navigateToAssociatedArtifactHandler = this.processViewModel.communicationManager.processDiagramCommunication
            .register(ProcessEvents.NavigateToAssociatedArtifact, this.navigateToAssociatedArtifact);
        this.openUtilityPanelHandler = this.processViewModel.communicationManager.processDiagramCommunication
            .register(ProcessEvents.OpenUtilityPanel, this.openUtilityPanel);
        this.userStoriesGeneratedHandler = this.processViewModel.communicationManager.processDiagramCommunication
            .register(ProcessEvents.UserStoriesGenerated, this.userStoriesGenerated);
        this.selectionChangedHandler = this.processViewModel.communicationManager.processDiagramCommunication
            .register(ProcessEvents.SelectionChanged, this.onDiagramSelectionChanged);

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
    };

    private copySelection = () => {
        ProcessCopyPasteHelper.copySectedShapes(this.graph, this.clipboard, this.shapesFactory);
    }
    private modelUpdate = (selectedNodeId: number) => {
        this.recreateProcessGraph(selectedNodeId);
    };

    private navigateToAssociatedArtifact = (info: any) => {
        const options = {
            id: info.id,
            version: info.version,
            enableTracking: info.enableTracking
        };
        this.navigationService.navigateTo(options);
    };

    private openUtilityPanel = () => {
        this.utilityPanelService.openPanelAsync(PanelType.Discussions);
    };

    private recreateProcessGraph = (selectedNodeId: number = undefined) => {
        this.graph.destroy();
        this.createProcessGraph(this.processViewModel, true, selectedNodeId);
    };

    private userStoriesGenerated = (userStories: IUserStory[]) => {
        this.graph.onUserStoriesGenerated(userStories);
    };

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
                this.clipboard
            );
             
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

    private onSubArtifactChanged(subArtifact: IStatefulSubArtifact) {
        if (!subArtifact && this.graph)  {
            this.graph.clearSelection();
        }
    }

    private onDiagramSelectionChanged = (elements: IDiagramNode[]) => {
        if (elements.length === 1) {
            // single-selection
            const subArtifactId: number = elements[0].model.id;
            const subArtifact = <IStatefulProcessSubArtifact>this.processArtifact.subArtifactCollection.get(subArtifactId);
            if (subArtifact) {
                subArtifact.loadProperties()
                    .then((loadedSubArtifact: IStatefulSubArtifact) => {

                        this.artifactManager.selection.setSubArtifact(loadedSubArtifact);
                    });
            }
        } else if (elements.length > 1) {
            // multi-selection 

            // disable the utility panel  
            // clear selection manager subartifact collection
            
            this.artifactManager.selection.clearSubArtifact();
            this.utilityPanelService.disableUtilityPanel();

        } else {
            this.artifactManager.selection.clearSubArtifact();
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

    public destroy() { 
        
        // tear down persistent objects and event handlers
        if (this.communicationManager) {
            if (this.communicationManager.toolbarCommunicationManager) {
                this.communicationManager.toolbarCommunicationManager
                    .removeToggleProcessTypeObserver(this.toggleProcessTypeHandler);
                this.communicationManager.toolbarCommunicationManager
                    .removeCopySelectionObserver(this.copySelectionHandler);
            }

            if (this.communicationManager.processDiagramCommunication) {
                this.communicationManager.processDiagramCommunication
                    .removeModelUpdateObserver(this.modelUpdateHandler);
                this.communicationManager.processDiagramCommunication
                    .unregister(ProcessEvents.NavigateToAssociatedArtifact, this.navigateToAssociatedArtifactHandler);
                this.communicationManager.processDiagramCommunication
                    .unregister(ProcessEvents.OpenUtilityPanel, this.openUtilityPanelHandler);
                this.communicationManager.processDiagramCommunication
                    .unregister(ProcessEvents.UserStoriesGenerated, this.userStoriesGeneratedHandler);
                this.processViewModel.communicationManager.processDiagramCommunication
                    .unregister(ProcessEvents.SelectionChanged, this.selectionChangedHandler);
            }
        }

        if (this.graph) {
            this.graph.destroy();
            this.graph = undefined;
        }

        if (this.processViewModel) {
            this.processViewModel.destroy();
            this.processViewModel = undefined;
        }
        
    }
}
