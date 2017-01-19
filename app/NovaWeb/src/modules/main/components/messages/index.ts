//3rd party(external) library dependencies used for this module
import "angular";
//internal dependencies used for this module
import {MessageService} from "./message.svc";
import {MessageComponent} from "./message";
import {MessageContainerComponent} from "./message-container";
//internal CSS/SCSS for this module
require("./messages.scss");

angular.module("bp.core.messages", [])
    .service("messageService", MessageService)
    .component("message", new MessageComponent())
    .component("messagesContainer", new MessageContainerComponent());
