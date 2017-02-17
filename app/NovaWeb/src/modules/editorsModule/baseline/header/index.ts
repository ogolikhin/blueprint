import "angular";
import {BpBaselineHeader} from "./baselineHeader.component";

export const BaselineHeader = angular.module("baselineHeader", [])
    .component("bpBaselineHeader", new BpBaselineHeader()).name;
