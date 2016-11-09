import {IDialogSettings, IDialogService} from "../../../shared";
import {Models, Enums, AdminStoreModels} from "../../models";
import {IPublishService} from "../../../managers/artifact-manager/publish.svc";
import {IArtifactManager, IProjectManager} from "../../../managers";
import {IStatefulArtifact} from "../../../managers/artifact-manager/artifact";
import {OpenProjectController} from "../dialogs/open-project/open-project";
import {ConfirmPublishController, IConfirmPublishDialogData} from "../dialogs/bp-confirm-publish";
import {
    CreateNewArtifactController,
    ICreateNewArtifactDialogData,
    ICreateNewArtifactReturn
} from "../dialogs/new-artifact";
import {BPTourController} from "../dialogs/bp-tour/bp-tour";
import {Project} from "../../../managers/project-manager/project";
import {ILoadingOverlayService} from "../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../core/messages/message.svc";
import {MessageType} from "../../../core/messages/message";
import {ILocalizationService} from "../../../core/localization/localizationService";
import {INavigationService} from "../../../core/navigation/navigation.svc";
import {IApplicationError} from "../../../core/error/applicationError";

interface IBPToolbarController {
    execute(evt: ng.IAngularEvent): void;
    showSubLevel(evt: ng.IAngularEvent): void;
}

export class BPToolbar implements ng.IComponentOptions {
    public template: string = require("./bp-toolbar.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPToolbarController;
}

class BPToolbarController implements IBPToolbarController {

    private _subscribers: Rx.IDisposable[];
    private _currentArtifact: IStatefulArtifact;

    private get discardAllManyThreshold(): number{
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
        "$rootScope",
        "loadingOverlayService",
        "$timeout",
        "$http"];

    constructor(private $q: ng.IQService,
                private localization: ILocalizationService,
                private dialogService: IDialogService,
                private projectManager: IProjectManager,
                private artifactManager: IArtifactManager,
                private publishService: IPublishService,
                private messageService: IMessageService,
                private navigationService: INavigationService,
                private $rootScope: ng.IRootScopeService,
                private loadingOverlayService: ILoadingOverlayService,
                private $timeout: ng.ITimeoutService, //Used for testing, remove later
                private $http: ng.IHttpService //Used for testing, remove later
    ) {
    }

    public execute(evt: any): void {
        if (!evt) {
            return;
        }
        evt.preventDefault();
        const element = evt.currentTarget;
        switch (element.id.toLowerCase()) {
            case `projectclose`:
                this.projectManager.remove();
                this.artifactManager.selection.clearAll();
                this.clearLockedMessages();
                break;
            case `projectcloseall`:
                this.projectManager.removeAll();
                this.artifactManager.selection.clearAll();
                this.clearLockedMessages();
                break;
            case `openproject`:
                this.dialogService.open(<IDialogSettings>{
                    okButton: this.localization.get("App_Button_Open"),
                    template: require("../dialogs/open-project/open-project.template.html"),
                    controller: OpenProjectController,
                    css: "nova-open-project" // removed modal-resize-both as resizing the modal causes too many artifacts with ag-grid
                }).then((project: AdminStoreModels.IInstanceItem) => {
                    if (project) {
                        const openProjectLoadingId = this.loadingOverlayService.beginLoading();

                        try {
                            this.projectManager.add(project)
                                .finally(() => {
                                    this.loadingOverlayService.endLoading(openProjectLoadingId);
                                });
                        } catch (err) {
                            this.loadingOverlayService.endLoading(openProjectLoadingId);
                            throw err;
                        }
                    }
                });
                break;
            case `tour`:
                this.dialogService.open(<IDialogSettings>{
                    template: require("../dialogs/bp-tour/bp-tour.html"),
                    controller: BPTourController,
                    backdrop: true,
                    css: "nova-tour"
                });
                break;
            case `discardall`:
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
                break;
            case `publishall`:
                getUnpublishedLoadingId = this.loadingOverlayService.beginLoading();
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
                break;
            case `refreshall`:
                let refreshAllLoadingId = this.loadingOverlayService.beginLoading();

                try {
                    this.projectManager.refreshAll()
                        .finally(() => {
                            this.loadingOverlayService.endLoading(refreshAllLoadingId);
                        });
                } catch (err) {
                    this.loadingOverlayService.endLoading(refreshAllLoadingId);
                    throw err;
                }
                break;
            case `createnewartifact`:
                this.createNewArtifact();
                break;
            default:
                this.dialogService.alert(`Selected Action is ${element.id || element.innerText}`);
                break;
        }
    }

    private clearLockedMessages() {
        this.messageService.messages.forEach(message => {
            if (message.messageType === MessageType.Lock) {
                this.messageService.deleteMessageById(message.id);
            }
        });
    }

