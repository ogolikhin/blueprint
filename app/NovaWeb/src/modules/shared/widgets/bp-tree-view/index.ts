import * as angular from "angular";
import {BPTreeViewComponent, IBPTreeViewController, BPTreeViewController, ITreeViewNodeVM, IColumn} from "./bp-tree-view";

angular.module("bp.widgets.treeView", [])
    .component("bpTreeView", new BPTreeViewComponent());    

export {IBPTreeViewController, BPTreeViewController, ITreeViewNodeVM, IColumn};
