import {BPButtonAction} from "../../../../shared";
import {IProjectManager} from "../../../../managers/project-manager";
import {IStatefulArtifact, IMetaDataService} from "../../../../managers/artifact-manager";
import {ItemTypePredefined} from "../../../../main/models/enums";
import {ILoadingOverlayService} from "../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {IMainBreadcrumbService} from "../../bp-page-content/mainbreadcrumb.svc";

export class RefreshAction extends BPButtonAction {
    constructor(
        private artifact: IStatefulArtifact,
        private localization: ILocalizationService,
        private projectManager: IProjectManager,
        private loadingOverlayService: ILoadingOverlayService,
        private metaDataService: IMetaDataService,
        private mainBreadcrumbService: IMainBreadcrumbService
    ) {
        super();

        if (!this.artifact) {
            throw new Error("Artifact not provided or is null");
        }

        if (!this.localization) {
            throw new Error("Localization service not provided or is null");
        }

        if (!this.projectManager) {
            throw new Error("Project manager not provided or is null");
        }

        if (!this.loadingOverlayService) {
            throw new Error("Loading overlay service not provided or is null");
        }

        if (!this.metaDataService) {
            throw new Error("MetaData service not provided or is null");
        }

        if (!this.mainBreadcrumbService) {
            throw new Error("Main breadcrumb service not provided or is null");
        }
    }

    public get icon(): string {
        return "fonticon2-refresh-line";
    }

    public get tooltip(): string {
        return this.localization.get("App_Toolbar_Refresh");
    }

    public get disabled(): boolean {
        const invalidTypes = [
            ItemTypePredefined.Collections,
            ItemTypePredefined.BaselinesAndReviews
        ];

        if (invalidTypes.indexOf(this.artifact.predefinedType) >= 0) {
            return true;
        }

        if (this.artifact.artifactState.dirty) {
            return true;
        }

        return false;
    }

    public execute(): void {
        const overlayId = this.loadingOverlayService.beginLoading();

        if (this.artifact.predefinedType === ItemTypePredefined.Project) {
            this.projectManager.refreshCurrent()
                .finally(() => {
                    this.projectManager.triggerProjectCollectionRefresh();
                    this.loadingOverlayService.endLoading(overlayId);
                });
        } else {
            //project is getting refreshed by listening to selection in bp-page-content
            this.mainBreadcrumbService.reloadBreadcrumbs(this.artifact);
            this.artifact.refresh()
                .finally(() => this.loadingOverlayService.endLoading(overlayId));
        }
    }
}
