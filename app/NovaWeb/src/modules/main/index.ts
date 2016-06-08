import "angular";
import "angular-sanitize";
import "angular-ui-router";
import "angular-ui-bootstrap";
import * as agGrid from "ag-grid/main";
import * as agGridEnterprise from "ag-grid-enterprise/main";
import "ng-draggable";
import "../shell";
import {IProjectRepository, ProjectRepository} from "./services/project-repository";
import {IProjectManager, ProjectManager, Models, SubscriptionEnum} from "./managers/project-manager";
import {BPTreeComponent} from "../core/widgets/bp-tree/bp-tree";
import {PageContent} from "./components/content/pagecontent";
import {BPToolbarComponent} from "./components/bp-toolbar/bp-toolbar";
import {BpSidebarLayout} from "./components/bp-sidebar-layout/bp-sidebar-layout";
import {BpAccordion} from "./components/bp-accordion/bp-accordion";
import {BpAccordionPanel} from "./components/bp-accordion/bp-accordion";
import {ProjectExplorerComponent} from "./components/projectexplorer/project-explorer";
import {MainViewComponent} from "./main.view";
import {config as routesConfig} from "./main.state";

config.$inject = ["$rootScope"];

export {
    IProjectRepository, ProjectRepository, 
    IProjectManager, ProjectManager, Models, SubscriptionEnum,
};

declare var VERSION: string; //Usages replaced by webpack.DefinePlugin
declare var BUILD_YEAR: string;

export function config($rootScope: ng.IRootScopeService) {
    $rootScope["config"] = window["config"] || { settings: {}, labels: {} };
    $rootScope["version"] = VERSION.split(".")[0] + "." + VERSION.split(".")[1] + " (" + VERSION.replace("-", ".") + ")";
    $rootScope["year"] = BUILD_YEAR;
}

if (agGridEnterprise["LicenseManager"] && angular.isFunction(agGridEnterprise["LicenseManager"].setLicenseKey)) {
    agGridEnterprise["LicenseManager"].setLicenseKey("Blueprint_Software_Systems_Inc._19-May-2016_MTQ5NTE0ODQwMDAwMA==5e9a534267a22bce0af6682e4bbcb799");
}

agGrid.initialiseAgGridWithAngular1(angular);
angular.module("app.main", ["ngSanitize", "app.shell", "ui.router", "ui.bootstrap", "agGrid", "ngDraggable"])
    .run(config)
    .service("projectRepository", ProjectRepository)
    .service("projectManager", ProjectManager)
    .component("bpTree", new BPTreeComponent())
    .component("bpMainView", new MainViewComponent())
    .component("pagecontent", new PageContent())
    .component("bpToolbar", new BPToolbarComponent())
    .component("bpSidebarLayout", new BpSidebarLayout())
    .component("bpAccordion", new BpAccordion())
    .component("bpAccordionPanel", new BpAccordionPanel())
    .component("bpProjectExplorer", new ProjectExplorerComponent())
    .config(routesConfig);


