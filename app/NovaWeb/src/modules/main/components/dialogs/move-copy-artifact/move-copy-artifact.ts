import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../bp-artifact-picker/bp-artifact-picker-dialog";
import {IArtifactPickerAPI} from "../../bp-artifact-picker/bp-artifact-picker";
import {Models} from "../../../../main/models";
import {IDialogSettings} from "../../../../shared/";
import {Enums} from "../../../../main/models";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";

export enum MoveCopyArtifactInsertMethod {
    Inside,
    Above,
    Below
}

export enum MoveCopyActionType {
    Move, Copy
}

export interface IMoveCopyArtifactPickerOptions extends IArtifactPickerOptions {
    currentArtifact?: Models.IArtifact;
    actionType: MoveCopyActionType;
}

export class MoveCopyArtifactResult {
    artifacts: Models.IArtifact[];
    insertMethod: MoveCopyArtifactInsertMethod;
}

export class MoveCopyArtifactPickerDialogController extends  ArtifactPickerDialogController {
    public insertMethod: MoveCopyArtifactInsertMethod = MoveCopyArtifactInsertMethod.Inside;
    private _currentArtifact: Models.IArtifact;
    private _actionType: MoveCopyActionType;
    public api: IArtifactPickerAPI;

    constructor($instance: ng.ui.bootstrap.IModalServiceInstance,
                dialogSettings: IDialogSettings,
                public dialogData: IMoveCopyArtifactPickerOptions,
                public localization: ILocalizationService) {
        super($instance, dialogSettings, dialogData, localization);

        dialogData.isItemSelectable = (item) => this.isItemSelectable(item);
        this._currentArtifact = dialogData.currentArtifact;
        this._actionType = dialogData.actionType;
    }

    public isItemSelectable(item: Models.IArtifact) {
        if (this._actionType === MoveCopyActionType.Move && MoveCopyArtifactPickerDialogController.checkAncestors(item, this._currentArtifact.id)) {
            return false;
        }

        //if we're moving a folder
        if (this._currentArtifact.predefinedType === Enums.ItemTypePredefined.PrimitiveFolder) {
            //can only insert into folders
            if (this.insertMethod === this.InsertMethodSelection) {
                return item.predefinedType === Enums.ItemTypePredefined.PrimitiveFolder;
            } else if (this.insertMethod === this.InsertMethodAbove || this.insertMethod === this.InsertMethodBelow) {
                //or move as a sibling to something with a folder as parent (or no parent(i.e. project))
                return !item.parentPredefinedType || item.parentPredefinedType === Enums.ItemTypePredefined.PrimitiveFolder;
            }
        }

        //if we're moving a baseline or review artifact/folder
        if (this._currentArtifact.predefinedType === Enums.ItemTypePredefined.BaselineFolder ||
            this._currentArtifact.predefinedType === Enums.ItemTypePredefined.ArtifactBaseline ||
            this._currentArtifact.predefinedType === Enums.ItemTypePredefined.ArtifactReviewPackage) {
            //can only insert into baseline folders
            if (this.insertMethod === this.InsertMethodSelection) {
                return item.predefinedType === Enums.ItemTypePredefined.BaselineFolder;
            } else if (this.insertMethod === this.InsertMethodAbove || this.insertMethod === this.InsertMethodBelow) {
                //or move as a sibling to anything but the main Baselines and Reviews folder
                return !(item.predefinedType === Enums.ItemTypePredefined.BaselineFolder
                && item.itemTypeId === Enums.ItemTypePredefined.BaselinesAndReviews);
            }
        }

        //if we're moving a collection artifact/folder
        if (this._currentArtifact.predefinedType === Enums.ItemTypePredefined.CollectionFolder ||
            this._currentArtifact.predefinedType === Enums.ItemTypePredefined.ArtifactCollection) {
            //can only insert into collection folders
            if (this.insertMethod === this.InsertMethodSelection) {
                return item.predefinedType === Enums.ItemTypePredefined.CollectionFolder;
            } else if (this.insertMethod === this.InsertMethodAbove || this.insertMethod === this.InsertMethodBelow) {
                //or move as a sibling to anything but the main Collections folder
                return !(item.predefinedType === Enums.ItemTypePredefined.CollectionFolder
                && item.itemTypeId === Enums.ItemTypePredefined.Collections);
            }
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

    public get InsertMethodSelection(): MoveCopyArtifactInsertMethod{
        return MoveCopyArtifactInsertMethod.Inside;
    }
    public get InsertMethodAbove(): MoveCopyArtifactInsertMethod{
        return MoveCopyArtifactInsertMethod.Above;
    }
    public get InsertMethodBelow(): MoveCopyArtifactInsertMethod{
        return MoveCopyArtifactInsertMethod.Below;
    }

    public onInsertMethodChange() {
        this.api.updateSelectableNodes();
    }

    public get okDisabled(): boolean {
        return !this.selectedVMs || this.selectedVMs.length === 0 || !this.isItemSelectable(this.selectedVMs[0].model);
    }

    public get returnValue(): any[] {
        return [<MoveCopyArtifactResult>{
            artifacts: this.selectedVMs.map(vm => vm.model),
            insertMethod: this.insertMethod
        }];
    };
}
