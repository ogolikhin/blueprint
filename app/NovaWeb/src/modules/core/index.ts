import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import { AppConstants, IAppConstants } from "./constants/app-constants";
import { LocalizationService, ILocalizationService } from "./localization";
import { ConfigValueHelper, IConfigValueHelper } from "./config.value.helper";

angular.module("app.core", ["ui.router", "ui.bootstrap"])
    .constant("appConstants", new AppConstants())
    .service("localization", LocalizationService)
    .service("configValueHelper", ConfigValueHelper);

export {
    IAppConstants,
    ILocalizationService, 
    IConfigValueHelper,
    ConfigValueHelper,
};