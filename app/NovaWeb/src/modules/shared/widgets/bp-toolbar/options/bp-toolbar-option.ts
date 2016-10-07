export interface IBPToolbarOption {
    type: string;
}

export interface IBPButtonToolbarOption extends IBPToolbarOption {
    click: () => void;
    icon: string;
    disabled?: boolean;
    label?: string;
    tooltip?: string;
}

export interface IBPDropdownMenuItemToolbarOption {
    label: string;
    click: () => void;
    disabled?: boolean;
}

export interface IBPDropdownToolbarOption extends IBPToolbarOption {
    icon: string;
    options: IBPDropdownMenuItemToolbarOption[];
    label?: string;
    disabled?: boolean;
}

export interface IBPToggleToolbarOption extends IBPToolbarOption {
    options: IBPButtonToolbarOption[];
    disabled?: boolean;
}

export interface IBPButtonGroupToolbarOption extends IBPToolbarOption {
    options: IBPToolbarOption[];
    disabled?: boolean;
}