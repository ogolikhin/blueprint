import {IDialogSettings, BaseDialogController} from "../../../shared/";
import {Models} from "../../models";
import {ILocalizationService} from "../../../core/localization/localizationService";

export interface IArtifactPickerDialogController {
    // BpArtifactPicker bindings
    onSelectionChanged(selectedVMs: Models.IViewModel<any>[]): any;
    onDoubleClick(vm: Models.IViewModel<any>): any;
    selectedVMs: Models.IViewModel<any>[];
}

export interface IArtifactPickerOptions {
    isItemSelectable?: (item: Models.IArtifact | Models.ISubArtifactNode) => boolean;
    selectableItemTypes?: Models.ItemTypePredefined[];
    selectionMode?: "single" | "multiple" | "checkbox";
    showSubArtifacts?: boolean;
    isOneProjectLevel?: boolean;
}

export class ArtifactPickerDialogController extends BaseDialogController implements IArtifactPickerDialogController {
    public hasCloseButton: boolean = true;
    public selectedVMs: Models.IViewModel<any>[];

    static $inject = [
        "$uibModalInstance",
        "dialogSettings",
        "dialogData",
        "localization"
    ];

    constructor($instance: ng.ui.bootstrap.IModalServiceInstance,
                dialogSettings: IDialogSettings,
                public dialogData: IArtifactPickerOptions,
                localization: ILocalizationService) {
        super($instance, dialogSettings);

        // Binding an optional callback to undefined doesn't behave as expected.
        if (!dialogData.isItemSelectable) {
            dialogData.isItemSelectable = () => true;
        }
    };

    public get returnValue(): any[] {
        return this.selectedVMs.map(vm => vm.model);
    };

    public onSelectionChanged(selectedVMs: Models.IViewModel<any>[]): void {
        this.selectedVMs = selectedVMs;
    }

    public onDoubleClick(vm: Models.IViewModel<any>): void {
        this.selectedVMs = [vm];
        this.ok();
    }
}
