import {BPButtonAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {PropertyTypePredefined} from "../../../models/enums";
import {ILoadingOverlayService} from "../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {IMessageService} from "../../messages/message.svc";
import {IDialogService, IDialogSettings} from "../../../../shared";
import {DialogTypeEnum} from "../../../../shared/widgets/bp-dialog/bp-dialog";

export class PublishAction extends BPButtonAction {
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
        return "fonticon2-publish-line";
    }

    public get tooltip(): string {
        return this.localization.get("App_Toolbar_Publish");
    }

    public get disabled(): boolean {
        return !this.artifact
            || !this.artifact.canBePublished();
    }

    private executeInternal() {
        let overlayId: number = this.loadingOverlayService.beginLoading();
        try {
            this.artifact.publish().catch((err) => {
                if (err) {
                    this.messageService.addError(err);
                }
            }).finally(() => this.loadingOverlayService.endLoading(overlayId));
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
