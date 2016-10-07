import {IBPButtonToolbarOption} from "./bp-toolbar-option";

export class BPButton implements IBPButtonToolbarOption {
    constructor(
        private _click: () => void,
        private _canClick: () => boolean,
        private _icon: string,
        private _label?: string,
        private _tooltip?: string
    ) {
    }

    public get type(): string {
        return "button";
    }

    public get icon(): string {
        return this._icon;
    }

    public get isDisabled(): boolean {
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