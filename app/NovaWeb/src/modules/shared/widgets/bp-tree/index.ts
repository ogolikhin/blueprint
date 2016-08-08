﻿import { BPTreeComponent, IBPTreeController, ITreeNode } from "./bp-tree";

angular.module("bp.widjets.tree", [])
    .component("bpTree", new BPTreeComponent());

export { IBPTreeController, ITreeNode };
