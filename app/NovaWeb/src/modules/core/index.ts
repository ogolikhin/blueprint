import "./configuration";
import "./navigation";
import "./services";

import {ConfigurationModule} from "./configuration";
import {HttpInterceptorModule} from "./httpInterceptor";
import {FileUpload} from "./fileUpload";
import {LoadingOverlay} from "./loadingOverlay";
import {LocalStorage} from "./localStorage";
import {Localization} from "./localization";

angular.module("bp.core", [
    "ui.router",
    "ui.bootstrap",
    FileUpload,
    LoadingOverlay,
    Localization,
    "bp.core.navigation",
    "bp.core.services",
    LocalStorage,
    ConfigurationModule,
    HttpInterceptorModule
]);

export {
    IWindowResize,
    IWindowVisibility,
    IUserOrGroupInfo,
    IUsersAndGroupsService,
    UsersAndGroupsService
} from "./services";

