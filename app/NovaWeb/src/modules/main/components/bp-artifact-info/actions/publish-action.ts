import {BPButtonAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";

export class PublishAction extends BPButtonAction {
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
                    artifact.publish()
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
            "fonticon2-publish-line",
            localization.get("App_Toolbar_Publish")
        );
    }
}
