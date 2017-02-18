import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {Enums} from "../../../main/models";
import {ItemTypePredefined} from "../../../main/models/itemTypePredefined.enum";
import {BPButtonAction} from "../../../shared";
import {IDialogService} from "../../../shared";
import {Helper} from "../../../shared/utils/helper";
import {IStatefulCollectionArtifact} from "../../configuration/classes/collection-artifact";
import {AnalyticsCategories, AnalyticsActions} from "../../../main/components/analytics";
import {IExtendedAnalyticsService} from "../../../main/components/analytics/analytics";

export class RapidReviewAction extends BPButtonAction {
    constructor(
        private artifact: IStatefulCollectionArtifact,
        private localization: ILocalizationService,
        private dialogService: IDialogService,
        private analyticsService: IExtendedAnalyticsService
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
            || this.artifact.artifacts.length === 0
            || !this.hasRequiredPermissions(this.artifact);
    }

    protected hasRequiredPermissions(artifact: IStatefulCollectionArtifact): boolean {
        return Helper.hasDesiredPermissions(artifact, Enums.RolePermissions.CreateRapidReview);
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
                .then(() => this.artifact.publish())
                .then(() => this.openRapidReview(this.artifact.id, this.getCollectionArtifactCount()));
        } else {
            this.openRapidReview(this.artifact.id, this.getCollectionArtifactCount());
        }

    }

    private getCollectionArtifactCount(): number {
        if (this.artifact && this.artifact.artifacts) {
            return this.artifact.artifacts.length;
        }
        return 0;
    }

    private openRapidReview(id: number, artifactCount: number): void {
        const startTime = new Date().getTime();
        const url = `Web/#/RapidReview/${id}/edit`;
        window.open(url);
        this.analyticsService.trackAnalyticsTemporalEvent(startTime,
            AnalyticsCategories.rapidReview, AnalyticsActions.rapidReviewCreate,
            undefined, {metric1: artifactCount});
    }
}
