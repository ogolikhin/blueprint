import * as angular from "angular";
import { BPTreeDragndrop } from "./bp-tree-dragndrop";


angular.module("bp.widjets.treedraganddrop", [])
    .directive("bpTreeDragndrop", BPTreeDragndrop.factory());

