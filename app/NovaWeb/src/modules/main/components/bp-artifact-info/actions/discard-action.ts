import {BPButtonAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {IProjectManager} from "../../../../managers";
import {ILoadingOverlayService} from "../../../../core/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../core/localization/localization.service";
import {INavigationService} from "../../../../core/navigation/navigation.service";
import {IMessageService} from "../../messages/message.svc";

export class DiscardAction extends BPButtonAction {
    constructor(
        private artifact: IStatefulArtifact,
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private projectManager: IProjectManager,
        private loadingOverlayService: ILoadingOverlayService,
        private navigationService: INavigationService
    ) {
        super();

        if (!this.localization) {
            throw new Error("Localization service not provided or is null");
        }

        if (!this.messageService) {
            throw new Error("Message service not provided or is null");
        }

        if (!this.projectManager) {
            throw new Error("Project manager not provided or is null");
        }

        if (!this.loadingOverlayService) {
            throw new Error("Loading overlay service not provided or is null");
        }

        if (!this.navigationService) {
            throw new Error("Navigation service not provided or is null");
        }
    }

    public get icon(): string {
        return "fonticon2-discard-line";
    }

    public get tooltip(): string {
        return this.localization.get("App_Toolbar_Discard");
    }

    public get disabled(): boolean {
        return !this.artifact
            || !this.artifact.canBePublished();
    }

    public execute(): void {
        this.artifact.discardArtifact()
            .then(() => {
                if (this.projectManager.projectCollection.getValue().length > 0) {
                    this.projectManager.refresh(this.artifact.projectId)
                        .then(() => this.projectManager.triggerProjectCollectionRefresh());
                } else {
                    // If artifact has never been published, navigate back to the main page;
                    // otherwise, refresh the artifact
                    if (this.artifact.version === -1) {
                        this.navigationService.navigateToMain(true);
                    } else {
                        this.artifact.refresh();
                    }
                }
            })
            .catch((err) => {
                if (err) {
                    this.messageService.addError(err);
                }
            });
    }
}
