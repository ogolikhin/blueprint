import {MomentDateTimeFilter} from "./momentDateTimeFilter";

angular.module("bp.components.momentDateTimeFilter", [])
    .filter("momentDateTime", MomentDateTimeFilter.filter);
