import {BPMenuAction, BPButtonOrDropdownAction} from "./bp-menu-action";

describe("BPButtonOrDropdownAction", () => {
    it("initializes properties and methods successfully", () => {
        // arrange
        const execute = () => {
            return;
        };
        const canExecute = () => false;
        const icon = "icon1";
        const label = "label1";
        const tooltip = "tooltip1";

        // act
        const buttonOrDropdownAction = new BPButtonOrDropdownAction(execute, canExecute, icon, tooltip, label);

        // assert
        expect(buttonOrDropdownAction.icon).toBe(icon);
        expect(buttonOrDropdownAction.tooltip).toBe(tooltip);
        expect(buttonOrDropdownAction.label).toBe(label);
        expect(buttonOrDropdownAction.execute).toEqual(execute);
        expect(buttonOrDropdownAction.disabled).toEqual(!canExecute());
    });
});

describe("BPMenuAction", () => {
    it("initializes properties and methods successfully", () => {
        // arrange
        const type = "menu";
        const tooltip = "tooltip2";
        const item1 = new BPButtonOrDropdownAction(() => {
            return;
        }, () => true, "iconItem1", "tooltipItem1", "labelItem1");
        const item2 = new BPButtonOrDropdownAction(() => {
            return;
        }, () => false, "iconItem2", "tooltipItem2", "labelItem2");

        // act
        const additionalMenu = new BPMenuAction(tooltip, item1, item2);

        // assert
        expect(additionalMenu.type).toBe(type);
        expect(additionalMenu.tooltip).toBe(tooltip);
        expect(additionalMenu.actions).toEqual([item1, item2]);
    });
});
