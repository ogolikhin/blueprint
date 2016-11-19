import * as angular from "angular";
import {BPTreeComponent, IBPTreeControllerApi, IColumn, IColumnRendererParams} from "./bp-tree";

angular.module("bp.widgets.tree", [])
    .component("bpTree", new BPTreeComponent());

export {IBPTreeControllerApi, IColumn, IColumnRendererParams};
