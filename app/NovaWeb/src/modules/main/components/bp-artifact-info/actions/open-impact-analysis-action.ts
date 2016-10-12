import {BPButtonAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {ILocalizationService} from "../../../../core";
import {ItemTypePredefined} from "../../../../main/models/enums";

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

                const invalidTypes = [
                    ItemTypePredefined.Project, 
                    ItemTypePredefined.ArtifactCollection, 
                    ItemTypePredefined.Collections, 
                    ItemTypePredefined.CollectionFolder
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
