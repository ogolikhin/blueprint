import {BPButtonAction} from "../../../../shared";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IArtifact, IPublishResultSet} from "../../../models/models";
import {IProjectManager} from "../../../../managers/project-manager/project-manager";
import {IUnpublishedArtifactsService} from "../../../../editors/unpublished/unpublished.svc";

export class DiscardArtifactsAction extends BPButtonAction {
    private artifactList: IArtifact[];

    constructor(
        private publishService: IUnpublishedArtifactsService,
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private loadingOverlayService: ILoadingOverlayService,
        private projectManager: IProjectManager
    ) {
        super();

        if (!this.publishService) {
            throw new Error("Publish service not provided or is null");
        }

        if (!this.localization) {
            throw new Error("Localization service not provided or is null");
        }

        if (!this.messageService) {
            throw new Error("Message service not provided or is null");
        }

        if (!this.loadingOverlayService) {
            throw new Error("Loading overlay service not provided or is null");
        }

        if (!this.projectManager) {
            throw new Error("Project manager not provided or is null");
        }
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
        const overlayId: number = this.loadingOverlayService.beginLoading();
        const artifactIds = this.artifactList.map(artifact => artifact.id);

        this.publishService.discardArtifacts(artifactIds)
            .then((result: IPublishResultSet) => {
                this.messageService.addInfo("Discard_All_Success_Message", result.artifacts.length);

                if (this.projectManager.projectCollection.getValue().length > 0) {
                    this.projectManager.refreshAll();
                }
            })
            .catch((error) => {
                this.publishService.getUnpublishedArtifacts();
                this.messageService.addError(error);
            })
            .finally(() => this.loadingOverlayService.endLoading(overlayId));
    }

    public updateList(artifactList: IArtifact[]) {
        this.artifactList = artifactList;
    }
}
