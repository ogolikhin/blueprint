import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {BPButtonAction} from "../../../../shared";
import {ItemTypePredefined} from "../../../models/item-type-predefined";

export class OpenImpactAnalysisAction extends BPButtonAction {
    constructor(
        private artifact: IStatefulArtifact,
        protected localization: ILocalizationService
    ) {
        super();

        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }
    }

    public get icon(): string {
        return "fonticon fonticon2-impact-analysis";
    }

    public get tooltip(): string {
        return this.localization.get("App_Toolbar_Open_Impact_Analysis");
    }

    public get disabled(): boolean {
        return !this.canOpenImpactAnalysis();
    }

    public execute(): void {
        this.openImpactAnalysis();
    }

    protected canOpenImpactAnalysis(): boolean {
        if (!this.artifact) {
            return false;
        }

        if (!this.artifact.predefinedType) {
            return false;
        }

        if (this.artifact.artifactState.historical) {
            return false;
        }

        const invalidTypes = [
            ItemTypePredefined.Project,
            ItemTypePredefined.Collections,
            ItemTypePredefined.BaselinesAndReviews,
            ItemTypePredefined.BaselineFolder,
            ItemTypePredefined.ArtifactBaseline,
            ItemTypePredefined.ArtifactReviewPackage,
            ItemTypePredefined.CollectionFolder,
            ItemTypePredefined.ArtifactCollection
        ];

        if (invalidTypes.indexOf(this.artifact.predefinedType) >= 0) {
            return false;
        }

        if (this.artifact.version <= 0) {
            return false;
        }

        return true;
    }

    protected openImpactAnalysis(): void {
        this.openImpactAnalysisInternal(this.artifact.id);
    }

    protected openImpactAnalysisInternal(id: number): void {
        let url = `Web/#/ImpactAnalysis/${id}`;
        window.open(url);
    }
}
