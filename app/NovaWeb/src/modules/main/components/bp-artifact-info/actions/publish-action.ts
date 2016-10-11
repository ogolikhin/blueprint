import {BPButtonAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {ILocalizationService} from "../../../../core";
import {ItemTypePredefined} from "../../../../main/models/enums";

export class PublishAction extends BPButtonAction {
    constructor(
        artifact: IStatefulArtifact,
        localization: ILocalizationService
    ) {
        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }
        
        super(
            (): void => {
                artifact.publish();
            },
            (): boolean => {
                if (!artifact) {
                    return false;
                }

                const invalidTypes = [
                    ItemTypePredefined.Project, 
                    ItemTypePredefined.Collections
                ];

                if (invalidTypes.indexOf(artifact.predefinedType) >= 0) {
                    return false;
                }

                if (artifact.artifactState.readonly) {
                    return false;
                }

                return true;
            },
            "fonticon fonticon2-publish",
            localization.get("App_Toolbar_Publish")
        );
    }
}