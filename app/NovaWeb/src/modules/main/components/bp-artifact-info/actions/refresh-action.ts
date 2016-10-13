import {ILocalizationService} from "../../../../core";
import {ILoadingOverlayService} from "../../../../core/loading-overlay";
import {BPButtonAction} from "../../../../shared";
import {IProjectManager} from "../../../../managers/project-manager";
import {IStatefulArtifact, IMetaDataService} from "../../../../managers/artifact-manager";
import {ItemTypePredefined} from "../../../../main/models/enums";

export class RefreshAction extends BPButtonAction {
    constructor(
        artifact: IStatefulArtifact,
        localization: ILocalizationService,
        projectManager: IProjectManager,
        loadingOverlayService: ILoadingOverlayService,
        metaDataService: IMetaDataService
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
                //loading overlay
                const overlayId = loadingOverlayService.beginLoading();

                artifact.refresh()
                    .then((artif) => {
                        metaDataService.remove(artif.projectId);
                        metaDataService.load(artif.projectId);
                    })
                    .catch((error) => {
                        // We're not interested in the error type.
                        // sometimes this error is created by artifact.load(), which returns the statefulArtifact instead of an error object.
                        const refreshOverlayId = loadingOverlayService.beginLoading();
                        projectManager.refresh(projectManager.getSelectedProject()).finally(() => {
                            projectManager.triggerProjectCollectionRefresh();
                            loadingOverlayService.endLoading(refreshOverlayId);
                        });
                    }).finally(() => {
                        loadingOverlayService.endLoading(overlayId);
                    });
            },
            (): boolean => {
                if (artifact.predefinedType === ItemTypePredefined.Project || artifact.predefinedType === ItemTypePredefined.Collections) {
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
