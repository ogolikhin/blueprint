import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../../main/components/bp-artifact-picker";
import {IDialogService, IDialogSettings} from "../../../shared/widgets/bp-dialog/bp-dialog";
import {IArtifactManager, ISelection, IArtifactService} from "../../../managers/artifact-manager";
import {Models} from "../../../main/models";
import {IBreadcrumbLink} from "../../../shared/widgets/bp-breadcrumb/breadcrumb-link";
import {INavigationService} from "../../../core/navigation/navigation.svc";

export class PageContent implements ng.IComponentOptions {
    public template: string = require("./bp-page-content.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = PageContentCtrl;
}

class PageContentCtrl {
    private _subscribers: Rx.IDisposable[];
    public breadcrumbLinks: IBreadcrumbLink[];

    public static $inject: [string] = [
        "dialogService",
        "artifactManager",
        "artifactService",
        "navigationService"
    ];

    constructor(private dialogService: IDialogService,
        private artifactManager: IArtifactManager,
        private artifactService: IArtifactService,
        protected navigationService: INavigationService) {
        this.breadcrumbLinks = [];
    }

    public $onInit() {
        const selectionObservable = this.artifactManager.selection.selectionObservable
            .distinctUntilChanged()
            .subscribe(this.onSelectionChanged);

        this._subscribers = [selectionObservable];
    }

    private onSelectionChanged = (selection: ISelection) => {
        if (selection.subArtifact) {
            return;
        }
        if (!selection.artifact && !selection.subArtifact) {
            this.breadcrumbLinks = [];
            return;
        }
        this.artifactService.getArtifactNavigationPath(selection.artifact.id)
            .then((result: Models.IArtifact[]) => {
                this.breadcrumbLinks = [];
                _.each(result, artifact => {
                    const breadcrumbLink: IBreadcrumbLink = {
                        id: artifact.id,
                        name: artifact.name,
                        isEnabled: !selection.artifact.artifactState.historical
                    };
                    this.breadcrumbLinks.push(breadcrumbLink);
                });
            });
    }

    public openArtifactPicker() {
        const dialogSettings = <IDialogSettings>{
            okButton: "Open",
            template: require("../../../main/components/bp-artifact-picker/bp-artifact-picker-dialog.html"),
            controller: ArtifactPickerDialogController,
            css: "nova-open-project",
            header: "Single project Artifact picker"
        };

        const dialogData: IArtifactPickerOptions = {
            showSubArtifacts: false,
            isOneProjectLevel: true
        };

        this.dialogService.open(dialogSettings, dialogData);
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => {
            it.dispose();
            return false;
        });
        delete this.breadcrumbLinks;
    }

    public navigateTo = (link: IBreadcrumbLink): void => {
        if (!!link && link.isEnabled) {
            this.navigationService.navigateTo({ id: link.id });
        }
    }
}
