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
                artifact.discardArtifact()
                .catch((err) => {
                    if (err) {
                        messageService.addError(err);
                    }
                });
            },
            (): boolean => artifact ? artifact.canBePublished() : false,
            "fonticon2-discard-line",
            localization.get("App_Toolbar_Discard")
        );
    }
}
