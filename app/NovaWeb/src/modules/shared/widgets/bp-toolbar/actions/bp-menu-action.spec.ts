import {BPMenuAction, BPButtonOrDropdownAction, BPButtonOrDropdownSeparator} from "./bp-menu-action";

describe("BPButtonOrDropdownAction", () => {
    it("initializes properties and methods successfully", () => {
        // arrange
        const execute = () => {
            return;
        };
        const canExecute = () => false;
        const icon = "icon";
        const label = "label";
        const tooltip = "tooltip";

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
        const icon = "fonticon2-more-menu";
        const tooltip = "tooltip";
        const item1 = new BPButtonOrDropdownAction(() => {
            return;
        }, () => true, "icon1", "tooltip1", "label1");
        const item2 = new BPButtonOrDropdownAction(() => {
            return;
        }, () => false, "icon2", "tooltip2", "label2");
        const separator = new BPButtonOrDropdownSeparator();

        // act
        const additionalMenu = new BPMenuAction(tooltip, item1, separator, item2);

        // assert
        expect(additionalMenu.type).toBe(type);
        expect(additionalMenu.tooltip).toBe(tooltip);
        expect(additionalMenu.icon).toBe(icon);
        expect(additionalMenu.disabled).toBeFalsy();
        expect(additionalMenu.actions).toEqual([item1, separator, item2]);
    });

    it("is disabled if all its children are disabled", () => {
        // arrange
        const item1 = new BPButtonOrDropdownAction(() => {
            return;
        }, () => false, "icon1", "tooltip1", "label1");
        const item2 = new BPButtonOrDropdownAction(() => {
            return;
        }, () => false, "icon2", "tooltip2", "label2");
        const separator = new BPButtonOrDropdownSeparator();

        // act
        const additionalMenu = new BPMenuAction("tooltip", item1, separator, item2);

        // assert
        expect(additionalMenu.disabled).toBeTruthy();
        expect(additionalMenu.actions).toEqual([item1, separator, item2]);
    });
});
