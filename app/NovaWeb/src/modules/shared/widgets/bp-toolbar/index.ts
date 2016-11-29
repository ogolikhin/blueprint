import * as angular from "angular";
import {
    BPToolbar,
    BPToolbarElement,
    BPToolbarButton,
    BPToolbarToggle,
    BPToolbarDropdown,
    BPToolbarDotsMenu,
    BPToolbarButtonGroup
} from "./components";

angular.module("bp.widgets.toolbar", [])
    .component("bpToolbar2", new BPToolbar())
    .component("bpToolbarElement", new BPToolbarElement())
    .component("bpToolbarButton", new BPToolbarButton())
    .component("bpToolbarToggle", new BPToolbarToggle())
    .component("bpToolbarDropdown", new BPToolbarDropdown())
    .component("bpToolbarDotsMenu", new BPToolbarDotsMenu())
    .component("bpToolbarButtonGroup", new BPToolbarButtonGroup());

export {
    IBPAction,
    IBPButtonOrDropdownAction,
    BPButtonAction,
    BPToggleItemAction,
    BPToggleAction,
    BPDropdownItemAction,
    BPDropdownAction,
    BPButtonOrDropdownAction,
    BPDotsMenuAction,
    BPButtonGroupAction
} from "./actions";
