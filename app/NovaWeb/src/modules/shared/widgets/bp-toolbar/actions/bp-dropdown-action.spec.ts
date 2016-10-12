import {BPDropdownAction, BPDropdownItemAction} from "./bp-dropdown-action";

describe("BPDropdownItemAction", () => {
    it("initializes properties and methods successfully", () => {
        // arrange
        const execute = () => {};
        const canExecute = () => false;
        const label = "label1";

        // act
        const dropdownItemAction = new BPDropdownItemAction(label, execute, canExecute);

        // assert
        expect(dropdownItemAction.label).toBe(label);
        expect(dropdownItemAction.execute).toEqual(execute);
        expect(dropdownItemAction.disabled).toEqual(!canExecute());
    });
});

describe("BPDropdownAction", () => {
    it("initializes properties and methods successfully", () => {
        // arrange
        const type = "dropdown";
        const canExecute = () => true;
        const icon = "test0";
        const tooltip = "test0";
        const label = "test0";
        const item1 = new BPDropdownItemAction("test1", () => {}, () => true);
        const item2 = new BPDropdownItemAction("test2", () => {}, () => false, );

        // act
        const dropdownAction = new BPDropdownAction(canExecute, icon, tooltip, label, item1, item2);

        // assert
        expect(dropdownAction.type).toBe(type);
        expect(dropdownAction.disabled).toBe(!canExecute());
        expect(dropdownAction.icon).toBe(icon);
        expect(dropdownAction.tooltip).toBe(tooltip);
        expect(dropdownAction.label).toBe(label);
        expect(dropdownAction.actions).toEqual([item1, item2]);
    });
});