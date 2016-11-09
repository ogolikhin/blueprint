import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../../main/components/bp-artifact-picker";
import {IDialogService, IDialogSettings} from "../../../shared/widgets/bp-dialog/bp-dialog";
import {IArtifactManager, ISelection, IArtifactService} from "../../../managers/artifact-manager";
import {IStatefulArtifact} from "../../../managers/artifact-manager/artifact";
import {Models} from "../../../main/models";
import {IBreadcrumbLink} from "../../../shared/widgets/bp-breadcrumb/breadcrumb-link";
import {INavigationService} from "../../../core/navigation/navigation.svc";
import {ItemTypePredefined} from "../../../main/models/enums";
import {IProjectService} from "../../../managers/project-manager/project-service";

export class PageContent implements ng.IComponentOptions {
    public template: string = require("./bp-page-content.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = PageContentCtrl;
}

class PageContentCtrl {
    private _subscribers: Rx.IDisposable[];
    private currentArtifact: IStatefulArtifact;
    public breadcrumbLinks: IBreadcrumbLink[];

    public static $inject: [string] = [
        "dialogService",
        "artifactManager",
        "artifactService",
        "navigationService",
        "projectService"
    ];

    constructor(private dialogService: IDialogService,
        private artifactManager: IArtifactManager,
        private artifactService: IArtifactService,
        protected navigationService: INavigationService,
        private projectService: IProjectService) {
        this.breadcrumbLinks = [];
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
            this.breadcrumbLinks = [];
            return;
        }
        // When the selected artifact is subartifact inside UseCase diagram
        if (this.artifactManager.selection.getExplorerArtifact() !== selection.artifact ||
            this.currentArtifact === selection.artifact) {
            return;
        }
        this.currentArtifact = selection.artifact;
        //For project we need to call GetProjectNavigationPath
        if (selection.artifact.predefinedType === ItemTypePredefined.Project) {
            this._subscribers.push(
                selection.artifact.getObservable().distinctUntilChanged().subscribe((project) => {
                    this.setProjectBreadCrumb(project.id);
                }));
        } else {
            this._subscribers.push(
                selection.artifact.getObservable().subscribe((artifact) => {
                    this.setArtifactBreadCrumb(artifact.id, artifact.artifactState.historical);
                }));
        }
    }

    private setProjectBreadCrumb = (projectId: number): void => {
        this.projectService.getProjectNavigationPath(projectId, false)
            .then((result: string[]) => {
                this.breadcrumbLinks = [];
                _.each(result, s => {
                    const breadcrumbLink: IBreadcrumbLink = {
                        // We do not need to navigate to Instance Folder
                        id: 0,
                        name: s,
                        isEnabled: false
                    };
                    this.breadcrumbLinks.push(breadcrumbLink);
                });
            });
    }

    private setArtifactBreadCrumb = (artifactId: number, isHistorical: boolean): void => {
        this.artifactService.getArtifactNavigationPath(artifactId)
            .then((result: Models.IArtifact[]) => {
                this.breadcrumbLinks = [];
                _.each(result, artifact => {
                    const breadcrumbLink: IBreadcrumbLink = {
                        id: artifact.id,
                        name: artifact.name,
                        isEnabled: !isHistorical
                    };
                    this.breadcrumbLinks.push(breadcrumbLink);
                });
            });
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => {
            it.dispose();
            return false;
        });
        delete this.breadcrumbLinks;
        delete this.currentArtifact;
    }

    public navigateTo = (link: IBreadcrumbLink): void => {
        if (!!link && link.isEnabled) {
            this.navigationService.navigateTo({ id: link.id });
        }
    }
}
