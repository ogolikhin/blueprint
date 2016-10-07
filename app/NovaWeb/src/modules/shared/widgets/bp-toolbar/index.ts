import * as angular from "angular";
import {BPToolbar} from "./bp-toolbar";
import {BPToolbarElement, BPToolbarButton, BPToolbarDropdown, BPToolbarToggle, BPToolbarButtonGroup} from "./components";

angular.module("bp.widgets.toolbar", [])
    .component("bpToolbar2", new BPToolbar())
    .component("bpToolbarElement", new BPToolbarElement())
    .component("bpToolbarButton", new BPToolbarButton())
    .component("bpToolbarDropdown", new BPToolbarDropdown())
    .component("bpToolbarToggle", new BPToolbarToggle())
    .component("bpToolbarButtonGroup", new BPToolbarButtonGroup());

export {
    IBPToolbarOption,
    BPButtonToolbarOption,
    BPDropdownToolbarOption,
    BPDropdownMenuItemToolbarOption,
    BPToggleToolbarOption,
    BPButtonGroupToolbarOption
} from "./options";