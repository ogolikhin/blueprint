import { BPTreeInlineEditing } from "./bp-tree-inline-editing";


angular.module("bp.widjets.inlineedit", [])
    .directive("bpTreeInlineEditing", BPTreeInlineEditing.factory());

