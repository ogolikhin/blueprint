import {IDialogSettings, IDialogService} from "../../../shared";
import {Models, Enums} from "../../models";
import {IArtifactManager, IProjectManager} from "../../../managers";
import {IStatefulArtifact} from "../../../managers/artifact-manager/artifact/artifact";
import {ConfirmPublishController, IConfirmPublishDialogData} from "../dialogs/bp-confirm-publish";
import {
    CreateNewArtifactController,
    ICreateNewArtifactDialogData,
    ICreateNewArtifactReturn
} from "../dialogs/new-artifact";
import {BPTourController} from "../dialogs/bp-tour/bp-tour";
import {ILoadingOverlayService} from "../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../core/messages/message.svc";
import {ILocalizationService} from "../../../core/localization/localizationService";
import {INavigationService} from "../../../core/navigation/navigation.svc";
import {IApplicationError} from "../../../core/error/applicationError";
import {IAnalyticsProvider} from "../analytics/analyticsProvider";
import {IUnpublishedArtifactsService} from "../../../editors/unpublished/unpublished.svc";

interface IPageToolbarController {
    openProject(evt?: ng.IAngularEvent);
    closeProject(evt?: ng.IAngularEvent);
    closeAllProjects(evt?: ng.IAngularEvent);
    createNewArtifact(evt?: ng.IAngularEvent);
    publishAll(evt?: ng.IAngularEvent);
    discardAll(evt?: ng.IAngularEvent);
    refreshAll(evt?: ng.IAngularEvent);
    openTour(evt?: ng.IAngularEvent);
    showSubLevel(evt: ng.IAngularEvent): void;
}

export class PageToolbar implements ng.IComponentOptions {
    public template: string = require("./page-toolbar.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = PageToolbarController;
}

export class PageToolbarController implements IPageToolbarController {

    private _subscribers: Rx.IDisposable[];
    private _currentArtifact: IStatefulArtifact;
    private _canCreateNew: boolean = false;

    private get discardAllManyThreshold(): number {
        return 50;
    }

    static $inject = [
        "$q",
        "localization",
        "dialogService",
        "projectManager",
        "artifactManager",
        "publishService",
        "messageService",
        "navigationService",
        "loadingOverlayService",
        "analytics"
    ];

    constructor(private $q: ng.IQService,
                private localization: ILocalizationService,
                private dialogService: IDialogService,
                private projectManager: IProjectManager,
                private artifactManager: IArtifactManager,
                private publishService: IUnpublishedArtifactsService,
                private messageService: IMessageService,
                private navigationService: INavigationService,
                private loadingOverlayService: ILoadingOverlayService,
                private analytics: IAnalyticsProvider) {
    }

    public $onInit() {
        const artifactStateSubscriber = this.artifactManager.selection.artifactObservable
            .subscribe(this.setCurrentArtifact);

        this._subscribers = [artifactStateSubscriber];
    }

    public $onDestroy() {
        this._subscribers.forEach(subscriber => {
            subscriber.dispose();
        });
        delete this._subscribers;
    }

    public openProject = (evt: ng.IAngularEvent) => {
        if (evt) {
            evt.preventDefault();
        }
        this.projectManager.openProjectWithDialog();
    };

    /**
     * Closes the selected project.
     *
     * If there is no opened projects, navigates to main state
     * Otherwise navigates to next project in project list
     *
     */
    public closeProject = (evt?: ng.IAngularEvent) => {
        const id = this.loadingOverlayService.beginLoading();
        if (evt) {
            evt.preventDefault();
        }
        this.artifactManager.autosave().then(() => {
            const artifact = this.artifactManager.selection.getArtifact();
            if (artifact) {
                return this.closeProjectInternal(artifact.projectId);
            }
            return this.$q.resolve();

        }).finally(() => {
            this.loadingOverlayService.endLoading(id);
        });
    };

