import {IDialogSettings, IDialogData, BaseDialogController} from "../../../shared/";
import {IArtifactPickerDialogController, ArtifactPickerDialogController} from "./bp-artifact-picker-dialog";

describe("ArtifactPickerDialogController", () => {
    let controller: ArtifactPickerDialogController;

    beforeEach(() => {
        const $instance = {} as ng.ui.bootstrap.IModalServiceInstance;
        const dialogSettings = {} as IDialogSettings;
        const dialogData = {} as IDialogData;
        controller = new ArtifactPickerDialogController($instance, dialogSettings, dialogData);
    });

    it("onSelectionChanged", () => {
        // Arrange
        const models = ["a", "b", "c"];
        const vms = models.map(m => ({model: m}));

        // Act
        controller.onSelectionChanged(vms);

        // Assert
        expect(controller.selectedVMs).toEqual(vms);
        expect(controller.returnValue).toEqual(models);
    });

    it("onDoubleClick", () => {
        // Arrange
        const model = {};
        const vm = {model: model};
        spyOn(controller, "ok");

        // Act
        controller.onDoubleClick(vm);

        // Assert
        expect(controller.selectedVMs).toEqual([vm]);
        expect(controller.returnValue).toEqual([model]);
        expect(controller.ok).toHaveBeenCalled();
    });
});
