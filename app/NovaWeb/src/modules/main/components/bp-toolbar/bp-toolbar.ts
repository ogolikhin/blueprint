import {ILocalizationService} from "../../../core";
import {IDialogSettings, IDialogService} from "../../../shared";
import {Models} from "../../models";
import {IArtifactManager, IProjectManager} from "../../../managers";
import {IStatefulArtifact} from "../../../managers/artifact-manager/artifact";
import {OpenProjectController} from "../dialogs/open-project/open-project";
import {BPTourController} from "../dialogs/bp-tour/bp-tour";
import {Helper} from "../../../shared/utils/helper";
import {ILoadingOverlayService} from "../../../core/loading-overlay";

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
        "localization",
        "dialogService",
        "projectManager",
        "artifactManager",
        "$rootScope",
        "loadingOverlayService",
        "$timeout",
        "$http"];

    constructor(private localization: ILocalizationService,
                private dialogService: IDialogService,
                private projectManager: IProjectManager,
                private artifactManager: IArtifactManager,
                private $rootScope: ng.IRootScopeService,
                private loadingOverlayService: ILoadingOverlayService,
                private $timeout: ng.ITimeoutService, //Used for testing, remove later
                private $http: ng.IHttpService //Used for testing, remove later
    ) {
    }

    execute(evt: any): void {
        if (!evt) {
            return;
        }
        evt.preventDefault();
        const element = evt.currentTarget;
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
                let discardPromise: ng.IPromise<number> = this.$timeout(() => {
                    return 0;
                }, 500);
                discardPromise.finally(() => {
                    this.loadingOverlayService.endLoading(discardLoadingId);
                    this.dialogService.alert(`Selected Action is ${element.id || element.innerText}`);
                });
                break;
            case `publishall`:
                //Test Code: Display load screen for 5s, then popup result.
                let publishLoadingId = this.loadingOverlayService.beginLoading();
                let publishPromise: ng.IPromise<number> = this.$timeout(() => {
                    return 0;
                }, 5000);
                publishPromise.finally(() => {
                    this.loadingOverlayService.endLoading(publishLoadingId);
                    this.dialogService.alert(`Selected Action is ${element.id || element.innerText}`);
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

        this._subscribers = [artifactStateSubscriber];
    }

    public $onDestroy() {
        this._subscribers.forEach(subscriber => {
            subscriber.dispose();
        });
        delete this._subscribers;
    }

    private displayArtifact = (artifact: IStatefulArtifact) => {
        this._currentArtifact =
            Helper.canUtilityPanelUseSelectedArtifact(artifact) &&
            (artifact.version > 0) ? artifact.id : null;
    }

    public get canRefreshAll(): boolean {
        return !!this.projectManager.getSelectedProject();
    }
}
