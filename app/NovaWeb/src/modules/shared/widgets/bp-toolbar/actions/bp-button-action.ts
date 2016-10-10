import {IBPAction} from "./bp-action";

export interface IBPButtonAction extends IBPAction {
    click: () => void;
    icon: string;
    disabled?: boolean;
    label?: string;
    tooltip?: string;
}

export class BPButtonAction implements IBPButtonAction {
    constructor(
        private _click: () => void,
        private _canClick: () => boolean,
        private _icon: string,
        private _tooltip?: string,
        private _label?: string
    ) {
    }

    public get type(): string {
        return "button";
    }

    public get icon(): string {
        return this._icon;
    }

    public get disabled(): boolean {
        return !this._canClick();
    }

    public get click(): () => void {
        return this._click;
    }

    public get label(): string {
        return this._label;
    }

    public get tooltip(): string {
        return this._tooltip;
    }
}