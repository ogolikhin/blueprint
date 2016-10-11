import {BPButtonAction} from "../../../../shared";
import {IStatefulArtifact, IArtifactManager} from "../../../../managers/artifact-manager";
import {ILocalizationService, IMessageService} from "../../../../core";
import {ItemTypePredefined} from "../../../../main/models/enums";
import {ILoadingOverlayService} from "../../../../core/loading-overlay";

export class SaveAction extends BPButtonAction {
    constructor(
        artifact: IStatefulArtifact,
        localization: ILocalizationService,
        messageService: IMessageService,
        loadingOverlayService: ILoadingOverlayService,
        artifactManager: IArtifactManager
    ) {
        super(
            (): void => {
                let overlayId: number = loadingOverlayService.beginLoading();
                try {
                    artifactManager.selection.getArtifact().save().finally(() => {
                        loadingOverlayService.endLoading(overlayId);
                    });
                } catch (err) {
                    messageService.addError(err);
                    loadingOverlayService.endLoading(overlayId);
                    throw err;
                }
            },
            (): boolean => {
                if (!artifact) {
                    return false;
                }

                const invalidTypes = [
                    ItemTypePredefined.Project, 
                    ItemTypePredefined.Collections
                ];

                if (invalidTypes.indexOf(artifact.predefinedType) >= 0) {
                    return false;
                }

                if (artifact.artifactState.readonly) {
                    return false;
                }

                if (!artifact.artifactState.dirty) {
                    return false;
                }

                return true;
            },
            "fonticon2-save-line",
            localization.get("App_Toolbar_Save")
        );
    }
}