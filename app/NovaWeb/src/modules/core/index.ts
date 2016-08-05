import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import { AppConstants, IAppConstants } from "./constants/";
import { LocalizationService, ILocalizationService } from "./localization/";
import { ConfigValueHelper, IConfigValueHelper } from "./configuration";
import { ItemState, IStateManager, StateManager } from "./services/state-manager";
import { IWindowResize, WindowResize} from "./services/window-resize";
import { IWindowVisibility, WindowVisibility} from "./services/window-visibility";
import "./messages";


angular.module("app.core", ["ui.router", "ui.bootstrap", "bp.messages"])
    .constant("appConstants", new AppConstants())
    .service("localization", LocalizationService)
    .service("configValueHelper", ConfigValueHelper)
    .service("stateManager", StateManager)
    .service("windowResize", WindowResize)
    .service("windowVisibility", WindowVisibility);

export {
    IAppConstants,
    ILocalizationService, 
    IConfigValueHelper,
    ConfigValueHelper,
    IStateManager,
    ItemState,
    IWindowResize,
    IWindowVisibility
};
export { IMessageService, IMessage, MessageService, Message, MessageType } from "./messages"
