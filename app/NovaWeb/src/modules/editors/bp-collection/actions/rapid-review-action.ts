import {BPButtonAction} from "../../../shared";
import {ILocalizationService} from "../../../core";
import {ItemTypePredefined} from "../../../main/models/enums";
import {IStatefulCollectionArtifact} from "../../../managers/artifact-manager/artifact/collection-artifact";

export class RapidReviewAction extends BPButtonAction {
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

                if (artifact.rapidReviewCreated) {
                    return false;
                }

                return true;
            },
            undefined,
            "Create Rapid Review",
            "RR"
        );
    }
}
