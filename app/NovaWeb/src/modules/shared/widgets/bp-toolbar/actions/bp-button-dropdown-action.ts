import {IBPAction} from "./bp-action";

export interface IBPButtonDropdownItemAction {
    label: string;
    execute: () => void;
    disabled?: boolean;
}

export class BPButtonDropdownItemAction implements IBPButtonDropdownItemAction {
    constructor(private _label: string,
                private _execute: () => void,
                private _canExecute: () => boolean) {
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

export interface IBPButtonDropdownAction extends IBPAction {
    icon: string;
    actions: IBPButtonDropdownItemAction[];
    disabled?: boolean;
    tooltip?: string;
    label?: string;
}

export class BPButtonDropdownAction implements IBPButtonDropdownAction {
    private _actions: IBPButtonDropdownItemAction[];

    constructor(private _canExecute?: () => boolean,
                private _icon?: string,
                private _tooltip?: string,
                private _label?: string,
                ...actions: IBPButtonDropdownItemAction[]) {
        this._actions = actions;
    }

    public get type(): string {
        return "buttondropdown";
    }

    public get icon(): string {
        return this._icon;
    }

    public get disabled(): boolean {
        return this._canExecute && !this._canExecute();
    }

    public get actions(): IBPButtonDropdownItemAction[] {
        return this._actions;
    }

    public get tooltip(): string {
        return this._tooltip;
    }

    public get label(): string {
        return this._label;
    }
}
