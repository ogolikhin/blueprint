import {BPButtonDropdownAction, BPButtonDropdownItemAction} from "./bp-button-dropdown-action";

describe("BPButtonDropdownAction", () => {
    it("initializes properties and methods successfully", () => {
        // arrange
        const execute = () => {
            return;
        };
        const canExecute = () => false;
        const label = "label1";

        // act
        const buttonDropdownItemAction = new BPButtonDropdownItemAction(label, execute, canExecute);

        // assert
        expect(buttonDropdownItemAction.label).toBe(label);
        expect(buttonDropdownItemAction.execute).toEqual(execute);
        expect(buttonDropdownItemAction.disabled).toEqual(!canExecute());
    });
});

describe("BPButtonDropdownAction", () => {
    it("initializes properties and methods successfully", () => {
        // arrange
        const type = "buttondropdown";
        const canExecute = () => true;
        const icon = "test0";
        const tooltip = "test0";
        const label = "test0";
        const item1 = new BPButtonDropdownItemAction("test1", () => {
            return;
        }, () => true);
        const item2 = new BPButtonDropdownItemAction("test2", () => {
            return;
        }, () => false);

        // act
        const buttonDropdownAction = new BPButtonDropdownAction(canExecute, icon, tooltip, label, item1, item2);

        // assert
        expect(buttonDropdownAction.type).toBe(type);
        expect(buttonDropdownAction.disabled).toBe(!canExecute());
        expect(buttonDropdownAction.icon).toBe(icon);
        expect(buttonDropdownAction.tooltip).toBe(tooltip);
        expect(buttonDropdownAction.label).toBe(label);
        expect(buttonDropdownAction.actions).toEqual([item1, item2]);
    });
});
