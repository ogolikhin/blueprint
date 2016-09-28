import * as angular from "angular";
import {BreadcrumbService} from "./breadcrumb.svc";
import {BPBreadcrumbComponent} from "./bp-breadcrumb";
 
 angular.module("bp.widgets.breadcrumb", [])
        .service("breadcrumbService", BreadcrumbService)
        .component("bpBreadcrumb", new BPBreadcrumbComponent());