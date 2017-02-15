import {INavigationService} from "../../../../commonModule/navigation/navigation.service";
import {IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {IStatefulSubArtifact} from "../../../../managers/artifact-manager/sub-artifact/sub-artifact";
import {ISelectionManager} from "../../../../managers/selection-manager/selection-manager";
import {IDialogService} from "../../../../shared";
import {IUtilityPanelService, PanelType} from "../../../../shell/bp-utility-panel/utility-panel.svc";
import {ICommunicationManager} from "../../../bp-process";
import {ProcessType} from "../../models/enums";
import {IProcess, IUserStory} from "../../models/process-models";
import {IStatefulProcessArtifact} from "../../process-artifact";
import {IClipboardService} from "../../services/clipboard.svc";
import {IDiagramNode, IProcessGraph} from "./presentation/graph/models/process-graph-interfaces";
import {ProcessGraph} from "./presentation/graph/process-graph";
import {SystemTask} from "./presentation/graph/shapes";
import {ShapesFactory} from "./presentation/graph/shapes/shapes-factory";
import {ProcessEvents} from "./process-diagram-communication";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {IFileUploadService} from "../../../../commonModule/fileUpload/fileUpload.service";
import {IProcessViewModel, ProcessViewModel} from "./viewmodel/process-viewmodel";
import {IMessageService} from "../../../../main/components/messages/message.svc";
import {Message, MessageType} from "../../../../main/components/messages/message";
import {ILoadingOverlayService} from "../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {IStatefulProcessSubArtifact} from "../../process-subartifact";

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
    private openPropertiesHandler: string;
    private selectionChangedHandler: string;
    private validationHandler: string;
    private validationSubscriber: Rx.Disposable;

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
                private selectionManager: ISelectionManager,
                private fileUploadService: IFileUploadService,
                private loadingOverlayService: ILoadingOverlayService) {

        this.processModel = null;

    }

    public createDiagram(process: any, htmlElement: HTMLElement) {
        this.checkParams(process.id, htmlElement);
        this.htmlElement = htmlElement;

        if (this.validationSubscriber) {
            this.validationSubscriber.dispose();
            this.validationSubscriber = null;
        }

        this.processModel = <IProcess>process;
        this.processArtifact = <IStatefulProcessArtifact>process;
        // #DEBUG
        //this.selectionManager.subArtifactObservable
        //    .subscribeOnNext(this.onSubArtifactChanged, this);

        if (this.processArtifact) {
            this.validationSubscriber = this.processArtifact.getValidationObservable().subscribeOnNext((shapesIds) => {
                this.shapeValidated(shapesIds);
            });
        }

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
                .unregister(ProcessEvents.OpenProperties, this.openPropertiesHandler);
            this.processViewModel.communicationManager.processDiagramCommunication
                .unregister(ProcessEvents.UserStoriesGenerated, this.userStoriesGeneratedHandler);
            this.processViewModel.communicationManager.processDiagramCommunication
                .unregister(ProcessEvents.SelectionChanged, this.selectionChangedHandler);
            this.processViewModel.communicationManager.processDiagramCommunication
                .unregister(ProcessEvents.ShapesValidated, this.validationHandler);

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
        this.openPropertiesHandler = this.processViewModel.communicationManager.processDiagramCommunication
            .register(ProcessEvents.OpenProperties, this.openProperties);
        this.userStoriesGeneratedHandler = this.processViewModel.communicationManager.processDiagramCommunication
            .register(ProcessEvents.UserStoriesGenerated, this.userStoriesGenerated);
        this.selectionChangedHandler = this.processViewModel.communicationManager.processDiagramCommunication
            .register(ProcessEvents.SelectionChanged, this.onDiagramSelectionChanged);
        this.validationHandler = this.communicationManager.processDiagramCommunication
            .register(ProcessEvents.ShapesValidated, this.shapeValidated);

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
        this.graph.copySelectedShapes();
    };

    private modelUpdate = (selectedNodeId: number) => {
        this.recreateProcessGraph(selectedNodeId);
    };

    private navigateToAssociatedArtifact = (info: any) => {
        if (!!info) {
            const options = {
                id: info.id,
                version: info.version,
                enableTracking: info.enableTracking
            };
            this.navigationService.navigateTo(options);
        }
    };

    private openUtilityPanel = () => {
        this.utilityPanelService.openPanelAsync(PanelType.Discussions);
    };

    private openProperties = () => {
        this.utilityPanelService.openPanelAsync(PanelType.Properties);
    };

    private recreateProcessGraph = (selectedNodeId: number = undefined) => {
        this.graph.destroy();
        this.createProcessGraph(this.processViewModel, true, selectedNodeId);
    };

    private userStoriesGenerated = (userStories: IUserStory[]) => {
        this.graph.onUserStoriesGenerated(userStories);
    };

    private shapeValidated = (shapesIds: number[]) => {
        this.graph.onValidation(shapesIds);

        if (this.graph.systemTaskErrorPresented > 0) {
            this.processTypeChanged(ProcessType.UserToSystemProcess);
        }
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
                this.clipboard,
                this.fileUploadService,
                this.$q,
                this.loadingOverlayService
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
        // clear any subartifact that may still be selected
        // by selection manager and/or utility panel

        if (this.selectionManager) {
            this.selectionManager.clearSubArtifact();
        }
    }

    private onSubArtifactChanged(subArtifact: IStatefulSubArtifact) {
        if (!subArtifact && this.graph)  {
            this.graph.clearSelection();
        }
    }

    private onDiagramSelectionChanged = (elements: IDiagramNode[]) => {
        // Note: need to trigger an angular $digest so that bindings will
        // work in other components

        if (elements.length === 1) {
            // single-selection
            const subArtifactId: number = elements[0].model.id;
            const subArtifact = <IStatefulProcessSubArtifact>this.processArtifact.subArtifactCollection.get(subArtifactId);
            if (subArtifact) {
                subArtifact.loadProperties()
                    .then((loadedSubArtifact: IStatefulSubArtifact) => {
                        this.setSubArtifactSelectionAsync(loadedSubArtifact);
                    },
                    (err: any) => {
                        if (err && err.statusCode === 404) {
                            this.handleSubArtifactDeletedOrMoved(subArtifact, err);
                        }
                    }
                );
            }
        } else if (elements.length > 1) {
            // multiple selection
            this.setSubArtifactSelectionAsync(undefined, true);
        } else {
            // empty selection
            this.$rootScope.$applyAsync(() => {
                if (this.canChangeSelection()) {
                    this.selectionManager.clearSubArtifact();
                }
            });
        }
    }

    private setSubArtifactSelectionAsync(subArtifact: IStatefulSubArtifact, multiSelect: boolean = false) {
        this.$rootScope.$applyAsync(() => {
            if (this.canChangeSelection()) {
                this.selectionManager.setSubArtifact(subArtifact, multiSelect);
            }
        });
    }

    private canChangeSelection() {
        //'this.graph' is used as isDestroyed flag, since this.graph set to undefined in 'destroy()' method
        if (!this.graph || !this.selectionManager) {
            return false;
        }
        const selectedArtifact = this.selectionManager.getArtifact();
        const selectedArtifactId = selectedArtifact ? selectedArtifact.id : NaN;
        const processArtifactId = this.processArtifact ? this.processArtifact.id : NaN;
        return selectedArtifactId === processArtifactId;
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

    private handleSubArtifactDeletedOrMoved(subArtifact: IStatefulSubArtifact, err: any) {
        // subArtifact has been deleted or is otherwise not found in the database
        // clear the selection and  show message
        this.$rootScope.$applyAsync(() => {
            if (this.canChangeSelection()) {
                // set isDeleted flag to true 
                this.selectionManager.setSubArtifact(subArtifact, false, true);
                this.messageService.addMessage(new Message(
                    MessageType.Warning, this.localization.get("ST_SubArtifact_Has_Been_Deleted")
                ));
            }
        });
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
                    .unregister(ProcessEvents.OpenProperties, this.openPropertiesHandler);
                this.communicationManager.processDiagramCommunication
                    .unregister(ProcessEvents.UserStoriesGenerated, this.userStoriesGeneratedHandler);
                this.communicationManager.processDiagramCommunication
                    .unregister(ProcessEvents.SelectionChanged, this.selectionChangedHandler);
                this.communicationManager.processDiagramCommunication
                    .unregister(ProcessEvents.ShapesValidated, this.validationHandler);
            }
        }

        if (this.validationSubscriber) {
            this.validationSubscriber.dispose();
            this.validationSubscriber = null;
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
