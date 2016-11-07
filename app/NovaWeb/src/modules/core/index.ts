import "angular-ui-router";
import "angular-ui-bootstrap";
import "./services";
import "./messages";
import "./navigation";
import "./loading-overlay";
import {LocalizationService, localeConfig} from "./localization/";

angular.module("app.core", [
    "bp.core.services",
    "bp.core.messages",
    "bp.core.navigation",
    "bp.core.loadingOverlay"])
    .service("localization", LocalizationService)
    .config(localeConfig);


export {
    ILocalizationService,
    BPLocale
} from "./localization/";

export {
    IWindowResize,
    IWindowVisibility,
    IUserOrGroupInfo,
    IUsersAndGroupsService,
    UsersAndGroupsService
} from "./services";

