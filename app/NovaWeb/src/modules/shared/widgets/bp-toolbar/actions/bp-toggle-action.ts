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
        private _disabled?: boolean,
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
        return this._disabled;
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
    currentValue: any;
    disabled?: boolean;
}

export class BPToggleAction implements IBPToggleAction {
    private _actions: IBPToggleItemAction[];
    private _currentValue: any;
    
    constructor(
        private initialValue: any,
        private toggle: (value: any) => void,
        private canToggle?: () => boolean,
        ... actions: IBPToggleItemAction[]
    ) {
        this._actions = actions;
        this._currentValue = initialValue;
    }

    public get type(): string {
        return "toggle";
    }

    public get actions(): IBPToggleItemAction[] {
        return this._actions;
    }

    public get currentValue(): any {
        return this._currentValue;
    }

    public set currentValue(value: any) {
        this._currentValue = value;

        if (this.toggle) {
            this.toggle(value);
        }
    }

    public get disabled(): boolean {
        return this.canToggle && !this.canToggle();
    }
}