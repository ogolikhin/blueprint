import {BPButtonAction} from "../../../../shared";
import {ILoadingOverlayService} from "../../../../core/loadingOverlay/loadingOverlay.service";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localization.service";
import {IArtifact, IPublishResultSet} from "../../../models/models";
import {IUnpublishedArtifactsService} from "../../../../editors/unpublished/unpublished.svc";
import {IDialogService, IDialogSettings} from "../../../../shared";
import {ConfirmPublishController, IConfirmPublishDialogData} from "../../../../main/components/dialogs/bp-confirm-publish";
import {HttpStatusCode} from "../../../../core/httpInterceptor/http-status-code";

export class PublishArtifactsAction extends BPButtonAction {
    private artifactList: IArtifact[];

    constructor(
        private publishService: IUnpublishedArtifactsService,
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private loadingOverlayService: ILoadingOverlayService,
        private dialogService: IDialogService
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
        if (!this.dialogService) {
            throw new Error("Dialog service not provided or is null");
        }
    }

    public get icon(): string {
        return "fonticon2-publish-line";
    }

    public get tooltip(): string {
        return this.localization.get("App_Toolbar_Publish");
    }

    public get disabled(): boolean {
        return !this.artifactList || !this.artifactList.length;
    }

    public execute(): void {
        const overlayId: number = this.loadingOverlayService.beginLoading();
        const artifactIds = this.artifactList.map(artifact => artifact.id);

        this.publishService.publishArtifacts(artifactIds)
            .then((result: IPublishResultSet) => {
                this.messageService.addInfo("Publish_All_Success_Message", result.artifacts.length);
            })
            .catch((error) => {
                if (error && error.statusCode === HttpStatusCode.Conflict) {
                    if (error.errorContent) {
                        this.publishDependents(error.errorContent);
                    }
                } else {
                    this.publishService.getUnpublishedArtifacts();
                    this.messageService.addError(error);
                }
            })
            .finally(() => this.loadingOverlayService.endLoading(overlayId));
    }

    public updateList(artifactList: IArtifact[]) {
        this.artifactList = artifactList;
    }

    private publishDependents(dependents: IPublishResultSet) {
        this.dialogService.open(<IDialogSettings>{
                okButton: this.localization.get("App_Button_Publish"),
                cancelButton: this.localization.get("App_Button_Cancel"),
                message: this.localization.get("Publish_Dependents_Dialog_Message"),
                template: require("../../../../main/components/dialogs/bp-confirm-publish/bp-confirm-publish.html"),
                controller: ConfirmPublishController,
            css: "nova-publish",
            header: this.localization.get("App_DialogTitle_Confirmation")
            },
            <IConfirmPublishDialogData>{
                artifactList: dependents.artifacts,
                projectList: dependents.projects,
                selectedProject: null
            })
            .then(() => {
                let publishOverlayId = this.loadingOverlayService.beginLoading();
                this.publishService.publishArtifacts(dependents.artifacts.map((d: IArtifact) => d.id))
                    .then(() => {
                        this.messageService.addInfo("Publish_Success_Message");
                    })
                    .catch((err) => {
                        this.messageService.addError(err);
                    }).finally(() => {
                    this.loadingOverlayService.endLoading(publishOverlayId);
                });
            });
    }

}
