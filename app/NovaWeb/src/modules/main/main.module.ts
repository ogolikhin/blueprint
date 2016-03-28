import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "../shell";
import {Sidebar} from "./components/sidebar/sidebar";
import {config as routesConfig} from "./main.state";

config.$inject = ["$rootScope"];
export function config($rootScope: ng.IRootScopeService) {
    $rootScope['config'] = window['config'] || { settings: {}, labels: {} };
}

angular.module("app.main", ["app.shell", "ui.router", "ui.bootstrap"])
    .run(config)
    .component("sidebar", new Sidebar())
    .config(routesConfig);