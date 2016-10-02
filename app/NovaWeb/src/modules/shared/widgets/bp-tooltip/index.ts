import * as angular from "angular";
import { BPTooltip } from "./bp-tooltip";

angular.module("bp.widgets.tooltip", [])
    .directive("bpTooltip", BPTooltip.factory());

