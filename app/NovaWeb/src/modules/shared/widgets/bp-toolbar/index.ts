import * as angular from "angular";
import {BPToolbar} from "./bp-toolbar";

angular.module("bp.widgets.toolbar", [])
    .component("bpToolbar2", new BPToolbar());

export {
    IBPToolbarOption,
    IBPButtonToolbarOption,
    IBPDropdownToolbarOption,
    IBPDropdownMenuItemToolbarOption,
    IBPToggleToolbarOption
} from "./options/bp-toolbar-option";