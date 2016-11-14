import {BPButtonAction, IDialogSettings, IDialogService} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {IProjectManager} from "../../../../managers";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../../../main/components/bp-artifact-picker";
import {Models} from "../../../../main/models";

export class MoveAction extends BPButtonAction {
    constructor(artifact: IStatefulArtifact,
                localization: ILocalizationService,
                messageService: IMessageService,
                projectManager: IProjectManager,
                loadingOverlayService: ILoadingOverlayService,
                dialogService: IDialogService) {
        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }
        super(
            (): void => {
                const dialogSettings = <IDialogSettings>{
                    okButton: localization.get("App_Button_Move"),
                    template: require("../../../../main/components/bp-artifact-picker/bp-artifact-picker-dialog.html"),
                    controller: ArtifactPickerDialogController,
                    css: "nova-open-project",
                    header: localization.get("Move_Artifacts_Picker_Header")
                };

                const dialogData: IArtifactPickerOptions = {
                    showSubArtifacts: false,
                    selectionMode: "single",
                    isOneProjectLevel: true,
                    isItemSelectable: (item: Models.IArtifact | Models.ISubArtifactNode) => true
                    /*isItemSelectable: (item: Models.IArtifact | Models.ISubArtifactNode) => {
                        let excludedArtifacts = _.map(artifact.artifacts, (artifact) => artifact.id);
                        return excludedArtifacts.indexOf(item.id) === -1;

                    }*/
                };

                dialogService.open(dialogSettings, dialogData).then((artifacts: Models.IArtifact[]) => {                        
                    /*if (artifacts && artifacts.length > 0) {
                        artifact.addArtifactsToCollection(artifacts);
                    }*/
                });                
                
            },
            (): boolean => true,
            "fonticon2-discard-line",
            localization.get("App_Toolbar_Discard")
        );
    }
}