    public closeAllProjects = (evt?: ng.IAngularEvent) => {
        const id = this.loadingOverlayService.beginLoading();
        if (evt) {
            evt.preventDefault();
        }

        this.artifactManager.autosave()
            .then(this.getProjectsWithUnpublishedArtifacts)
            .then((projectsWithUnpublishedArtifacts) => {
                const unpublishedArtifactsByProject = _.countBy(projectsWithUnpublishedArtifacts);
                const openProjects = _.map(this.projectManager.projectCollection.getValue(), (project) => project.model.id);
                let numberOfUnpublishedArtifacts = 0;
                _.forEach(openProjects, (projectId) => numberOfUnpublishedArtifacts += unpublishedArtifactsByProject[projectId] || 0);

                if (numberOfUnpublishedArtifacts > 0) {
                    //If the project we're closing has unpublished artifacts, we display a modal
                    let message: string = this.localization.get("Close_Project_UnpublishedArtifacts")
                        .replace(`{0}`, numberOfUnpublishedArtifacts.toString());
                    return this.dialogService.alert(message, null, "App_Button_ConfirmCloseProject", "App_Button_Cancel").then(() => {
                        this.closeAllProjectsInternal();
                    });
                } else {
                    //Otherwise, just close it
                    return this.closeAllProjectsInternal();
                }
            }).finally(() => {
            this.loadingOverlayService.endLoading(id);
        });
    };

    public createNewArtifact = (evt?: ng.IAngularEvent) => {
        if (evt) {
            evt.preventDefault();
        }
        const artifact = this._currentArtifact; 
        const projectId = artifact.projectId;
        const parentId = artifact.predefinedType !== Enums.ItemTypePredefined.ArtifactCollection ? artifact.id : artifact.parentId;
        this.dialogService.open(<IDialogSettings>{
                okButton: this.localization.get("App_Button_Create"),
                cancelButton: this.localization.get("App_Button_Cancel"),
                template: require("../dialogs/new-artifact/new-artifact.html"),
                controller: CreateNewArtifactController,
                css: "nova-new-artifact"
            },
            <ICreateNewArtifactDialogData>{
                projectId: projectId,
                parentId: parentId,
                parentPredefinedType: artifact.predefinedType
            })
            .then((result: ICreateNewArtifactReturn) => {
                const createNewArtifactLoadingId = this.loadingOverlayService.beginLoading();

                const itemTypeId = result.artifactTypeId;
                const name = result.artifactName;

                this.artifactManager.create(name, projectId, parentId, itemTypeId)
                    .then((data: Models.IArtifact) => {
                        const newArtifactId = data.id;
                        this.projectManager.refresh(projectId, null, true)
                            .finally(() => {
                                this.projectManager.triggerProjectCollectionRefresh();
                                this.navigationService.navigateTo({id: newArtifactId})
                                    .finally(() => {
                                        this.loadingOverlayService.endLoading(createNewArtifactLoadingId);
                                    });
                            });
                    })
                    .catch((error: IApplicationError) => {
                        if (error.statusCode === 404 && error.errorCode === 102) {
                            // project not found, we refresh all
                            this.projectManager.refreshAll()
                                .then(() => {
                                    this.messageService.addError("Create_New_Artifact_Error_404_102", true);
                                    this.loadingOverlayService.endLoading(createNewArtifactLoadingId);
                                });
                        } else if (error.statusCode === 404 && error.errorCode === 101) {
                            // parent not found, we refresh the single project and move to the root
                            this.navigationService.navigateTo({id: projectId})
                                .finally(() => {
                                    this.projectManager.refresh(projectId)
                                        .then(() => {
                                            this.projectManager.triggerProjectCollectionRefresh();
                                            this.messageService.addError("Create_New_Artifact_Error_404_101", true);
                                            this.loadingOverlayService.endLoading(createNewArtifactLoadingId);
                                        });
                                });
                        } else if (error.statusCode === 404 && error.errorCode === 109) {
                            // artifact type not found, we refresh the single project
                            this.projectManager.refresh(projectId)
                                .then(() => {
                                    this.projectManager.triggerProjectCollectionRefresh();
                                    this.messageService.addError("Create_New_Artifact_Error_404_109", true);
                                    this.loadingOverlayService.endLoading(createNewArtifactLoadingId);
                                });
                        } else if (!error.handled) {
                            this.messageService.addError("Create_New_Artifact_Error_Generic");
                        }

                        if (error.statusCode !== 404) {
                            this.loadingOverlayService.endLoading(createNewArtifactLoadingId);
                        }
                    });
            });
    };

