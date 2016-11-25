import {IDialogSettings, IDialogData, BaseDialogController} from "../../../../shared/";
import {IArtifactPickerDialogController} from "../../bp-artifact-picker/bp-artifact-picker-dialog";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {MoveArtifactPickerDialogController, IMoveArtifactPickerOptions, MoveArtifactInsertMethod} from "./move-artifact";
import {Models, Enums} from "../../../../main/models";

describe("MoveArtifactPickerDialogController", () => {
    let controller: MoveArtifactPickerDialogController;

    beforeEach(() => {
        const $instance = {} as ng.ui.bootstrap.IModalServiceInstance;
        const dialogSettings = {} as IDialogSettings;
        const dialogData = {
            currentArtifact: <Models.IArtifact>{id: 1, name: "test", predefinedType: Enums.ItemTypePredefined.PrimitiveFolder}
        } as IMoveArtifactPickerOptions;
        const localization = {} as ILocalizationService;
        controller = new MoveArtifactPickerDialogController($instance, dialogSettings, dialogData, localization);
    });

    it("isItemSelectable same item id", () => {
        // Arrange
        const item = <Models.IArtifact>{id: 1, name: "test"};

        // Act
        let result: boolean = controller.isItemSelectable(item);

        // Assert
        expect(result).toEqual(false);
    });

    it("isItemSelectable same item id ancestor", () => {
        // Arrange
        const item = <Models.IArtifact>{id: 3, name: "test", idPath: [3, 1]};

        // Act
        let result: boolean = controller.isItemSelectable(item);

        // Assert
        expect(result).toEqual(false);
    });

    it("isItemSelectable folder inside", () => {
        // Arrange
        const item = <Models.IArtifact>{id: 5, name: "test", predefinedType: Enums.ItemTypePredefined.PrimitiveFolder};
        controller.insertMethod = MoveArtifactInsertMethod.Inside;

        // Act
        let result: boolean = controller.isItemSelectable(item);

        // Assert
        expect(result).toEqual(true);
    });

    it("okDisabled", () => {
        // Arrange
        const model = {};
        const vm = {model: model};
        //spyOn(controller, "ok");

        // Act
        let result: boolean = controller.okDisabled;

        // Assert
        expect(result).toEqual(true);
    });
});