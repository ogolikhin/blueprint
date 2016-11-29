import {IBPAction} from "./bp-action";

export interface IBPButtonOrDropdownAction {
    execute: () => void;
    icon?: string;
    disabled?: boolean;
    label?: string;
    tooltip?: string;
}

export class BPButtonOrDropdownAction implements IBPButtonOrDropdownAction {
    constructor(private _execute?: () => void,
                private _canExecute?: () => boolean,
                private _icon?: string,
                private _tooltip?: string,
                private _label?: string) {
    }

    public get execute(): () => void {
        return this._execute;
    }

    public get icon(): string {
        return this._icon;
    }

    public get disabled(): boolean {
        return this._canExecute && !this._canExecute();
    }

    public get label(): string {
        return this._label;
    }

    public get tooltip(): string {
        return this._tooltip;
    }
}

export interface IBPDotsMenuAction extends IBPAction {
    icon: string;
    actions: IBPButtonOrDropdownAction[];
    tooltip?: string;
}

export class BPDotsMenuAction implements IBPDotsMenuAction {
    private _actions: IBPButtonOrDropdownAction[];

    constructor(private _tooltip?: string,
                ...actions: IBPButtonOrDropdownAction[]) {
        this._actions = actions;
    }

    public get type(): string {
        return "dotsmenu";
    }

    public get icon(): string {
        return "fonticon2-navigation";
    }

    public get actions(): IBPButtonOrDropdownAction[] {
        return this._actions;
    }

    public get tooltip(): string {
        return this._tooltip;
    }
}
