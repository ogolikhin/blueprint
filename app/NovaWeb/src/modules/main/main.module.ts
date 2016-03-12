import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "bootstrap/dist/css/bootstrap.css"
import {PageAboutComponent} from "./components/about/about.component";
import {AboutSvc} from "./components/about/about.service";
import {Sidebar} from "./components/sidebar/sidebar";
import {config as routesConfig} from "./main.routes";

angular.module("app.main", ["ui.router", "ui.bootstrap"])
    .component("sidebar", new Sidebar())
    .component("pageAbout", new PageAboutComponent())
    .service("about", AboutSvc)
    .component("test", <ng.IComponentOptions>{
        template: "<div>Test</test>"
    })
    .config(routesConfig);