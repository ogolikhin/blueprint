import {BPButtonAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {ItemTypePredefined} from "../../../../main/models/enums";
import {ILocalizationService} from "../../../../core/localization/localizationService";

export class OpenImpactAnalysisAction extends BPButtonAction {
    constructor(artifact: IStatefulArtifact,
                localization: ILocalizationService) {
        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }

        super(
            () => {
                let url = `Web/#/ImpactAnalysis/${artifact.id}`;
                window.open(url);
            },
            () => {
                if (!artifact) {
                    return false;
                }

                if (!artifact.predefinedType) {
                    return false;
                }

                if (artifact.artifactState.deleted) {
                    return false;
                }

                const invalidTypes = [
                    ItemTypePredefined.Project,
                    ItemTypePredefined.ArtifactCollection,
                    ItemTypePredefined.Collections,
                    ItemTypePredefined.CollectionFolder,
                    ItemTypePredefined.ArtifactBaseline,
                    ItemTypePredefined.BaselineFolder,
                    ItemTypePredefined.Baseline,
                    ItemTypePredefined.ArtifactReviewPackage
                ];

                if (invalidTypes.indexOf(artifact.predefinedType) >= 0) {
                    return false;
                }

                if (artifact.version <= 0) {
                    return false;
                }

                return true;
            },
            "fonticon fonticon2-impact-analysis",
            localization.get("App_Toolbar_Open_Impact_Analysis")
        );
    }
}
