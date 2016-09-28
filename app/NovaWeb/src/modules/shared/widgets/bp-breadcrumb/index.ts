import * as angular from "angular";
import {BPBreadcrumbComponent} from "./bp-breadcrumb";
 
 angular.module("bp.widgets.breadcrumb", [])
        .component("bpBreadcrumb", new BPBreadcrumbComponent());