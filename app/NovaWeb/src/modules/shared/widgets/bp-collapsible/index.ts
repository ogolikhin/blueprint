import * as angular from "angular";
import {BPCollapsible} from "./bp-collapsible";

angular.module("bp.widgets.collapsible", [])
    .directive("bpCollapsible", BPCollapsible.factory());

