export interface IBPToolbarOption {
    type: string;
}

export interface IBPButtonToolbarOption extends IBPToolbarOption {
    click: () => void;
    icon: string;
    isDisabled?: boolean;
    label?: string;
    tooltip?: string;
}

export interface IBPDropdownMenuItemToolbarOption {
    label: string;
    click: () => void;
    isDisabled?: boolean;
}

export interface IBPDropdownToolbarOption extends IBPToolbarOption {
    icon: string;
    menuItems: IBPDropdownMenuItemToolbarOption[];
    label?: string;
    isDisabled?: boolean;
}

export interface IBPToggleToolbarOption extends IBPToolbarOption {
    toggleOptions: IBPButtonToolbarOption[];
    isDisabled?: boolean;
}