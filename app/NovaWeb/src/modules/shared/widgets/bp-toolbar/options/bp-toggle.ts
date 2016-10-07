import {IBPToggleToolbarOption, IBPButtonToolbarOption} from "./bp-toolbar-option";
import {BPButton} from "./bp-button";

export class BPToggle implements IBPToggleToolbarOption {
    constructor(
        private _toggleOptions: IBPButtonToolbarOption[],
        private _canToggle: () => boolean
    ) {
    }

    public get type(): string {
        return "toggle";
    }

    public get toggleOptions(): IBPButtonToolbarOption[] {
        return this._toggleOptions;
    }

    public get isDisabled(): boolean {
        return !this._canToggle();
    }
}

export class BPProcessTypeToggle extends BPToggle {
    constructor(
        toggleBusiness: () => void,
        toggleUserToSystem: () => void,
        canToggle: () => boolean
    ) {
        const toggleOptions: IBPButtonToolbarOption[] = [];
        toggleOptions.push(
            new BPButton(toggleBusiness, canToggle, "fonticon fonticon2-user-user", undefined, "Business process type"),
            new BPButton(toggleUserToSystem, canToggle, "fonticon fonticon2-user-system", undefined, "User-to-System process type")
        );

        super(toggleOptions, canToggle);
    }
}