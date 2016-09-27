import { ILocalizationService } from "../../../core";
import { IDialogSettings, IDialogService } from "../../../shared";
import { Models} from "../../models";
import { IArtifactManager, IProjectManager } from "../../../managers";

import { OpenProjectController } from "../dialogs/open-project";
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
        "localization",
        "dialogService",
        "projectManager",
        "artifactManager",
        "$rootScope",
        "loadingOverlayService",
        "$timeout",
        "$http"];

    constructor(
        private localization: ILocalizationService,
        private dialogService: IDialogService,
        private projectManager: IProjectManager,
        private artifactManager: IArtifactManager,
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
            case `deleteartifact`:
                this.dialogService.open(<IDialogSettings>{
                    okButton: this.localization.get("App_Button_Ok"),
                    template: require("../../../shared/widgets/bp-dialog/bp-dialog.html"),
                    header: this.localization.get("App_DialogTitle_Alert"),
                    message: "Are you sure you would like to delete the artifact"
                }).then((confirm: boolean) => {
                    if (confirm) {
                        this.dialogService.alert("you clicked confirm!");
                        this.deleteArtifact();
                    };
                });
                break;
            case `openproject`:
                this.dialogService.open(<IDialogSettings>{
                    okButton: this.localization.get("App_Button_Open"),
                    template: require("../dialogs/open-project.template.html"),
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
                //Test Code: Display load screen for 5s, then popup result.
                let publishLoadingId = this.loadingOverlayService.beginLoading();
                let publishPromise: ng.IPromise<number> = this.$timeout(() => { return 0; }, 5000);
                publishPromise.finally(() => {
                    this.loadingOverlayService.endLoading(publishLoadingId);
                    this.dialogService.alert(`Selected Action is ${element.id || element.innerText}`);
                });
                break;
            case `refreshall`:
                let refreshAllLoadingId = this.loadingOverlayService.beginLoading();

                if(this._currentArtifact){
                    try{
                        this.projectManager.refresh(this.projectManager.getSelectedProject())
                        .finally(() => {
                            this.loadingOverlayService.endLoading(refreshAllLoadingId);
                        });
                    }catch(err){
                        this.loadingOverlayService.endLoading(refreshAllLoadingId);
                        throw err;
                    }
                }
                break;
            case `gotoimpactanalysis`:
                let url = `Web/#/ImpactAnalysis/${this._currentArtifact}`;
                window.open(url);
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

    //temporary
    private deleteArtifact() {
    }

    public goToImpactAnalysis() {
        let url = `Web/#/ImpactAnalysis/${this._currentArtifact}`;
        window.open(url);
    }

    public $onInit() {
        this._subscribers = [
            this.artifactManager.selection.artifactObservable.subscribe(this.displayArtifact)
        ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    private displayArtifact = (artifact: Models.IArtifact) => {
        this._currentArtifact =
            Helper.canUtilityPanelUseSelectedArtifact(artifact) && 
            artifact.version !== 0 ? artifact.id : null;
    }
    
    public get canRefreshAll(): boolean{
        return !!this._currentArtifact;
    }

}