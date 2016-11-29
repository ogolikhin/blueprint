import * as angular from "angular";
import "./bp-dialog";
import "./bp-avatar";
import "./bp-tree-view";
import "./bp-tooltip";
import "./bp-infinite-scroll";
import "./bp-select";
import "./bp-item-icon";
import "./bp-speciallink";
import "./bp-compile-html";
import "./bp-collapsible";
import "./bp-file-upload";
import "./bp-toggle";
import "./bp-breadcrumb";
import "./bp-toolbar";
import "./bp-artifact-list";
import "./bp-filtered-input";
import "./bp-goto";
import "./tabSlider";

angular.module("bp.widgets", [
    "bp.widgets.dialog",
    "bp.widgets.avatar",
    "bp.widgets.treeView",
    "bp.widgets.tooltip",
    "bp.widgets.infinitescroll",
    "bp.widgets.select",
    "bp.widgets.itemicon",
    "bp.widgets.speciallink",
    "bp.widgets.compilehtml",
    "bp.widgets.collapsible",
    "bp.widgets.fileupload",
    "bp.widgets.toggle",
    "bp.widgets.breadcrumb",
    "bp.widgets.toolbar",
    "bp.widgets.artifactList",
    "bp.widgets.filtered-input",
    "bp.widgets.goto",
    "bp.widgets.tabSlider"
]);

export {
    IUploadStatusDialogData
} from "./bp-file-upload-status/bp-file-upload-status";

export {
    IDialogSettings,
    IDialogData,
    IDialogService,
    BaseDialogController
} from "./bp-dialog";

export {
    IBPAction,
    IBPButtonOrDropdownAction,
    BPButtonAction,
    BPDropdownItemAction,
    BPDropdownAction,
    BPToggleItemAction,
    BPToggleAction,
    BPButtonGroupAction,
    BPButtonOrDropdownAction,
    BPDotsMenuAction
} from "./bp-toolbar";
