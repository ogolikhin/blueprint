import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "bootstrap/dist/css/bootstrap.css";
import "../shell";
import {Sidebar} from "./components/sidebar/sidebar";
import {config as routesConfig} from "./main.state";

angular.module("app.main", ["app.shell", "ui.router", "ui.bootstrap"])
    .component("sidebar", new Sidebar())
    .config(routesConfig);