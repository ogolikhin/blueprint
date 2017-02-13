import {IApplicationError} from "../../../../shell/error/applicationError";
import {ILoadingOverlayService} from "../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {INavigationService} from "../../../../commonModule/navigation/navigation.service";
import {ConfirmDeleteController} from "../../dialogs/bp-confirm-delete/bp-confirm-delete";
import {ItemTypePredefined, RolePermissions} from "../../../models/enums";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {BPButtonAction, IDialogService, IDialogSettings} from "../../../../shared";
import {IArtifact, IArtifactWithProject} from "../../../models/models";
import {IMessageService} from "../../messages/message.svc";
import {Message, MessageType} from "../../messages/message";
import {IProjectExplorerService} from "../../bp-explorer/project-explorer.service";

export class DeleteAction extends BPButtonAction {
    constructor(
        private artifact: IStatefulArtifact,
        protected localization: ILocalizationService,
        private messageService: IMessageService,
        private projectExplorerService: IProjectExplorerService,
        private loadingOverlayService: ILoadingOverlayService,
        private dialogService: IDialogService,
        private navigationService: INavigationService
    ) {
        super();
    }

    public get icon(): string {
        return "fonticon fonticon2-delete";
    }

    public get tooltip(): string {
        return this.localization.get("App_Toolbar_Delete");
    }

    public get disabled(): boolean {
        return !this.canDelete();
    }

    public execute(): void {
        this.delete();
    }

    protected canDelete(): boolean {
        if (!this.artifact) {
            return false;
        }

        const invalidTypes = [
            ItemTypePredefined.Project,
            ItemTypePredefined.Collections,
            ItemTypePredefined.BaselinesAndReviews
        ];

        if (invalidTypes.indexOf(this.artifact.itemTypeId) >= 0) {
            return false;
        }

        if (this.artifact.artifactState.readonly) {
            return false;
        }

        if (!this.hasRequiredPermissions()) {
            return false;
        }

        return true;
    }

    protected delete(): void {
        const overlayId: number = this.loadingOverlayService.beginLoading();

        this.projectExplorerService.getDescendantsToBeDeleted(this.artifact)
            .then((descendants: IArtifactWithProject[]) => {
                this.loadingOverlayService.endLoading(overlayId);

                const settings = <IDialogSettings>{
                    okButton: this.localization.get("App_Button_Delete"),
                    cancelButton: this.localization.get("App_Button_Cancel"),
                    message: this.localization.get(descendants.length ?
                        "Delete_Artifact_Confirmation_All_Descendants" : "Delete_Artifact_Confirmation_Single"),
                    template: require("../../../../main/components/dialogs/bp-confirm-delete/bp-confirm-delete.html"),
                    controller: ConfirmDeleteController,
                    css: "nova-publish modal-alert",
                    header: this.localization.get("App_DialogTitle_Alert")
                };

                this.dialogService.open(settings, descendants)
                    .then(() => {
                        const deleteOverlayId = this.loadingOverlayService.beginLoading();

                        this.artifact.delete()
                        .then((deletedArtifacts: IArtifact[]) => this.complete(deletedArtifacts))
                        .catch((error: IApplicationError) => {
                            if (!error.handled) {
                                this.messageService.addError(error);
                            }
                        })
                        .finally(() => this.loadingOverlayService.endLoading(deleteOverlayId));
                    });
            })
            .catch((error: IApplicationError) => {
                this.loadingOverlayService.endLoading(overlayId);

                if (!error.handled) {
                    this.messageService.addError(error);
                }
            });
    }

    protected hasRequiredPermissions(): boolean {
        return this.hasDesiredPermissions(RolePermissions.Delete);
    }

    protected hasDesiredPermissions(permissions: RolePermissions): boolean {
        return this.artifact
            && ((this.artifact.permissions & permissions) === permissions);
    }

    private complete(deletedArtifacts: IArtifact[]) {
        if (this.artifact.parentId) {
            const parentArtifact = {
                id: this.artifact.parentId,
                projectId: this.artifact.projectId
            } as IArtifact;

            this.projectExplorerService.setSelectionId(parentArtifact.id);
            this.projectExplorerService.refresh(parentArtifact.projectId, parentArtifact);
            this.navigationService.navigateTo({id: parentArtifact.id});

        } else {
            this.artifact.refresh();
        }

        const message = new Message(
            MessageType.Info,
            deletedArtifacts.length > 1 ? "Delete_Artifact_All_Success_Message" : "Delete_Artifact_Single_Success_Message",
            true,
            deletedArtifacts.length);
        message.timeout = 6000;
        this.messageService.addMessage(message);
    }
}
