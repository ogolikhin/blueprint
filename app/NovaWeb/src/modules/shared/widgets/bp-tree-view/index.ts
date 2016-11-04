import * as angular from "angular";
import {BPTreeViewComponent, IBPTreeViewController, BPTreeViewController, ITreeViewNode, IColumn} from "./bp-tree-view";

angular.module("bp.widgets.treeView", [])
    .component("bpTreeView", new BPTreeViewComponent());    

export {IBPTreeViewController, BPTreeViewController, ITreeViewNode, IColumn};
