import "angular";
import {BpBaselineHeader} from "./baselineHeader.component";
import {CollectionEditors} from "../../collection";


export const BaselineHeader = angular.module("baselineHeader", [CollectionEditors])
    .component("bpBaselineHeader", new BpBaselineHeader()).name;
