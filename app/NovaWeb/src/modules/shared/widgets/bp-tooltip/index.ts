import * as angular from "angular";
import { BPTooltip } from "./bp-tooltip";

angular.module("bp.widjets.tooltip", [])
    .directive("bpTooltip", BPTooltip.factory());

