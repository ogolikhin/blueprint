import {BPButtonAction} from "../../../shared";
import {IStatefulArtifact} from "../../../managers/artifact-manager";
import {ILocalizationService} from "../../../core";
import {ItemTypePredefined} from "../../../main/models/enums";
import {IStatefulCollectionArtifact} from "../collection-artifact";

export class AddCollectionArtifactAction extends BPButtonAction {
    constructor(artifact: IStatefulCollectionArtifact,
        localization: ILocalizationService) {
        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }

            super(
            (): void => {
                let a = 5;
            },
            (): boolean => {
                if (!artifact) {
                    return false;
                }

                if (artifact.predefinedType !== ItemTypePredefined.ArtifactCollection) {
                    return false;
                }

                if (artifact.artifactState.readonly) {
                    return false;
                }

                return true;
            },
            "fonticon fonticon2-add-artifact",
            "Add artifact to collection"
        );
    }
}
