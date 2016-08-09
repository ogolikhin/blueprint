﻿import "./bp-dialog";
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
    "bp.widjets.compilehtml"
]);
    
export { IBPTreeController, ITreeNode } from "./bp-tree"
export { IDialogSettings, IDialogService, BaseDialogController} from "./bp-dialog";
