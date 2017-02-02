import {BPButtonAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {ILoadingOverlayService} from "../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {INavigationService} from "../../../../commonModule/navigation/navigation.service";
import {IMessageService} from "../../messages/message.svc";
import {IProjectExplorerService} from "../../bp-explorer/project-explorer.service";

export class DiscardAction extends BPButtonAction {
    constructor(
        private artifact: IStatefulArtifact,
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private projectExplorerService: IProjectExplorerService,
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

        if (!this.projectExplorerService) {
            throw new Error("Project explorer service not provided or is null");
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
                if (this.projectExplorerService.projects.length) {
                    this.projectExplorerService.refresh(this.artifact.projectId);
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
