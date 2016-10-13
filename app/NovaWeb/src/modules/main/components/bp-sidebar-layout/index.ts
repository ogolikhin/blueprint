import * as angular from "angular";
import {BpSidebarLayout} from "./bp-sidebar-layout";

angular.module("bp.components.sidebar", [])
    .component("bpSidebarLayout", new BpSidebarLayout());
