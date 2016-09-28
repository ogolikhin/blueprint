import * as angular from "angular";
import { PageContent } from "./bp-page-content";

angular.module("bp.components.pagecontent", [])
    .component("pagecontent", new PageContent());