    public publishAll = (evt?: ng.IAngularEvent) => {
        if (evt) {
            evt.preventDefault();
        }
        this.artifactManager.autosave().then(() => {
            const getUnpublishedLoadingId = this.loadingOverlayService.beginLoading();
            //get a list of unpublished artifacts
            this.publishService.getUnpublishedArtifacts()
                .then((data: Models.IPublishResultSet) => {
                    if (data.artifacts.length === 0) {
                        this.messageService.addInfo("Publish_All_No_Unpublished_Changes");
                    } else {
                        this.confirmPublishAll(data);
                    }
                })
                .finally(() => {
                    this.loadingOverlayService.endLoading(getUnpublishedLoadingId);
                });
        });
    };

    public discardAll = (evt?: ng.IAngularEvent) => {
        if (evt) {
            evt.preventDefault();
        }
        let getUnpublishedLoadingId = this.loadingOverlayService.beginLoading();
        //get a list of unpublished artifacts
        this.publishService.getUnpublishedArtifacts()
            .then((data: Models.IPublishResultSet) => {
                if (data.artifacts.length === 0) {
                    this.messageService.addInfo("Discard_All_No_Unpublished_Changes");
                } else {
                    this.confirmDiscardAll(data);
                }
            })
            .finally(() => {
                this.loadingOverlayService.endLoading(getUnpublishedLoadingId);
            });
    };

    public refreshAll = (evt?: ng.IAngularEvent) => {
        if (evt) {
            evt.preventDefault();
        }
        let promise: ng.IPromise<any>;
        let artifact: IStatefulArtifact;
        if (this.isProjectOpened) {
            promise = this.projectManager.refreshAll();    
        } else if (artifact = this.artifactManager.selection.getArtifact()) {
            promise = artifact.refresh();
        }
        if (promise) {
            let refreshAllLoadingId = this.loadingOverlayService.beginLoading();
            promise.finally(() => {
                this.loadingOverlayService.endLoading(refreshAllLoadingId);
            });
        }
    };
    public openTour = (evt?: ng.IAngularEvent) => {
        if (evt) {
            evt.preventDefault();
        }
        this.dialogService.open(<IDialogSettings>{
            template: require("../dialogs/bp-tour/bp-tour.html"),
            controller: BPTourController,
            backdrop: true,
            css: "nova-tour"
        });
    };

    private confirmDiscardAll(data: Models.IPublishResultSet) {
        const selectedProjectId: number = this.projectManager.getSelectedProjectId();
        this.dialogService.open(<IDialogSettings>{
                okButton: this.localization.get("App_Button_Discard_All"),
                cancelButton: this.localization.get("App_Button_Cancel"),
                message: data.artifacts && data.artifacts.length > this.discardAllManyThreshold
                    ? this.localization.get("Discard_All_Many_Dialog_Message")
                    : this.localization.get("Discard_All_Dialog_Message"),
                template: require("../dialogs/bp-confirm-publish/bp-confirm-publish.html"),
                controller: ConfirmPublishController,
                css: "modal-alert nova-publish",
                header: this.localization.get("App_DialogTitle_Alert")
            },
            <IConfirmPublishDialogData>{
                artifactList: data.artifacts,
                projectList: data.projects,
                selectedProject: selectedProjectId
            })
            .then(() => {
                this.discardAllInternal(data);
            });
    }

    private confirmPublishAll(data: Models.IPublishResultSet) {
        const selectedProjectId: number = this.projectManager.getSelectedProjectId();
        this.dialogService.open(<IDialogSettings>{
                okButton: this.localization.get("App_Button_Publish_All"),
                cancelButton: this.localization.get("App_Button_Cancel"),
                message: this.localization.get("Publish_All_Dialog_Message"),
                template: require("../dialogs/bp-confirm-publish/bp-confirm-publish.html"),
                controller: ConfirmPublishController,
                css: "nova-publish",
                header: this.localization.get("App_DialogTitle_Confirmation")
            },
            <IConfirmPublishDialogData>{
                artifactList: data.artifacts,
                projectList: data.projects,
                selectedProject: selectedProjectId
            })
            .then(() => {
                this.publishAllInternal(data);
            });
    }

    private closeProjectInternal(currentProjectId: number): ng.IPromise<any> {
        return this.getProjectsWithUnpublishedArtifacts().then((projectsWithUnpublishedArtifacts) => {
            const unpublishedArtifactCount = _.countBy(projectsWithUnpublishedArtifacts)[currentProjectId];
            if (unpublishedArtifactCount > 0) {
                //If the project we're closing has unpublished artifacts, we display a modal
                let message: string = this.localization.get("Close_Project_UnpublishedArtifacts")
                    .replace(`{0}`, unpublishedArtifactCount.toString());
                return this.dialogService.alert(message, null, "App_Button_ConfirmCloseProject", "App_Button_Cancel").then(() => {
                    this.closeProjectById(currentProjectId);
                });
            } else {
                //Otherwise, just close it
                return this.closeProjectById(currentProjectId);
            }
        });
    }

