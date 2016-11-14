import * as angular from "angular";
import {BPTreeComponent, IBPTreeControllerApi} from "./bp-tree";

angular.module("bp.widgets.tree", [])
    .component("bpTree", new BPTreeComponent());

export {IBPTreeControllerApi};
