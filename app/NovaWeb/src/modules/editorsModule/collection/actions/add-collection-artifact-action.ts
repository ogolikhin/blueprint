import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../../main/components/bp-artifact-picker";
import {Models} from "../../../main/models";
import {ItemTypePredefined} from "../../../main/models/itemTypePredefined.enum";
import {BPButtonAction} from "../../../shared";
import {IDialogService, IDialogSettings} from "../../../shared";
import {IStatefulCollectionArtifact} from "../../configuration/classes/collection-artifact";

export class AddCollectionArtifactAction extends BPButtonAction {
    constructor(
        private artifact: IStatefulCollectionArtifact,
        private localization: ILocalizationService,
        private dialogService: IDialogService
    ) {
        super();

        if (!this.localization) {
            throw new Error("Localization service is not provided or is null");
        }

        if (!this.dialogService) {
            throw new Error("Dialog service is not provided or is null");
        }
    }

    public get icon(): string {
        return "fonticon fonticon2-add-artifact";
    }

    public get tooltip(): string {
        return this.localization.get("Artifact_Add_To_Collection_Picker_Header");
    }

    public get disabled(): boolean {
        return !this.artifact
            || this.artifact.predefinedType !== ItemTypePredefined.ArtifactCollection
            || this.artifact.artifactState.readonly;
    }

    public execute(): void {
        if (this.disabled) {
            return;
        }

        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get("App_Button_Add"),
            template: require("../../../main/components/bp-artifact-picker/bp-artifact-picker-dialog.html"),
            controller: ArtifactPickerDialogController,
            css: "nova-open-project",
            header: this.localization.get("Artifact_Collection_Add_Artifacts_Picker_Header")
        };

        const dialogData: IArtifactPickerOptions = {
            selectionMode: "checkbox",
            showProjects: false,
            isItemSelectable: (item: Models.IArtifact | Models.ISubArtifactNode) => {
                const excludedArtifacts = _.map(this.artifact.artifacts, (artifact) => artifact.id);
                return excludedArtifacts.indexOf(item.id) === -1;
            }
        };

        this.dialogService.open(dialogSettings, dialogData)
            .then((artifacts: Models.IArtifact[]) => {
                if (artifacts && artifacts.length > 0) {
                    this.artifact.addArtifactsToCollection(artifacts);
                }
            });
    }
}
