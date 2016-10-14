import {BPToggleAction, BPToggleItemAction} from "./bp-toggle-action";

describe("BPToggleItemAction", () => {
    it("initializes properties and methods successfully", () => {
        // arrange
        const icon = "icon1";
        const value = 1;
        const disabled = false;
        const tooltip = "tooltip1";
        const label = "label1";

        // act
        const toggleItem = new BPToggleItemAction(icon, value, disabled, tooltip, label);

        // assert
        expect(toggleItem.icon).toBe(icon);
        expect(toggleItem.value).toBe(value);
        expect(toggleItem.disabled).toBe(disabled);
        expect(toggleItem.tooltip).toBe(tooltip);
        expect(toggleItem.label).toBe(label);
    });
});

describe("BPToggleAction", () => {
    it("initializes properties and methods successfully", () => {
        // arrange
        const type = "toggle";
        const execute = () => {
        };
        const canExecute = () => true;
        const item1 = new BPToggleItemAction("test1", 1, true, "test1", "test1");
        const item2 = new BPToggleItemAction("test2", 2, false);

        // act
        const toggleAction = new BPToggleAction(1, execute, canExecute, item1, item2);

        // assert
        expect(toggleAction.type).toBe(type);
        expect(toggleAction.disabled).toBe(!canExecute());
        expect(toggleAction.actions).toEqual([item1, item2]);
    });

    it("calls execute with correct value when current value is changed", () => {
        // arrange
        const executeSpy = jasmine.createSpy("execute");
        const item1 = new BPToggleItemAction("test1", 1, true, "test1", "test1");
        const item2 = new BPToggleItemAction("test2", 2, false);
        const toggleAction = new BPToggleAction(1, executeSpy, () => true, item1, item2);

        // act
        toggleAction.currentValue = 2;

        // assert
        expect(executeSpy).toHaveBeenCalledWith(2);
        expect(toggleAction.currentValue).toBe(2);
    });
});