    private confirmDiscardAll(data: Models.IPublishResultSet) {
        const selectedProject: Project = this.projectManager.getSelectedProject();
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
                selectedProject: selectedProject ? selectedProject.id : undefined
            })
            .then(() => {
                this.discardAll(data);
            });
    }

    private discardAll(data: Models.IPublishResultSet) {
        const publishAllLoadingId = this.loadingOverlayService.beginLoading();
        //perform publish all
        this.publishService.discardAll()
            .then(() => {
                //remove lock on current artifact
                const selectedArtifact = this.artifactManager.selection.getArtifact();
                if (selectedArtifact) {
                    selectedArtifact.artifactState.unlock();
                    selectedArtifact.refresh();
                }

                this.messageService.addInfoWithPar("Discard_All_Success_Message", [data.artifacts.length]);
            })
            .finally(() => {
                this.loadingOverlayService.endLoading(publishAllLoadingId);
            });
    }

    private confirmPublishAll(data: Models.IPublishResultSet) {
        const selectedProject: Project = this.projectManager.getSelectedProject();
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
                selectedProject: selectedProject ? selectedProject.id : undefined
            })
            .then(() => {
                this.saveAndPublishAll(data);
            });
    }

    private saveAndPublishAll(data: Models.IPublishResultSet) {
        this.saveArtifactsAsNeeded(data.artifacts).then(() => {
            const publishAllLoadingId = this.loadingOverlayService.beginLoading();
            //perform publish all
            this.publishService.publishAll()
                .then(() => {
                    //remove lock on current artifact
                    const selectedArtifact = this.artifactManager.selection.getArtifact();
                    if (selectedArtifact) {
                        selectedArtifact.artifactState.unlock();
                        selectedArtifact.refresh();
                    }

                    this.messageService.addInfoWithPar("Publish_All_Success_Message", [data.artifacts.length]);
                })
                .finally(() => {
                    this.loadingOverlayService.endLoading(publishAllLoadingId);
                });
        });
    }

    private saveArtifactsAsNeeded(artifactsToSave: Models.IArtifact[]): ng.IPromise<any> {
        const saveArtifactsLoader = this.loadingOverlayService.beginLoading();
        const savePromises = [];
        const selectedArtifact = this.artifactManager.selection.getArtifact();
        artifactsToSave.forEach((artifactModel) => {
            let artifact = this.projectManager.getArtifact(artifactModel.id);
            if (!artifact) {
                if (selectedArtifact && selectedArtifact.id === artifactModel.id) {
                    artifact = selectedArtifact;
                }
            }
            if (artifact && artifact.canBeSaved()) {
                savePromises.push(artifact.save());
            }
        });

        const allPromises = this.$q.all(savePromises);

        allPromises.catch((err) => {
            this.messageService.addError(err);
        }).finally(() => {
            this.loadingOverlayService.endLoading(saveArtifactsLoader);
        });

        return allPromises;
    }

    showSubLevel(evt: any): void {
        // this is needed to allow tablets to show submenu (as touch devices don't understand hover)
        if (!evt) {
            return;
        }
        evt.preventDefault();
        evt.stopImmediatePropagation();
    }

    public $onInit() {
        const artifactStateSubscriber = this.artifactManager.selection.artifactObservable
            .map(selection => {
                if (!selection) {
                    this._currentArtifact = null;
                }
                return selection;
            })
            .filter(selection => !!selection)
            .flatMap(selection => selection.getObservable())
            .subscribe(this.setCurrentArtifact);

        this._subscribers = [artifactStateSubscriber];
    }

    public $onDestroy() {
        this._subscribers.forEach(subscriber => {
            subscriber.dispose();
        });
        delete this._subscribers;
    }

    public get canRefreshAll(): boolean {
        return !!this.projectManager.getSelectedProject();
    }

    private setCurrentArtifact = (artifact: IStatefulArtifact) => {
        this._currentArtifact = artifact;
    };

    public get canCreateNew(): boolean {
        const currArtifact = this._currentArtifact;
        // if no artifact/project is selected and the project explorer is not open at all, always disable the button
        return currArtifact &&
            !!this.projectManager.getSelectedProject() &&
            !currArtifact.artifactState.historical &&
            !currArtifact.artifactState.deleted &&
            (currArtifact.permissions & Enums.RolePermissions.Edit) === Enums.RolePermissions.Edit;
    }

    private createNewArtifact() {
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
                        this.projectManager.refresh(projectId)
                            .finally(() => {
                                this.projectManager.triggerProjectCollectionRefresh();
                                this.navigationService.navigateTo({id: newArtifactId})
                                    .finally(() => {
                                        this.loadingOverlayService.endLoading(createNewArtifactLoadingId);
                                    });
                            });
                    })
                    .catch((error: IApplicationError) => {
                        if (error.statusCode === 404) {
                            this.projectManager.refresh(projectId)
                                .then(() => {
                                    this.projectManager.triggerProjectCollectionRefresh();
                                    this.messageService.addError("Create_New_Artifact_Error_404", true);
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
    }
}
