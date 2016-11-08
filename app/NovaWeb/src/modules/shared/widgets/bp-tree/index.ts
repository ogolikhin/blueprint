import * as angular from "angular";
import {BPTreeComponent, IBPTreeControllerApi, ITreeNode} from "./bp-tree";

angular.module("bp.widgets.tree", [])
    .component("bpTree", new BPTreeComponent());

export {IBPTreeControllerApi, ITreeNode};
