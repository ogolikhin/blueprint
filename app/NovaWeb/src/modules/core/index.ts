import * as angular from "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "./services";
import "./messages";
import "./navigation";
import "./loading-overlay";
import {AppConstants, IAppConstants} from "./constants/";
import {IFileUploadService, FileUploadService, IFileResult} from "./file-upload/";
import {LocalizationService, localeConfig} from "./localization/";
import {SettingsService, ISettingsService} from "./configuration";

angular.module("app.core", [
    "bp.core.services",
    "bp.core.messages",
    "bp.core.navigation",
    "bp.core.loadingOverlay"])
    .constant("appConstants", new AppConstants())
    .service("fileUploadService", FileUploadService)
    .service("localization", LocalizationService)
    .service("settings", SettingsService)
    .config(localeConfig);

export {
    IAppConstants,
    ISettingsService,
    SettingsService,
};

export {
    IFileUploadService,
    IFileResult
};

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

export {
    IMessageService,
    IMessage,
    MessageService,
    Message,
    MessageType
} from "./messages";

export {
    IHttpInterceptorConfig,
    HttpStatusCode
} from "./http";

export {
    INavigationState,
    INavigationService
} from "./navigation";
