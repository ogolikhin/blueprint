import {ILocalizationService} from "../../../../core";
import {ILoadingOverlayService} from "../../../../core/loading-overlay";
import {BPButtonAction} from "../../../../shared";
import {IProjectManager} from "../../../../managers/project-manager";
import {IArtifactManager} from "../../../../managers/artifact-manager";

export class RefreshAction extends BPButtonAction {
    constructor(
        localization: ILocalizationService,
        projectManager: IProjectManager,
        artifactManager: IArtifactManager,
        loadingOverlayService: ILoadingOverlayService
    ) {
        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }

        if (!projectManager) {
            throw new Error("Project manager not provided or is null");
        }

        if (!artifactManager) {
            throw new Error("Artifact manager not provided or is null");
        }

        if (!loadingOverlayService) {
            throw new Error("Loading overlay service not provided or is null");
        }

        super(
            (): void => {
                //loading overlay
                const overlayId = loadingOverlayService.beginLoading();
                const currentArtifact = artifactManager.selection.getArtifact();

                currentArtifact.refresh()
                    .catch((error) => {
                        // We're not interested in the error type.
                        // sometimes this error is created by artifact.load(), which returns the statefulArtifact instead of an error object.
                        projectManager.refresh(projectManager.getSelectedProject());
                    }).finally(() => {
                        loadingOverlayService.endLoading(overlayId);
                    });
            },
            (): boolean => {
                const currentArtifact = artifactManager.selection.getArtifact();
                if (!currentArtifact) {
                    return false;
                }

                if (currentArtifact.artifactState.readonly) {
                    return false;
                }

                if (currentArtifact.artifactState.dirty) {
                    return false;
                }

                return true;
            },
            "fonticon2-refresh-line",
            localization.get("App_Toolbar_Refresh")
        );
    }
}