import {IDialogSettings, BaseDialogController} from "../../../shared/";
import {Models, AdminStoreModels} from "../../models";
import {ILocalizationService} from "../../../core/localization/localizationService";
import {InstanceItemType} from "../../models/admin-store-models";

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
    showProjects?: boolean;
    showArtifacts?: boolean;
    showCollections?: boolean;
    showSubArtifacts?: boolean;
}

export class ArtifactPickerDialogController extends BaseDialogController implements IArtifactPickerDialogController {
    public hasCloseButton: boolean = true;
    public selectedVMs: Models.IViewModel<any>[];
    public disableOkButton: boolean = true;

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

        if (_.find(this.selectedVMs, (vm) => {
                return vm.model.type === AdminStoreModels.InstanceItemType.Folder;
            })) {
            this.isOkDisabled(true);
        } else {
            this.isOkDisabled(false);
        };
    }

    public onDoubleClick(vm: Models.IViewModel<any>): void {
        this.selectedVMs = [vm];
        this.ok();
    }

    public isOkDisabled (disableOnFolderSelection) {
        this.disableOkButton = this.selectedVMs && this.selectedVMs.length === 0 || disableOnFolderSelection;
    }
}
