import {BPButtonAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {ILocalizationService} from "../../../../core";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../../core/messages/message.svc";

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
