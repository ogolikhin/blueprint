import {BPButtonAction} from "./bp-button-action";

describe("BPButtonAction", () => {
    it("initializes properties and methods successfully", () => {
        // arrange
        const type = "button";
        const execute = () => {};
        const canExecute = () => false;
        const icon = "test1";
        const tooltip = "test1";
        const label = "test1";

        // act
        const buttonAction = new BPButtonAction(execute, canExecute, icon, tooltip, label);

        // assert
        expect(buttonAction.type).toBe(type);
        expect(buttonAction.execute).toBe(execute);
        expect(buttonAction.disabled).toBe(!canExecute());
        expect(buttonAction.icon).toBe(icon);
        expect(buttonAction.tooltip).toBe(tooltip);
        expect(buttonAction.label).toBe(label);
    });
});