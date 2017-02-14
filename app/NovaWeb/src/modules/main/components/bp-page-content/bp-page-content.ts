import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {ISelection} from "../../../managers/artifact-manager";
import {IStatefulArtifact} from "../../../managers/artifact-manager/artifact/artifact";
import {IProjectManager} from "../../../managers/project-manager";
import {ISelectionManager} from "../../../managers/selection-manager/selection-manager";
import {IDialogService, IDialogSettings} from "../../../shared/widgets/bp-dialog/bp-dialog";
import {ItemTypePredefined} from "../../models/itemTypePredefined.enum";
import {BPTourController} from "../dialogs/bp-tour/bp-tour";
import {IMainBreadcrumbService} from "./mainbreadcrumb.svc";

export class PageContent implements ng.IComponentOptions {
    public template: string = require("./bp-page-content.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = PageContentCtrl;
}

export class PageContentCtrl {
    private _subscribers: Rx.IDisposable[];
    private currentArtifact: IStatefulArtifact;

    public static $inject: [string] = [
        "dialogService",
        "selectionManager",
        "mainbreadcrumbService",
        "$state",
        "projectManager",
        "localization"
    ];

    constructor(private dialogService: IDialogService,
                private selectionManager: ISelectionManager,
                private mainBreadcrumbService: IMainBreadcrumbService,
                private $state: ng.ui.IStateService,
                private projectManager: IProjectManager,
                private localization: ILocalizationService) {
    }

    public $onInit() {
        const selectionObservable = this.selectionManager.selectionObservable
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
        const explorerArtifact = this.selectionManager.getExplorerArtifact();
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
