import * as angular from "angular";
import {
    BPToolbar,
    BPToolbarElement,
    BPToolbarButton,
    BPToolbarToggle,
    BPToolbarDropdown,
    BPToolbarButtonDropdown,
    BPToolbarButtonGroup
} from "./components";

angular.module("bp.widgets.toolbar", [])
    .component("bpToolbar2", new BPToolbar())
    .component("bpToolbarElement", new BPToolbarElement())
    .component("bpToolbarButton", new BPToolbarButton())
    .component("bpToolbarToggle", new BPToolbarToggle())
    .component("bpToolbarDropdown", new BPToolbarDropdown())
    .component("bpToolbarButtonDropdown", new BPToolbarButtonDropdown())
    .component("bpToolbarButtonGroup", new BPToolbarButtonGroup());

export {
    IBPAction,
    BPButtonAction,
    BPToggleItemAction,
    BPToggleAction,
    BPDropdownItemAction,
    BPDropdownAction,
    BPButtonDropdownItemAction,
    BPButtonDropdownAction,
    BPButtonGroupAction
} from "./actions";
