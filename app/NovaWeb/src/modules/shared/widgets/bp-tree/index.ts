import * as angular from "angular";
import { BPTreeComponent, IBPTreeController, ITreeNode } from "./bp-tree";

angular.module("bp.widgets.tree", [])
    .component("bpTree", new BPTreeComponent());

export { IBPTreeController, ITreeNode };
