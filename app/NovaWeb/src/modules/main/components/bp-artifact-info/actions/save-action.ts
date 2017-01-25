import {BPButtonAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {ItemTypePredefined} from "../../../models/enums";
import {ILoadingOverlayService} from "../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {IMessageService} from "../../messages/message.svc";
import {IDialogService, IDialogSettings} from "../../../../shared/widgets/bp-dialog";
import {DialogTypeEnum} from "../../../../shared/widgets/bp-dialog/bp-dialog";

export class SaveAction extends BPButtonAction {
    constructor(
        private artifact: IStatefulArtifact,
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private loadingOverlayService: ILoadingOverlayService,
        private dialogService: IDialogService
    ) {
        super();

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
        return "fonticon2-save-line";
    }

    public get tooltip(): string {
        return this.localization.get("App_Toolbar_Save");
    }

    public get disabled(): boolean {
        if (!this.artifact) {
            return true;
        }

        const invalidTypes = [
            ItemTypePredefined.Project,
            ItemTypePredefined.Collections
        ];

        if (invalidTypes.indexOf(this.artifact.predefinedType) >= 0) {
            return true;
        }

        if (this.artifact.artifactState.readonly) {
            return true;
        }

        if (!this.artifact.artifactState.dirty) {
            return true;
        }

        return false;
    }

    public execute(): void {
        let overlayId: number = this.loadingOverlayService.beginLoading();

        try {
            this.artifact.save()
                .then(() => {
                    this.messageService.addInfo("App_Save_Artifact_Error_200");
                    if (this.artifact.predefinedType === ItemTypePredefined.Process) {
                        this.artifact.metadata.getProcessSubArtifactPropertyTypes().then((subArtifactsPropertyTypes) => {
                            if (subArtifactsPropertyTypes.filter(a => a.isRequired || a.isValidated)) {
                                let dialogSettings = <IDialogSettings>{
                                    type: DialogTypeEnum.Confirm,
                                    header: this.localization.get("App_DialogTitle_Confirmation"),
                                    message: "There might be some validation errors with sub-artifacts, click validate to see any possible missing data.",
                                    okButton: "Ok",
                                    cancelButton: null,
                                    css: "nova-messaging"
                                };
                                this.dialogService.open(dialogSettings);
                            }
                        });
                    }
                })
                .catch((err) => {
                    if (err) {
                        this.messageService.addError(err);
                    }
                })
                .finally(() => this.loadingOverlayService.endLoading(overlayId));
        } catch (err) {
            this.loadingOverlayService.endLoading(overlayId);

            if (err) {
                this.messageService.addError(err);
                throw err;
            }
        }
    }
}
