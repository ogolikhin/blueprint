import {IBPToggleToolbarOption, IBPButtonToolbarOption} from "./bp-toolbar-option";

export class BPToggleToolbarOption implements IBPToggleToolbarOption {
    private _options: IBPButtonToolbarOption[]
    
    constructor(
        private _canToggle: () => boolean,
        ... options: IBPButtonToolbarOption[]
    ) {
        this._options = options;
    }

    public get type(): string {
        return "toggle";
    }

    public get options(): IBPButtonToolbarOption[] {
        return this._options;
    }

    public get disabled(): boolean {
        return !this._canToggle();
    }
}