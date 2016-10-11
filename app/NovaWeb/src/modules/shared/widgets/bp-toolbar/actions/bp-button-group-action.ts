import {IBPAction} from "./bp-action";
import {IBPButtonAction} from "./bp-button-action";

export interface IBPButtonGroupAction extends IBPAction {
    actions: IBPButtonAction[];
    disabled?: boolean;
}

export class BPButtonGroupAction implements IBPButtonGroupAction {
    private _actions: IBPButtonAction[];

    constructor(
        ... actions: IBPButtonAction[]
    ) {
        this._actions = actions;
    }

    public get type(): string {
        return "buttongroup";
    }

    public get actions(): IBPButtonAction[] {
        return this._actions;
    }
}