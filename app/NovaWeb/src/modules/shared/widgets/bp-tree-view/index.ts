import { BPTreeViewComponent, IBPTreeViewController, ITreeViewNodeVM, IColumn } from "./bp-tree-view";

angular.module("bp.widjets.treeView", [])
    .component("bpTreeView", new BPTreeViewComponent());

export { IBPTreeViewController, ITreeViewNodeVM, IColumn };
