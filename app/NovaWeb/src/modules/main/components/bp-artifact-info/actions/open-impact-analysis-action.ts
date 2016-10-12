import {BPButtonAction} from "../../../../shared";
import {Helper} from "../../../../shared/utils/helper";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {ILocalizationService} from "../../../../core";

export class OpenImpactAnalysisAction extends BPButtonAction {
    constructor(
        artifact: IStatefulArtifact,
        localization: ILocalizationService
    ) {
        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }

        super(
            () => {
                let url = `Web/#/ImpactAnalysis/${artifact.id}`;
                window.open(url);
            },
            () => {
                if (!Helper.canUtilityPanelUseSelectedArtifact(artifact)) {
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