import "./bp-dialog";
import "./bp-avatar";
import "./bp-tree";
import "./bp-tree-dragndrop";
import "./bp-tree-inline-editing";
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

angular.module("bp.widjets", [
    "bp.widjets.dialog",
    "bp.widjets.avatar",
    "bp.widjets.tree",
    "bp.widjets.treedraganddrop",
    "bp.widjets.inlineedit",
    "bp.widjets.tooltip",
    "bp.widjets.infinitescroll",
    "bp.widjets.select",
    "bp.widjets.itemicon",
    "bp.widjets.speciallink",
    "bp.widjets.compilehtml",
    "bp.widjets.collapsible",
    "bp.widjets.fileupload",
    "bp.widjets.toggle",
    "bp.widgets.breadcrumb"
]);
    
export { IUploadStatusDialogData } from "./bp-file-upload-status/bp-file-upload-status";
export { IBPTreeController, ITreeNode } from "./bp-tree"
export { IDialogSettings, IDialogData, IDialogService, BaseDialogController} from "./bp-dialog";
