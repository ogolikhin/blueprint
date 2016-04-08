import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "../shell";
import {PageContent} from "./components/content/pagecontent";
import {Toolbar} from "./components/toolbar/toolbar";
import {Sidebar} from "./components/sidebar/sidebar";
import {ProjectExplorer} from "./components/projectexplorer/projectexplorer";
import {config as routesConfig} from "./main.state";

config.$inject = ["$rootScope"];

declare var VERSION: string;

export function config($rootScope: ng.IRootScopeService) {
    $rootScope["config"] = window["config"] || { settings: {}, labels: {} };
    $rootScope["version"] = VERSION.split(".")[0] + "." + VERSION.split(".")[1] + " (" + VERSION.replace("-", ".") + ")";
    $rootScope["year"] = new Date().getFullYear().toString();
}

angular.module("app.main", ["app.shell", "ui.router", "ui.bootstrap"])
    .run(config)
    .component("pagecontent", new PageContent())
    .component("sidebar", new Sidebar())
    .component("projectexplorer", new ProjectExplorer())
    .component("toolbar", new Toolbar())
    .config(routesConfig);