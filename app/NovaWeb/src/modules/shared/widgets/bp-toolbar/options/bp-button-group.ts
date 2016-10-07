import {IBPButtonGroupToolbarOption, IBPButtonToolbarOption} from "./bp-toolbar-option";

export class BPButtonGroupToolbarOption implements IBPButtonGroupToolbarOption {
    private _options: IBPButtonToolbarOption[];

    constructor(
        ... options: IBPButtonToolbarOption[]
    ) {
        this._options = options;
    }

    public get type(): string {
        return "buttongroup";
    }

    public get options(): IBPButtonToolbarOption[] {
        return this._options;
    }
}