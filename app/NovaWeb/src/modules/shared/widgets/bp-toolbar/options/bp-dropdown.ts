import {IBPDropdownToolbarOption, IBPDropdownMenuItemToolbarOption} from "./bp-toolbar-option";

export class BPDropdownToolbarOption implements IBPDropdownToolbarOption {
    private _options: IBPDropdownMenuItemToolbarOption[];

    constructor(
        private _canClick: () => boolean,
        private _icon: string,
        private _label?: string,
        private _tooltip?: string,
        ... options: IBPDropdownMenuItemToolbarOption[]
    ) {
        this._options = options;
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

    public get options(): IBPDropdownMenuItemToolbarOption[] {
        return this._options;
    }

    public get label(): string {
        return this._label;
    }

    public get tooltip(): string {
        return this._tooltip;
    }
}

export class BPDropdownMenuItemToolbarOption implements IBPDropdownMenuItemToolbarOption {
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