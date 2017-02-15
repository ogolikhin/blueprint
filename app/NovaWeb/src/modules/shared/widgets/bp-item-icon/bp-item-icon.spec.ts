import "./";
import "angular-mocks";
import "lodash";
import {ItemTypePredefined} from "../../../main/models/itemTypePredefined.enum";
import {BPItemTypeIconController, IBPItemTypeIconController} from "./bp-item-icon";

describe("BPArtifactListComponent", () => {
    let bindings: IBPItemTypeIconController;
    let componentController: ng.IComponentControllerService;
    let controller: BPItemTypeIconController;

    beforeEach(angular.mock.module("bp.widgets.itemicon"));

    beforeEach(inject(($componentController: ng.IComponentControllerService) => {
        componentController = $componentController;
    }));

    it("Values are bound", () => {
        // Arrange
        const itemTypeId = 1;
        const itemTypeIconId = 2;
        const predefinedType = ItemTypePredefined.TextualRequirement;
        bindings = {
            itemTypeId: itemTypeId,
            itemTypeIconId: itemTypeIconId,
            predefinedType: predefinedType
        };
        controller = <BPItemTypeIconController>componentController("bpItemTypeIcon", null, bindings);

        // Act

        // Assert
        expect(controller.itemTypeId).toEqual(itemTypeId);
        expect(controller.itemTypeIconId).toEqual(itemTypeIconId);
        expect(controller.predefinedType).toEqual(predefinedType);
        expect(controller.fallback).toBeFalsy();
    });

    it("Show basic icon if no custom icon is provided", () => {
        // Arrange
        const itemTypeId = 1;
        const itemTypeIconId = null;
        const predefinedType = ItemTypePredefined.TextualRequirement;
        bindings = {
            itemTypeId: itemTypeId,
            itemTypeIconId: itemTypeIconId,
            predefinedType: predefinedType
        };
        controller = <BPItemTypeIconController>componentController("bpItemTypeIcon", null, bindings);

        // Act
        controller.$onChanges();

        // Assert
        expect(controller.showBasicIcon).toBeTruthy();
    });

    it("Show custom icon if custom icon is provided", () => {
        // Arrange
        const itemTypeId = 1;
        const itemTypeIconId = 2;
        const predefinedType = ItemTypePredefined.TextualRequirement;
        bindings = {
            itemTypeId: itemTypeId,
            itemTypeIconId: itemTypeIconId,
            predefinedType: predefinedType
        };
        const imageSource = `/shared/api/itemTypes/${itemTypeId}/icon?id=${itemTypeIconId}`;
        controller = <BPItemTypeIconController>componentController("bpItemTypeIcon", null, bindings);

        // Act
        controller.$onChanges();

        // Assert
        expect(controller.showBasicIcon).toBeFalsy();
        expect(controller.imageSource).toBe(imageSource);
    });
});
