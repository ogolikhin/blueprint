import {ILoadingOverlayService} from "../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {INavigationService} from "../../../../commonModule/navigation/navigation.service";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {IProjectManager} from "../../../../managers/project-manager";
import {ISelectionManager} from "../../../../managers/selection-manager/selection-manager";
import {BPButtonAction, IDialogService, IDialogSettings} from "../../../../shared";
import {IApplicationError} from "../../../../shell/error/applicationError";
import {RolePermissions} from "../../../models/enums";
import {ItemTypePredefined} from "../../../models/itemTypePredefined.enum";
import {IArtifact, IArtifactWithProject} from "../../../models/models";
import {ConfirmDeleteController} from "../../dialogs/bp-confirm-delete/bp-confirm-delete";
import {Message, MessageType} from "../../messages/message";
import {IMessageService} from "../../messages/message.svc";

export class DeleteAction extends BPButtonAction {
    constructor(
        private artifact: IStatefulArtifact,
        protected localization: ILocalizationService,
        private messageService: IMessageService,
        private selectionManager: ISelectionManager,
        private projectManager: IProjectManager,
        private loadingOverlayService: ILoadingOverlayService,
        private dialogService: IDialogService,
        private navigationService: INavigationService
    ) {
        super();

        if (!this.localization) {
            throw new Error("Localization service not provided or is null");
        }

        if (!this.messageService) {
            throw new Error("Message service not provided or is null");
        }

        if (!this.selectionManager) {
            throw new Error("Selection manager not provided or is null");
        }

        if (!this.projectManager) {
            throw new Error("Project manager not provided or is null");
        }

        if (!this.loadingOverlayService) {
            throw new Error("Loading overlay service not provided or is null");
        }

        if (!this.dialogService) {
            throw new Error("Dialog service not provided or is null");
        }

        if (!this.navigationService) {
            throw new Error("Navigation service not provided or is null");
        }
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

        this.projectManager.getDescendantsToBeDeleted(this.artifact)
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
            this.navigationService.navigateTo({id: this.artifact.parentId})
                .then(() => this.projectManager.refresh(this.artifact.projectId, this.artifact.parentId, true))
                .then(() => this.projectManager.triggerProjectCollectionRefresh());
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
