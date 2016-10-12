import {BPButtonAction, IDialogSettings, IDialogService} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {ILocalizationService} from "../../../../core";
import {ItemTypePredefined} from "../../../../main/models/enums";

export class DeleteAction extends BPButtonAction {
    constructor(artifact: IStatefulArtifact,
                localization: ILocalizationService,
                dialogService: IDialogService) {
        super(
            () => {
                dialogService.open(<IDialogSettings>{
                    okButton: localization.get("App_Button_Ok"),
                    template: require("../../../../shared/widgets/bp-dialog/bp-dialog.html"),
                    header: localization.get("App_DialogTitle_Alert"),
                    message: "Are you sure you would like to delete the artifact?"
                }).then((confirm: boolean) => {
                    if (confirm) {
                        dialogService.alert("you clicked confirm!");
                        this.deleteArtifact();
                    }
                });
            },
            () => {
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
            "fonticon fonticon2-delete",
            localization.get("App_Toolbar_Delete")
        );
    }

    private deleteArtifact() {
//fixme: if this is not needed, it should be not here or undefined
    }
}
