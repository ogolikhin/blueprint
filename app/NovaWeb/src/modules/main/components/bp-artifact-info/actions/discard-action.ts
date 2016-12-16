import {BPButtonAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {IProjectManager} from "../../../../managers";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {INavigationService} from "../../../../core/navigation/navigation.svc";

export class DiscardAction extends BPButtonAction {
    constructor(artifact: IStatefulArtifact,
                localization: ILocalizationService,
                messageService: IMessageService,
                projectManager: IProjectManager,
                loadingOverlayService: ILoadingOverlayService,
                navigationService: INavigationService) {
        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }
        super(
            (): void => {
                artifact.discardArtifact()
                .then(() => {
                    if (projectManager.projectCollection.getValue().length > 0) {
                        projectManager.refresh(artifact.projectId).then(() => {
                            projectManager.triggerProjectCollectionRefresh();
                        });
                    } else {
                        // If artifact has never been published, navigate back to the main page;
                        // otherwise, refresh the artifact
                        if (artifact.version === -1) {
                            navigationService.navigateToMain(true);
                        } else {
                            artifact.refresh();
                        }
                    }
                })
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
