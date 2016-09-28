import * as angular from "angular";
import { BPTreeViewComponent, IBPTreeViewController, ITreeViewNodeVM, IColumn } from "./bp-tree-view";

angular.module("bp.widjets.treeView", [])
    .component("bpTreeView", new BPTreeViewComponent());

export { IBPTreeViewController, ITreeViewNodeVM, IColumn };
