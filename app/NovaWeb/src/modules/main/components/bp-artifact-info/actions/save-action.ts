import {BPButtonAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {ILocalizationService, IMessageService} from "../../../../core";
import {ItemTypePredefined} from "../../../../main/models/enums";
import {ILoadingOverlayService} from "../../../../core/loading-overlay";

export class SaveAction extends BPButtonAction {
    constructor(
        artifact: IStatefulArtifact,
        localization: ILocalizationService,
        messageService: IMessageService,
        loadingOverlayService: ILoadingOverlayService
    ) {
        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }

        if (!messageService) {
            throw new Error("Message service not provided or is null");
        }

        if (!loadingOverlayService) {
            throw new Error("Loading overlay service not provided or is null");
        }

        super(
            (): void => {
                let overlayId: number = loadingOverlayService.beginLoading();

                try {
                    artifact.save().finally(() => {
                        loadingOverlayService.endLoading(overlayId);
                    });
                } catch (err) {
                    loadingOverlayService.endLoading(overlayId);

                    if (err) {
                        messageService.addError(err);
                        throw err;
                    }
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
            "fonticon fonticon2-save",
            localization.get("App_Toolbar_Save")
        );
    }
}