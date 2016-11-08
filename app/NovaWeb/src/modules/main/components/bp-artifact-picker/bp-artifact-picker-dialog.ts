import {IDialogSettings, BaseDialogController} from "../../../shared/";
import {Models, TreeViewModels} from "../../models";

export interface IArtifactPickerDialogController {
    // BpArtifactPicker bindings
    onSelectionChanged(selectedVMs: TreeViewModels.IViewModel<any>[]): any;
    onDoubleClick(vm: TreeViewModels.IViewModel<any>): any;
    selectedVMs: TreeViewModels.IViewModel<any>[];
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
    public selectedVMs: TreeViewModels.IViewModel<any>[];

    static $inject = [
        "$uibModalInstance",
        "dialogSettings",
        "dialogData"
    ];

    constructor($instance: ng.ui.bootstrap.IModalServiceInstance,
                dialogSettings: IDialogSettings,
                public dialogData: IArtifactPickerOptions) {
        super($instance, dialogSettings);

        // Binding an optional callback to undefined doesn't behave as expected.
        if (!dialogData.isItemSelectable) {
            dialogData.isItemSelectable = () => true;
        }
    };

    public get returnValue(): any[] {
        return this.selectedVMs.map(vm => vm.model);
    };

    public onSelectionChanged(selectedVMs: TreeViewModels.IViewModel<any>[]): void {
        this.selectedVMs = selectedVMs;
    }

    public onDoubleClick(vm: TreeViewModels.IViewModel<any>): void {
        this.selectedVMs = [vm];
        this.ok();
    }
}
