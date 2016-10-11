import {IBPAction} from "./bp-action";

export interface IBPDropdownItemAction {
    label: string;
    execute: () => void;
    disabled?: boolean;
}

export class BPDropdownItemAction implements IBPDropdownItemAction {
    constructor(
        private _label: string,
        private _execute: () => void,
        private _canExecute: () => boolean
    ) {
    }

    public get label(): string {
        return this._label;
    }

    public get execute(): () => void {
        return this._execute;
    }

    public get disabled(): boolean {
        return this._canExecute && !this._canExecute();
    }
}

export interface IBPDropdownAction extends IBPAction {
    icon: string;
    actions: IBPDropdownItemAction[];
    disabled?: boolean;
    tooltip?: string;
    label?: string;
}

export class BPDropdownAction implements IBPDropdownAction {
    private _actions: IBPDropdownItemAction[];

    constructor(
        private _canExecute: () => boolean,
        private _icon: string,
        private _tooltip?: string,
        private _label?: string,
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
        return this._canExecute && !this._canExecute();
    }

    public get actions(): IBPDropdownItemAction[] {
        return this._actions;
    }

    public get tooltip(): string {
        return this._tooltip;
    }

    public get label(): string {
        return this._label;
    }
}
