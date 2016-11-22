import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../bp-artifact-picker/bp-artifact-picker-dialog";
import {IArtifactPickerAPI} from "../../bp-artifact-picker/bp-artifact-picker";
import {Models} from "../../../../main/models";
import {IDialogSettings} from "../../../../shared/";
import {Enums} from "../../../../main/models";
import {ILocalizationService} from "../../../../core/localization/localizationService";

export enum MoveArtifactInsertMethod {
    Selection,
    Above,
    Below
}

export interface IMoveArtifactPickerOptions extends IArtifactPickerOptions {
    currentArtifact?: Models.IArtifact;
}

export class MoveArtifactResult {
    artifacts: Models.IArtifact[];
    insertMethod: MoveArtifactInsertMethod;
}

export class MoveArtifactPickerDialogController extends  ArtifactPickerDialogController {
    public insertMethod: MoveArtifactInsertMethod = MoveArtifactInsertMethod.Below;
    private _currentArtifact: Models.IArtifact;
    public api: IArtifactPickerAPI;

    constructor($instance: ng.ui.bootstrap.IModalServiceInstance,
                dialogSettings: IDialogSettings,
                public dialogData: IMoveArtifactPickerOptions,
                public localization: ILocalizationService) {
        super($instance, dialogSettings, dialogData, localization);
        
        dialogData.isItemSelectable = (item) => this.isItemSelectable(item);
        this._currentArtifact = dialogData.currentArtifact;
    }

    public isItemSelectable(item: Models.IArtifact) {
        if (MoveArtifactPickerDialogController.checkAncestors(item, this._currentArtifact.id)) {
            return false;
        }

        if (this.insertMethod === this.InsertMethodSelection && this._currentArtifact.predefinedType === Enums.ItemTypePredefined.PrimitiveFolder) {
            return item.predefinedType === Enums.ItemTypePredefined.PrimitiveFolder;
        }

        return true;
    }

    private static checkAncestors(item: Models.IArtifact, id: number): boolean {
        if (item.id === id) {
            return true;
        }
        let found: boolean;
        _.forEach(item.idPath, (ancestorId) => {
            if (ancestorId === id) {
                found = true;
                return;
            }
        });
        return found;
    }

    public get InsertMethodSelection(): MoveArtifactInsertMethod{
        return MoveArtifactInsertMethod.Selection;
    }
    public get InsertMethodAbove(): MoveArtifactInsertMethod{
        return MoveArtifactInsertMethod.Above;
    }
    public get InsertMethodBelow(): MoveArtifactInsertMethod{
        return MoveArtifactInsertMethod.Below;
    }

    public onInsertMethodChange() {
        this.api.updateSelectableNodes();
    }

    public get returnValue(): any[] {
        return [<MoveArtifactResult>{
            artifacts: this.selectedVMs.map(vm => vm.model),
            insertMethod: this.insertMethod
        }];
    };
}