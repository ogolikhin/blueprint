﻿import {ILocalizationService, IMessageService, MessageType} from "../../../core";
import {IDialogSettings, IDialogService} from "../../../shared";
import {DialogTypeEnum} from "../../../shared/widgets/bp-dialog/bp-dialog";
import {Models} from "../../models";
import {IPublishService} from "../../../managers/artifact-manager/publish.svc";
import {IArtifactManager, IProjectManager} from "../../../managers";
import {IStatefulArtifact} from "../../../managers/artifact-manager/artifact";
import {OpenProjectController} from "../dialogs/open-project/open-project";
import {ConfirmPublishController, IConfirmPublishDialogData} from "../dialogs/bp-confirm-publish";
import {CreateNewArtifactController, ICreateNewArtifactDialogData} from "../dialogs/new-artifact";
import {BPTourController} from "../dialogs/bp-tour/bp-tour";
import {ILoadingOverlayService} from "../../../core/loading-overlay";
import {Project} from "../../../managers/project-manager/project";

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

    static $inject = [
        "$q",
        "localization",
        "dialogService",
        "projectManager",
        "artifactManager",
        "publishService",
        "messageService",
        "$rootScope",
        "loadingOverlayService",
        "$timeout",
        "$http"];

    constructor(
        private $q: ng.IQService,
        private localization: ILocalizationService,
        private dialogService: IDialogService,
        private projectManager: IProjectManager,
        private artifactManager: IArtifactManager,
        private publishService: IPublishService,
        private messageService: IMessageService,
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
                }).then((project: Models.IProject) => {
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
            okButton: this.localization.get("Discard_All_Ok_Button"),
            cancelButton: this.localization.get("Discard_All_Cancel_Button"),
            message: this.localization.get("Discard_All_Dialog_Message"),
            template: require("../dialogs/bp-confirm-publish/bp-confirm-publish.html"),
            controller: ConfirmPublishController,
            css: "nova-publish modal-alert",
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
            okButton: this.localization.get("App_Button_Publish"),
            cancelButton: this.localization.get("App_Button_Cancel"),
            message: this.localization.get("Publish_All_Dialog_Message"),
            template: require("../dialogs/bp-confirm-publish/bp-confirm-publish.html"),
            controller: ConfirmPublishController,
            css: "nova-publish"
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
        // if no artifact/project is selected and the project explorer is not open at all, always disable the button
        return this._currentArtifact && !!this.projectManager.getSelectedProject() ? !this._currentArtifact.artifactState.readonly : false;
    }

    private createNewArtifact() {
        const artifact = this._currentArtifact;
        this.dialogService.open(<IDialogSettings>{
                okButton: this.localization.get("App_Button_Create"),
                cancelButton: this.localization.get("App_Button_Cancel"),
                message: this.localization.get("Create_New_Dialog_Message"),
                template: require("../dialogs/new-artifact/new-artifact.html"),
                controller: CreateNewArtifactController,
                css: "nova-model" //temp
            },
            <ICreateNewArtifactDialogData>{
                projectId: artifact.projectId,
                parentId: artifact.id,
                parentPredefinedType: artifact.predefinedType
            })
            .then(() => {
                console.log(artifact);
            });
    }
}
