import {BPButtonAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {ItemTypePredefined, PropertyTypePredefined} from "../../../models/enums";
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

    private executeInternal() {
        let overlayId: number = this.loadingOverlayService.beginLoading();
        try {
            this.artifact.save().then(() => {
                this.messageService.addInfo("App_Save_Artifact_Error_200");
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

    public execute(): void {
        if (this.artifact.predefinedType === ItemTypePredefined.Process) {
            this.artifact.metadata.getProcessSubArtifactPropertyTypes().then((subArtifactsPropertyTypes) => {
                if (subArtifactsPropertyTypes.filter(a => 
                    (a.isRequired && a.propertyTypePredefined !== PropertyTypePredefined.Name) || a.isValidated)
                    .length > 0) {
                    let dialogSettings = <IDialogSettings>{
                        type: DialogTypeEnum.Confirm,
                        header: this.localization.get("App_DialogTitle_Confirmation"),
                        message: this.localization.get("App_Possible_SubArtifact_Validation_Error"),
                        okButton: "Ok",
                        cancelButton: null,
                        css: "nova-messaging"
                    };
                    this.dialogService.open(dialogSettings).then(() => {
                        this.executeInternal();
                    });
                } else {
                    this.executeInternal();
                }
            });
        } else {
            this.executeInternal();
        }
    }
}
