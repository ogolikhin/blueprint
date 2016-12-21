import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../bp-artifact-picker/bp-artifact-picker-dialog";
import {IArtifactPickerAPI} from "../../bp-artifact-picker/bp-artifact-picker";
import {Models} from "../../../../main/models";
import {IDialogSettings} from "../../../../shared/";
import {ILocalizationService} from "../../../../core/localization/localizationService";

export interface IAddArtifactToCollectionOptions extends IArtifactPickerOptions {
    currentArtifact?: Models.IArtifact;
}

export class AddArtifactToCollectionResult {
    artifact: Models.IArtifact;
}

export class AddArtifactToCollectionDialogController extends  ArtifactPickerDialogController {
    public addDescendants: boolean = false;

    constructor($instance: ng.ui.bootstrap.IModalServiceInstance,
                dialogSettings: IDialogSettings,
                public dialogData: IAddArtifactToCollectionOptions,
                public localization: ILocalizationService) {
        super($instance, dialogSettings, dialogData, localization);
    }

    public addAllDescendants() {
        this.addDescendants = !this.addDescendants;
    }

}
