import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../bp-artifact-picker/bp-artifact-picker-dialog";
import {Models, TreeModels} from "../../../../main/models";
import {IDialogSettings} from "../../../../shared/";
import {ILocalizationService} from "../../../../core/localization/localizationService";

export interface IAddArtifactToCollectionOptions extends IArtifactPickerOptions {
    currentArtifact?: Models.IArtifact;
}

export class IAddArtifactToCollectionResult {
    collectionId: number;
    addDescendants: boolean;

}

export class AddArtifactToCollectionDialogController extends  ArtifactPickerDialogController {
    public addDescendants: boolean = false;
    public selectedVMs: TreeModels.ITreeNodeVM<any>[] = [];

    constructor($instance: ng.ui.bootstrap.IModalServiceInstance,
                dialogSettings: IDialogSettings,
                public dialogData: IAddArtifactToCollectionOptions,
                public localization: ILocalizationService) {
        super($instance, dialogSettings, dialogData, localization);
    }

    public onSelectionChanged(selectedVMs: TreeModels.ITreeNodeVM<any>[]): void {
        this.selectedVMs = selectedVMs;
    }

    public addAllDescendants() {
        this.addDescendants = !this.addDescendants;
    }

    public get okDisabled(): boolean {
        return !this.selectedVMs.length;
    }

    public get returnValue(): any {
        return <IAddArtifactToCollectionResult>{addDescendants: this.addDescendants, collectionId: this.selectedVMs[0] && this.selectedVMs[0].model.id};
    };
}
