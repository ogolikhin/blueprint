import {BPButtonAction, IDialogSettings, IDialogService} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {IProjectManager} from "../../../../managers";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IArtifactPickerOptions} from "../../../../main/components/bp-artifact-picker";
import {
    MoveArtifactPickerDialogController, 
    MoveArtifactResult, 
    MoveArtifactInsertMethod
} from "../../../../main/components/dialogs/move-artifact/move-artifact";
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
                    template: require("../../../../main/components/dialogs/move-artifact/move-artifact-dialog.html"),
                    controller: MoveArtifactPickerDialogController,
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

                dialogService.open(dialogSettings, dialogData).then((result: MoveArtifactResult[]) => {
                    if (result && result.length === 1) {
                        const artifacts: Models.IArtifact[] = result[0].artifacts;
                        if (artifacts && artifacts.length > 0) {
                            artifact.move(artifacts[0].id, result[0].orderIndex);
                        }
                    }
                    
                    //console.log(result[0].insertMethod);                      
                    /*if (artifacts && artifacts.length > 0) {
                        artifact.addArtifactsToCollection(artifacts);
                    }*/
                });                
                
            },
            (): boolean => true,
            "fonticon2-move",
            localization.get("App_Toolbar_Discard")
        );
    }
}
