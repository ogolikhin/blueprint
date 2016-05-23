import "angular";
import "angular-sanitize";
import "angular-ui-router";
import "angular-ui-bootstrap";
import * as agGrid from "ag-grid/main";
import "ag-grid-enterprise/main";
import "ng-draggable";
import "../shell";
import {NotificationService} from "../core/notification";
import {ProjectRepository} from "./services/project-repository";
import {ProjectNotification} from "./services/project-notification";
import {ProjectManager} from "./managers/project-manager";

import {BPTreeComponent} from "../core/widgets/bp-tree/bp-tree";
import {BPTreeInlineEditing} from "../core/widgets/bp-tree-inline-editing/bp-tree-inline-editing";
import {BPTreeDragndrop} from "../core/widgets/bp-tree-dragndrop/bp-tree-dragndrop";
import {BPTooltip} from "../core/widgets/bp-tooltip/bp-tooltip";
import {PageContent} from "./components/content/pagecontent";
import {BPToolbarComponent} from "./components/bp-toolbar/bp-toolbar";
import {BpSidebarLayout} from "./components/bp-sidebar-layout/bp-sidebar-layout";
import {BpAccordion} from "./components/bp-accordion/bp-accordion";
import {BpAccordionPanel} from "./components/bp-accordion/bp-accordion";
import {ProjectExplorerComponent} from "./components/projectexplorer/project-explorer";
import {MainViewComponent} from "./main.view";
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
angular.module("app.main", ["ngSanitize", "app.shell", "ui.router", "ui.bootstrap", "agGrid", "ngDraggable"])
    .run(config)
    .service("notification", NotificationService)
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
    .directive("bpTreeInlineEditing", BPTreeInlineEditing.Factory())
    .directive("bpTreeDragndrop", BPTreeDragndrop.Factory())
    .directive("bpTooltip", BPTooltip.Factory())
    .config(routesConfig);


