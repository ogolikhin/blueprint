import * as angular from "angular";
import "./bp-accordion";
import "./bp-artifact-info";
import "./bp-page-toolbar";
import "./bp-sidebar-layout";
import "./bp-explorer";
import "./bp-page-content";
import "./quickSearch";
import "./momentDateFilter";
import "./pagination";

angular.module("bp.components", [
    "bp.components.momentDateFilter",
    "bp.components.accordion",
    "bp.components.artifactinfo",
    "bp.components.pagetoolbar",
    "bp.components.sidebar",
    "bp.components.explorer",
    "bp.components.pagecontent",
    "bp.components.artifactpicker",
    "bp.components.quickSearch",
    "bp.components.pagination"
]);
