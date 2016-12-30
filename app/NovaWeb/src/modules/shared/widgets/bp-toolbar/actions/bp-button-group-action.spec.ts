import {BPButtonGroupAction} from "./bp-button-group-action";
import {BPButtonAction} from "./bp-button-action";

class TestButton extends BPButtonAction {
    public icon: string;
    public tooltip: string;
    public disabled: boolean;

    constructor() {
        super();
    }

    public execute(): void {
        return;
    }
}

describe("BPButtonGroupAction", () => {
    it("initializes properties and methods successfully", () => {
        // arrange
        const type = "buttongroup";
        const button1 = new TestButton();
        const button2 = new TestButton();

        // act
        const buttonGroupAction = new BPButtonGroupAction(button1, button2);

        // assert
        expect(buttonGroupAction.type).toBe(type);
        expect(buttonGroupAction.actions).toEqual([button1, button2]);
    });
});
