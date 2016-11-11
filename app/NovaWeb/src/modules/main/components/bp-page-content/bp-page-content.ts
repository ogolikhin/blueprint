import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../../main/components/bp-artifact-picker";
import {IDialogService, IDialogSettings} from "../../../shared/widgets/bp-dialog/bp-dialog";
import {IArtifactManager, ISelection} from "../../../managers/artifact-manager";
import {IStatefulArtifact} from "../../../managers/artifact-manager/artifact";
import {IBreadcrumbLink} from "../../../shared/widgets/bp-breadcrumb/breadcrumb-link";
import {INavigationService} from "../../../core/navigation/navigation.svc";
import {ItemTypePredefined} from "../../../main/models/enums";
import {IMainBreadcrumbService} from "./mainbreadcrumb.svc";

export class PageContent implements ng.IComponentOptions {
    public template: string = require("./bp-page-content.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = PageContentCtrl;
}

class PageContentCtrl {
    private _subscribers: Rx.IDisposable[];
    private currentArtifact: IStatefulArtifact;

    public static $inject: [string] = [
        "dialogService",
        "artifactManager",
        "navigationService",
        "mainbreadcrumbService"
    ];

    constructor(private dialogService: IDialogService,
        private artifactManager: IArtifactManager,
        private navigationService: INavigationService,
        private mainBreadcrumbService: IMainBreadcrumbService) {
    }

    public $onInit() {
        const selectionObservable = this.artifactManager.selection.selectionObservable
            .distinctUntilChanged()
            .subscribe(this.onSelectionChanged);

        this._subscribers = [selectionObservable];
    }

    private onSelectionChanged = (selection: ISelection) => {
        // When selection is empty we need to remove breascrumb
        if (!selection.artifact && !selection.subArtifact) {
            this.currentArtifact = null;
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
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => {
            it.dispose();
            return false;
        });
        delete this.currentArtifact;
    }

    public navigateTo = (link: IBreadcrumbLink): void => {
        if (!!link && link.isEnabled) {
            this.navigationService.navigateTo({ id: link.id });
        }
    }
}
