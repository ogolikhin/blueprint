import {IBPDropdownToolbarOption, IBPDropdownMenuItemToolbarOption} from "./bp-toolbar-option";

export class BPDropdown implements IBPDropdownToolbarOption {
    constructor(
        private _menuItems: IBPDropdownMenuItemToolbarOption[],
        private _canClick: () => boolean,
        private _icon: string,
        private _label?: string,
        private _tooltip?: string
    ) {
    }

    public get type(): string {
        return "dropdown";
    }

    public get icon(): string {
        return this._icon;
    }

    public get isDisabled(): boolean {
        return !this._canClick();
    }

    public get menuItems(): IBPDropdownMenuItemToolbarOption[] {
        return this._menuItems;
    }

    public get label(): string {
        return this._label;
    }

    public get tooltip(): string {
        return this._tooltip;
    }
}

export class BPDropdownMenuItem implements IBPDropdownMenuItemToolbarOption {
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

    public get isDisabled(): boolean {
        return !this._canClick();
    }
}

export class BPUserStoryGenerationDropdown extends BPDropdown {
    constructor(
        generateAll: () => void,
        generateSelected: () => void,
        canGenerate: () => boolean
    ) {
        const menuItems: IBPDropdownMenuItemToolbarOption[] = [];
        menuItems.push(
            new BPDropdownMenuItem("Generate All", generateAll, canGenerate),
            new BPDropdownMenuItem("Generate Selected", generateAll, canGenerate)
        );

        super(menuItems, canGenerate, "fonticon fonticon2-news", undefined, "Generate User Stories");
    }
}