import {IBPAction} from "./bp-action";

export interface IBPButtonAction extends IBPAction {
    execute: () => void;
    icon: string;
    disabled?: boolean;
    label?: string;
    tooltip?: string;
}

export class BPButtonAction implements IBPButtonAction {
    constructor(private _execute: () => void,
                private _canExecute: () => boolean,
                private _icon: string,
                private _tooltip?: string,
                private _label?: string) {
    }

    public get type(): string {
        return "button";
    }

    public get icon(): string {
        return this._icon;
    }

    public get disabled(): boolean {
        return this._canExecute && !this._canExecute();
    }

    public get execute(): () => void {
        return this._execute;
    }

    public get label(): string {
        return this._label;
    }

    public get tooltip(): string {
        return this._tooltip;
    }
}
