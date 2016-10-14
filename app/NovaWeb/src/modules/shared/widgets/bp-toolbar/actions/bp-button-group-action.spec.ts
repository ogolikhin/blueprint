import {BPButtonGroupAction} from "./bp-button-group-action";
import {BPButtonAction} from "./bp-button-action";

describe("BPButtonGroupAction", () => {
    it("initializes properties and methods successfully", () => {
        // arrange
        const type = "buttongroup";
        const button1 = new BPButtonAction(() => {
        }, () => true, "test1", "test1", "test1");
        const button2 = new BPButtonAction(() => {
        }, () => false, "test2");

        // act
        const buttonGroupAction = new BPButtonGroupAction(button1, button2);

        // assert
        expect(buttonGroupAction.type).toBe(type);
        expect(buttonGroupAction.actions).toEqual([button1, button2]);
    });
});
