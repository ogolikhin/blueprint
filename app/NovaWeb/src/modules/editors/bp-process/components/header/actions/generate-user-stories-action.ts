import {BPDropdownAction, BPDropdownItemAction} from "../../../../../shared";
import {ILocalizationService} from "../../../../../core";
import {ISelectionManager} from "../../../../../managers/selection-manager";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {StatefulProcessSubArtifact} from "../../../process-subartifact";
import {ProcessShapeType} from "../../../models/enums";

export class GenerateUserStoriesAction extends BPDropdownAction {
    constructor(processArtifact: StatefulProcessArtifact,
                selectionManager: ISelectionManager,
                localization: ILocalizationService) {
        super(
            () => !processArtifact.artifactState.readonly,
            "fonticon fonticon2-news",
            localization.get("ST_Generate_Toolbar_Button"),
            undefined,
            new BPDropdownItemAction(
                localization.get("ST_Generate_Contextual_Toolbar_Button"),
                () => {
                    console.log("'Generate from Task' clicked");
                },
                () => {
                    if (processArtifact.artifactState.readonly) {
                        return false;
                    }

                    const subArtifact = selectionManager.getSubArtifact() as StatefulProcessSubArtifact;
                    if (!subArtifact) {
                        return false;
                    }

                    const subArtifactType: ProcessShapeType = subArtifact.propertyValues["clientType"].value;
                    if (subArtifactType !== ProcessShapeType.UserTask) {
                        return false;
                    }

                    if (subArtifact.id < 0) {
                        return false;
                    }

                    return true;
                }
            ),
            new BPDropdownItemAction(
                localization.get("ST_Generate_All_Contextual_Toolbar_Button"),
                () => {
                    console.log("'Generate All' clicked");
                },
                () => !processArtifact.artifactState.readonly
            )
        );
    }
}
