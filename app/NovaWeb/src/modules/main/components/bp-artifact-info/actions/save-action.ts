import {ILoadingOverlayService} from "../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {BPButtonAction} from "../../../../shared";
import {ItemTypePredefined} from "../../../models/item-type-predefined";
import {IMessageService} from "../../messages/message.svc";

export class SaveAction extends BPButtonAction {
    constructor(
        private artifact: IStatefulArtifact,
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private loadingOverlayService: ILoadingOverlayService
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
            ItemTypePredefined.Collections,
            ItemTypePredefined.BaselinesAndReviews
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
        this.executeInternal();
    }
}
