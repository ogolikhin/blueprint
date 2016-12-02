import * as angular from "angular";
import {
    BPToolbar,
    BPToolbarElement,
    BPToolbarButton,
    BPToolbarToggle,
    BPToolbarDropdown,
    BPToolbarMenu,
    BPToolbarButtonGroup
} from "./components";

angular.module("bp.widgets.toolbar", [])
    .component("bpToolbar2", new BPToolbar())
    .component("bpToolbarElement", new BPToolbarElement())
    .component("bpToolbarButton", new BPToolbarButton())
    .component("bpToolbarToggle", new BPToolbarToggle())
    .component("bpToolbarDropdown", new BPToolbarDropdown())
    .component("bpToolbarMenu", new BPToolbarMenu())
    .component("bpToolbarButtonGroup", new BPToolbarButtonGroup());

export {
    IBPAction,
    IBPDropdownAction,
    IBPButtonOrDropdownAction,
    BPButtonAction,
    BPToggleItemAction,
    BPToggleAction,
    BPDropdownItemAction,
    BPDropdownAction,
    BPButtonOrDropdownAction,
    BPButtonOrDropdownSeparator,
    BPMenuAction,
    BPButtonGroupAction
} from "./actions";
