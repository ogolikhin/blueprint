import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import { AppConstants, IAppConstants } from "./constants/";
import { LocalizationService, ILocalizationService } from "./localization/";
import { ConfigValueHelper, IConfigValueHelper } from "./configuration";
import { IStateManager, StateManager} from "./services";
import "./messages";


angular.module("app.core", ["ui.router", "ui.bootstrap", "bp.messages"])
    .constant("appConstants", new AppConstants())
    .service("localization", LocalizationService)
    .service("configValueHelper", ConfigValueHelper)
    .service("stateManager", StateManager);

export {
    IAppConstants,
    ILocalizationService, 
    IConfigValueHelper,
    ConfigValueHelper,
    IStateManager
};
export {IMessageService, IMessage, MessageService, Message, MessageType} from "./messages"
