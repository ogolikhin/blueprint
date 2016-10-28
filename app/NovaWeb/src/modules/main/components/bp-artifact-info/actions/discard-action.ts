import {BPButtonAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {ILocalizationService, IMessageService} from "../../../../core";
import {ItemTypePredefined} from "../../../../main/models/enums";
import {ILoadingOverlayService} from "../../../../core/loading-overlay";

export class DiscardAction extends BPButtonAction {
    constructor(artifact: IStatefulArtifact,
                localization: ILocalizationService,
                messageService: IMessageService,
                loadingOverlayService: ILoadingOverlayService) {
        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }

        super(
            (): void => {
                let overlayId: number = loadingOverlayService.beginLoading();

                try {
                    artifact.discard()
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
            (): boolean => artifact ? artifact.canBePublished() : false,
            "fonticon2-discard-line",
            localization.get("App_Toolbar_Discard")
        );
    }
}
