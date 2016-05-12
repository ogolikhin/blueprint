import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import {LocalizationService} from "./localization";
import {ConfigValueHelper} from "./config.value.helper";
import {DialogService} from "../services/dialog.svc";

angular.module("app.core", ["ui.router", "ui.bootstrap"])
    .service("localization", LocalizationService)
    .service("dialogService", DialogService)
    .service("configValueHelper", ConfigValueHelper);

