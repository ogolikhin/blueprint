import {IDialogSettings} from "../../../shared/";
import {ArtifactPickerDialogController} from "./bp-artifact-picker-dialog";
import {ILocalizationService} from "../../../core/localization/localizationService";

describe("ArtifactPickerDialogController", () => {
    let controller: ArtifactPickerDialogController;

    beforeEach(() => {
        const $instance = {} as ng.ui.bootstrap.IModalServiceInstance;
        const dialogSettings = {} as IDialogSettings;
        const dialogData = {};
        const localization = {} as ILocalizationService;
        controller = new ArtifactPickerDialogController($instance, dialogSettings, dialogData, localization);
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
