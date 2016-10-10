import {IBPAction} from "./bp-action";

export interface IBPDropdownItemAction {
    label: string;
    click: () => void;
    disabled?: boolean;
}

export class BPDropdownItemAction implements IBPDropdownItemAction {
    constructor(
        private _label: string,
        private _click: () => void,
        private _canClick: () => boolean
    ) {
    }

    public get label(): string {
        return this._label;
    }

    public get click(): () => void {
        return this._click;
    }

    public get disabled(): boolean {
        return !this._canClick();
    }
}

export interface IBPDropdownAction extends IBPAction {
    icon: string;
    actions: IBPDropdownItemAction[];
    label?: string;
    disabled?: boolean;
}

export class BPDropdownAction implements IBPDropdownAction {
    private _actions: IBPDropdownItemAction[];

    constructor(
        private _canClick: () => boolean,
        private _icon: string,
        private _label?: string,
        private _tooltip?: string,
        ... actions: IBPDropdownItemAction[]
    ) {
        this._actions = actions;
    }

    public get type(): string {
        return "dropdown";
    }

    public get icon(): string {
        return this._icon;
    }

    public get disabled(): boolean {
        return !this._canClick();
    }

    public get actions(): IBPDropdownItemAction[] {
        return this._actions;
    }

    public get label(): string {
        return this._label;
    }

    public get tooltip(): string {
        return this._tooltip;
    }
}
