import * as angular from "angular";
import { ILocalizationService, IMessageService } from "../../../core";
import { IDialogSettings, IDialogService } from "../../../shared";
import { Models} from "../../models";
import { IPublishService } from "../../../managers/artifact-manager/publish";
import { IArtifactManager, IProjectManager } from "../../../managers";
import { IStatefulArtifact } from "../../../managers/artifact-manager/artifact";
import { OpenProjectController } from "../dialogs/open-project/open-project";
import { ConfirmPublishController, IConfirmPublishDialogData } from "../dialogs/bp-confirm-publish/bp-confirm-publish";
import { BPTourController } from "../dialogs/bp-tour/bp-tour";
import { Helper } from "../../../shared/utils/helper";
import { ILoadingOverlayService } from "../../../core/loading-overlay";

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
    private _currentArtifact: number;

    public get currentArtifact() {
        return this._currentArtifact;
    }
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
    ) { }

    execute(evt: any): void {
        if (!evt) {
            return;
        }
        evt.preventDefault();
        var element = evt.currentTarget;
        switch (element.id.toLowerCase()) {
            case `projectclose`:
                this.projectManager.remove();
                break;
            case `projectcloseall`:
                this.projectManager.remove(true);
                break;
            case `openproject`:
                this.dialogService.open(<IDialogSettings>{
                    okButton: this.localization.get("App_Button_Open"),
                    template: require("../dialogs/open-project/open-project.template.html"),
                    controller: OpenProjectController,
                    css: "nova-open-project" // removed modal-resize-both as resizing the modal causes too many artifacts with ag-grid
                }).then((project: Models.IProject) => {
                    if (project) {
                        this.projectManager.add(project);
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
                //Test Code: Display load screen for 0.4s (invisible), then popup result.
                let discardLoadingId = this.loadingOverlayService.beginLoading();
                let discardPromise: ng.IPromise<number> = this.$timeout(() => { return 0; }, 500);
                discardPromise.finally(() => {
                    this.loadingOverlayService.endLoading(discardLoadingId);
                    this.dialogService.alert(`Selected Action is ${element.id || element.innerText}`);
                });
                break;
            case `publishall`:
                let getUnpublishedLoadingId = this.loadingOverlayService.beginLoading();
                try {
                    //get a list of unpublished artifacts
                    this.publishService.getUnpublishedArtifacts()
                    .then((data) => {
                        this.loadingOverlayService.endLoading(getUnpublishedLoadingId);

                        if (data.artifacts.length === 0) {
                            this.messageService.addInfo("Publish_All_No_Unpublished_Changes");
                        } else {

                            //confirm that the user wants to continue
                            this.dialogService.open(<IDialogSettings>{
                                okButton: this.localization.get("App_Button_Yes"),
                                cancelButton: this.localization.get("App_Button_No"),
                                message: "Publish_All_Dialog_Message",
                                template: require("../dialogs/bp-confirm-publish/bp-confirm-publish.html"),
                                controller: ConfirmPublishController,
                                css: "nova-open-project" // removed modal-resize-both as resizing the modal causes too many artifacts with ag-grid
                            },
                            <IConfirmPublishDialogData>{
                                artifactList: data.artifacts,
                                projectList: data.projects
                            })
                            .then(() => {
                                let publishAllLoadingId = this.loadingOverlayService.beginLoading();
                                try {
                                    let artifactsToSave = [];
                                    data.artifacts.forEach((artifact) => {
                                        let foundArtifact = this.projectManager.getArtifact(artifact.id);
                                        if (foundArtifact && foundArtifact.isCanBeSaved()) {
                                            artifactsToSave.push(foundArtifact.save());
                                        }
                                    });

                                    this.$q.all(artifactsToSave).then(() => {
                                        //perform publish all
                                        this.publishService.publishAll()
                                        .then(() => {
                                            this.messageService.addInfoWithPar("Publish_All_Success_Message", [data.artifacts.length]);
                                        })
                                        .finally(() => {
                                            this.loadingOverlayService.endLoading(publishAllLoadingId);
                                        });
                                    }).catch((err) => {
                                        this.messageService.addError(err);
                                        this.loadingOverlayService.endLoading(publishAllLoadingId);
                                    });
                                } catch (err) {
                                    this.loadingOverlayService.endLoading(publishAllLoadingId);
                                    throw err;
                                }
                            });
                        }
                    })
                    .finally(() => {
                        this.loadingOverlayService.endLoading(getUnpublishedLoadingId);
                    });
                } catch (err) {
                    this.loadingOverlayService.endLoading(getUnpublishedLoadingId);
                    throw err;
                }

                
                break;
            case `refreshall`:
                let refreshAllLoadingId = this.loadingOverlayService.beginLoading();

                try {
                    this.projectManager.refreshAll()
                    .finally(() => {
                        this.loadingOverlayService.endLoading(refreshAllLoadingId);
                    });
                }catch (err) {
                    this.loadingOverlayService.endLoading(refreshAllLoadingId);
                    throw err;
                }
                break;
            default:
                this.dialogService.alert(`Selected Action is ${element.id || element.innerText}`);
                break;
        }
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
            .subscribe(this.displayArtifact);

        this._subscribers = [ artifactStateSubscriber ];
    }

    public $onDestroy() {
        this._subscribers.forEach(subscriber => { subscriber.dispose(); });
        delete this._subscribers;
    }

    private displayArtifact = (artifact: IStatefulArtifact) => {
        this._currentArtifact =
            Helper.canUtilityPanelUseSelectedArtifact(artifact) &&
            (artifact.version > 0) ? artifact.id : null;
    }

    public get canRefreshAll(): boolean{
        return !!this.projectManager.getSelectedProject();
    }
}
