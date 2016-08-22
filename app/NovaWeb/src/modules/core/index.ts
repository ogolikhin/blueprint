import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "./services";
import "./messages";
import { AppConstants, IAppConstants } from "./constants/";
import { IFileUploadService, FileUploadService, IFileResult } from "./file-upload/";
import { LocalizationService, localeConfig } from "./localization/";
import { SettingsService, ISettingsService } from "./configuration";


angular.module("app.core", [
    "bp.core.services",
    "bp.core.messages"])
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
}
export {
    ILocalizationService,
    BPLocale} from "./localization/";

export {
    IStateManager,
    IPropertyChangeSet,
    ItemState,
    IWindowResize,
    IWindowVisibility
} from "./services";

export {
    IMessageService,
    IMessage,
    MessageService,
    Message,
    MessageType
} from "./messages";
