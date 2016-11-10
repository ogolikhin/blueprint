import {IApplicationError} from "../../../../core/error/applicationerror";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IMessageService} from "../../../../core/messages/message.svc";
import {Models} from "../../../../main/models";
import {BPButtonAction, IDialogSettings, IDialogService} from "../../../../shared";
import {IStatefulArtifact, IArtifactManager} from "../../../../managers/artifact-manager";
import {IProjectManager} from "../../../../managers/project-manager";
import {ItemTypePredefined} from "../../../../main/models/enums";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {ConfirmDeleteController} from "../../../../main/components/dialogs/bp-confirm-delete";


export class DeleteAction extends BPButtonAction {
    constructor(private artifact: IStatefulArtifact,
                private localization: ILocalizationService,
                private messageService: IMessageService,
                private artifactManager: IArtifactManager,
                private projectManager: IProjectManager,
                private loadingOverlayService: ILoadingOverlayService,
                private dialogService: IDialogService
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
    }

    public get icon(): string {
        return "fonticon fonticon2-delete";
    }

    public get tooltip(): string {
        return this.localization.get("App_Toolbar_Delete");
    }

    public get disabled(): boolean {
        return !this.canExecute();
    }

    public get execute()  {
        return this.deleteArtifact;
    }

    private canExecute() {
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

        return true;

    }

    private deleteArtifact() {
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
                const deeleteOverlayId = this.loadingOverlayService.beginLoading();
                this.artifact.delete().then(() => {
                    const artifact = this.artifactManager.get(this.artifact.parentId); 
                    this.artifactManager.selection.setArtifact(artifact);
                    this.projectManager.refresh(this.artifact.projectId, true).then(() => {
                        this.projectManager.triggerProjectCollectionRefresh();
                    });
                    if (descendants.length) {
                        this.messageService.addInfoWithPar("Delete_Artifact_All_Success_Message", [descendants.length]);
                    } else {
                        this.messageService.addInfo("Delete_Artifact_Single_Success_Message");

                    }
                }).catch((error: IApplicationError) => {
                    if (!error.handled) {
                        this.messageService.addError(error);
                    }
                }).finally(() => {
                    this.loadingOverlayService.endLoading(deeleteOverlayId);
                });

            });
        }).catch((error: IApplicationError) => {
            this.loadingOverlayService.endLoading(overlayId);
            if (!error.handled) {
                this.messageService.addError(error);
            }
        });
    };




}
