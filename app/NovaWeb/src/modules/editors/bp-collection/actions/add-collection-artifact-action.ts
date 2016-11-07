import {BPButtonAction} from "../../../shared";
import {IStatefulArtifact} from "../../../managers/artifact-manager";
import {ILocalizationService} from "../../../core";
import {ItemTypePredefined} from "../../../main/models/enums";
import {IStatefulCollectionArtifact} from "../collection-artifact";
import {Enums, Models} from "../../../main/models";
import {IDialogSettings, IDialogService} from "../../../shared";
import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../../main/components/bp-artifact-picker";

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
                    okButton: localization.get("App_Button_Open"),
                    template: require("../../../main/components/bp-artifact-picker/bp-artifact-picker-dialog.html"),
                    controller: ArtifactPickerDialogController,
                    css: "nova-open-project",                    
                    header: localization.get("App_Properties_Actor_InheritancePicker_Title")
                };

                const dialogData: IArtifactPickerOptions = {                    
                    showSubArtifacts: false,
                    selectionMode: "checkbox",
                    isOneProjectLevel: true
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
