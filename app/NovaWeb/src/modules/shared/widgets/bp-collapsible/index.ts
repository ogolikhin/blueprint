import "./bp-collapsible.scss";

import {BPCollapsible} from "./bp-collapsible";

angular.module("bp.widgets.collapsible", [])
    .directive("bpCollapsible", BPCollapsible.instance());

