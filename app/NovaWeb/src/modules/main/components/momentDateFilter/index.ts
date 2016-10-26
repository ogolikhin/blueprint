import {MomentDateFilter} from "./momentDateFilter";

angular.module("bp.components.momentDateFilter", [])
    .filter("momentDate", MomentDateFilter.filter);
