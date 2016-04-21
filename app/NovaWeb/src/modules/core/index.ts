import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import {LocalizationService} from "./localization";
import {ConfigValueHelper} from "./config.value.helper";
import {DialogService} from "../services/dialog.svc";
import {ProjectService} from "../services/project.svc";

angular.module("app.core", ["ui.router", "ui.bootstrap"])
    //.component("app", new AppComponent());
    //.config(routesConfig);
    .service("localization", LocalizationService)
    .service("dialogService", DialogService)
    .service("configValueHelper", ConfigValueHelper)
    .service("projectService", ProjectService);

