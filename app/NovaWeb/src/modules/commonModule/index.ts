import "angular";
import {ConfigurationModule} from "./configuration";
import {HttpInterceptorModule} from "./httpInterceptor";
import {FileUpload} from "./fileUpload";
import {LoadingOverlay} from "./loadingOverlay";
import {LocalStorage} from "./localStorage";
import {Localization} from "./localization";
import {ItemInfo} from "./itemInfo";
import {Navigation} from "./navigation";
import {Download} from "./download";
import {CoreServices} from "./services/index";
import * as uiBootstrap from "angular-ui-bootstrap";

export const CommonModule = angular.module("commonModule", [
    "ui.router",
    uiBootstrap,
    Navigation,
    ItemInfo,
    FileUpload,
    LoadingOverlay,
    Localization,
    CoreServices,
    LocalStorage,
    ConfigurationModule,
    HttpInterceptorModule,
    Download
])
    .name;
