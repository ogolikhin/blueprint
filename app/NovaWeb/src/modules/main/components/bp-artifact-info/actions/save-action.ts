import {BPButtonAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {ItemTypePredefined} from "../../../../main/models/enums";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";

export class SaveAction extends BPButtonAction {
    constructor(artifact: IStatefulArtifact,
                localization: ILocalizationService,
                messageService: IMessageService,
                loadingOverlayService: ILoadingOverlayService) {
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
                    artifact.save()
                        .then(() => {
                            messageService.addInfo("App_Save_Artifact_Error_200");
                        })
                        .catch((err) => {
                            if (err) {
                                messageService.addError(err);
                            }
                        })
                        .finally(() => {
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
            "fonticon2-save-line",
            localization.get("App_Toolbar_Save")
        );
    }
}
