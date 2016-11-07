import {BPButtonAction} from "../../../shared";
import {ILocalizationService} from "../../../core";
import {ItemTypePredefined} from "../../../main/models/enums";
import {IStatefulCollectionArtifact} from "../collection-artifact";
import {IDialogService} from "../../../shared";

export class RapidReviewAction extends BPButtonAction {
    constructor(artifact: IStatefulCollectionArtifact,
        localization: ILocalizationService,
        dialogService: IDialogService
        ) {
        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }

        super(
            (): void => {
                if (!artifact.artifactState.published) {
                    dialogService.confirm(
                        localization.get(
                            "Confirm_Publish_Collection",
                            "Please publish your changes before entering the review. Would you like to proceed?"
                            )).then( () => {
                                artifact.publish().then(() => {
                                    let url = `Web/#/RapidReview/${artifact.id}/edit`;
                                    window.open(url);
                                });
                            });
                } else {
                    let url = `Web/#/RapidReview/${artifact.id}/edit`;
                    window.open(url);
                }
            },
            (): boolean => {
                if (
                    !artifact 
                    || artifact.predefinedType !== ItemTypePredefined.ArtifactCollection
                    || artifact.artifactState.readonly 
                    || artifact.rapidReviewCreated
                    || artifact.artifacts.length === 0
                    ) {
                        return false;
                }
                return true;
            },
            undefined,
            localization.get("Create_Rapid_Review", "Create Rapid Review"),
            "RR"
        );
    }
}
