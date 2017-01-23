import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../bp-artifact-picker/bp-artifact-picker-dialog";
import {Models, TreeModels} from "../../../../main/models";
import {IDialogSettings} from "../../../../shared/";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {IProjectService} from "../../../../../modules/managers/project-manager";

export interface IAddArtifactToCollectionOptions extends IArtifactPickerOptions {
    currentArtifact?: any;
}

export class IAddArtifactToCollectionResult {
    collectionId: number;
    addDescendants: boolean;
    hideDescendantsCheckbox: boolean;

}

export class AddArtifactToCollectionDialogController extends  ArtifactPickerDialogController {
    public addDescendants: boolean = false;
    public selectedVMs: TreeModels.ITreeNodeVM<any>[] = [];
    public hideDescendantsCheckbox: boolean = false;

    static $inject = [
        "$uibModalInstance",
        "dialogSettings",
        "dialogData",
        "localization",
        "projectService"
    ];

    constructor($instance: ng.ui.bootstrap.IModalServiceInstance,
                dialogSettings: IDialogSettings,
                public dialogData: IAddArtifactToCollectionOptions,
                public localization: ILocalizationService,
                private projectService: IProjectService) {
        super($instance, dialogSettings, dialogData, localization);
    }

    public $onInit = () => {
        return this.projectService.getArtifacts(this.dialogData.currentArtifact.artifact.projectId,
            this.dialogData.currentArtifact.artifact.id)
            .then((result) => {
                if (!result.length) {
                    this.hideDescendantsCheckbox = true;
                }
            });
    };

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
