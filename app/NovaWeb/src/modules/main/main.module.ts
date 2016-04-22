import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import * as agGrid from "ag-grid/main";
import "ag-grid-enterprise/main";
import "../shell";
import {PageContent} from "./components/content/pagecontent";
import {Toolbar} from "./components/toolbar/toolbar";
import {BpSidebarLayout} from "./components/bp-sidebar-layout/bp-sidebar-layout";
import {BpAccordion} from "./components/bp-accordion/bp-accordion";
import {BpAccordionPanel} from "./components/bp-accordion/bp-accordion";
import {config as routesConfig} from "./main.state";

config.$inject = ["$rootScope"];

declare var VERSION: string; //Usages replaced by webpack.DefinePlugin
declare var BUILD_YEAR: string;

export function config($rootScope: ng.IRootScopeService) {
    $rootScope["config"] = window["config"] || { settings: {}, labels: {} };
    $rootScope["version"] = VERSION.split(".")[0] + "." + VERSION.split(".")[1] + " (" + VERSION.replace("-", ".") + ")";
    $rootScope["year"] = BUILD_YEAR;
}
agGrid.initialiseAgGridWithAngular1(angular);
angular.module("app.main", ["app.shell", "ui.router", "ui.bootstrap", "agGrid"])
    .run(config)
    .component("pagecontent", new PageContent())
    .component("toolbar", new Toolbar())
    .component("bpSidebarLayout", new BpSidebarLayout())
    .component("bpAccordion", new BpAccordion())
    .component("bpAccordionPanel", new BpAccordionPanel())
    .config(routesConfig);


