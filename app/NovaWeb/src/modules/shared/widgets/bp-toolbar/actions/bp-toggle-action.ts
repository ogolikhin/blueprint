import {IBPAction} from "./bp-action";

export interface IBPToggleItemAction {
    icon: string;
    value: any;
    disabled?: boolean;
    tooltip?: string;
    label?: string;
}

export class BPToggleItemAction implements IBPToggleItemAction {
    constructor(
        private _icon: string,
        private _value: any,
        private _canExecute?: () => boolean,
        private _tooltip?: string,
        private _label?: string
    ) {
    }

    public get icon(): string {
        return this._icon;
    }

    public get value(): any {
        return this._value;
    }

    public get disabled(): boolean {
        return this._canExecute && this._canExecute();
    }

    public get tooltip(): string {
        return this._tooltip;
    }

    public get label(): string {
        return this._label;
    }
}

export interface IBPToggleAction extends IBPAction {
    actions: IBPToggleItemAction[];
    disabled?: boolean;
}

export class BPToggleAction implements IBPToggleAction {
    private _actions: IBPToggleItemAction[];
    
    constructor(
        private _canToggle: () => boolean,
        ... actions: IBPToggleItemAction[]
    ) {
        this._actions = actions;
    }

    public get type(): string {
        return "toggle";
    }

    public get actions(): IBPToggleItemAction[] {
        return this._actions;
    }

    public get disabled(): boolean {
        return !this._canToggle();
    }
}