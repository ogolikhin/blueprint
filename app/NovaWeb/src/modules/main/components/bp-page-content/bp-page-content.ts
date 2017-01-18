import {IDialogService, IDialogSettings} from "../../../shared/widgets/bp-dialog/bp-dialog";
import {IArtifactManager, ISelection} from "../../../managers/artifact-manager";
import {IStatefulArtifact} from "../../../managers/artifact-manager/artifact/artifact";
import {ItemTypePredefined} from "../../models/enums";
import {IMainBreadcrumbService} from "./mainbreadcrumb.svc";
import {IProjectManager} from "../../../managers/project-manager";
import {ILocalizationService} from "../../../core/localization/localization.service";
import {BPTourController} from "../dialogs/bp-tour/bp-tour";

export class PageContent implements ng.IComponentOptions {
    public template: string = require("./bp-page-content.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = PageContentCtrl;
}

export class PageContentCtrl {
    private _subscribers: Rx.IDisposable[];
    private currentArtifact: IStatefulArtifact;

    public static $inject: [string] = [
        "dialogService",
        "artifactManager",
        "mainbreadcrumbService",
        "$state",
        "projectManager",
        "localization"
    ];

    constructor(private dialogService: IDialogService,
                private artifactManager: IArtifactManager,
                private mainBreadcrumbService: IMainBreadcrumbService,
                private $state: ng.ui.IStateService,
                private projectManager: IProjectManager,
                private localization: ILocalizationService) {
    }

    public $onInit() {
        const selectionObservable = this.artifactManager.selection.selectionObservable
            .distinctUntilChanged()
            .subscribe(this.onSelectionChanged);

        this._subscribers = [selectionObservable];
    }

    public openProductTour(evt?: ng.IAngularEvent) {
        if (evt) {
            evt.preventDefault();
        }
        this.dialogService.open(<IDialogSettings>{
            template: require("../../../main/components/dialogs/bp-tour/bp-tour.html"),
            controller: BPTourController,
            backdrop: true,
            css: "nova-tour"
        });
    }

    public isMainState(): boolean {
        return this.$state.current.name === "main";
    }

    public openProject(): void {
        this.projectManager.openProjectWithDialog();
    }

    private onSelectionChanged = (selection: ISelection) => {
        // When selection is empty we need to remove breascrumb
        if (!selection.artifact && !selection.subArtifact) {
            this.currentArtifact = null;
            this.mainBreadcrumbService.breadcrumbLinks = [];
            return;
        }
        if (this.currentArtifact === selection.artifact) {
            return;
        }
        // When the selected artifact is subartifact inside UseCase diagram
        const explorerArtifact = this.artifactManager.selection.getExplorerArtifact();
        if (explorerArtifact &&
            explorerArtifact.predefinedType === ItemTypePredefined.UseCaseDiagram &&
            explorerArtifact !== selection.artifact) {
            return;
        }
        this.currentArtifact = selection.artifact;

        this.mainBreadcrumbService.reloadBreadcrumbs(this.currentArtifact);
    };

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => {
            it.dispose();
            return false;
        });
        delete this.currentArtifact;
    }
}
