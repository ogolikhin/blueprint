import {BPButtonAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {ILocalizationService, IMessageService} from "../../../../core";
import {ItemTypePredefined} from "../../../../main/models/enums";
import {ILoadingOverlayService} from "../../../../core/loading-overlay";


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
                    artifact.publish().finally(() => {
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

                return true;
            },
            "fonticon2-publish-line",
            localization.get("App_Toolbar_Publish")
        );
    }
}
