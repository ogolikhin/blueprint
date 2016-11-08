import {MessageService} from "./message.svc";
import {MessageComponent} from "./message";
import {MessageContainerComponent} from "./message-container";

require("./messages.scss");

angular.module("bp.core.messages", [])
    .service("messageService", MessageService)
    .component("message", new MessageComponent())
    .component("messagesContainer", new MessageContainerComponent());
