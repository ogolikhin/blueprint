import {BPButtonAction} from "../../../../shared";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IArtifact} from "../../../models/models";
import {IPublishService} from "../../../../managers/artifact-manager/publish.svc/publish.svc";
import {IProjectManager} from "../../../../managers/project-manager/project-manager";
import {INavigationService} from "../../../../core/navigation/navigation.svc";

export class DiscardArtifactsAction extends BPButtonAction {
    private artifactList: IArtifact[];

    constructor(publishService: IPublishService,
                localization: ILocalizationService,
                messageService: IMessageService,
                loadingOverlayService: ILoadingOverlayService,
                projectManager: IProjectManager,
                navigationService: INavigationService) {
        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }

        super(
            // execute
            (): void => {
                const overlayId: number = loadingOverlayService.beginLoading();
                const artifactIds = this.artifactList.map(artifact => artifact.id);

                publishService.discardArtifacts(artifactIds)
                    .then(() => {
                        if (projectManager.projectCollection.getValue().length > 0) {
                            projectManager.refreshAll();
                        }
                    })
                    .catch(error => {
                        messageService.addError(error);
                    })
                    .finally(() => {
                        loadingOverlayService.endLoading(overlayId);
                        navigationService.reloadCurrentState();
                    });
            },

            // canExecute
            (): boolean => this.artifactList && this.artifactList.length > 0,

            // icon
            "fonticon2-discard-line",
            localization.get("App_Toolbar_Discard")
        );
    }

    public updateList(artifactList: IArtifact[]) {
        this.artifactList = artifactList;
    }
}
