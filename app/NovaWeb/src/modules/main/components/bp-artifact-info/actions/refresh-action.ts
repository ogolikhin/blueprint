import {ILocalizationService} from "../../../../core";
import {ILoadingOverlayService} from "../../../../core/loading-overlay";
import {BPButtonAction} from "../../../../shared";
import {IProjectManager} from "../../../../managers/project-manager";
import {IArtifactManager} from "../../../../managers/artifact-manager";

export class RefreshAction extends BPButtonAction {
    constructor(
        localization: ILocalizationService,
        private projectManager: IProjectManager,
        private artifactManager: IArtifactManager,
        private loadingOverlayService: ILoadingOverlayService
    ) {
        super(
            (): void => {
                //loading overlay
                const overlayId = this.loadingOverlayService.beginLoading();
                const currentArtifact = this.artifactManager.selection.getArtifact();
                
                currentArtifact.refresh()
                    .catch((error) => {
                        // We're not interested in the error type.
                        // sometimes this error is created by artifact.load(), which returns the statefulArtifact instead of an error object.
                        this.projectManager.refresh(this.projectManager.getSelectedProject());
                    }).finally(() => {
                        this.loadingOverlayService.endLoading(overlayId);
                    });
            },
            (): boolean => {
                const currentArtifact = this.artifactManager.selection.getArtifact();
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