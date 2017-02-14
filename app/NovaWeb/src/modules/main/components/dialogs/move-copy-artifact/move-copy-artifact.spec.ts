import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {Enums, Models} from "../../../../main/models";
import {IDialogSettings} from "../../../../shared/";
import {ItemTypePredefined} from "../../../models/item-type-predefined";
import {IMoveCopyArtifactPickerOptions, MoveCopyActionType, MoveCopyArtifactInsertMethod, MoveCopyArtifactPickerDialogController} from "./move-copy-artifact";

describe("MoveArtifactPickerDialogController", () => {
    let controller: MoveCopyArtifactPickerDialogController;

    beforeEach(() => {
        const $instance = {} as ng.ui.bootstrap.IModalServiceInstance;
        const dialogSettings = {} as IDialogSettings;
        const dialogData = {
            currentArtifact: <Models.IArtifact>{id: 1, name: "test", predefinedType: ItemTypePredefined.PrimitiveFolder},
            actionType: MoveCopyActionType.Move
        } as IMoveCopyArtifactPickerOptions;
        const localization = {} as ILocalizationService;
        controller = new MoveCopyArtifactPickerDialogController($instance, dialogSettings, dialogData, localization);
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
        const item = <Models.IArtifact>{id: 5, name: "test", predefinedType: ItemTypePredefined.PrimitiveFolder};
        controller.insertMethod = MoveCopyArtifactInsertMethod.Inside;

        // Act
        let result: boolean = controller.isItemSelectable(item);

        // Assert
        expect(result).toEqual(true);
    });

    it("okDisabled selection exists", () => {
        // Arrange
        const model = {id: 3, name: "test"};
        const vm = {model: model};
        controller.selectedVMs = [vm];
        controller.insertMethod = MoveCopyArtifactInsertMethod.Above;

        // Act
        let result: boolean = controller.okDisabled;

        // Assert
        expect(result).toEqual(false);
    });

    it("okDisabled no selection", () => {
        // Arrange
        controller.selectedVMs = [];

        // Act
        let result: boolean = controller.okDisabled;

        // Assert
        expect(result).toEqual(true);
    });
});
