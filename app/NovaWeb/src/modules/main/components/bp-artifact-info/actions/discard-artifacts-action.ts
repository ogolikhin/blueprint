import {BPButtonAction} from "../../../../shared";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IArtifact} from "../../../models/models";
import {IProjectManager} from "../../../../managers/project-manager/project-manager";
import {INavigationService} from "../../../../core/navigation/navigation.svc";
import {IUnpublishedArtifactsService} from "../../../../editors/unpublished/unpublished.svc";

export class DiscardArtifactsAction extends BPButtonAction {
    private artifactList: IArtifact[];

    constructor(publishService: IUnpublishedArtifactsService,
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
                        publishService.getUnpublishedArtifacts();
                        messageService.addError(error);
                    })
                    .finally(() => {
                        loadingOverlayService.endLoading(overlayId);
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
