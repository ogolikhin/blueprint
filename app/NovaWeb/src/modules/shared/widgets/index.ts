import * as angular from "angular";
import "./bp-dialog";
import "./bp-avatar";
import "./bp-tree";
import "./bp-tree-dragndrop";
import "./bp-tree-inline-editing";
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
import "./bp-filtered-input";
import "./bp-goto";

angular.module("bp.widgets", [
    "bp.widgets.dialog",
    "bp.widgets.avatar",
    "bp.widgets.tree",
    "bp.widgets.treedraganddrop",
    "bp.widgets.treeView",
    "bp.widgets.inlineedit",
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
    "bp.widgets.filtered-input",
    "bp.widgets.goto"
]);

export {
    IUploadStatusDialogData
} from "./bp-file-upload-status/bp-file-upload-status";

export {
    IBPTreeController,
    ITreeNode
} from "./bp-tree"

export {
    IDialogSettings,
    IDialogData,
    IDialogService,
    BaseDialogController
} from "./bp-dialog";

export {
    IBPAction,
    BPButtonAction,
    BPDropdownItemAction,
    BPDropdownAction,
    BPToggleItemAction,
    BPToggleAction,
    BPButtonGroupAction
} from "./bp-toolbar";
