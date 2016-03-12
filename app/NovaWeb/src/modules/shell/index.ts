import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "bootstrap/dist/css/bootstrap.css"
import "../core";
import {AppComponent} from "./app.component";

let name: string = "app.shell";

angular.module("app.shell", ["app.core", "ui.router", "ui.bootstrap"])
    .component("app", new AppComponent());
    //.config(routesConfig);

export default name;