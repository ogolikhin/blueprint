import * as angular from "angular";
import {BPTreeDragndrop} from "./bp-tree-dragndrop";


angular.module("bp.widgets.treedraganddrop", [])
    .directive("bpTreeDragndrop", BPTreeDragndrop.factory());