    private closeProjectById(projectId: number) {
        const isOpened = _.some(this.projectManager.projectCollection.getValue(), (p) => p.model.id === projectId);
        if (isOpened) {
            this.projectManager.remove(projectId);
        }
        const nextProject = _.first(this.projectManager.projectCollection.getValue());
        if (nextProject) {
            this.navigationService.navigateTo({id: nextProject.model.id});
        } else {
            this.navigationService.navigateToMain();
        }
        this.clearStickyMessages();
    }

    private closeAllProjectsInternal (): ng.IPromise<any> {
        this.projectManager.removeAll();
        this.clearStickyMessages();
        return this.navigationService.navigateToMain();
    }

    private getProjectsWithUnpublishedArtifacts = (): ng.IPromise<number[]> => {
        //We can't use artifactManager.list() because lock state is lazy-loaded
        return this.publishService.getUnpublishedArtifacts().then((unpublishedArtifactSet) => {
            const projectsWithUnpublishedArtifacts = _.map(unpublishedArtifactSet.artifacts, (artifact) => artifact.projectId);
            //We don't use _.uniq because we care about the count of artifacts.
            return projectsWithUnpublishedArtifacts;
        });
    }

    private publishAllInternal(data: Models.IPublishResultSet) {
        const publishAllLoadingId = this.loadingOverlayService.beginLoading();
        //perform publish all
        this.publishService.publishAll()
            .then(() => {
                let artifact = this.artifactManager.selection.getArtifact();
                //remove lock on current artifact
                if (artifact) {
                    artifact.artifactState.unlock();
                    artifact.refresh();
                }

                this.messageService.addInfo("Publish_All_Success_Message", data.artifacts.length);

                if (_.find(data.artifacts, {predefinedType: Enums.ItemTypePredefined.Process})) {
                    this.messageService.addInfo("ST_ProcessType_RegenerateUSS_Message");
                 }
            })
            .catch((error) => {
                this.messageService.addError(error);
            })
            .finally(() => {
                this.loadingOverlayService.endLoading(publishAllLoadingId);
            });
    }

    private discardAllInternal(data: Models.IPublishResultSet) {
        const publishAllLoadingId = this.loadingOverlayService.beginLoading();
        //perform publish all
        this.publishService.discardAll()
            .then(() => {
                const statefulArtifact = this.artifactManager.selection.getArtifact();
                if (statefulArtifact) {
                    statefulArtifact.discard();
                }

                // If the current artifact has never been published, navigate back to the main page;
                // otherwise, refresh all
                if (!this.isProjectOpened && statefulArtifact && statefulArtifact.version === -1) {
                    this.navigationService.navigateToMain(true);
                } else {
                    this.refreshAll();
                }

                this.messageService.addInfo("Discard_All_Success_Message", data.artifacts.length);
            })
            .finally(() => {
                this.loadingOverlayService.endLoading(publishAllLoadingId);
            });
    }

    showSubLevel(evt: any): void {
        // this is needed to allow tablets to show submenu (as touch devices don't understand hover)
        if (!evt) {
            return;
        }
        evt.preventDefault();
        evt.stopImmediatePropagation();
    }

    private clearStickyMessages() {
        this.messageService.messages.forEach(message => {
            if (!message.canBeClosedManually) {
                this.messageService.deleteMessageById(message.id);
            }
        });
    }

    private setCurrentArtifact = (artifact: IStatefulArtifact) => {
        this._currentArtifact = artifact;
        //calculate properties
        this._canCreateNew = this._currentArtifact && 
                            !this._currentArtifact.artifactState.historical && 
                            !this._currentArtifact.artifactState.deleted &&
                            (this._currentArtifact.permissions & Enums.RolePermissions.Edit) === Enums.RolePermissions.Edit;
    };

    public get isProjectOpened(): boolean {
        return this.projectManager.projectCollection.getValue().length > 0;
    }

    public get isArtifactSelected(): boolean {
        return this.isProjectOpened && !!this._currentArtifact;
    }

    public get canCreateNew(): boolean {
        return this._canCreateNew;
    }


}
