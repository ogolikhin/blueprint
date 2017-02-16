import {BPButtonAction} from "../../../../shared";
import {IDialogService} from "../../../../shared";
import {ILoadingOverlayService} from "../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {IArtifact, IPublishResultSet} from "../../../models/models";
import {IUnpublishedArtifactsService} from "../../../../editorsModule/unpublished/unpublished.service";
import {IMessageService} from "../../messages/message.svc";
import {IProjectExplorerService} from "../../bp-explorer/project-explorer.service";

export class DiscardArtifactsAction extends BPButtonAction {
    private artifactList: IArtifact[];

    constructor(
        private publishService: IUnpublishedArtifactsService,
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private loadingOverlayService: ILoadingOverlayService,
        private projectExplorerService: IProjectExplorerService,
        private dialogService: IDialogService
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
        return !this.artifactList
        || !this.artifactList.length;
    }

    public execute(): void {
        const artifactIds = this.artifactList.map(artifact => artifact.id);
        const message = artifactIds.length === 1 ?
        this.localization.get("Discard_Single_Artifact_Confirm") :
        this.localization.get("Discard_Multiple_Artifacts_Confirm").replace("{0}", artifactIds.length.toString());
        this.dialogService.alert(message, "Confirm Discard", "Discard", "Cancel").then(() => {
            const overlayId: number = this.loadingOverlayService.beginLoading();

            this.publishService.discardArtifacts(artifactIds)
            .then((result: IPublishResultSet) => {
                this.messageService.addInfo("Discard_All_Success_Message", result.artifacts.length);

                if (this.projectExplorerService.projects.length > 0) {
                    this.projectExplorerService.refreshAll();
                }
            })
            .catch((error) => {
                this.publishService.getUnpublishedArtifacts();
                this.messageService.addError(error);
            })
            .finally(() => this.loadingOverlayService.endLoading(overlayId));
        });
    }

    public updateList(artifactList: IArtifact[]) {
        this.artifactList = artifactList;
    }
}
