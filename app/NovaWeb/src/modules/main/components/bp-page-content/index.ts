import {PageContent} from "./bp-page-content";
import {MainBreadcrumbService} from "./mainbreadcrumb.svc";

angular.module("bp.components.pagecontent", [])
    .component("pagecontent", new PageContent())
    .service("mainbreadcrumbService", MainBreadcrumbService);
