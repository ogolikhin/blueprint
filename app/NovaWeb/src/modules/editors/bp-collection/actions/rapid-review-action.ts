import {BPButtonAction} from "../../../shared";
import {ItemTypePredefined} from "../../../main/models/enums";
import {IStatefulCollectionArtifact} from "../collection-artifact";
import {ILocalizationService} from "../../../core/localization/localizationService";
import {IDialogService} from "../../../shared";

export class RapidReviewAction extends BPButtonAction {
    constructor(
        private artifact: IStatefulCollectionArtifact,
        private localization: ILocalizationService,
        private dialogService: IDialogService
    ) {
        super();

        if (!this.localization) {
            throw new Error("Localization service not provided or is null");
        }

        if (!this.dialogService) {
            throw new Error("Dialog service not provided or is null");
        }
    }

    public get icon(): string {
        return "fonticon fonticon2-rapid-review";
    }

    public get tooltip(): string {
        return this.localization.get("Create_Rapid_Review", "Create Rapid Review");
    }

    public get disabled(): boolean {
        return !this.artifact 
            || this.artifact.predefinedType !== ItemTypePredefined.ArtifactCollection
            || this.artifact.artifactState.readonly
            || this.artifact.rapidReviewCreated
            || !this.artifact.artifacts
            || this.artifact.artifacts.length === 0;
    }

    public execute(): void {
        if (this.disabled) {
            return;
        }

        if (!this.artifact.artifactState.published) {
            const message = this.localization.get(
                "Confirm_Publish_Collection", 
                "Please publish your changes before entering the review. Would you like to proceed?"
            );

            this.dialogService.confirm(message)
                .then( () => this.artifact.publish())
                .then(() => this.openRapidReview(this.artifact.id));
        } else {
            this.openRapidReview(this.artifact.id);
        }
    }

    private openRapidReview(id: number): void {
        const url = `Web/#/RapidReview/${id}/edit`;
        window.open(url);
    }
}
