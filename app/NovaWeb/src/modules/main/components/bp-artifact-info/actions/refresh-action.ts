import {BPButtonAction} from "../../../../shared";
import {IStatefulArtifact, IMetaDataService} from "../../../../managers/artifact-manager";
import {ItemTypePredefined} from "../../../models/enums";
import {ILoadingOverlayService} from "../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {IMainBreadcrumbService} from "../../bp-page-content/mainbreadcrumb.svc";
import {IProjectExplorerService} from "../../bp-explorer/project-explorer.service";
import {INavigationService} from "../../../../commonModule/navigation/navigation.service";

export class RefreshAction extends BPButtonAction {
    constructor(
        private artifact: IStatefulArtifact,
        private localization: ILocalizationService,
        private projectExplorerService: IProjectExplorerService,
        private loadingOverlayService: ILoadingOverlayService,
        private metaDataService: IMetaDataService,
        private navigationService: INavigationService,
        private mainBreadcrumbService: IMainBreadcrumbService
    ) {
        super();
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
        if (this.artifact.predefinedType === ItemTypePredefined.Project) {
            this.projectExplorerService.refresh(this.artifact.id).then(() => {
                this.navigationService.reloadCurrentState();
            });

        } else {
            //project is getting refreshed by listening to selection in bp-page-content
            this.mainBreadcrumbService.reloadBreadcrumbs(this.artifact);

            const overlayId = this.loadingOverlayService.beginLoading();
            this.artifact.refresh()
                .finally(() => this.loadingOverlayService.endLoading(overlayId));
        }
    }
}
