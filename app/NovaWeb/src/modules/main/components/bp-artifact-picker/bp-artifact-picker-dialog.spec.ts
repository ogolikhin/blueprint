import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {IDialogSettings} from "../../../shared/";
import {IInstanceItem, InstanceItemType} from "../../models/admin-store-models";
import {InstanceItemNodeVM} from "../../models/tree-node-vm-factory";
import {ArtifactPickerDialogController} from "./bp-artifact-picker-dialog";

describe("ArtifactPickerDialogController", () => {
    let controller: ArtifactPickerDialogController;

    beforeEach(() => {
        const $instance = {} as ng.ui.bootstrap.IModalServiceInstance;
        const dialogSettings = {} as IDialogSettings;
        const dialogData = {};
        const localization = {} as ILocalizationService;
        controller = new ArtifactPickerDialogController($instance, dialogSettings, dialogData, localization);
    });

    it("onSelectionChanged, when no instance folders, sets selection and enables ok button", () => {
        // Arrange
        const models = ["a", "b", "c"];
        const vms = models.map(m => ({model: m}));

        // Act
        controller.onSelectionChanged(vms);

        // Assert
        expect(controller.selectedVMs).toEqual(vms);
        expect(controller.returnValue).toEqual(models);
        expect(controller.disableOkButton).toEqual(false);
    });

    it("onSelectionChanged, when an instance folder, clears selection and disables ok button", () => {
        // Arrange
        const vms = [
            {model: "a"},
            new InstanceItemNodeVM(undefined, {type: InstanceItemType.Folder} as IInstanceItem)
        ];

        // Act
        controller.onSelectionChanged(vms);

        // Assert
        expect(controller.selectedVMs).toEqual([]);
        expect(controller.returnValue).toEqual([]);
        expect(controller.disableOkButton).toEqual(true);
    });

    it("onDoubleClick, when not an instance folder, sets selection, enables ok button and calls ok", () => {
        // Arrange
        const model = {};
        const vm = {model: model};
        spyOn(controller, "ok");

        // Act
        controller.onDoubleClick(vm);

        // Assert
        expect(controller.selectedVMs).toEqual([vm]);
        expect(controller.returnValue).toEqual([model]);
        expect(controller.disableOkButton).toEqual(false);
        expect(controller.ok).toHaveBeenCalled();
    });

    it("onDoubleClick, when an instance folder, clears selection, disables ok button and does not call ok", () => {
        // Arrange
        const vm = new InstanceItemNodeVM(undefined, {type: InstanceItemType.Folder} as IInstanceItem);
        spyOn(controller, "ok");

        // Act
        controller.onDoubleClick(vm);

        // Assert
        expect(controller.selectedVMs).toEqual([]);
        expect(controller.returnValue).toEqual([]);
        expect(controller.disableOkButton).toEqual(true);
        expect(controller.ok).not.toHaveBeenCalled();
    });
});
