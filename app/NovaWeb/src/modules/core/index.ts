import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import {LocalizationService} from "./localization";

angular.module("app.core", ["ui.router", "ui.bootstrap"])
    //.component("app", new AppComponent());
    //.config(routesConfig);
    .service("localization", LocalizationService);