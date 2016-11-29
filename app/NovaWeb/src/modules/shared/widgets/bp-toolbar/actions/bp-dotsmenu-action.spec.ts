import {BPDotsMenuAction, BPButtonOrDropdownAction} from "./bp-dotsmenu-action";

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

describe("BPDotsMenuAction", () => {
    it("initializes properties and methods successfully", () => {
        // arrange
        const type = "dotsmenu";
        const canExecute = () => true;
        const icon = "icon2";
        const tooltip = "tooltip2";
        const label = "label2";
        const item1 = new BPButtonOrDropdownAction(() => {
            return;
        }, () => true, "iconItem1", "tooltipItem1", "labelItem1");
        const item2 = new BPButtonOrDropdownAction(() => {
            return;
        }, () => false, "iconItem2", "tooltipItem2", "labelItem2");

        // act
        const dotsMenu = new BPDotsMenuAction(canExecute, icon, tooltip, label, item1, item2);

        // assert
        expect(dotsMenu.type).toBe(type);
        expect(dotsMenu.disabled).toBe(!canExecute());
        expect(dotsMenu.icon).toBe(icon);
        expect(dotsMenu.tooltip).toBe(tooltip);
        expect(dotsMenu.label).toBe(label);
        expect(dotsMenu.actions).toEqual([item1, item2]);
    });
});
