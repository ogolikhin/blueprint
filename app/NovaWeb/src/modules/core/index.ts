import "./configuration";
import "./services";

import {ConfigurationModule} from "./configuration";
import {HttpInterceptorModule} from "./httpInterceptor";
import {FileUpload} from "./fileUpload";
import {LoadingOverlay} from "./loadingOverlay";
import {LocalStorage} from "./localStorage";
import {Localization} from "./localization";
import {ItemInfo} from "./itemInfo";
import {Navigation} from "./navigation";

angular.module("bp.core", [
    "ui.router",
    "ui.bootstrap",
    Navigation,
    ItemInfo,
    FileUpload,
    LoadingOverlay,
    Localization,
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

