import {BPButtonAction, IDialogSettings, IDialogService} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {ILocalizationService} from "../../../../core";
import {ItemTypePredefined} from "../../../../main/models/enums";

export class DeleteAction extends BPButtonAction {
    constructor(artifact: IStatefulArtifact,
                localization: ILocalizationService,
                dialogService: IDialogService,
                deleteDialogSettings: IDialogSettings) {
        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }

        if (!dialogService) {
            throw new Error("Dialog service not provided or is null");
        }

        if (!deleteDialogSettings) {
            throw new Error("Delete dialog settings not provided or is null");
        }

        super(
            () => {
                dialogService.open(deleteDialogSettings).then((confirm: boolean) => {
                    if (confirm) {
                        dialogService.alert("you clicked confirm!");
                        this.deleteArtifact();
                    }
                    ;
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
        //fixme: empty blocks should be removed
    }
}
