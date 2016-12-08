import {IApplicationError} from "../../../../core/error/applicationerror";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IMessageService} from "../../../../core/messages/message.svc";
import {Message, MessageType} from "../../../../core/messages/message";
import {Models, Enums} from "../../../../main/models";
import {BPButtonAction, IDialogSettings, IDialogService} from "../../../../shared";
import {IStatefulArtifact, IArtifactManager} from "../../../../managers/artifact-manager";
import {IProjectManager} from "../../../../managers/project-manager";
import {ItemTypePredefined} from "../../../../main/models/enums";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {ConfirmDeleteController} from "../../../../main/components/dialogs/bp-confirm-delete";
import {INavigationService} from "../../../../core/navigation/navigation.svc";


export class DeleteAction extends BPButtonAction {
    constructor(
        private artifact: IStatefulArtifact,
        protected localization: ILocalizationService,
        private messageService: IMessageService,
        private artifactManager: IArtifactManager,
        private projectManager: IProjectManager,
        private loadingOverlayService: ILoadingOverlayService,
        private dialogService: IDialogService,
        private navigationService: INavigationService
    ) {
        super();

        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }

        if (!projectManager) {
            throw new Error("Project manager not provided or is null");
        }

        if (!dialogService) {
            throw new Error("Dialog service not provided or is null");
        }

        this._tooltip = this.localization.get("App_Toolbar_Delete");
    }

    public get icon(): string {
        return "fonticon fonticon2-delete";
    }

    public get tooltip(): string {
        return this._tooltip;
    }

    public get disabled(): boolean {
        return !this.canDelete();
    }

    public get execute() {
        return this.delete;
    }

    protected canDelete() {
        if (!this.artifact) {
            return false;
        }

        const invalidTypes = [
            ItemTypePredefined.Project,
            ItemTypePredefined.Collections
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

    protected hasRequiredPermissions(): boolean {
        return this.hasDesiredPermissions(Enums.RolePermissions.Delete);
    }

    protected hasDesiredPermissions(permissions: Enums.RolePermissions): boolean {
        if ((this.artifact.permissions & permissions) !== permissions) {
            return false;
        }
        return true;
    }

    protected delete() {
        const overlayId: number = this.loadingOverlayService.beginLoading();

        this.projectManager.getDescendantsToBeDeleted(this.artifact).then((descendants: Models.IArtifactWithProject[]) => {
            this.loadingOverlayService.endLoading(overlayId);

            this.dialogService.open(<IDialogSettings>{
                okButton: this.localization.get("App_Button_Delete"),
                cancelButton: this.localization.get("App_Button_Cancel"),
                message: this.localization.get(descendants.length ?
                    "Delete_Artifact_Confirmation_All_Descendants" : "Delete_Artifact_Confirmation_Single"),
                template: require("../../../../main/components/dialogs/bp-confirm-delete/bp-confirm-delete.html"),
                controller: ConfirmDeleteController,
                css: "nova-publish modal-alert",
                header: this.localization.get("App_DialogTitle_Alert")
            }, descendants).then(() => {
                const deleteOverlayId = this.loadingOverlayService.beginLoading();
                this.artifact.delete().then((deletedArtifacts: Models.IArtifact[]) => {
                    this.complete(deletedArtifacts);
                }).catch((error: IApplicationError) => {
                    if (!error.handled) {
                        this.messageService.addError(error);
                    }
                }).finally(() => {
                    this.loadingOverlayService.endLoading(deleteOverlayId);
                });

            });
        }).catch((error: IApplicationError) => {
            this.loadingOverlayService.endLoading(overlayId);
            if (!error.handled) {
                this.messageService.addError(error);
            }
        });
    };

    private complete(deletedArtifacts: Models.IArtifact[]) {
        const parentArtifact = this.artifactManager.get(this.artifact.parentId);
        if (parentArtifact) {
            this.projectManager.refresh(parentArtifact.projectId, null, true).then(() => {
                this.projectManager.triggerProjectCollectionRefresh();
                this.navigationService.navigateTo({id: parentArtifact.id});

            });
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
