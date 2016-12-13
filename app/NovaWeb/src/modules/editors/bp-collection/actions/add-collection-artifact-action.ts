import {BPButtonAction} from "../../../shared";
import {ItemTypePredefined} from "../../../main/models/enums";
import {IStatefulCollectionArtifact} from "../collection-artifact";
import {Models} from "../../../main/models";
import {IDialogSettings, IDialogService} from "../../../shared";
import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../../main/components/bp-artifact-picker";
import {ILocalizationService} from "../../../core/localization/localizationService";

export class AddCollectionArtifactAction extends BPButtonAction {
    constructor(artifact: IStatefulCollectionArtifact,
        localization: ILocalizationService,
        dialogService: IDialogService) {
            super(
            (): void => {
                if (!artifact ||
                    artifact.predefinedType !== ItemTypePredefined.ArtifactCollection ||
                    artifact.artifactState.readonly) {
                    return;
                }

                const dialogSettings = <IDialogSettings>{
                    okButton: localization.get("App_Button_Add"),
                    template: require("../../../main/components/bp-artifact-picker/bp-artifact-picker-dialog.html"),
                    controller: ArtifactPickerDialogController,
                    css: "nova-open-project",
                    header: localization.get("Artifact_Collection_Add_Artifacts_Picker_Header")
                };

                const dialogData: IArtifactPickerOptions = {
                    selectionMode: "checkbox",
                    showProjects: false,
                    isItemSelectable: (item: Models.IArtifact | Models.ISubArtifactNode) => {
                        let excludedArtifacts = _.map(artifact.artifacts, (artifact) => artifact.id);
                        return excludedArtifacts.indexOf(item.id) === -1;

                    }
                };

                dialogService.open(dialogSettings, dialogData).then((artifacts: Models.IArtifact[]) => {
                    if (artifacts && artifacts.length > 0) {
                            artifact.addArtifactsToCollection(artifacts);
                        }
                    });
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
