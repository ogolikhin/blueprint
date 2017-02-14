import {BPButtonAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {INavigationService} from "../../../../commonModule/navigation/navigation.service";
import {IMessageService} from "../../messages/message.svc";
import {IProjectExplorerService} from "../../bp-explorer/project-explorer.service";
import {IArtifact} from "../../../models/models";

export class DiscardAction extends BPButtonAction {
    constructor(
        private artifact: IStatefulArtifact,
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private projectExplorerService: IProjectExplorerService,
        private navigationService: INavigationService
    ) {
        super();
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
                const hasEverBeenPublished = this.artifact.version !== -1;
                if (this.projectExplorerService.projects.length) {
                    if (hasEverBeenPublished) {
                        this.projectExplorerService.refresh(this.artifact.projectId, this.artifact);
                    } else {
                        const parentArtifact: IArtifact = {
                            id: this.artifact.parentId,
                            projectId: this.artifact.projectId
                        };
                        this.navigationService.navigateTo({id: parentArtifact.id}).then(() => {
                            this.projectExplorerService.refresh(this.artifact.projectId, parentArtifact);
                        });
                    }
                } else {
                    if (hasEverBeenPublished) {
                        this.artifact.refresh();
                    } else {
                        this.navigationService.navigateToMain(true);
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
