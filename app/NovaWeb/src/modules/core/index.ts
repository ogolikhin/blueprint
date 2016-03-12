import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "bootstrap/dist/css/bootstrap.css";
import {AuthSvc} from "./services/auth.svc";

let name: string = "app.core";

angular.module("app.core", ["ui.router", "ui.bootstrap"])
    .service("auth", AuthSvc);
    //.component("app", new AppComponent());
    //.config(routesConfig);

export default name;