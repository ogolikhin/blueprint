import * as angular from "angular";
import { BPCollapsible } from "./bp-collapsible";

angular.module("bp.widjets.collapsible", [])
    .directive("bpCollapsible", BPCollapsible.factory());

