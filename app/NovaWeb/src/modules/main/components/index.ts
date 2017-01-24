import * as angular from "angular";
import "./bp-accordion";
import "./bp-artifact-info";
import "./page-toolbar";
import "./bp-explorer";
import "./bp-page-content";
import "./quickSearch";
import "./momentDateFilter";
import "./momentDateTimeFilter";
import "./pagination";
import "./dialogs/open-project";
import "./messages";
import "./projectControls";

angular.module("bp.components", [
    "bp.components.messages",
    "bp.components.momentDateFilter",
    "bp.components.momentDateTimeFilter",
    "bp.components.accordion",
    "bp.components.artifactinfo",
    "bp.components.pagetoolbar",
    "bp.components.explorer",
    "bp.components.pagecontent",
    "bp.components.artifactpicker",
    "bp.components.quickSearch",
    "bp.components.pagination",
    "bp.components.projectControls"
]);
