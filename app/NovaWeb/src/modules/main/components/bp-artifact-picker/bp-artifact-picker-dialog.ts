import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {BaseDialogController, IDialogSettings} from "../../../shared/";
import {Models} from "../../models";
import {InstanceItemType} from "../../models/admin-store-models";
import {ItemTypePredefined} from "../../models/item-type-predefined";
import {InstanceItemNodeVM} from "../../models/tree-node-vm-factory";

export interface IArtifactPickerDialogController {
    // BpArtifactPicker bindings
    onSelectionChanged(selectedVMs: Models.IViewModel<any>[]): any;
    onDoubleClick(vm: Models.IViewModel<any>): any;
    selectedVMs: Models.IViewModel<any>[];
}

export interface IArtifactPickerOptions {
    isItemSelectable?: (item: Models.IArtifact | Models.ISubArtifactNode) => boolean;
    selectableItemTypes?: ItemTypePredefined[];
    selectionMode?: "single" | "multiple" | "checkbox";
    showProjects?: boolean;
    showArtifacts?: boolean;
    showBaselinesAndReviews?: boolean;
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

    private setSelectedVMs(selectedVMs: Models.IViewModel<any>[]) {
        if (_.find(selectedVMs, vm => vm instanceof InstanceItemNodeVM && vm.model.type === InstanceItemType.Folder)) {
            selectedVMs = [];
        }

        this.selectedVMs = selectedVMs;
        this.disableOkButton = this.selectedVMs && !this.selectedVMs.length;
    }

    public onSelectionChanged(selectedVMs: Models.IViewModel<any>[]): void {
        this.setSelectedVMs(selectedVMs);
    }

    public onDoubleClick(vm: Models.IViewModel<any>): void {
        this.setSelectedVMs([vm]);
        if (!this.disableOkButton) {
            this.ok();
        }
    }
}
