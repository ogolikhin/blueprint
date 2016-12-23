import {BPButtonAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {ItemTypePredefined} from "../../../../main/models/enums";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";

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
                .then(() => this.messageService.addInfo("App_Save_Artifact_Error_200"))
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
