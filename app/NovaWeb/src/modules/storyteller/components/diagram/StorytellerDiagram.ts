module Storyteller {
    /**
    * Storyteller Diagram Class 
    *
    * The StorytellerDiagram class contains the event handlers  
    * that create, update and destroy the diagram as needed.
    */
    class callbackClass {
        public cb: (param: any) => void;
        public arg1: any;
    }

    export class StorytellerDiagram {

        private utilityPanel: Shell.IPropertiesMw;
        private eventListeners = [];
        public storytellerViewModel: IStorytellerViewModel = null;
        private graph: ProcessGraph = null;
        private isLocking = false;
        private callbackQueue: callbackClass[] = [];

        constructor(
            private processId: string,
            private $rootScope: ng.IRootScopeService,
            private $scope: ng.IScope,
            private header: IStorytellerHeader,
            private processModelService: IProcessModelService,
            private artifactVersionControlService: Shell.IArtifactVersionControlService,
            private userstoryService: IUserstoryService,
            private selectionManager: ISelectionManager,
            private $timeout: ng.ITimeoutService,
            private shapesFactoryService: ShapesFactoryService,
            private messageService: Shell.IMessageService,
            private breadcrumbService: Shell.IBreadcrumbService,
            private bpUrlParsingService: IBpUrlParsingService,
            private $log: ng.ILogService,
            private artifactsIndicatorInfo: Shell.ArtifactsIndicatorInfo) {

            this.artifactsIndicatorInfo.artifactIds = [];
            this.addEventListeners();

        }

        private getUtilityPanel(): Shell.IPropertiesMw {
            let propertiesSvc = this.$scope.$root["propertiesSvc"];
            return propertiesSvc ? propertiesSvc() : null;
        }

        private addEventListeners() {
            if (this.eventListeners.length > 0) {
                this.removeEventListeners();
            }
            window.addEventListener("graphUpdated", (e) => this.graphUpdated(e), true);

            this.eventListeners.push(
                this.$scope.$on("processSaved", (e, data) => this.handleOnSave(e, data)
                ));
            this.eventListeners.push(
                this.$scope.$on("processModelUpdate", (e, nodeId) => this.handleOnModelUpdated(e, nodeId)
                ));
            this.eventListeners.push(
                this.$scope.$on(BaseModalDialogController.dialogOpenEventName,
                    () => this.handleOnDialogOpen()
                ));
            this.eventListeners.push(
                this.$scope.$on("changeSystemTasksVisibility",
                    (e, value) => this.handleOnChangeSystemTasksVisibility(e, value)
                ));
            this.eventListeners.push(
                this.$scope.$on("generateUserStories",
                    (e, taskId, value) => this.handleOnGenerateUserStories(e, taskId, value)
                ));
            this.eventListeners.push(
                this.$scope.$on("processChangesDiscarded",
                    () => this.handleOnDiscard()
                ));

            this.artifactsIndicatorInfo.onIndicatorFlagChange(this.$scope, this.onIndicatorFlagsChanged);
        }
        public onIndicatorFlagsChanged = (artifactId: number, flag: ItemIndicatorFlags) => {
            let diagramNode = this.graph.getNodeById(artifactId.toString()) as ITask;
            if (diagramNode &&
                diagramNode.activateButton) {
                diagramNode.activateButton(flag);
            }
        }
        public createDiagram(processId: string) {
            // retrieve the specified process from the server and 
            // create a new diagram

            const storytellerParams = this.bpUrlParsingService.getStateParams();            

            this.processModelService.load(processId,
                storytellerParams.versionId,
                storytellerParams.revisionId,
                storytellerParams.baselineId,
                storytellerParams.readOnly).then((process: IProcess) => {
                    this.onLoad(process);
                    if (!this.storytellerViewModel.isReadonly) {
                        this.storytellerViewModel.isWithinShapeLimit(0, true);
                    }
            }).catch((err: Shell.IHttpError) => {
                // if access to proccess info is forbidden
                if (err && err.statusCode === 403) {
                    var toolbar = document.getElementById("storytellerToolbarSection");
                    toolbar.hidden = true;
                    var editorView = document.getElementById("storytellerEditorView");
                    editorView.hidden = true;
                }
            });
        }

        private reloadDiagram(processId: string) {
            // reload the specified process from the server and 
            // create a new diagram
            this.processModelService.load(processId).then((process: IProcess) => {
                this.reload(process);
            });
        }

        public destroy() {
            // tear down embedded objects and event handlers
            this.handleOnDestroy();
        }

        private handleOnSave(event: any, process: IProcess) {
            this.reload(process);
        }

        private reload(process: IProcess) {
            this.graph.destroy();
            this.processModelService.isChanged = false;
            this.clearSelectedNodes();
            this.onLoad(process);
            this.resetHeaderFieldValues();
        }

        private handleOnDiscard() {

            //For discard we always load the latest published version. 
            //We do not need to specify version of already loaded process
            this.processModelService.load(this.processModelService.processModel.id.toString()).then(() => {
                this.reload(this.processModelService.processModel);
                this.messageService.addMessage(new Shell.Message(Shell.MessageType.Success, this.$rootScope["config"].labels["ST_Successfully_Discarded_Message"]));
            });
        }

        private handleOnModelUpdated(event: any, selectedNodeId: number) {
            this.graph.destroy();
            // create and populate new diagram
            this.createGraph(this.storytellerViewModel, true, selectedNodeId);
        }

        private handleOnDialogOpen() {
            this.closeUtilityPanelModalDialog();
        }

        private closeUtilityPanelModalDialog() {
            if (this.utilityPanel && this.utilityPanel.isModalDialogOpen()) {
                if (this.utilityPanel.loadBaseItemInfoPromise) {
                    this.utilityPanel.loadBaseItemInfoPromise.then(() => {
                        this.utilityPanel.closeModalDialog();
                    });
                } else {
                    this.utilityPanel.closeModalDialog();
                    
                }
            }
        }

        private handleOnChangeSystemTasksVisibility(event: any, value: boolean) {
            let isLocked: boolean = this.storytellerViewModel.isLocked;

            // try to lock
            if (!isLocked) {
                this.lock(this.updateOnChangeSystemTasksVisibility, value);
                return;
            }

            if (isLocked && this.storytellerViewModel.isLockedByMe) {
                this.updateOnChangeSystemTasksVisibility(value);
            }
        }

        private updateOnChangeSystemTasksVisibility = (value: boolean) => {
            if (window.console) {
                console.log("On changeSystemTasksVisibility, value = " + value);
            }

            this.graph.setSystemTasksVisible(value);
            this.processModelService.updateProcessType(value);
            this.storytellerViewModel.isChanged = true;

            if (!value &&
                this.graph.graph.getSelectionCells().filter(e => e instanceof SystemTask).length > 0)
                this.graph.graph.clearSelection();
            this.closeUtilityPanelModalDialog();
        };

        private handleOnGenerateUserStories(event: any, taskId: number, value: boolean) {
            this.graph.updateGraphNodes((cell) => {
                if (cell.getNodeType && cell.getNodeType() === NodeType.UserTask) {
                    if (!taskId || parseInt((<UserTask>cell).getId()) === taskId) {
                        return true;
                    }
                    return false;
                }
                return false;
            }, (cell: UserTask) => {
                cell.userStoryId = this.userstoryService.getUserStoryId(cell.model.id);
            });
        }

        private handleOnDestroy() {
            this.removeEventListeners();

            if (this.storytellerViewModel != null) {
                this.storytellerViewModel.destroy();
                this.storytellerViewModel = null;
            }
            if (this.graph != null) {
                this.graph.destroy();
                this.graph = null;
                this.$scope["vm"].graph = null;
            }
            if (this.processModelService != null) {
                this.processModelService.dispose();
                this.processModelService = null;
            }
            if (this.selectionManager != null)
                this.selectionManager.destroy();
        }

        private clearSelectedNodes() {
            var selectedNodes = this.selectionManager.getSelectedNodes();
            if (selectedNodes && selectedNodes.length > 0) {
                selectedNodes.length = 0;
                this.selectionManager.setSelectedNodes(selectedNodes);
                this.selectionManager.clearHighlightEdges();
            }
        }

        private createProcessViewModel(process: IProcess): IStorytellerViewModel {
            if (this.storytellerViewModel == null) {
                this.storytellerViewModel = new StorytellerViewModel(process, this.header, this.$scope, this.messageService);
            } else {
                this.storytellerViewModel.updateProcessClientModel(process);
            }
            return this.storytellerViewModel;
        }

        private onLoad(process: IProcess, useAutolayout: boolean = false, selectedNodeId: number = undefined) {
            let storytellerViewModel = this.createProcessViewModel(process);
            if (storytellerViewModel.isReadonly) this.disableStorytellerToolbar();
            this.createGraph(storytellerViewModel, useAutolayout, selectedNodeId);
        }

        private createGraph(storytellerViewModel: IStorytellerViewModel, useAutolayout: boolean = false, selectedNodeId: number = undefined) {

            this.graph = new ProcessGraph(
                this.$rootScope,
                this.$scope,
                this.processModelService,
                this.artifactVersionControlService,
                storytellerViewModel,
                this.shapesFactoryService,
                this.messageService,
                this.$log);

            // Note: add the graph as a property of the directive controller scope 
            // because the dialogs depend on it being there 
            this.$scope["vm"].graph = this.graph;

            this.graph.addSelectionListener((elements) => {
                this.selectionManager.setSelectedNodes(elements);
                this.selectionManager.highlightNodeEdges(elements, this.graph);
                this.selectionManager.updateUtilityPanel(elements, this.graph, this.utilityPanel);
            });

            this.graph.render(useAutolayout, selectedNodeId);

            this.utilityPanel = this.getUtilityPanel();
            
            this.graph.addIconRackListener((element: IProcessShape) => {
                if (this.utilityPanel != null) {
                    this.utilityPanel.openModalDialogWithProcessShape(element);
                }
            });
        }

        private lock(callback: (param: any) => void, param: any): void {
            let newCallbackObject = new callbackClass();
            newCallbackObject.cb = callback;
            newCallbackObject.arg1 = param;

            this.callbackQueue.push(newCallbackObject);

            if (!this.isLocking) {
                this.isLocking = true;
                this.artifactVersionControlService.lock([this.processModelService.processModel]).then((result: ILockResultInfo[]) => {
                    if (result[0].result === LockResult.Success) {
                        this.storytellerViewModel.isLocked = true;
                        this.storytellerViewModel.isLockedByMe = true;
                        this.isLocking = false;
                        while (this.callbackQueue && this.callbackQueue.length > 0) {
                            let callbackObject = this.callbackQueue.pop();
                            if (callbackObject != null && callbackObject.cb != null && callbackObject.arg1 != null) {
                                callbackObject.cb(callbackObject.arg1);
                            }
                        }
                    } else {
                        // reload diagram if lock failed
                        this.processLockFailure(result[0].result);
                    }
                }).catch(() => {
                    this.isLocking = false;
                });
            }
        }
        
        private processLockFailure(lockResult: LockResult) {

            let errorMessage: string;

            if (lockResult === LockResult.AccessDenied) {
                errorMessage = this.$rootScope["config"].labels["Artifact_Inaccessible"];
                // reload diagram if lock failed
                this.reloadDiagram(this.processId);
            } else if (lockResult === LockResult.DoesNotExist) {
                errorMessage = this.$rootScope["config"].labels["Artifact_Inaccessible"];
            } else if (lockResult === LockResult.AlreadyLocked) {
                errorMessage = this.$rootScope["config"].labels["ST_Artifact_Already_Locked_Message"];
                // reload diagram if lock failed
                this.reloadDiagram(this.processId);
            } else {
                errorMessage = this.$rootScope["config"].labels["ST_Artifact_Failure_AcquireLock_Message"];
                // reload diagram if lock failed
                this.reloadDiagram(this.processId);
            }

            this.messageService.addMessage(new Shell.Message(Shell.MessageType.Error, errorMessage));
            this.isLocking = false;
        }

        private graphUpdated = (event) => {
            if (this.storytellerViewModel != null &&
                this.storytellerViewModel.id === event.detail.processId) {

                let isLocked: boolean = this.storytellerViewModel.isLocked;

                // try to lock
                if (!isLocked) {
                    this.lock(this.refreshGraphOnUpdate, event.detail.nodeChanges);
                }

                if (isLocked && this.storytellerViewModel.isLockedByMe) {
                    this.refreshGraphOnUpdate(event.detail.nodeChanges);
                }
            }
        };

        private refreshGraphOnUpdate = (nodeChanges) => {
            if (!this.storytellerViewModel.isChanged) {
                this.$timeout(() => {
                    // set isChanged flag in viewmodel 
                    // this will also set isChanged property of header
                    this.storytellerViewModel.isChanged = true;
                    // TODO: remove state from processModelService 
                    // but must keep isChanged for the time being 
                    // because Navigation command relies on it 
                    if (this.processModelService) {
                        this.processModelService.isChanged = true;
                    }
                });
            }

            for (var i in nodeChanges) {
                var nodeChange = nodeChanges[i];

                if (nodeChange.redraw) {
                    var cell = this.graph.graph.getModel().getCell(nodeChange.nodeId);
                    try {
                        this.graph.graph.getModel().beginUpdate();
                        this.graph.graph.getView().clear(cell, true, true);
                        this.graph.graph.getView().validate(cell);
                    }
                    finally {
                        this.graph.graph.getModel().endUpdate();
                    }
                }
            }
        };

        private enableStorytellerToolbar() {
            this.$scope.$emit("enableStorytellerToolbar");
        }

        private disableStorytellerToolbar() {
            this.$scope.$emit("disableStorytellerToolbar");
        }

        private resetHeaderFieldValues() {
            if (this.header) {
                this.header.isUserToSystemProcess = this.processModelService.isUserToSystemProcess();
                this.header.name = this.storytellerViewModel.name;
                this.header.description = this.storytellerViewModel.description;

                const versionId = this.processModelService.processModel.requestedVersionInfo &&
                    this.processModelService.processModel.status &&
                    this.processModelService.processModel.status.hasEverBeenPublished &&
                    this.processModelService.processModel.requestedVersionInfo.versionId &&
                    this.processModelService.processModel.requestedVersionInfo.versionId > 0 ?
                    this.processModelService.processModel.requestedVersionInfo.versionId
                    : null;

                const revisionId = this.processModelService.processModel.requestedVersionInfo &&
                    this.processModelService.processModel.requestedVersionInfo.revisionId &&
                    this.processModelService.processModel.requestedVersionInfo.revisionId > 0 ?
                    this.processModelService.processModel.requestedVersionInfo.revisionId : null;

                const baselineId = this.processModelService.processModel.requestedVersionInfo &&
                    this.processModelService.processModel.requestedVersionInfo.baselineId &&
                    this.processModelService.processModel.requestedVersionInfo.baselineId > 0 ?
                    this.processModelService.processModel.requestedVersionInfo.baselineId : null;

                let navigationSegments: string;
                if (!this.breadcrumbService.artifactPathLinks ||
                    (this.breadcrumbService.artifactPathLinks && !this.breadcrumbService.artifactPathLinks.length)) {
                    navigationSegments = this.processModelService.processModel.id.toString();
                } else {
                    let segments = [];
                    for (let i = 0; i < this.breadcrumbService.artifactPathLinks.length; i++) {
                        segments.push(this.breadcrumbService.artifactPathLinks[i].id.toString());
                    }
                    navigationSegments = segments.join("/");
                }
                
                this.breadcrumbService.getNavigationPath(navigationSegments,
                    versionId,
                    revisionId,
                    baselineId).then((result: IArtifactReference[]) => {
                    this.header.artifactPathLinks = result;
                });
            }
        }

        private removeEventListeners() {
            window.removeEventListener("graphUpdated", this.graphUpdated, true);
            if (this.eventListeners.length > 0) {
                for (var i = 0; i < this.eventListeners.length; i++) {
                    this.eventListeners[i]();
                    this.eventListeners[i] = null;
                }
            }
            this.eventListeners = [];
        }
    }

}