import {BPButtonAction} from "../../../../shared";
import {IProjectManager} from "../../../../managers/project-manager";
import {IStatefulArtifact, IMetaDataService} from "../../../../managers/artifact-manager";
import {ItemTypePredefined} from "../../../../main/models/enums";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IMainBreadcrumbService} from "../../bp-page-content/mainbreadcrumb.svc";

export class RefreshAction extends BPButtonAction {
    constructor(artifact: IStatefulArtifact,
                localization: ILocalizationService,
                projectManager: IProjectManager,
                loadingOverlayService: ILoadingOverlayService,
                metaDataService: IMetaDataService,
                mainBreadcrumbService: IMainBreadcrumbService
                ) {
        if (!artifact) {
            throw new Error("Artifact not provided or is null");
        }

        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }

        if (!projectManager) {
            throw new Error("Project manager not provided or is null");
        }

        if (!loadingOverlayService) {
            throw new Error("Loading overlay service not provided or is null");
        }

        if (!metaDataService) {
            throw new Error("MetaData service not provided or is null");
        }

        super(
            (): void => {
                const overlayId = loadingOverlayService.beginLoading();

                if (artifact.predefinedType === ItemTypePredefined.Project) {
                    projectManager.refreshCurrent().finally(() => {
                        projectManager.triggerProjectCollectionRefresh();
                        loadingOverlayService.endLoading(overlayId);
                    });
                } else {
                    //project is getting refreshed in project-manager doRefresh part
                    mainBreadcrumbService.reloadBreadcrumbs(artifact);
                    artifact.refresh().finally(() => {
                        loadingOverlayService.endLoading(overlayId);
                    });
                }
            },
            (): boolean => {

                const invalidTypes = [
                    ItemTypePredefined.Collections
                ];
                if (invalidTypes.indexOf(artifact.predefinedType) >= 0) {
                    return false;
                }

                if (artifact.artifactState.dirty) {
                    return false;
                }

                return true;
            },
            "fonticon2-refresh-line",
            localization.get("App_Toolbar_Refresh")
        );
    }
}
